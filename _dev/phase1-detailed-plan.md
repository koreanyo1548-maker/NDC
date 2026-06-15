# Phase 1 상세 실행 계획: PlayFab 완전 제거 + 로컬 저장 전환

> 분석일: 2026-06-14

## 0. 핵심 발견 (Executive Summary)

- 프로젝트는 **asmdef가 없음** → 모든 코드가 단일 `Assembly-CSharp`로 컴파일됨. PlayFab 타입을 참조하는 파일이 **하나라도** 컴파일 오류면 전체 빌드가 막힘. 따라서 "PlayFab 패키지 삭제 먼저"는 위험. **API 호환 레이어를 먼저 만들고 내부 구현만 교체**하는 전략이 안전.
- 크래시 직접 원인은 `PlayFabManager.cs:119` `OnPlayFabLoginSuccess`의 `titleData["ClientVer"]` 버전 체크. 로컬 전환 시 자연 소멸됨.
- **핵심 설계 결정**: `PlayFabManager.Store / .Leaderboard / .Data`의 **public API 시그니처를 그대로 유지**하면 51개 호출처를 거의 건드리지 않아도 됨. 내부 구현만 PlayFab → 로컬로 교체. 컴파일 오류를 최소화하는 가장 빠른 길.
- 단, **2개 타입이 PlayFab 모델에 외부 노출**되어 있어 별도 처리 필요:
  - `PlayerLeaderboardEntry` (PlayFab.ClientModels) — `UI_Ranking.cs`, `UI_Friend_Item.cs`에서 사용. (`UI_Ranking_Item.cs`는 직접 참조 없음)
  - `GetPlayerCombinedInfoRequestParams` (PlayFab.ClientModels) — `LoginScene.cs`의 `[SerializeField]` 필드 + `PlayFabManager.PlayfabParams`.
- 사전 파악된 51개 중 다수는 주석 처리된 호출이라 실제 컴파일 영향 없음. 실제 활성 호출처는 아래 의존성 그래프 참조.

---

## 1. 핵심 5개 파일 구조 분석

### 1.1 PlayFabManager.cs (정적 허브)
- 정적 필드: `_store`, `_leaderboard`, `_data` + public 게터 `Store`, `Leaderboard`, `Data`.
- `public static GetPlayerCombinedInfoRequestParams PlayfabParams;` ← **PlayFab 모델 노출**.
- public API: `Init()`, `ManualGuestLogin(string)`, `LoginWithGooglePlayGamesServices`, `LoginWithGoogle`, `LoginWithApple`, `TrySetDisplayName(name, Action<string>, Action<PlayFabErrorCode>)`, `LogError(string,string)`, `LogCheat(string, Dictionary)`.
- `OnPlayFabLoginSuccess`: 버전체크(크래시 원인) → `Data.CheckKick` → `Store.CheckDevice` → `AfterLogin`(UpdateUserData DeviceId → `_store.LoadData()` → GetAccountInfo로 닉네임 → `DbLoadChecker.I.Check()`).
- **주의**: `TrySetDisplayName`의 실패 콜백 시그니처가 `Action<PlayFabErrorCode>` → 호출처(`PlayFabStore.SetAutoNickname`, `UI_Nickname.cs`)가 이 타입에 의존.

### 1.2 PlayFabStore.cs (저장/시간/디바이스 - 가장 큰 파일)
공개 API 표면:
- `LastUpdatedTime` (DateTime, 필드), `OfflineTime` (TimeSpan, get)
- `CheckTime()`, `DoWithTime(Action<DateTime>)` ← **최다 사용**
- `SetLog(string)`, `SaveLog()`
- `CheckDevice(Action whenPass, Action whenConflict)`
- `SetBackgroundTime(TimeSpan)`
- `ForceSave(Action toDo=null, bool exitGame=false, bool needRestart=false)` ← **최다 사용**
- `SaveAndExit()`, `SaveAndRestart()`, `ResetUserData()`
- `LoadData()`
- `SetAutoNickname()`
- `Exit()`
- 내부 `_saves` 딕셔너리 키: Time, Equipments, Records, Currency, Info, Ability, Relic. 이 키 구조가 로컬 JSON 스키마의 기준이 됨.

`LoadData()`는 **361~532줄**. 데이터 변환 파이프라인은 **375~465줄**이 **전체 게임 상태 복원의 핵심**이며, 데이터 소스만 바뀔 뿐 그대로 재사용해야 함.

### 1.3 PlayFabLeaderboard.cs (랭킹)
- `DontRank` (bool 필드)
- `StartUpdating()`, `UpdateStage()`, `UpdateTraining()`, `IsStageUpdated()`, `IsTrainingUpdated()`
- `GetStageLeaderboard(int)`, `GetTrainingLeaderboard(int)`, `GetMyStageLeaderboard()`, `GetMyTrainingLeaderboard(Action<int>=null)`, `GetMyTrainingRanking(Action<int>)`
- `GetRankingInfo(string id, Action<int,int,int,int,int>)`, `GetRankingOf(string id, Action<PlayerLeaderboardEntry>)` ← **PlayFab 모델 노출**
- `CheckIdIsValid(string, Action, Action)`, `UpdateCheat(bool, string)`, `PostCheatLog`, `Encode(BigInteger)` (Encode는 private — 외부 호출처 없음)
- public 중첩 클래스 `RankingInfo` (level/title/profile/stage/training) — 직렬화용, 유지.
- 콜백으로 `OnStageLoaded`/`OnMyStageLoaded` 등 `UI_Ranking`에 `List<PlayerLeaderboardEntry>` 전달.

### 1.4 PlayFabTitleData.cs (서버 설정값)
- `CanShowCoupon` (bool)
- `CheckCoupon(string, Action)`, `CheckMail(Action<MailInfo,bool>, bool)`, `CheckKick(Action<bool>)`, `CheckDontRank()`, `CheckAppStoreReview()`
- `CheckKick`은 `#if UNITY_EDITOR`에서 즉시 `afterResponse(true)` → 에디터에서는 이미 통과.
- Mail/Coupon 시스템이 TitleData(서버 운영 데이터)에 의존 → 로컬에서는 빈 구현으로 대체.

### 1.5 PlayFabFunctions.cs (Cloud Script)
- `MonoBehaviour`, `Start()`에서 `GetQuestReward()` 호출. 실제 게임 로직에서 결과를 안 씀(디버그 로그만). 씬에 컴포넌트로 붙어있지 않으면 dead code. 가장 먼저/안전하게 제거 가능.

---

## 2. 의존성 그래프

### 2.1 `DoWithTime` (시간 조회) 사용처 — 13곳
`PlayFabStore.CheckTime/TrySave/LoadData`(내부), `PlayFabTitleData.CheckCoupon`, `EventAttendController:20`, `AttendController:42`, `QuestController:25`, `CurrencyController:995,1080`, `DbUserMail:57`, `DbUserCurrency:357,367`, `UI_AdBuff_Item:101`, `UI_Book:94`, `UI_Inventory:178`, `UI_OncePackage:63`, `UI_Shop:113`, `BackgroundManager:49,73`, `UI_ForceQuit:85`.

### 2.2 `ForceSave` (저장) 사용처 — 활성 호출 약 14곳
`AbilityController:98`, `CurrencyController:111,131,374,409,430`, `LevelController:706`, `DbUserPet:78`, `DbUserPass:49`, `IAPManager:136`, `UI_ResetLevelPoint:67`, `UI_Bookshelf_Item:170`, `UI_AccountSetting:130`, `UI_Setting:209`, `BackgroundManager:68`.

### 2.3 `SaveAndExit / SaveAndRestart / Exit / ResetUserData`
`UI_Quit:27`(SaveAndExit), `UI_ForceQuit:71`(SaveAndRestart)/:75(Exit), `UI_Cheat:212`(ResetUserData).

### 2.4 `Leaderboard.*`
`FieldScene:118`(StartUpdating), `UI_MainTop:126`(UpdateStage), `LevelController:426`(UpdateTraining), `UI_Ranking`(다수), `UI_Ranking_Item:123`(GetRankingInfo), `UI_Friend_Item:121,127`(GetRankingInfo/GetRankingOf), `UI_Friend:156`(CheckIdIsValid), `UI_Dungeon_Item_Training:23`(GetMyTrainingRanking), `CurrencyController:1031,1096,1097`(UpdateCheat), `PlayFabTitleData:204`(DontRank).

### 2.5 `Data.*` (TitleData)
`UI_Setting:118,166`(CanShowCoupon/CheckCoupon), `DbUserCurrency:361,371`(CheckMail), `PlayFabStore:256`(CheckKick), `PlayFabManager:135,152,174`(CheckKick/CheckDontRank/CheckAppStoreReview).

### 2.6 로그인/식별
`LoginScene:225,236,239,281,329,340`, `UI_Login:131`, `PlayFabManager`(전부). `SettingController.UID`(=PlayFabId), `.Nickname`, `.Id`.

### 2.7 직접 PlayFab 타입 노출 파일 (핵심 5개 외)
SDK 삭제(Step 9) 전 아래 파일 전부 `using PlayFab*` 제거 필요:
- `LoginScene.cs` — `using PlayFab.ClientModels;` + `GetPlayerCombinedInfoRequestParams` 필드.
- `FieldScene.cs` — `using PlayFab.ClientModels;` (실사용 없음, 제거만 하면 됨).
- `UI_Ranking.cs` — `using PlayFab.ClientModels;` + `PlayerLeaderboardEntry` 직접 사용.
- `UI_Friend_Item.cs` — `GetRankingOf` 콜백 람다에서 타입 암묵 의존. (`UI_Ranking_Item.cs`는 직접 참조 없어 수정 불필요)
- `IAPManager.cs` — `using PlayFab;` / `using PlayFab.ClientModels;` 존재. ForceSave만 호출하나 SDK 삭제 시 컴파일 오류 → using 제거 필요.
- `UI_Nickname.cs` — `using PlayFab;` + `PlayFabErrorCode` 사용 (TrySetDisplayName 실패콜백).
- `UI_Summon_Item.cs` — `using PlayFab.ClientModels;` (미사용 using, 제거만 하면 됨).
- `UI_Dungeon_Item.cs` — `using PlayFab.ClientModels;` (미사용 using, 제거만 하면 됨).
- `DbSkill.cs` — `using PlayFab.ProfilesModels;` (미사용 using, 제거만 하면 됨).

---

## 3. LocalSaveManager 및 호환 레이어 설계

### 3.1 전략: API 호환 유지 + 내부 교체

`PlayFabManager/Store/Leaderboard/Data/Functions` 5개 파일의 **클래스명·메서드 시그니처를 그대로 둔 채** 내부 PlayFab 호출만 로컬 구현으로 바꾼다. 신규 `LocalSaveManager.cs`는 순수 파일 I/O + JSON 유틸만 담당하고, `PlayFabStore` 등이 이를 호출한다.

- 장점: 51개 호출처 거의 무수정. 컴파일 오류 최소. 점진적/되돌리기 쉬움.
- 단점: 클래스명이 "PlayFab*"로 남음 (Phase 2에서 리네임).

### 3.2 신규 `LocalSaveManager.cs` 인터페이스

```csharp
namespace ThirdParty
{
    public static class LocalSaveManager
    {
        // 저장 슬롯 키 = 기존 _saves 키와 동일
        // (Time, Equipments, Records, Currency, Info, Ability, Relic)
        // ZLog는 _saves 외 별도 키로 LoadData에서 처리됨

        public static string SavePath { get; } // Application.persistentDataPath + "/save.json"

        // 전체 dict 저장 (atomic write: temp 파일 → File.Replace)
        public static void SaveAll(Dictionary<string,string> data);

        // 부분 키 갱신
        public static void SaveKeys(Dictionary<string,string> data);

        // 전체 로드. 없으면 null/빈 dict
        public static Dictionary<string,string> LoadAll();

        public static bool HasSave();           // 신규유저 판별
        public static void RemoveKeys(IEnumerable<string> keys); // ResetUserData 대체
        public static void DeleteAll();

        // 시간: DateTime.UtcNow.AddHours(9) — 기존 KST 규칙 유지
        public static DateTime Now();

        // 닉네임/식별: PlayerPrefs 기반
        public static string GetOrCreateNickname();
        public static void SetNickname(string name);
    }
}
```

설계 근거:
- 기존 `_saves` 딕셔너리(7키) 구조를 그대로 유지하면 `LoadData()`의 거대한 변환 로직을 1줄(`result.Data` 소스만 교체)로 살릴 수 있다.
- 저장은 반드시 **원자적 쓰기**(temp 후 `File.Replace`)로 — 모바일 강제종료 시 세이브 손상 방지.
- 시간: 기존 코드가 전부 `AddHours(9)` KST 기준이므로 `Now()`도 동일 규칙 적용.

### 3.3 신규 `LeaderboardEntry.cs` (PlayerLeaderboardEntry 치환 타입)

```csharp
public class LeaderboardEntry {
    public int Position;
    public int StatValue;
    public string DisplayName;
    public string PlayFabId; // 필드명 유지 시 UI_Ranking_Item 무수정 (의미상 userId)
}
```

이후 `UI_Ranking.cs` / `UI_Friend_Item.cs`의 `PlayerLeaderboardEntry` → `LeaderboardEntry` 치환 + `using PlayFab.ClientModels;` 제거. (`UI_Ranking_Item.cs`는 직접 참조 없어 수정 불필요)

### 3.4 UserInfo 구조

`UserInfo`는 이미 완전한 직렬화 모델이며 구조 변경 불필요. 단 한 곳:
- `UserInfo.cs:672` `PlayFabManager.Store.SaveLog();` → 로컬 저장 트리거로 교체.

---

## 4. 컴파일 오류가 안 나는 작업 순서

> 원칙: ① 새 타입/매니저 추가 → ② 5개 파일 내부 교체(외부 시그니처 유지) → ③ PlayFab 모델 노출 2건 치환 → ④ 컴파일 그린 확인 → ⑤ PlayFab 패키지/import 제거. **각 단계마다 컴파일 확인**.

### Step 0 — 안전망
브랜치 생성, `_dev/history.md` 기록. PlayFab SDK는 `Assets/Plugins/PlayFabSDK`에 있음(마지막에 삭제).

### Step 1 — 신규 파일 추가 (영향 0)
- `LocalSaveManager.cs` 작성
- `LeaderboardEntry.cs` 작성
- 기존 컴파일에 영향 없음 → 그린 유지.

### Step 2 — PlayFabFunctions.cs 처리 (가장 안전)
씬/프리팹에서 컴포넌트 참조 여부 확인 후 파일 삭제 또는 `Start()` 본문 비우기.

### Step 3 — PlayFabStore.cs 내부 교체
- `DoWithTime`: PlayFab `GetTime` → `toDo(LocalSaveManager.Now())` 즉시 콜백.
- `ForceSave/TrySave`: `_saves` 채우는 로직 유지 → `UpdateUserData` 대신 `LocalSaveManager.SaveAll(_saves)`.
- `LoadData`: `var data = LocalSaveManager.LoadAll();` 후 `data.ContainsKey("Equipments")` 분기. **375~465줄 변환 로직 전부 보존** (접근자만 `result.Data[k].Value` → `data[k]`로 변경. LoadData() 전체 범위는 361~532줄).
- `CheckDevice`: `whenPass()` 즉시 호출.
- `ResetUserData` → `LocalSaveManager.RemoveKeys(...)`.
- `SetAutoNickname` → `LocalSaveManager.GetOrCreateNickname()` 기반.
- 시그니처 전부 유지 → 호출처 무수정.

### Step 4 — PlayFabTitleData.cs 내부 교체
- `CheckKick(cb)` → `cb(true)`.
- `CheckDontRank()` / `CheckAppStoreReview()` → no-op.
- `CheckCoupon` → "사용 불가" 처리 또는 로컬 쿠폰 테이블.
- `CheckMail` → 빈 메일 처리.

### Step 5 — PlayFabLeaderboard.cs 내부 교체
- 모든 통신 메서드를 로컬 스텁으로.
- `Get*Leaderboard` 콜백은 빈 `List<LeaderboardEntry>` 반환.
- `GetRankingOf` 콜백 시그니처 `Action<PlayerLeaderboardEntry>` → `Action<LeaderboardEntry>`로 변경 (UI_Friend_Item 동반 수정).
- `UpdateStage/UpdateTraining/UpdateCheat` no-op. `DontRank` 필드 유지.

### Step 6 — PlayFabManager.cs 내부 교체
- `ManualGuestLogin/LoginWith*`: PlayFab 로그인 제거 → 로컬 식별자 세팅 후 직접 AfterLogin 경로 호출.
- `SettingController.UID = SystemInfo.deviceUniqueIdentifier` 등으로 세팅.
- 버전체크 블록 삭제(크래시 해소).
- **DbLoadChecker 카운터 보존**: PlayFab 플로우에서 `Check()`가 호출되던 횟수를 로컬 플로우에서 정확히 동일하게 호출해야 로딩 게이지가 완료됨.
- `TrySetDisplayName` → `TrySetDisplayName(string, Action<string> onSuccess)`로 단순화 + `UI_Nickname` 동반 수정.
- `LogError/LogCheat` → 로컬 로그 또는 no-op.

### Step 7 — PlayFab 모델 노출 치환 + 미사용 using 전체 정리
- `LoginScene.cs`: `[SerializeField] GetPlayerCombinedInfoRequestParams playfabParams;` 및 `PlayFabManager.PlayfabParams` 제거. `using PlayFab.ClientModels;` 제거.
- `FieldScene.cs`: `using PlayFab.ClientModels;` 제거.
- `UI_Ranking.cs` / `UI_Friend_Item.cs`: `PlayerLeaderboardEntry` → `LeaderboardEntry`, `using PlayFab...` 제거. (`UI_Ranking_Item.cs`는 수정 불필요)
- `UI_Nickname.cs`: `TrySetDisplayName` 실패 콜백에서 `PlayFabErrorCode` 제거 (Step 6과 연동).
- `IAPManager.cs`: `using PlayFab;` / `using PlayFab.ClientModels;` 제거.
- `UI_Summon_Item.cs`, `UI_Dungeon_Item.cs`, `DbSkill.cs`: 미사용 `using PlayFab*` 제거.

### Step 8 — 컴파일 그린 확인 (PlayFab SDK 아직 존재)
이 시점에 모든 게임 코드가 PlayFab 타입을 참조하지 않음. SDK는 남아있어도 그린이어야 함.

### Step 9 — PlayFab SDK 제거
`Assets/Plugins/PlayFabSDK` 폴더 + `.meta` 삭제. 잔존 `using PlayFab*` grep으로 0 확인 후 재컴파일.

---

## 5. 파일별 변경 명세 요약

| 파일 | 변경 내용 | 시그니처 영향 |
|---|---|---|
| `LocalSaveManager.cs` (신규) | 파일 I/O, JSON, 시간, 닉네임 | - |
| `LeaderboardEntry.cs` (신규) | PlayerLeaderboardEntry 치환 POCO | - |
| `PlayFabFunctions.cs` | 삭제 또는 Start 비우기 | 없음 |
| `PlayFabStore.cs` | DoWithTime/ForceSave/LoadData/CheckDevice/ResetUserData/SetAutoNickname 내부 교체 | **유지** |
| `PlayFabTitleData.cs` | CheckKick→true, 나머지 no-op/로컬 | **유지** |
| `PlayFabLeaderboard.cs` | 전부 로컬 스텁, GetRankingOf 콜백 타입 변경 | GetRankingOf만 변경 |
| `PlayFabManager.cs` | 로그인 로컬화, 버전체크 삭제, DbLoadChecker 카운트 보존, TrySetDisplayName 단순화 | TrySetDisplayName 변경 |
| `LoginScene.cs` | playfabParams/using 제거 | - |
| `FieldScene.cs` | using PlayFab 제거 | - |
| `UI_Ranking.cs` / `UI_Friend_Item.cs` | LeaderboardEntry로 치환, using 제거 | - |
| `UI_Ranking_Item.cs` | 수정 불필요 (직접 PlayFab 참조 없음) | - |
| `UI_Nickname.cs` | TrySetDisplayName 실패콜백 시그니처 동반 수정, using PlayFab 제거 | - |
| `UserInfo.cs:672` | SaveLog 호출 로컬화 | - |
| `IAPManager.cs` | using PlayFab 제거 (ForceSave 시그니처 유지로 로직 무수정) | - |
| `UI_Summon_Item.cs`, `UI_Dungeon_Item.cs`, `DbSkill.cs` | 미사용 using PlayFab* 제거 | - |

호출처 51개 중 활성 코드 대부분(ForceSave/DoWithTime/SaveAndExit 등)은 시그니처 유지 덕에 무수정. 실제 손대는 호출처는 랭킹 3개 UI + 닉네임 1개 + 씬 2개 ≈ **6개**뿐.

---

## 6. 리스크 및 주의사항

1. **DbLoadChecker 카운터(119)**: 로딩 완료 게이트. 119 = `DbModel.cs:21` / `DbUserModel.cs:49`의 반복 호출(117회) + `PlayFabManager:169`(GetAccountInfo 콜백) + `PlayFabStore:528`(LoadData 끝). 로컬 전환이 DbModel/DbUserModel 경로를 건드리지 않으면 117회는 자동 보존됨. 반드시 놓치지 말아야 할 건 로그인 플로우의 **나머지 2회(169줄, 528줄)**.
2. **단일 어셈블리**: 컴파일 오류 1개 = 전체 빌드 정지. 반드시 Step 단위로 컴파일 확인하며 진행.
3. **세이브 원자성**: `File.Replace`(temp→실파일) 패턴 필수. `_AutoSaveRoutine`(5초마다)과 `OnApplicationPause`가 동시에 쓰지 않도록 단일 진입점 고려.
4. **시간 신뢰성**: 로컬 `DateTime`은 유저가 기기 시계를 조작 가능 → 출석/일일보상 악용 가능. Phase 1에서는 수용, Phase 2에서 정책 결정.
5. **+9시간(KST) 규칙**: 기존 전 코드가 `AddHours(9)` 가정. `LocalSaveManager.Now()`도 반드시 동일 규칙. 누락 시 일일 리셋 경계가 9시간 어긋남.
6. **신규 vs 기존 유저 분기**: `LoadData`의 `ContainsKey("Equipments")` → `LocalSaveManager.HasSave()`. 첫 실행 시 신규 생성 경로가 정상 동작하는지 반드시 플레이 테스트.
7. **OfflineReward**: `OfflineTime = now - prevTime`. 로컬 시계 조작 시 거대 오프라인 보상 가능 → 상한 클램프 고려.
8. **PlayerLeaderboardEntry 누락 멤버**: 치환 POCO에 UI가 쓰는 멤버(`Position`, `StatValue`, `DisplayName`, `PlayFabId`) 빠짐없이. 빈 리스트면 `rankings[0]` 인덱스 예외 → 스텁이 최소 1엔트리 반환하거나 UI에서 null/empty 가드 추가.
9. **UI_NetworkError / UI_ForceQuit**: 네트워크 개념 소멸로 이 UI들이 다신 안 뜸. 코드 경로 제거 시 컴파일 의존성(enum QuitType 등) 확인.
10. **인스펙터 직렬화 손실**: `LoginScene`의 `[SerializeField] playfabParams` 제거 시 씬 dirty 발생 → 커밋 시 .unity 변경 포함됨.

---

## 7. 권장 검증 순서 (구현 후)

1. 에디터에서 신규 유저로 첫 실행 → 로딩 100% → 필드 진입 → 세이브 파일 생성 확인.
2. 앱 재시작 → 기존 세이브 로드, 재화/장비/스테이지 복원 확인.
3. 강제저장 트리거(설정 저장 버튼, 장비합성, 구매) 동작 확인.
4. 백그라운드 전환/복귀(OfflineReward) 확인.
5. 랭킹/친구 UI 열어 예외 없이 표시되는지(빈 데이터) 확인.
6. PlayFab SDK 삭제 후 최종 컴파일 그린 + 위 1~5 재확인.

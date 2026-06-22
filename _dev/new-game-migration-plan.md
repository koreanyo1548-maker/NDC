# 새 게임 전환 마이그레이션 계획

> 분석 기준일: 2026-06-14  
> 최종 업데이트: 2026-06-22  
> 현재 상태: Mushroom Hero → 신규 게임 (스탠드얼론 전환) / Player Spine 교체 완료, Monster 교체 진행 중

---

## 1. 현재 외부 의존성 전체 현황

### 1-A. PlayFab (제거 대상)

| 기능 | 현재 구현 | 대체 방안 |
|------|-----------|-----------|
| 로그인 | GPGS(Android) / AppleAuth(iOS) → PlayFab | 디바이스 UUID 자동 게스트 로그인 |
| 세이브/로드 | PlayFab UserData (서버) | 로컬 JSON 파일 저장 |
| 서버 시간 | PlayFab GetTime API | `DateTime.Now` (추후 NTP 검토) |
| 랭킹 | PlayFab Leaderboard (Stage, Training) | 스텁 처리 → 추후 Firebase |
| 쿠폰 | PlayFab TitleData | 제거 (추후 Firebase Remote Config) |
| 메일 | PlayFab TitleData + UserData | 제거 (추후 설계) |
| 버전 체크 | PlayFab TitleData `ClientVer` | 하드코딩 or Firebase Remote Config |
| 점검/차단(Kick) | PlayFab TitleData `KickUser` | 제거 (추후 Firebase Remote Config) |
| 닉네임 자동 생성 | PlayFab Leaderboard `Nickname` 통계 활용 | 로컬 랜덤 생성 |
| 멀티기기 방지 | PlayFab UserData `DeviceId` 비교 | 제거 (로컬 전환 시 불필요) |
| 에러/치트 로깅 | PlayFab WritePlayerEvent | 제거 |
| 오토세이브 | 5초마다 PlayFab UpdateUserData | 로컬 파일 저장 |
| 백그라운드 시간 | PlayFab 서버 시간 기준 | 로컬 시간 기준 |
| AppStore 리뷰 모드 | PlayFab TitleData `AppStoreReview` | 제거 |

**영향받는 파일 수: 51개**

ThirdParty (5개):
- `PlayFabManager.cs` - 로그인/초기화 허브
- `PlayFabStore.cs` - 세이브/로드/시간/닉네임
- `PlayFabLeaderboard.cs` - 랭킹
- `PlayFabTitleData.cs` - 서버 설정값 (쿠폰/메일/점검)
- `PlayFabFunctions.cs` - Cloud Script (GetQuestReward 등)

장면 (2개):
- `LoginScene.cs` - GPGS/Apple 인증 흐름 전체
- `FieldScene.cs` - 로그인 후 닉네임/오프라인 리워드 처리

컨트롤러/데이터 (약 20개):
- `BackgroundManager.cs` - 백그라운드 복귀 시 서버 시간 체크
- `CurrencyController.cs`, `LevelController.cs`, `QuestController.cs` 등 - ForceSave 호출
- `DbUserMail.cs`, `DbUserCurrency.cs`, `DbUserPass.cs` 등 - PlayFab 데이터 처리

UI (약 15개):
- `UI_Ranking.cs` - 랭킹 팝업 전체
- `UI_Setting.cs`, `UI_AccountSetting.cs` - 설정/계정 관리
- `UI_ForceQuit.cs`, `UI_Quit.cs` - 강제 종료 (서버 에러 시)
- `UI_Cheat.cs`, `UI_Shop.cs`, `UI_Friend.cs` 등

---

### 1-B. 광고 SDK - Google AdMob (계정 교체 대상)

현재 Publisher ID: `ca-app-pub-9236708445713349`

| 광고 슬롯 | Android ID | iOS ID |
|-----------|-----------|--------|
| 소환 | 9077983126 | 5441391378 |
| 던전 | 3036714184 | 7652621745 |
| 버프 | 5745372658 | 8802051883 |
| 오프라인 리워드 | 9301474285 | 4951099491 |
| 책장 | 2170355079 | 5162189771 |
| 상점 | 5918028394 | 8012377101 |

- `AdManager.cs` - 리워드 광고 6개 슬롯 운영
- Unity Package: `GoogleMobileAds` (Assets/GoogleMobileAds)
- **해야 할 일**: 새 AdMob 계정의 App ID/Ad Unit ID로 교체

---

### 1-C. 결제 SDK - Unity IAP (설정 교체 대상)

- Unity Package: `com.unity.purchasing 4.12.2` (manifest.json)
- Google Play Store 상품 ID 사용 중
- `IAPManager.cs` - 구매 처리, 패스 구매 후 PlayFab ForceSave 호출
- **해야 할 일**:
  - 새 Google Play 개발자 계정에서 상품 등록
  - `IAPManager.cs`에서 `PlayFabManager.Store.ForceSave()` 호출 → 로컬 저장으로 교체
  - 현재 AppsFlyer 이벤트 코드는 `#if APPSFLYER_ENABLE` 조건으로 비활성화 상태 (그대로 유지 가능)

---

### 1-D. Firebase Analytics (설정 교체 대상)

현재 설치: `firebase-analytics:22.1.2` + `firebase-common:21.0.0` 만 설치됨  
(Auth, Firestore, Realtime DB 없음 - 순수 Analytics 전용)

- `Assets/Firebase/` - Firebase Unity SDK
- `Assets/google-services.json` - Firebase 프로젝트 설정 파일
- **해야 할 일**:
  - 새 Firebase 프로젝트 생성 후 `google-services.json` 교체
  - 새 Bundle ID / App ID 반영

---

### 1-E. Google Play Games Services (제거 대상, Android 로그인 전용)

- `Assets/GooglePlayGames/` - GPGS SDK
- `LoginScene.cs` → `onLogin_GPGS()` 에서 인증 후 PlayFab으로 연결
- PlayFab 제거 시 함께 제거 예정

---

### 1-F. Apple Sign In (제거 대상, iOS 로그인 전용)

- `Assets/AppleAuth/` - AppleAuth SDK
- `LoginScene.cs` → `onLogin_Apple()` 에서 인증 후 PlayFab으로 연결
- PlayFab 제거 시 함께 제거 예정

---

### 1-G. Unity Analytics (유지 or 교체)

- `com.unity.services.analytics 6.0.3` (manifest.json)
- 코드에서 직접 사용 흔적 없음 (자동 수집 only)
- **해야 할 일**: 새 Unity 프로젝트 ID로 연결 (UnityConnectSettings.asset 수정)

---

### 1-H. HiveManager (제거 대상)

- `Assets/Scripts/ThirdParty/HiveManager.cs` 존재
- 모든 호출부 주석 처리 상태 (`// HiveManager.I.SetLocalPushAndQuit()` 등)
- 파일만 남아있는 상태 → 안전하게 삭제 가능

---

## 2. 크래시 원인 (현재)

```
PlayFabManager.cs:124 - Debug.LogError("최신 버전으로 업데이트 해주세요. 최신: 1.03.044")
```

PlayFab 서버의 `ClientVer`(1.03.044) > 로컬 앱 버전 → 서버가 강제 차단 중.  
PlayFab을 제거하면 자동 해결됨.

---

## 3. 데이터 저장 구조 (현재 vs 전환 후)

### 현재 (PlayFab 서버)
```
서버 UserData 키:
- Equipments  (무기/악세/스킬/목걸이)
- Records     (퀘스트/타이틀/펫/어빌리티/유물)
- Currency    (재화/패스/광고버프/책장/열정)
- Info        (레벨/장비착용/스텟/시즌패스/출석/메인퀘스트/친구)
- Ability
- Relic
- Time        (마지막 저장 시각)
- DeviceId    (기기 중복 체크)
- ZLog        (플레이 로그)
- Mail        (유저 개별 메일)
- RankingInfo (랭킹 표시용 스냅샷)
```

### 전환 후 (로컬 파일)
```
Application.persistentDataPath/save.json 에 동일 구조 JSON 저장
- 기존 UserInfo 클래스 구조 그대로 활용 가능
- 서버 시간 → DateTime.UtcNow
- 오토세이브 5초 → 로컬 파일 write
```

---

## 4. 작업 우선순위 및 단계별 계획

### Phase 1: PlayFab 제거 & 스탠드얼론 전환 ✅ 완료 (2026-06-14)

> **목표**: 서버 없이 로컬에서 완전히 실행되는 상태

**1-1. 로그인 플로우 교체** (`LoginScene.cs`)
- [x] `PlayFabManager.ManualGuestLogin(deviceId)` → 내부적으로 바로 AfterLogin 호출하도록 교체
- [x] `LoginScene.cs`에서 PlayFab 초기화 코드 제거
- [x] GPGS / AppleAuth 코드는 `#if` 블록 안에 유지 (SDK 유지 결정 — 출시 시 재활용 가능)

**1-2. 세이브/로드 시스템 교체** (`PlayFabStore.cs` 대체)
- [x] `LocalSaveManager.cs` 신규 작성 (`Application.persistentDataPath/save.json`, atomic write)
- [x] `LeaderboardEntry.cs` 신규 작성 (PlayFab 타입 대체)
- [x] `PlayFabStore.cs` 내부 구현 전면 교체 (로컬 파일 저장)
- [x] `PlayFabManager.cs` 재작성 (디바이스 UUID 자동 로그인)

**1-3. 랭킹 스텁 처리** (`PlayFabLeaderboard.cs`)
- [x] `PlayFabLeaderboard` 스텁으로 교체 (public 시그니처 유지 → Firebase 재활용 가능)
- [x] `UI_Ranking.cs` → `LeaderboardEntry` 타입으로 교체

**1-4. TitleData 기능 제거** (`PlayFabTitleData.cs`)
- [x] 모든 메서드 no-op 또는 즉시 콜백 반환으로 교체

**1-5. using 정리 및 기타 파일 수정**
- [x] `PlayFab.*` using 전체 제거 (IAPManager, FieldScene, UI_Summon_Item, UI_Dungeon_Item, DbSkill, UI_Nickname 등)
- [x] `PlayFabFunctions.cs` 삭제
- [x] `PlayFab SDK` (`Assets/Plugins/PlayFabSDK/`) 삭제

---

### Phase 2: SDK 물리적 제거 & 프로젝트 신원 교체

**2-1. SDK 폴더 삭제**
- [x] `Assets/Plugins/PlayFabSDK/` 삭제
- [ ] `Assets/GooglePlayGames/` — **유지 결정** (출시 시 재활용 가능, `#if`로 격리됨)
- [ ] `Assets/AppleAuth/` — **유지 결정** (동일 이유)
- [x] `Assets/Scripts/ThirdParty/HiveManager.cs` 삭제
- [x] `Assets/Scripts/ThirdParty/PlayFabFunctions.cs` 삭제

**2-2. AdMob 계정 교체** (`AdManager.cs`)
- [ ] 새 AdMob 계정에서 앱 등록 및 광고 유닛 ID 발급
- [ ] `GetAdId()` 함수의 모든 ID 교체
- [ ] `AndroidManifest.xml`의 AdMob App ID 교체

**2-3. Firebase 프로젝트 교체**
- [ ] 신규 Firebase 프로젝트 생성 (새 앱 이름/Bundle ID 기준)
- [ ] `Assets/google-services.json` 교체
- [ ] `Assets/GoogleService-Info.plist` 교체 (iOS)

**2-4. Unity 프로젝트 ID 교체**
- [ ] `ProjectSettings/UnityConnectSettings.asset` - Unity Project ID 교체
- [ ] Unity 대시보드에서 새 프로젝트 연결

**2-5. 앱 기본 정보 교체** (`ProjectSettings/ProjectSettings.asset`)
- [x] `companyName`: "Ndolphin Connect" → "NDC" ✅
- [x] `productName`: "Mushroom Hero" → "Webtoon Hero" ✅
- [ ] `bundleIdentifier` (Android/iOS) 교체 — 현재 `com.NdolphinConnect.MushroomHero` / `mush.room.hero`
- [ ] 버전 설정 (1.00.000부터 시작)

**2-6. IAP 상품 ID 교체**
- [ ] 새 Google Play 개발자 계정에서 앱 등록
- [ ] 상품 ID 등록 후 `IAPManager.cs`의 product ID 교체
- [ ] App Store Connect 앱 등록 및 상품 ID 교체

---

### Phase 2-B: 비주얼 에셋 교체 (신규 - Phase 1 이후 추가)

> 상세 분석: `_dev/asset-migration-analysis.md`

**2B-1. Player → Shinabro MiniFantasyCharacters** ✅ 코드 완료

- [x] `Assets/Layer Lab/` 삭제 (LayerLab CharacterMaker 완전 제거)
- [x] `Assets/Downloads/Shinabro/MiniFantasyCharacters/` 에셋 추가
- [x] `Player.cs` 수정 — Spine 애니메이션명, 무기별 분기(`GetAttackAnimation`, `GetSkillAnimation`)
- [x] `SimpleSpineSkinAssigner.cs` 수정 (public AssignSkins, Start 추가)
- [x] `Character_Controller.controller` 수정 (AttackSpeed 파라미터, 전체 상태 추가)
- [ ] Player.prefab 에디터 작업 확인 (Shinabro Variant 자식 배치, scale 10,10,10)

**2B-2. Monster → 2D SD Monster Pack** ⚠️ 진행 중

- [x] 에셋 분석 완료 (15종 파악, 애니메이션명 불일치 파악)
- [x] **Monster.cs 코드 수정** — `"Attack"` → `"attack"`, `"Walk"` → `"walk"`, `"Die"` → `"die"`
- [x] 기존 몬스터 ↔ SD Pack 매핑표 확정 (`_dev/monster-mapping.md`)
- [x] walk 없는 몬스터(Bat, Beholder) 처리 — idle 폴백 로직
- [x] 보스 `Attack2` 처리 완료 (단일 보스는 attack 재사용, 이중 보스는 smash/bow 분기)
- [ ] 프리팹 에디터 작업 (매핑 확정 후 1종씩 교체)
- [ ] **[버그] Slime2 풀 재활용 시 die 애니메이션으로 시작** — 임시 조치(`animator.Play("idle")` 추가) 완료, 검증 필요
  - 다음 세션: Init/OnEnable/Clear 시점에 Debug.Log 심고 플레이 확인

---

### Phase 3: 새 Git 레포 생성

- [ ] 현재 `.git` 폴더 삭제
- [ ] `git init`
- [ ] `.gitignore` 재확인 (Library, Temp, build 제외)
- [ ] 초기 커밋 (Phase 1+2 완료 후)
- [ ] GitHub/GitLab 새 레포 생성 및 remote 연결

---

### Phase 4: Firebase 인증 & 랭킹 (선택적 / 추후)

> 현실성 검토 후 진행 여부 결정

**4-1. Firebase Authentication (익명 로그인)**
- Firebase Auth SDK 추가
- 디바이스 UUID 기반 익명 계정 생성
- (선택) Google/Apple 계정 연동

**4-2. Firebase Firestore - 세이브 데이터**
- 로컬 JSON → Firestore 문서 저장
- 온라인 시 동기화, 오프라인 시 로컬 캐시 활용

**4-3. Firebase Leaderboard**
- Firestore 기반 랭킹 or Firebase Extensions 활용
- `UI_Ranking.cs` 복원 및 Firebase 데이터 연동

---

## 5. 예상 작업 복잡도

| Phase | 예상 난이도 | 예상 소요 시간 |
|-------|------------|--------------|
| Phase 1 (PlayFab 제거) | 높음 - 51개 파일 수정 | 3~5일 |
| Phase 2 (SDK 교체 + 설정) | 낮음 - 설정 값 교체 위주 | 1~2일 |
| Phase 3 (새 Git) | 매우 낮음 | 1시간 |
| Phase 4 (Firebase 연동) | 매우 높음 - 설계 필요 | 1~2주+ |

---

## 6. 패키지 및 설정 정리 현황

### 즉시 삭제 가능 (게임 실행에 영향 없음)

| 항목 | 위치 | 이유 |
|------|------|------|
| ~~HiveManager.cs~~ | ~~`Assets/Scripts/ThirdParty/HiveManager.cs`~~ | ~~모든 호출부 이미 주석 처리됨. 파일만 존재~~ ✅ 삭제 완료 |

### 코드 수정 후 삭제 가능 (삭제 자체는 간단)

| 항목 | 위치 | 조건 |
|------|------|------|
| PlayFab SDK | `Assets/Plugins/PlayFabSDK/` | Phase 1 코드 교체 후 |
| Google Play Games SDK | `Assets/GooglePlayGames/` | LoginScene.cs 수정 후 |
| Apple Sign In SDK | `Assets/AppleAuth/` | LoginScene.cs 수정 후 |
| PlayFabFunctions.cs | `Assets/Scripts/ThirdParty/` | PlayFab 제거와 함께 |

### 삭제가 아니라 교체 대상 (설정 파일)

| 항목 | 이유 |
|------|------|
| `Assets/google-services.json` | 새 Firebase 프로젝트로 교체 |
| `Assets/GoogleService-Info.plist` | 동일 |
| `AndroidManifest.xml`의 AdMob App ID | 새 계정으로 교체 |
| `ProjectSettings/UnityConnectSettings.asset` | 새 Unity 프로젝트 ID로 교체 |
| `ProjectSettings/ProjectSettings.asset` | 번들 ID, 앱 이름 등 교체 |

---

## 7. 현재 당장 실행할 수 있는 것 (Phase 1 착수 순서 제안)

1. **`PlayFabManager.cs`** - `OnPlayFabLoginSuccess()` 에서 버전 체크를 없애고, 바로 로컬 모드로 AfterLogin을 호출하도록 수정 → **크래시 즉시 해결**
2. **`LocalSaveManager.cs`** 신규 작성 → 로컬 파일 저장 시스템 구축
3. **`PlayFabStore.cs`** → 로컬 저장으로 내부 교체
4. 컴파일 오류가 나는 파일부터 순서대로 정리
5. 랭킹 스텁 처리 → 게임 전체 실행 확인

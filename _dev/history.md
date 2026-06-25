# 프로젝트 의사결정 기록

---

## 2026-06-14

### 결정: Mushroom Hero → 스탠드얼론 신규 게임 전환

**배경**  
Mushroom Hero 코드베이스를 재활용해 신규 게임을 제작하기로 결정.  
현재 PlayFab 서버의 버전 체크(`ClientVer: 1.03.044`)가 앱을 강제 차단 중 → 크래시 발생.

**결정 내용**  
- PlayFab 완전 제거 → 로컬 JSON 저장으로 대체
- GPGS / Apple Sign In 제거 (PlayFab 로그인과 연동되어 있어 함께 제거)
- AdMob / Firebase / IAP는 제거하지 않고 새 계정으로 교체
- 랭킹은 일단 스텁 처리, 추후 Firebase로 재설계

**이유**  
서버 의존성을 없애 빠르게 게임을 실행 가능한 상태로 만드는 것이 우선.  
Firebase 기반 재설계는 복잡도가 높아 Phase 4로 미룸.

---

### 다음 세션에 이어할 것 → ✅ Phase 1 완료

---

## 2026-06-14 (2차 세션)

### Phase 1 완료: PlayFab 완전 제거 & 로컬 저장 전환

**완료 내용**
- `LocalSaveManager.cs`, `LeaderboardEntry.cs` 신규 작성
- `PlayFabStore.cs`, `PlayFabManager.cs`, `PlayFabLeaderboard.cs`, `PlayFabTitleData.cs` 전면 재작성 (로컬 스탠드얼론)
- `PlayFabFunctions.cs` 삭제
- using PlayFab.* 잔존 파일 전수 정리 (IAPManager, FieldScene, UI_Summon_Item 등)
- `Assets/Plugins/PlayFabSDK/` 폴더 삭제
- 컴파일 오류 해결 (누락된 `using Controller.Play`, `using Controller.Infos` 추가)

**핵심 결정: GPGS / Apple Sign In SDK 유지**
- 이유: 출시 시 구글/애플 로그인 재활용 가능성 높음. SDK 재설치 비용이 큼.
- `LoginScene.cs`의 GPGS/Apple 코드는 `#if UNITY_ANDROID` / `#if UNITY_IOS` 블록 안에 그대로 유지.
- PlayFab은 서버 의존성이 핵심 문제였으므로 완전 제거가 맞았고, GPGS/Apple은 성격이 다름.

**핵심 결정: 랭킹 public 시그니처 유지**
- `PlayFabLeaderboard`의 모든 public 메서드를 스텁으로만 교체 (시그니처 유지).
- Phase 4에서 Firebase 리더보드 구현 시 내부만 교체하면 호출부 수정 불필요.

### 다음 세션에 이어할 것

- **Phase 2**: AdMob 계정 교체, Firebase 프로젝트 교체, Unity 프로젝트 ID 교체, 앱 이름/번들 ID 교체
- Phase 2는 대부분 설정값 교체라 난이도 낮음. 새 계정/프로젝트 준비 여부 확인 후 착수.

---

## 2026-06-15

### 결정: LayerLab CharacterMaker → Player 비주얼 교체

**배경**  
LayerLab 2D Minimal-CharacterMaker 에셋을 기반으로 플레이어 캐릭터 비주얼을 교체하기로 결정.  
기존 CostumeSetter + CostumeScriptableObject(고정 스프라이트 슬롯) 방식을 PartsManager 기반으로 전환.

**결정 내용**  
- CharacterMaker 프리팹을 Player body의 **자식**으로 배치 (비주얼 레이어만 교체)
- Player.prefab의 ROOT/Positioner 계층 및 게임 전용 오브젝트(DamagePosition, Bubble, PetPositions, DashBar 등) 유지
- Animation Event 방식 → Update normalizedTime 폴링 방식으로 전환 (LayerLab 에셋 무수정)
- LayerLab _Controller에 "Die" State(Dead1.anim) + "AttackSpeed" 파라미터 추가

**이유**  
- CharacterMaker의 Animator는 자신의 루트 기준 상대경로로 SR 제어 → Animator를 상위로 옮기면 클립 경로 깨짐
- Animation Event를 LayerLab 클립에 추가하면 에셋 업데이트 시 손실 위험 → 코드 폴링이 더 안전
- 지금은 비주얼 교체만. 파츠 선택 UI는 이후 Phase

**상세 설계 문서**: (별도 파일 미작성 — 이후 Shinabro로 대체되어 의미 없어짐)

### 다음 세션에 이어할 것 → ✅ 완료 후 Shinabro로 대체됨

---

## 2026-06-15 (2차 세션)

> ⚠️ **이 세션의 CharacterMaker 파이프라인 전체는 2026-06-21에 Shinabro (Spine)로 대체됨. 역사 기록으로 보존.**

### CharacterMaker 이식 완료: Player 비주얼 교체 동작 확인

**완료 내용 (코드)**
- `Player.cs` 수정
  - `using Costume;` 제거
  - `_animator = GetComponentInChildren<Animator>()` (CharacterMaker 자식 Animator 탐색)
  - `GetOrAddComponent<CostumeSetter>()` 제거
  - `_animator.Play("Die")` → `_animator.Play("Dead1")`
  - `_hitFired`, `HIT_NORMALIZED_TIME = 0.5f` 필드 추가
  - `UpdateAttackEvents()` 메서드 추가 (normalizedTime 폴링 방식)
  - `case PlayerState.Action:` → `UpdateAttackEvents()` 호출
  - `DoSkill()` → 항상 `"Skill"` 재생 (LayerLab _Controller에 Skill 하나만 있음)
- `_Controller.controller` 수정
  - `AttackSpeed` float 파라미터 추가 (DefaultFloat: 1)
  - Attack State: `SpeedParameterActive: 1`, `SpeedParameter: AttackSpeed`
- `Attack.anim`, `Skill.anim`, `Jump Attack.anim` — 빈 Animation Event 제거 (`m_Events: []`)

**완료 내용 (에디터 - 사용자 직접)**
- Player.prefab: 기존 비주얼 자식(head, eyes, Body 등) 삭제
- `Character_20260615_220922.prefab` → Player body 자식으로 배치
- Player body의 Animator 컴포넌트 제거
- 캐릭터 비주얼 변경 및 애니메이션 재생 확인 ✅

**트러블슈팅 기록**
- `GetComponentInChildren`이 Player body 자신의 Animator(구 컨트롤러)를 먼저 잡아 애니메이션 불재생 → Player body Animator 제거로 해결
- `DoSkill(skill.Animation)` → 구 컨트롤러 상태명 불일치로 `State could not be found` → `"Skill"` 고정으로 해결
- LayerLab 클립 빈 Animation Event 경고 → 해당 이벤트 직접 제거로 해결

**미결: HIT_NORMALIZED_TIME 조정**
- 현재 0.5f (기본값). Attack.anim을 Animation 창에서 보고 실제 히트 타이밍에 맞춰 조정 필요.

### CharacterMaker 캐릭터 파이프라인 (실제 구현 기준)

#### Player.prefab 실제 계층 구조

```
ROOT "Player"
  └── Positioner (localScale: 1,1,1)
        └── Player body "Player" (Player.cs 부착, localScale: 0.1, 0.1, 0.1, Animator 없음)
              ├── normalSkill, Attack2~4Skill (스킬 이펙트, 유지)
              └── Character_20260615_220922 (PrefabInstance, localScale 오버라이드: 10,10,10)
                    ├── Animator (_Controller.controller) ← Player.cs가 GetComponentInChildren으로 탐색
                    ├── PartsManager 컴포넌트
                    ├── CharacterPrefabData 컴포넌트
                    └── Head, Hair, Chest, HandRight, HandLeft ... (비주얼)
```

> 스케일 주의: Player body가 0.1이므로 CharacterMaker localScale을 10으로 오버라이드해야 실제 크기 1.0이 됨.

#### _Controller.controller 실제 상태

| 상태명 | 용도 | 비고 |
|--------|------|------|
| Idle | 대기 | 기본 상태 |
| Walk | 이동 | |
| Attack | 일반 공격 | **AttackSpeed** 파라미터 연결됨 |
| Skill | 스킬 | DoSkill()이 항상 이 이름 재생 |
| Dead1 | 사망 | 이 세션에서 추가 |
| Dead2, Dead3, Dance, Victory, Stun, Roll, Jump 등 | 미사용 | LayerLab 기본 제공 |

파라미터: `AttackSpeed` (float, default 1) — Attack 상태에만 SpeedParameter로 연결

#### Player.cs 실제 연결 구조

```csharp
// Awake: 자식 Animator 자동 탐색
_animator = GetComponentInChildren<Animator>();

// AttackSpeed 실시간 반영 (이벤트 기반)
TotalStatController.I.AttackSpeed.ValueChanged += WhenAttackSpeedChanged;
void WhenAttackSpeedChanged() => _animator.SetFloat("AttackSpeed", TotalStatController.I.AttackSpeed.Value);

// 사망
_animator.Play("Dead1", 0, 0);

// 스킬 (파라미터 skillAnimation은 무시, 항상 "Skill" 재생)
void DoSkill(string skillAnimation) => _animator.Play("Skill", 0, 0);

// 공격 히트 타이밍: normalizedTime 폴링 (Animation Event 없음)
const float HIT_NORMALIZED_TIME = 0.5f; // ← 아직 실제 클립 보고 조정 필요
```

#### PartsManager 핵심 API

```csharp
var pm = GetComponentInChildren<PartsManager>();

pm.EquipParts(PartsType.Sword, index);      // 무기 변경
pm.UnequipParts(PartsType.Helmet);          // 파츠 제거
pm.SetColor(ColorTargetType.Hair, color);   // 색상 변경
pm.ToPresetItem();                          // 현재 외형 직렬화 → LocalSaveManager 저장용
pm.ApplyPresetItem(item);                   // 저장된 외형 복원
```

HandRight 무기 그룹 (동시에 하나만 활성): Sword, Axe, Bow, Wand, Staff, Spear, Blunt, Crossbow  
자동 연동: Bow/Crossbow 장착 시 Arrow/Bolt 표시, Helmet 장착 시 HelmetHair로 교체

### 다음 세션에 이어할 것 → ~~CharacterMaker 계획은 Shinabro 교체로 폐기~~ / Phase 2 별도 진행

- ~~**스케일 자동 설정**: LayerLab CharacterMaker 관련~~ → Shinabro로 대체
- ~~**파츠 연동 설계**: LayerLab PartsManager API~~ → Shinabro `SimpleSpineSkinAssigner`로 대체
- **Phase 2** (별도): AdMob·Firebase·Unity 프로젝트 ID 교체 (새 계정 준비 후 착수)

---

## 2026-06-15 (3차 세션)

### Dashboard 히스토리 탭 추가 & CharacterMaker 파이프라인 문서화

**완료 내용**
- `Dashboard.html`에 히스토리 섹션(📜) 추가 — 의사결정 기록을 브라우저에서 바로 확인 가능
- Dashboard.html이 정적 생성 파일임을 확인 → 목적(동적 현황 파악, 에디터 확장)에 부합하지 않음 → 백로그에 Flask 서버 전환 등록

**CharacterMaker 파이프라인 실제 코드 기준으로 확정**
- `Player.cs`, `_Controller.controller`, `Player.prefab`, `PartsManager.cs` 직접 확인
- 설계 문서와 실제 구현의 차이 파악 및 정정:
  - Player body localScale: 0.1 → CharacterMaker localScale 오버라이드 **10** (실제 크기 1.0)
  - `DoSkill()` 파라미터 무시, 항상 `"Skill"` 고정
  - `AttackSpeed`는 `WhenAttackSpeedChanged()` 이벤트로 실시간 `SetFloat()`
- 파이프라인 정식 문서화 (history.md + Dashboard.html 동기화)

---

## 2026-06-21

### Player → Shinabro MiniFantasyCharacters 교체 (LayerLab → Spine)

**배경**  
LayerLab CharacterMaker 기반 플레이어 비주얼을 Shinabro의 MiniFantasyCharacters (Spine SkeletonMecanim)로 교체.  
무기 종류에 따른 애니메이션 분기가 필요했고, Spine 스킨 시스템이 이를 더 잘 지원함.

**결정 내용**
- `Assets/Layer Lab/` 폴더 전체 삭제 (6000+개 파일)
- `Assets/Downloads/Shinabro/MiniFantasyCharacters/` 에셋 사용
- Player.cs 전면 수정 — Spine Mecanim 기반 애니메이션명으로 교체
- `SimpleSpineSkinAssigner.cs` 수정 (당시 기록: AssignSkins public, Start() 추가)
  - 2026-06-24 재확인 결과: 실제 코드는 `Start()`만 추가된 상태이고 `AssignSkins()`는 아직 private.
- `Character_Controller.controller` 수정 (AttackSpeed 파라미터, 전체 상태 추가)

**Player.cs 주요 변경 (asset-migration-analysis.md 1-5 기준)**
- `"Dead1"` → `"Die"`, `"Idle"` → `"Wait1"`, `"Walk"` → `"Walk1"`, `"Roll"` → `"Run1"`
- `"Attack"` → `GetAttackAnimation()` (무기 타입별 분기: OneHand/TwoHand/DualHand/Shoot/Spell)
- `"Skill"` → `GetSkillAnimation()` (무기 타입별 분기: Cast/Spell/Shoot)
- `SimpleSpineSkinAssigner _skinAssigner` 필드 추가 — 무기 스킨 문자열로 애니메이션 판별
- `LookAt()` 반전 — Shinabro 기본 방향이 왼쪽이라 LookLeft/LookRight 로직 반전
- `_skillAnimAlt` 토글로 Skill 1/2 교차 재생

**상세 분석 문서**: `_dev/asset-migration-analysis.md`

### 다음 세션에 이어할 것 → 현재 진행 중

---

## 2026-06-22

### Monster → 2D SD Monster Pack 교체 (코드/설정 완료, 에디터 작업 대기)

**배경**  
기존 Mushroom Hero 몬스터 비주얼을 2D SD Monster Pack (Unity 2D Animation, SpriteLibrary/SpriteResolver 기반)으로 교체.

**완료 내용**

- `Monster.cs` 애니메이션명 소문자 교체 완료
  - `"Attack"` → `"attack"`, `"Attack2"` → `"attack2"`, `"Walk"` → `"walk"`, `"Die"` → `"die"`
- 몬스터 매핑 확정 (`_dev/monster-mapping.md` 작성)
  - Stage 테이블 분석으로 실제 구조 파악: 5개 월드 × 20스테이지, 일반 몬스터 17종 + 보스 10종
  - SD Pack 11종을 월드별 배분: 일반 7종(Rat/Spider/Bat/Crow/Goblin/Skeleton/Zombie), 보스 4종(Worm/Ghost/Beholder/Bigguy)
- Animator Controller 수정 완료 (7개 파일)
  - **Bat**: `walk` 상태 추가 → `idle.anim` 연결 (walk/fly 모두 없어서 idle로 대체)
  - **Beholder**: `walk` 상태 추가 → `fly.anim` 연결 / `attack2` 추가 (보스용, attack 동일 클립)
  - **Goblin, Zombie, Ghost, Skeleton**: `attack` 상태 추가 → 각 `attack-smash` / `attack-stab` 클립 연결
  - **Ghost, Bigguy**: `attack2` 추가 → `attack-bow.anim` 연결 (보스 2번째 공격용)
  - **Bigguy**: `attack` 추가 → `attack-smash.anim`
  - **Worm**: `attack2` 추가 → `attack.anim` 재사용 (단일 공격 보스)

**핵심 결정: 매핑 전략**  
SD Pack 11종으로 기존 27종 커버. 계열별 동일 외형 공유 (스탯만 다름).  
DB 테이블 구조는 그대로 유지 — 프리팹 내부 비주얼만 교체.

**핵심 결정: 보스 attack2**  
단일 공격 보스(Worm/Beholder)는 `attack2` 상태를 `attack` 동일 클립으로 연결.  
이중 공격 보스(Ghost/Bigguy)는 `attack` → smash, `attack2` → bow 분기.

### Phase 2 일부 완료

- ✅ `productName`: "Mushroom Hero" → "Webtoon Hero"
- ✅ `companyName`: → "NDC"
- ❌ `bundleIdentifier`: 교체 미완료. 당시 기록은 `com.NdolphinConnect.MushroomHero` / `mush.room.hero`였으나, 2026-06-24 재확인 기준 실제 값은 Android `com.thumpquest.thumpquest`, iOS `mush.room.hero`.

### 다음 세션에 이어할 것

- **에디터 프리팹 교체** — `_dev/monster-mapping.md` 5절 절차대로 20종 프리팹 교체
  - 기존 Child 0의 비주얼/Animator 제거 → SD Pack 해당 몬스터 내부 오브젝트 자식 배치
  - DamagePosition, TargetingPosition 위치 조정
- **bundleIdentifier 교체** (Phase 2)

---

## 2026-06-23

### Shinabro 얼굴 스프라이트 교체 및 Spine 버전 호환 픽스

**완료 내용**

- `Character.png` 아틀라스에서 `headA` 얼굴 영역(601, 451, 82×84px)을 신규 이미지로 교체  
  - Python + Pillow 스크립트로 합성 (`Character_backup.png` 백업 보존)
  - 소스 이미지가 이미 82×84 RGBA였으므로 리사이즈 불필요
  - 아틀라스 좌표계: **좌상단 기준** (PIL과 동일 — Spine도 top-left origin)
  - `Character.atlas.txt` 수정 불필요 (좌표 그대로)

- Spine 버전 불일치 오류 수정  
  - `Character.json`: Spine 4.2.43으로 익스포트됨  
  - 설치된 런타임: spine-unity 4.1  
  - 수정 파일: `Assets/Spine/Runtime/spine-unity/Asset Types/SkeletonDataCompatibility.cs`  
  - `compatibleBinaryVersions`, `compatibleJsonVersions` 에 `{ 4, 2, 0 }` 추가

**핵심 결정: 버전 체크 완화 (런타임 업그레이드 대신)**  
4.1 런타임이 4.2 JSON을 파싱 가능한지 실제 동작으로 확인하는 방향 선택.  
런타임 전체 업그레이드는 API 변경 리스크가 있어 일단 보류.  
만약 애니메이션 깨짐 등 이상 발생 시 → spine-unity 4.2 런타임 전체 교체 필요.

**상세 작업 문서**: `_dev/shinabro-face-replacement.md`

### 다음 세션에 이어할 것

- 플레이어 파츠 구분 변경

---

## 2026-06-23 (2차 세션)

### Shinabro Character_Controller.controller 애니메이션 상태 전체 추가

**완료 내용**

- `Character_Controller.controller` 수정
  - `m_AnimatorParameters`: `AttackSpeed` (float, default 1) 추가
  - `AnimatorState` 14개 신규 추가: Walk1, Run1, Die, Attack_OneHand1/2, Attack_TwoHand1/2, Attack_DualHand1/2, Shoot1, Spell1/2, Cast1/2
  - `m_ChildStates`에 전체 15개 상태 등록
  - Attack 계열 상태: `SpeedParameterActive: 1`, `SpeedParameter: AttackSpeed` 설정
  - 각 상태 → 기존 AnimationClip(fileID) 참조

**배경**  
컨트롤러에 AnimationClip은 32개 모두 존재했으나, AnimatorState 객체는 Wait1 하나뿐이었음.  
Player.cs에서 `animator.Play("Walk1")` 등을 호출해도 상태가 없어 애니메이션 불재생.

### Slime2 풀 재활용 시 die 애니메이션으로 시작되는 버그 — 미해결

**증상**  
죽은 Slime2가 오브젝트 풀로 반환된 후 재소환될 때, 잠깐 die 상태로 시작됨.

**조사한 내용**
- `Slime.controller` default state는 `idle` — 정상
- `m_KeepAnimatorStateOnDisable: 0` — disable 시 Animator 상태 리셋 — 정상
- `Monster.Init()` 내 `animator.Rebind()` + `animator.Update(0f)` 존재
- AnimationEvent 없음, Pool/HP 초기화 로직 정상
- `Clear()` 순서: `SetActive(false)` 이후에 `_materialSetter.ChangeColor(Color.black)` 호출 (비정상적 순서이나 동작에는 영향 없음)

**임시 조치**  
`Monster.Init()`에 `animator.Play("idle", 0, 0)` 명시 추가 (Rebind 직후).  
실제 효과는 미확인 — 다음 세션에서 디버그 로그로 검증 필요.

### 다음 세션에 이어할 것

- **Slime2 die 버그 디버그**: `Monster.cs`의 Init/Die/Clear 및 Pool OnEnable 시점에 Debug.Log 심고, 실제 플레이로 타이밍 확인
  - 확인 포인트: `Init()` 호출 시점, `OnEnable()` 호출 시점, 어느 시점에 die 애니메이션이 보이는지
- 플레이어 파츠 구분 변경

---

## 2026-06-24

### 문서/히스토리 실제 코드 기준 정리

**배경**
프로젝트 문서 일부가 이전 작업 상태를 그대로 유지하고 있어 실제 코드와 불일치했다.
특히 `CLAUDE.md`는 Phase 1 진행 중으로 되어 있었고, `new-game-migration-plan.md`는 Player 파츠 API와 Monster 진행 상태 일부를 실제 구현보다 앞서 완료로 기록하고 있었다.

**확인한 실제 상태**
- PlayFab 서버 SDK 폴더(`Assets/Plugins/PlayFabSDK/`)와 `PlayFabFunctions.cs`는 제거됨.
- `PlayFabManager`, `PlayFabStore`, `PlayFabLeaderboard`, `PlayFabTitleData`는 이름만 유지한 로컬 저장/스텁 호환 레이어로 동작.
- `IAPManager.cs`의 `#if APPSFLYER_ENBALE` 블록에는 `PlayFabClientAPI.GetUserData/UpdateUserData` 직접 호출이 남아 있음. 현재 심볼 오타로 비활성화 상태지만 추후 AppsFlyer 활성화 시 교체 필요.
- Player는 Shinabro Spine `SkeletonMecanim` 기반으로 `Wait1`, `Walk1`, `Die`, `Attack_*`, `Shoot1`, `Spell*`, `Cast*` 애니메이션명을 사용.
- `SimpleSpineSkinAssigner`는 Start/OnEnable/OnValidate에서 내부적으로 스킨을 적용하지만 `AssignSkins()`는 아직 private이고, `Player.SkinAssigner` getter도 없음.
- Monster는 SD Pack 소문자 상태명(`idle`, `walk`, `attack`, `die`) 기준으로 동작하며, `Init()`/`OnEnable()`에서 `ResetAnimatorToIdle()`을 호출.
- Slime2 die 시작 버그는 `Rebind` + `Play("idle")` + `Update(0f)` 임시 조치가 들어간 상태지만 플레이 검증은 아직 필요.

**문서 업데이트**
- `CLAUDE.md`: 현재 단계와 세션 시작 시 확인 문서, 코드 기준 주의사항 갱신.
- `_dev/new-game-migration-plan.md`: 최종 업데이트일, PlayFab 상태, 크래시 상태, Phase 2/2B 체크리스트, 현재 즉시 할 일 갱신.
- `_dev/player-parts-system.md`: 외부 제어 API 미구현 상태와 실제 Player 애니메이션 분기 반영.
- `_dev/monster-mapping.md`: Bat/Beholder walk 처리, attack/attack2 컨트롤러 처리 완료 상태, Monster.cs 초기화 로직 반영.

### 다음 세션에 이어할 것

- Unity에서 Slime2 풀 재활용 후 `die` 상태로 시작하는지 실제 플레이로 검증.
- Player 파츠 런타임 교체가 필요하면 `AssignSkins()` public 전환 및 `Player.SkinAssigner` getter 추가.
- `IAPManager.cs`의 비활성 PlayFab 직접 호출 정리.
- Monster 프리팹 교체/위치 조정 상태를 `_dev/monster-mapping.md` 기준으로 검증.

---

## 2026-06-24 (2차 세션)

### IAP 무한 로딩 방지 및 테스트 구매 성공 처리 추가

**배경**
빌드에서 IAP 연결이 끊긴 상태로 구매를 실행하면 로딩 UI가 계속 남는 문제가 있었다.

**원인**
- `IAPManager.Buy()`에서 `UI_Loading`을 먼저 띄운 뒤 상품 조회를 수행함.
- `storeController.products.WithID(productId)` 결과가 없거나 `availableToPurchase == false`인 경우 로그만 출력하고 종료.
- 이 실패 경로에서 `UI_Loading`을 닫지 않고 `_buyingItem`도 유지되어 무한 로딩/구매 진행 중 상태가 됨.
- 패스 구매 성공 경로도 로딩 UI를 닫지 않는 문제가 있었다.

**수정 내용**
- `IAPManager.cs`에 Inspector 옵션 추가:
  - `Test Purchase > Use Test Purchase Fallback`
  - 기본값 `true`
- IAP 미초기화, 상품 없음, 구매 불가 상황에서 옵션이 켜져 있으면 `TryCompleteTestPurchase()`로 임의 성공 처리.
- 실제 실패 처리용 `FailCurrentPurchase()` 추가.
  - 에러 로그 출력
  - `UI_Loading` 닫기
  - `_buyingItem = null`로 구매 진행 상태 해제
- 구매 성공 처리 공통 메서드 `CompletePurchase()` 추가.
- 로딩 UI 정리 공통 메서드 `CloseLoading()` 추가.
- 패스 구매 성공 시에도 로딩 UI가 닫히도록 수정.
- 상품 가격 세팅 시 `storeController.products.WithID(...)` 결과가 null일 수 있어 `val != null` 방어 추가.

**현재 동작**
- `Use Test Purchase Fallback`이 켜져 있으면 IAP 연결이 안 되어도 테스트 성공 처리로 보상 지급.
- 옵션을 끄면 상품 없음/구매 불가/초기화 실패 시 로딩을 닫고 내부 구매 상태를 해제하며 에러 로그만 남김.
- 실패 팝업/토스트는 아직 없음.

### 다음 세션에 이어할 것

- Unity Inspector에서 `IAPManager`의 `Use Test Purchase Fallback` 노출 확인.
- 에디터/빌드에서 상품 없음, 구매 불가, 구매 실패, 테스트 성공 처리 각각 플레이 검증.
- 실제 서비스 결제 연결 전 `Use Test Purchase Fallback`을 반드시 끄는 운영 체크 추가 검토.
- 실패 시 유저에게 보여줄 토스트 또는 팝업 문구 추가 여부 결정.

---

## 백로그 (추후 과제)

### Dashboard 로컬 서버 전환

**배경**  
현재 `_dev/Dashboard.html`은 `generate_dashboard.py`로 생성하는 정적 파일.  
프로젝트 파일 변경이 자동으로 반영되지 않고, 갱신 버튼도 없음.

**목표**  
- 프로젝트 파일(md, json 등)을 동적으로 읽어 항상 최신 상태를 표시
- 브라우저 새로고침 또는 버튼으로 즉시 갱신
- 향후 외부 에디터 기능으로 확장 가능한 구조

**방향**  
Python 경량 서버(Flask 등)로 전환. 요청 시 파일을 읽어 렌더링, API 엔드포인트로 에디터 확장 연결.

---

## 2026-06-24 (Monster part sorting)

### SD Monster Pack 몬스터 파츠 정렬 보정

**배경**
- Player를 제외한 몬스터 비주얼은 SD Monster Pack 기반이다.
- SD Monster Pack은 각 파츠 Transform의 local Z로 내부 앞뒤 깊이를 표현한다.
- 현재 프로젝트는 Y축 기준 전역 투명도 정렬을 사용하므로, 몬스터 내부 파츠가 에셋 의도와 다르게 정렬될 수 있었다.
- `GraphicsSettings`의 전역 투명도 정렬 모드/축을 바꾸면 필드 전체 렌더링에 영향이 크므로 1차 해결책에서 제외.

**결정**
- 전역 투명도 정렬 설정은 유지한다.
- 몬스터 하나를 외부 정렬 단위로 묶기 위해 루트에 `SortingGroup`을 붙인다.
- 몬스터 내부 파츠만 visual root 기준 relative Z를 `sortingOrder`로 변환해 재정렬한다.
- 파츠 이름 기반 예외 처리는 v1에서 넣지 않는다. 아티스트가 잡아둔 Z 의도와 충돌할 수 있기 때문.

**구현**
- `Assets/Scripts/Utils/MonsterPartSorter.cs` 추가.
  - visual root 하위 `SpriteRenderer`를 수집.
  - `sortingOrder = baseOrder + round(-relativeZ * zStep)` 적용.
  - 기본 `zStep = 100`.
  - Animator 평가 후 파츠 위치 변화를 반영하기 위해 `LateUpdate()`에서 갱신.
- `Assets/Scripts/Fight/Units/Monster.cs` 수정.
  - 몬스터 루트에 `SortingGroup` 자동 보장.
  - `MonsterPartSorter` 자동 보장 및 `Init(transform)` 호출.
  - `ResetAnimatorToIdle()`에서 Animator 강제 갱신 후 `_partSorter.Apply()` 호출.

**검증**
- Unity `2022.3.62f2` batchmode compile check 완료, exit code 0.
- `Logs/codex-compile-check.log` 기준 C# 컴파일 에러 없음.
- `MonsterPartSorter.cs.meta` GUID 충돌 없음 확인.

**메모**
- Prefab YAML은 직접 수정하지 않았다. 컴포넌트는 `Monster.cs`에서 런타임에 자동 부착한다.
- 이 처리는 `Monster.cs`와 SD Monster Pack 비주얼 계층을 사용하는 몬스터에 적용된다.
- 일반 몬스터, 보스, 이펙트 겹침 케이스는 Play Mode 시각 검증이 아직 필요하다.

---

## 2026-06-24 (문서 최신화)

### 프로젝트 현재 코드 기준으로 `_dev` 문서 정정

**배경**
일부 문서가 과거 완료 기록과 현재 코드 상태를 혼재해서 보여주고 있었다.
특히 `_dev/asset-migration-analysis.md`는 `SimpleSpineSkinAssigner.AssignSkins()` public 전환이 완료된 것처럼 적혀 있었지만, 실제 코드는 아직 private이다.
또 IAP 테스트 구매 fallback 기록은 의도와 구현이 섞여 있어, 현재 코드상 `_isConnected == false`에서는 초반 guard에서 return한다는 점을 별도로 명시할 필요가 있었다.

**정정 내용**
- `_dev/asset-migration-analysis.md`
  - Player 파츠 외부 제어 API를 미구현 상태로 정정.
  - `AssignSkins()` public 전환과 `Player.SkinAssigner` getter를 미결 항목으로 이동.
  - Monster 쪽에 `MonsterPartSorter`/`SortingGroup` 기반 내부 파츠 정렬 보정을 추가.
- `_dev/new-game-migration-plan.md`
  - 현재 상태 문구를 Player 외부 파츠 API 미완료, Monster 정렬 보정 완료 기준으로 갱신.
  - Android/iOS Bundle ID 현재값을 실제 `ProjectSettings.asset` 기준(Android `com.thumpquest.thumpquest`, iOS `mush.room.hero`)으로 정정.
  - IAP fallback은 상품 없음/구매 불가 상황에서는 테스트 성공 처리가 가능하지만, 미초기화 상태는 현재 early return한다고 명시.
- `_dev/monster-mapping.md`
  - `MonsterPartSorter` 동작 방식과 Play Mode 시각 검증 필요 항목 추가.
- `_dev/player-parts-system.md`
  - 현재 문서가 Player 파츠 API 상태의 최신 기준임을 명시.
- `_dev/phase1-detailed-plan.md`
  - Phase 1 실행 전 상세 계획 보존 문서임을 명시.

**현재 기준으로 이어할 것**
- Player 런타임 파츠 교체가 필요하면 `AssignSkins()` public 전환과 `Player.SkinAssigner` getter 추가.
- IAP 미초기화 상태 테스트 fallback은 이후 `IAPManager.cs`에서 처리됨. 실제 빌드 검증과 운영 비활성화 체크가 남아 있다.
- Monster 파츠 정렬은 컴파일 검증만 완료된 상태이므로 Play Mode에서 일반/보스/이펙트 겹침을 확인.
- Bundle ID, AdMob, Firebase, IAP 계정값 교체는 아직 남아 있다.

---

## 2026-06-24 (Monster animation state name casing)

### 몬스터 Animator 상태명 대소문자 잔여 호출 정리

**배경**
SD Monster Pack의 Animator 상태명은 소문자(`idle`, `walk`, `attack`, `die`) 기준이다.
하지만 기존 Mushroom Hero/이전 컨트롤러 흐름에서는 대문자 상태명(`Idle`, `Walk`, `Attack`, `Die`)을 사용했고, 일부 우회 호출에 잔여 대문자 호출이 남아 있었다.

스턴 해제 후 몬스터가 정상 복귀하지 않는 현상과 리스폰 후 `die` 상태 유지 현상의 원인을 조사하던 중, `PlayController`의 훈련장 타임리밋 흐름에서 `PlayAnimation("Die")`가 남아 있는 것을 확인했다.

**수정 내용**
- `Assets/Scripts/Controller/Play/PlayController.cs`
  - `Manager.Field.GetFirst().PlayAnimation("Die")` → `PlayAnimation("die")`
- `Assets/Scripts/Fight/Units/Monster.cs`
  - `PlayAnimation(string animationName)`에 구 상태명 방어 매핑 추가:
    - `"Die"` → `"die"`
    - `"Walk"` → `"walk"`
    - `"Idle"` → `"idle"`
    - `"Attack"` → `"attack"`
    - `"Attack2"` → `"attack2"`

**판단**
이번 변경은 픽서나 애니메이션 클립을 건드리지 않는 저위험 정리다.
대소문자 혼재가 실제 원인이었는지 확인하려면 Play Mode에서 스턴 해제, 훈련장 타임리밋, 풀 재사용 리스폰을 다시 검증해야 한다.

**남은 의심 지점**
- `MonsterRespawnAnimationClipFixer`가 2026-06-23에 SD Monster Pack의 `idle.anim`/`walk.anim`을 수정한 흔적이 있다.
- 대소문자 정리 후에도 스턴 복귀 문제가 남으면, 다음 원인은 fixer로 인한 `idle/walk` 클립 오염 가능성이 높다.

---

## 2026-06-24 (Monster SortingGroup default order)

### Monster root sorting order normalized to 5

**Background**
- The monster part sorting fix only recalculates child `SpriteRenderer.sortingOrder` values inside each monster visual root.
- Cross-object ordering against backgrounds/UI depends on the monster root `SortingGroup`.
- Existing monster prefabs were checked under `Assets/Resources/Prefabs/Characters`.
  - All monster prefabs used root `SortingGroup.m_SortingOrder = 5` except `BossTraining`, which used `4`.
  - Background prefabs use the Default sorting layer with orders mostly from `-1` to `3`.

**Changes**
- `Assets/Resources/Prefabs/Characters/BossTraining.prefab`
  - Changed root `SortingGroup.m_SortingOrder` from `4` to `5`.
- `Assets/Scripts/Fight/Units/Monster.cs`
  - Runtime monster root `SortingGroup.sortingOrder` is now explicitly set to `5`.
  - This also covers new or inconsistent monster prefabs where `GetOrAddComponent<SortingGroup>()` creates a default order `0` group.

**Verification**
- Re-parsed all non-player prefabs in `Assets/Resources/Prefabs/Characters`.
- Every monster prefab root `SortingGroup` now reports order `5`.

---

## 2026-06-24 (Korean number unit formatting)

### `Define.AddUnit` supports Korean locale units

**Background**
- Numeric UI text was globally formatted through `Define.AddUnit(...)`.
- Non-Korean locales used the existing alphabet unit format such as `A/B/C`.
- For Korean, the desired display was Korean-style units instead of alphabet abbreviations.

**Changes**
- `Assets/Scripts/Utils/Define.cs`
  - Added a Korean-locale branch inside `Define.AddUnit(...)`.
  - Korean locale detection uses `LocalizationSettings.SelectedLocale` and applies when the locale code starts with `ko`.
  - Korean numeric display now uses:
    - `1,000` -> `1천`
    - `1,400` -> `1천400`
    - `10,000` -> `1만`
    - `123,456` -> `12만3456`
    - `123,456,789` -> `1억2345만`
  - Non-Korean locales keep the previous `A/B/C` behavior.

**Notes**
- The Korean formatter is centralized in `Define.AddUnit(...)`, so existing UI call sites do not need individual changes.
- Existing visible text refresh still depends on each UI's normal value/language refresh path.

---

## 2026-06-25

### 현재 프로젝트 기준 히스토리 재확인

**확인한 기준**
- 현재 HEAD: `23d410f6 260624`
- Unity 버전: `2022.3.62f2`
- `_dev/history.md`는 2026-06-24 작업 기록까지 남아 있었고, 현재 프로젝트 상태와 큰 방향은 일치한다.

**현재 코드 상태**
- Player는 `Assets/Scripts/Fight/Units/Player.cs`에서 Shinabro `SkeletonMecanim` 기반 애니메이션을 사용한다.
  - 대기/이동: `Wait1`, `Walk1`
  - 사망: `Die`
  - 일반 공격: `GetAttackAnimation()`으로 무기 스킨 문자열 기준 분기
  - 스킬: `GetSkillAnimation()`과 `_skillAnimAlt`로 1/2 교차 재생
- `SimpleSpineSkinAssigner.AssignSkins()`는 여전히 private이다.
  - Start/OnEnable/OnValidate에서는 내부 적용됨.
  - 런타임 외부 파츠 교체 API로 쓰려면 `AssignSkins()` public 전환 또는 별도 public 메서드 추가 필요.
- Monster는 `Assets/Scripts/Fight/Units/Monster.cs`에서 SD Pack 소문자 상태명 기준으로 동작한다.
  - `idle`, `walk`, `attack`, `attack2`, `die`
  - `Init()`과 `OnEnable()`에서 `ResetAnimatorToIdle()` 호출.
  - `ResetAnimatorToIdle()`은 `Rebind()` → `Play("idle")` → `Update(0f)` → `_partSorter.Apply()` 순서.
  - `PlayAnimation()`에는 기존 대문자 상태명 방어 매핑이 들어가 있다.
- 몬스터 루트에는 런타임에 `SortingGroup.sortingOrder = 5`와 `MonsterPartSorter`가 자동 적용된다.
- `Define.AddUnit(...)`는 한국어 locale에서 `천/만/억` 단위 포맷을 사용하고, 그 외 locale은 기존 `A/B/C` 단위 포맷을 유지한다.
- IAP는 `IAPManager.cs`에 테스트 구매 fallback과 실패 시 로딩 닫기 처리가 들어가 있다.
  - IAP 미초기화, 상품 없음, 구매 불가 상황에서 `Use Test Purchase Fallback`이 켜져 있으면 테스트 성공 처리로 빠진다.
  - 옵션을 끄면 `FailCurrentPurchase()`로 로딩 UI와 구매 상태를 정리한다.

**문서 정리 내용**
- `Monster part sorting` 섹션을 한국어로 정리하고 기존 히스토리 문체에 맞춤.
- 2026-06-25 기준 확인한 Player/Monster/IAP/숫자 포맷 상태를 추가.

### 다음 세션에 이어할 것

- Unity에서 Slime2 및 SD Pack 몬스터가 풀 재활용 후 `die` 상태로 시작하지 않는지 플레이 검증.
- 몬스터 파츠 정렬과 root `SortingGroup` order 5가 일반/보스/이펙트 겹침 상황에서 의도대로 보이는지 확인.
- Player 파츠 런타임 교체가 필요하면 `SimpleSpineSkinAssigner` 외부 제어 API를 먼저 설계/추가.
- 실제 서비스 결제 연결 전 `Use Test Purchase Fallback`을 꺼야 하는 운영 체크를 코드 또는 빌드 절차에 추가 검토.

---

## 2026-06-25 (2차 세션)

### 플레이어 일반 공격 위치 판정 보정

**배경**
- 몬스터와 플레이어가 화면상 거의 겹쳐 있는데도 몬스터는 플레이어를 공격하고, 플레이어는 일반 공격을 시작하지 못하는 상황이 있었다.
- 원인 분석 결과 몬스터 공격 판정은 `monster.root.position`과 `player.Position()`의 거리 기준이지만, 플레이어 일반 공격 진입 판정은 `Monster.GetNormalAttackPosition()`이 반환하는 `Square` 좌/우 중앙점 기준이었다.
- SD Monster Pack 교체 후 `Square`의 bounds 중심/좌우 edge가 실제 화면상 접촉감과 어긋나면, 겹쳐 보이는데도 플레이어가 아직 공격 위치가 아니라고 판단할 수 있다.

**수정 내용**
- `Assets/Scripts/Fight/Units/Monster.cs`
  - `GetNormalAttackPosition(Vector3 attackerPosition)`을 수정.
  - 기존: `Square.bounds.center`의 Y와 좌/우 edge X를 조합한 고정 지점 반환.
  - 변경: `bounds.ClosestPoint(attackerPosition)`로 플레이어 위치에서 몬스터 bounds에 가장 가까운 지점 반환.

**판단**
- `NORMAL_ATTACK_POSITION_SQR_EPSILON = 0.01f`는 유지했다.
- 실제 거리 기준 0.1을 줄이는 것은 문제를 더 빡빡하게 만들 수 있으므로, 숫자 보정보다 목표 지점 계산 방식을 바꾸는 쪽을 선택했다.

### 한국어 숫자 단위 절삭 규칙 추가

**배경**
- 한국어 locale에서는 `Define.AddUnit(...)`이 `천/만/억/조` 계열 단위를 사용한다.
- 큰 숫자에서 하위 단위가 길게 붙어 UI 가독성이 떨어질 수 있어 절삭 규칙을 추가했다.

**수정 내용**
- `Assets/Scripts/Utils/Define.cs`
  - `AddKoreanUnit(BigInteger number)`에 절삭 규칙 추가.
  - `10,000,000` 이상이면 `천` 이하 절삭 (`number % 10000` 제거).
  - `100,000,000,000` 이상이면 `만` 이하 절삭 (`number % 100000000` 제거).

**예시**
- `12,345,678` → `1234만`
- `123,456,789` → `1억2345만`
- `123,456,789,000` → `1234억`

### 데미지 폰트 한국어 단위 적용 여부 조사

**확인 결과**
- 데미지 텍스트는 일반 UI의 `Define.AddUnit(...)`을 사용하지 않는다.
- `Assets/Downloads/DamageNumbersPro/Scripts/Internal/DamageNumber.cs` 내부의 자체 `AddUnit(number, 7)`을 사용하며, 현재 단위는 `A/B/C/...` 기준이다.
- `DamageText.prefab`이 사용하는 TMP FontAsset은 `Assets/Fonts/TMPFonts/DamageFont.asset`이다.
- `DamageFont.asset`의 character table에는 숫자, `A~H`, `+`, `*`, `_`, `...` 정도만 있고 `천/만/억/조` 글리프가 없다.
- `m_FallbackFontAssetTable`도 비어 있어, 현재 상태에서 데미지 폰트에 한국어 단위를 바로 적용하면 글자가 누락될 가능성이 높다.

**결정**
- 데미지 폰트 한국어 단위 적용은 보류.
- 추후 적용하려면 먼저 `DamageFont`에 `천/만/억/조` 글리프를 추가하거나 한국어 TMP FontAsset fallback을 연결해야 한다.

### 데미지 텍스트 정렬 순서 조정

**배경**
- 데미지 텍스트가 게임 상에서 캐릭터보다 앞에 뜨는 문제가 있었다.
- `DamageText.prefab`의 `SortingGroup.sortingOrder`가 `1000`으로 설정되어 있었다.
- 현재 기준 정렬값은 몬스터 루트 `5`, 데미지 텍스트 `1000`, 플레이어 `8`이었다.

**수정 내용**
- `Assets/Resources/Prefabs/Fonts/DamageText.prefab`
  - `SortingGroup.m_SortingOrder`를 `1000`에서 `7`로 변경.

**판단**
- 변경 후 정렬 관계는 몬스터 `5` < 데미지 텍스트 `7` < 플레이어 `8`이다.
- 데미지 텍스트가 몬스터보다는 앞에 보이되, 플레이어보다는 뒤로 가도록 조정했다.

### 몬스터 죽음 애니메이션 루프 해제

**배경**
- SD Monster Pack의 몬스터 죽음 애니메이션 `die.anim`에 loop 설정이 켜져 있었다.
- `Monster.Die()`는 `animator.Play("die")` 후 `_FadeRoutine()`에서 1초 대기/페이드 후 정리하므로, 현재 코드 흐름은 죽음 애니메이션 루프에 의존하지 않는다.

**수정 내용**
- 아래 `die.anim` 파일의 `m_LoopTime`을 `1`에서 `0`으로 변경.
  - `Assets/Downloads/2D SD Monster Pack/Animations/Slime/die.anim`
  - `Assets/Downloads/2D SD Monster Pack/MonsterAnimations/Bat/die.anim`
  - `Assets/Downloads/2D SD Monster Pack/MonsterAnimations/Beholder/die.anim`
  - `Assets/Downloads/2D SD Monster Pack/MonsterAnimations/Bigguy/die.anim`
  - `Assets/Downloads/2D SD Monster Pack/MonsterAnimations/Crow/die.anim`
  - `Assets/Downloads/2D SD Monster Pack/MonsterAnimations/Ghost/die.anim`
  - `Assets/Downloads/2D SD Monster Pack/MonsterAnimations/Goblin/die.anim`
  - `Assets/Downloads/2D SD Monster Pack/MonsterAnimations/Rat/die.anim`
  - `Assets/Downloads/2D SD Monster Pack/MonsterAnimations/Skeleton/die.anim`
  - `Assets/Downloads/2D SD Monster Pack/MonsterAnimations/Spider/die.anim`
  - `Assets/Downloads/2D SD Monster Pack/MonsterAnimations/Worm/die.anim`
  - `Assets/Downloads/2D SD Monster Pack/MonsterAnimations/Zombie/die.anim`

**검증**
- 12개 `die.anim` 모두 `m_LoopTime: 0`으로 변경된 것을 확인.
- `Assets/Downloads/2D SD Monster Pack` 범위에 대해 `git diff --check` 완료.

**검증**
- `Monster.cs`, `Define.cs` 변경에 대해 `git diff --check` 완료.
- Unity 컴파일/Play Mode 검증은 아직 수행하지 않았다.

### 다음 세션에 이어할 것

- Play Mode에서 플레이어와 몬스터가 겹치는 상황의 일반 공격 진입 여부 확인.
- 한국어 locale에서 일반 UI 숫자 절삭 표시 확인.
- 데미지 폰트에 한국어 단위를 적용하려면 폰트 글리프/fallback 구성부터 결정.
- Play Mode에서 데미지 텍스트가 몬스터보다 앞, 플레이어보다 뒤에 보이는지 확인.
- 몬스터 사망 시 `die` 애니메이션이 반복되지 않고 페이드 정리되는지 확인.

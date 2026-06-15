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

**상세 설계 문서**: `_dev/character-visual-migration-20260615.md`

### 다음 세션에 이어할 것 → ✅ 완료 (2026-06-15 2차 세션)

---

## 2026-06-15 (2차 세션)

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

#### 새 캐릭터로 교체할 때 에디터 절차

1. LayerLab CharacterMaker 씬에서 파츠/색상 선택
2. "Save Prefab" → `Assets/CharacterPrefabs/Character_YYYYMMDD_HHMMSS.prefab` 저장
3. Player.prefab 열기 → Player body 안 기존 CharacterMaker 자식 제거
4. 새 프리팹 드래그 → Player body 하위 배치
5. 새 프리팹 Transform의 localScale을 **(10, 10, 10)** 으로 설정
6. Player body에 Animator 컴포넌트가 없는지 확인 (있으면 제거)
7. 실행 후 Idle/Walk/Attack/Dead1 애니메이션 확인

### 다음 세션에 이어할 것

- **스케일 자동 설정**: Player.cs Awake에서 CharacterMaker 자식 localScale 고정 (원하는 크기 결정 필요)
- **파츠 연동 설계**: LayerLab `PartsManager` API 확인 → 장비/코스튬 변경 시 파츠 교체 연결
  - 무기 종류 → HandRight 파츠 교체
  - 코스튬 → Chest, Hair 등 파츠 교체
  - 선택된 파츠 `LocalSaveManager`에 저장/복원
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

### 다음 세션에 이어할 것

(2차 세션 이어서)
- **스케일 자동 설정**: Player.cs Awake에서 CharacterMaker 자식 localScale 고정
- **파츠 연동 설계**: 무기 → HandRight 교체, 코스튬 → Chest/Hair 교체, LocalSaveManager 저장/복원
- **Phase 2** (별도): AdMob·Firebase·Unity 프로젝트 ID 교체 (새 계정 준비 후 착수)

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

# 플레이어 비주얼 교체 계획 (CharacterMaker 이식)

> 작성일: 2026-06-15  
> 세션 주제: LayerLab 2D Minimal-CharacterMaker → 머시룸 히어로 플레이어 비주얼 교체

---

## 1. 결정 사항

**방향**: LayerLab CharacterMaker로 만든 캐릭터 프리팹을 Player.prefab의 "비주얼 자식"으로 배치한다.  
**범위**: 지금은 비주얼 교체만. 파츠 개별 선택 UI는 이후 Phase에서 추가.

---

## 2. 현재 구조 분석

### Player.prefab 실제 계층 (fileID 추적 완료)

```
ROOT "Player"           ← _root = transform.parent.parent
  ├── Bubble            ← _root.Find("Bubble")
  ├── DamagePosition    ← _root.Find("DamagePosition")
  ├── DashBar           ← _root.Find("DashBar") / Bar1~4
  ├── HpBar → Hp
  ├── PlayerSorting
  ├── Effect            ← Util.FindChild(_root, "Effect", true)
  └── Positioner        ← _positioner = transform.parent
        ├── Shadow
        ├── PetPositions ← _positioner.Find("PetPositions") / 4개
        └── "Player" body ← transform  (Player.cs 붙어 있음)
              ├── Animator (현재: Crow/Player.controller)
              ├── CostumeSetter
              ├── head, Hair, Body, Weapon, cloak ... (← 교체 대상)
              ├── eyes, face, mouth, Costume_* ...    (← 교체 대상)
              └── normalSkill, Attack2~4Skill          (← 유지)
```

**핵심**: DamagePosition, Bubble, PetPositions, DashBar, Effect 등 게임 전용 오브젝트가  
모두 ROOT / Positioner 레벨에 있어서 "Player body" 비주얼만 교체하면 됨.  
3단 계층(_root / _positioner / transform) 구조 그대로 유지 가능.

### Player.cs 참조 정리

| 참조 | 위치 | 교체 영향 |
|------|------|-----------|
| `_root = transform.parent.parent` | ROOT GO | 없음 |
| `_positioner = transform.parent` | Positioner GO | 없음 |
| `_animator = transform.GetComponent<Animator>()` | Player body | **수정 필요** |
| `_root.Find("DamagePosition")` | ROOT 자식 | 없음 |
| `_root.Find("Bubble")` | ROOT 자식 | 없음 |
| `_root.Find("DashBar")` | ROOT 자식 | 없음 |
| `_positioner.Find("PetPositions")` | Positioner 자식 | 없음 |
| `Util.FindChild(_root, "Effect")` | ROOT 하위 | 없음 |
| `GetOrAddComponent<CostumeSetter>()` | Player body | **제거 필요** |

---

## 3. LayerLab 2D Minimal-CharacterMaker 구조

### 에셋 위치
```
Assets/Layer Lab/2D Minimal-CharacterMaker/
  ├── Common/
  │   ├── Animations/       ← 애니메이션 클립 + _Controller.controller
  │   ├── Scripts/Core/     ← PartsManager.cs, PartsType.cs, Player.cs(데모용)
  │   ├── Scripts/Data/     ← CharacterPrefabData.cs, CharacterPrefabSaver.cs, PresetData.cs
  │   └── Scripts/UI/       ← PanelPartsControl 등 커스터마이징 UI
  └── Extensions/
      ├── Parts Pack Base/  ← 기본 파츠 스프라이트
      └── Parts Pack Vol.1/ ← 추가 파츠 스프라이트
```

### CharacterMaker 프리팹 구조 (Character_20260615_220922.prefab)
```
Character_... (Root, Scale: 2.0872)
  ├── Animator (_Controller.controller)
  ├── PartsManager 컴포넌트
  ├── CharacterPrefabData 컴포넌트
  ├── Ghost (SpriteRenderer)
  ├── [Body 영역] → HandRight, HandLeft
  │   ├── HandRight: Sword/Axe/Bow/Crossbow/Spear/Staff/Wand/Blunt + Arrow/Bolt
  │   └── HandLeft: Shield / Sub_Item
  ├── Head (local pos: -0.031, 0.316)
  │   ├── Hair (SR)
  │   ├── HelmetHair (SR)
  │   ├── Helmet (SR)
  │   ├── Beard (SR)
  │   └── Eye (parent)
  │       ├── Eye_Normal / Eye_Stun / Eye_Defeat2
  └── Chest (SR)
```

### PartsType 목록 (18개)
Eye, Hair, Helmet, Beard, Chest, Sword, Axe, Bow, Shield, Wand, Staff, Spear, Blunt, Crossbow, SubItem, Arrow, HelmetHair, Skin

---

## 4. 애니메이션 호환성 분석

### LayerLab _Controller.controller 보유 State

| State | 게임에서 필요 | 상태 |
|-------|-------------|------|
| Idle | `animator.Play("Idle")` | ✅ |
| Walk | `animator.Play("Walk")` | ✅ |
| Attack | `animator.Play("Attack")` | ✅ |
| Skill | `animator.Play(skillAnimation)` | ✅ |
| Dead1, Dead2, Dead3 | `animator.Play("Die")` | ❌ 이름 불일치 |
| (없음) | `AttackSpeed` float 파라미터 | ❌ 파라미터 없음 |
| Dance, Victory, Stun, Run, Roll, Jump 등 | 미사용 | 여분 |

### 해결 방법: LayerLab에 맞춰 게임 코드 수정

Animation Event 방식 → **코드 기반 타이밍으로 전환**

**현재 (Animation Event 의존)**:
```
Attack() → animator.Play("Attack")
  → [클립 이벤트] OnAttackHit()   ← 몬스터 데미지
  → [클립 이벤트] OnAttackDone()  ← 다음 공격 트리거
```

**변경 후 (Update 폴링)**:
```
Attack() → animator.Play("Attack")
  → Update에서 normalizedTime >= hitThreshold → OnAttackHit()
  → Update에서 normalizedTime >= 1.0f         → OnAttackDone()
```

`animator.GetCurrentAnimatorStateInfo(0).normalizedTime` 으로 진행률 읽음.

### _Controller.controller 수정 사항 (YAML)
1. "Die" State 추가 → Dead1.anim 연결
2. "AttackSpeed" float 파라미터 추가
3. Attack State에 SpeedParameterActive = 1 설정

---

## 5. 설계 결론

### 최종 Player.prefab 구조

```
ROOT "Player"
  └── Positioner
        └── Player body  (Player.cs 유지, Animator 제거, CostumeSetter 제거)
              ├── normalSkill, Attack2~4Skill  (그대로 유지)
              └── [CharacterMaker 프리팹] ← 자식으로 배치
                    ├── Animator (_Controller) ← 실제 애니메이션 담당
                    ├── PartsManager
                    ├── CharacterPrefabData   ← Start()에서 외형 자동 적용
                    └── Head, Hair, Eye, Weapon ... (비주얼)
```

**Animator를 CharacterMaker 루트에 유지해야 하는 이유**:  
LayerLab 애니메이션 클립들이 CharacterMaker 루트 기준 상대경로("Head", "Hair" 등)로  
SpriteRenderer를 참조함. Animator를 상위 GO로 옮기면 경로가 깨짐.

---

## 6. 캐릭터 생성 워크플로우

### 1단계: 에디터 작업 (개발 중 한 번)

```
① LayerLab CharacterMaker 씬에서 캐릭터 파츠/색상 선택
② "Save Prefab" → CharacterPrefabs/Character_YYYYMMDD_HHMMSS.prefab 생성
③ Player.prefab 열기
④ Player body 안 기존 비주얼 자식 삭제 (head, Hair, Body, Weapon 등)
⑤ Character_XXX.prefab → Player body 하위에 드래그 앤 드롭
⑥ Player body의 Animator 컴포넌트 제거
⑦ Player.cs 코드 수정 (아래 참조)
```

### 2단계: 게임 실행 시 (자동)

```
① 씬 로드 → Player.prefab Instantiate
② Player.cs Awake()
   - GetComponentInChildren<Animator>() → CharacterMaker Animator 발견
   - DamagePosition, Bubble, PetPositions, DashBar 참조 (기존 그대로)
③ CharacterPrefabData.Start() 자동 실행
   - PartsManager.Init()
   - ApplyPresetItem() → 저장된 파츠/색상/가시성 복원
   - 캐릭터 외형 화면에 표시
④ 전투 시작 → 기존 게임 로직 그대로 작동
```

---

## 7. 코드 수정 목록

### Player.cs (`Assets/Scripts/Fight/Units/Player.cs`)

```csharp
// [1] Awake() - Animator 참조 변경
// 현재:
_animator = transform.GetComponent<Animator>();
// 변경:
_animator = GetComponentInChildren<Animator>();

// [2] Awake() - CostumeSetter 제거
// 현재:
gameObject.GetOrAddComponent<CostumeSetter>();
// 변경: 이 줄 삭제

// [3] Die 애니메이션 이름 변경
// 현재:
_animator.Play("Die", 0, 0);
// 변경:
_animator.Play("Dead1", 0, 0);

// [4] OnAttackHit / OnAttackDone → Animation Event 제거, Update 폴링으로 전환
// Player.cs에 아래 필드 추가:
private bool _hitFired;
private const float HIT_NORMALIZED_TIME = 0.5f;  // Attack 클립의 히트 타이밍

// OnUpdate()에 추가 (PlayerState.Action 처리):
case PlayerState.Action:
    UpdateAttackEvents();
    break;

// 메서드 추가:
private void UpdateAttackEvents()
{
    var info = _animator.GetCurrentAnimatorStateInfo(0);
    if (!info.IsName("Attack")) return;
    float t = info.normalizedTime % 1f;
    if (!_hitFired && t >= HIT_NORMALIZED_TIME)
    {
        _hitFired = true;
        OnAttackHit();
    }
    if (t >= 0.95f)
    {
        _hitFired = false;
        OnAttackDone();
    }
}
```

### _Controller.controller (YAML 수정)
- "Die" State 추가 (Dead1.anim 참조)
- "AttackSpeed" float 파라미터 추가
- Attack State SpeedParameterActive = 1

---

## 8. 남은 작업 (다음 세션)

### 코드 수정 (Claude가 작업)
- [ ] `_Controller.controller` YAML 수정 (Die State + AttackSpeed 파라미터)
- [ ] `Player.cs` 수정 (Animator 참조, CostumeSetter 제거, Die 이름, OnAttack 폴링)

### 에디터 작업 (사용자가 Unity에서 직접)
- [ ] Player.prefab 열기 → 기존 비주얼 자식 삭제
- [ ] Character_20260615_220922.prefab → Player body 하위 배치
- [ ] Player body의 Animator 컴포넌트 제거
- [ ] 실행 테스트: Idle/Walk/Attack/Die 애니메이션 확인
- [ ] HIT_NORMALIZED_TIME 값 조정 (Attack.anim 보고 맞추기)

### 이후 Phase (별도 계획)
- [ ] 게임 안에서 파츠 선택 UI (PanelPartsControl 연동)
- [ ] 선택된 파츠 LocalSaveManager에 저장/복원
- [ ] DbUserEquip.BodyCostume / WeaponCostume 대체 데이터 설계

---

## 9. 관련 파일 경로

| 파일 | 경로 |
|------|------|
| Player.prefab | `Assets/Resources/Prefabs/Characters/Player.prefab` |
| Player.cs | `Assets/Scripts/Fight/Units/Player.cs` |
| CostumeSetter.cs | `Assets/Scripts/Costume/CostumeSetter.cs` |
| LayerLab _Controller | `Assets/Layer Lab/2D Minimal-CharacterMaker/Common/Animations/_Controller.controller` |
| 저장된 CharacterMaker 프리팹 | `Assets/CharacterPrefabs/Character_20260615_220922.prefab` |
| CharacterPrefabData.cs | `Assets/Layer Lab/.../Scripts/Data/CharacterPrefabData.cs` |
| PartsManager.cs | `Assets/Layer Lab/.../Scripts/Core/PartsManager.cs` |

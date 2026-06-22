# 에셋 교체 분석 문서

> 작성일: 2026-06-21  
> 대상: Player (Shinabro), Monster (2D SD Monster Pack)

---

## 목차

1. [Player → Shinabro 교체](#1-player--shinabro-교체)
2. [Monster → 2D SD Monster Pack 교체](#2-monster--2d-sd-monster-pack-교체)

---

## 1. Player → Shinabro 교체

### 1-1. Shinabro 에셋 특성

- **Spine 기반 캐릭터** (`Assets/Spine`에 Spine Unity Runtime 설치 확인됨)
- `SkeletonMecanim` 컴포넌트를 통해 Unity Animator와 브릿지
- 스킨 조합: `SimpleSpineSkinAssigner` (5개 슬롯별 스킨 직렬화 + 런타임 적용)

**프리팹 구조 (자식 없는 단일 오브젝트):**

```
Simple Spine Character Sample.prefab
  ├── MeshFilter + MeshRenderer     (Spine 렌더링)
  ├── Animator → Character_Controller.controller
  ├── SkeletonMecanim               (Spine↔Mecanim 브릿지)
  └── SimpleSpineSkinAssigner       (스킨 슬롯 직렬화 및 적용)
```

---

### 1-2. 스킨 슬롯 구조 (Character.json 파싱 확인)

`SimpleSpineSkinAssigner`의 5개 필드와 실제 스킨 목록:

| 슬롯 필드 | 스킨 카테고리 | 주요 내용 |
|---|---|---|
| `bodySkin` | `BODY/` | archer/cleric/knight/rogue/warrior/wizard × 5등급 × 2색 (60종) |
| `hairSkin` | `HAIR/` | hairA~B × a/b/c 타입 × 12색상 (78종) |
| `headSkin` | `HEAD/` | headA/B × 6종 (14종) |
| `rightHandWeaponSkin` | `RIGHTHAND/` | Bow, Dagger, Scepter, Spear_OneHand/TwoHand, Staff, Sword_OneHand/TwoHand × 5등급 |
| `leftHandWeaponSkin` | `LEFTHAND/` | Dagger, Shield_Large/Small, Spear_OneHand, Sword_OneHand × 5등급 |

---

### 1-3. 전체 애니메이션 목록 (Character.json 파싱 확인)

```
대기:     Wait1, Wait2, Wait3, Wait4
이동:     Walk1, Walk2, Walk3, Walk4
달리기:   Run1, Run2, Run3, Run4

근접공격: Attack_OneHand1, Attack_OneHand2
          Attack_TwoHand1, Attack_TwoHand2
          Attack_DualHand1, Attack_DualHand2

원거리:   Shoot1
마법시전: Spell1, Spell2
시전:     Cast1, Cast2

피격:     Damage1, Damage2
방어:     Guard1, Guard2
사망:     Die

생활:     Life_Cooking, Life_Crafting, Life_Gathering, Life_Mining  (전투 무관, 미사용)
```

---

### 1-4. 애니메이션 분기 설계

#### 분기 없음

| Player 상태 | 사용 애니메이션 | 비고 |
|---|---|---|
| Idle | `Wait1` | 고정 |
| Move | `Walk1` | 고정 |
| Die | `Die` | 단일 |

#### 분기 필요 — Attack

RIGHTHAND + LEFTHAND 조합 기준:

| RIGHTHAND | LEFTHAND | 애니메이션 |
|---|---|---|
| `Sword_TwoHand_*`, `Spear_TwoHand_*` | (없음) | `Attack_TwoHand1/2` |
| `Sword_OneHand_*`, `Dagger_*`, `Spear_OneHand_*` | 무기류 (Dagger/Sword/Spear) | `Attack_DualHand1/2` |
| `Sword_OneHand_*`, `Dagger_*`, `Spear_OneHand_*` | Shield 또는 없음 | `Attack_OneHand1/2` |
| `Bow_*` | (없음) | `Shoot1` |
| `Scepter_*`, `Staff_*` | (없음) | `Spell1/2` |

#### 분기 필요 — Skill

RIGHTHAND 무기 타입 기준:

| RIGHTHAND | 애니메이션 |
|---|---|
| `Bow_*` | `Shoot1` |
| `Scepter_*`, `Staff_*` | `Spell1/2` |
| 나머지 근접 전부 | `Cast1/2` |

#### `1/2` 변형 — 콤보 연타

`_1`과 `_2`가 있는 애니메이션은 Player.cs의 기존 `_attackCount`로 자연스럽게 교차:

```
1번째 타격 → _1
2번째 타격 → _2
3번째 타격 → _1
4번째 타격 → _2 → 이후 대시
```

`Spell`, `Cast`도 동일하게 1/2 교차 적용.

#### 분기 판별 방법

필요한 정보는 두 가지:
1. **무기 타입**: `rightHandWeaponSkin` 문자열 파싱 → `"RIGHTHAND/Sword_TwoHand_..."` → `TwoHand`
2. **왼손 상태**: `leftHandWeaponSkin`이 Shield인지 / 무기인지 / 비어있는지

→ 이 두 값을 읽어 반환하는 헬퍼 메서드 1개로 처리 가능.

---

### 1-5. 완료된 코드 수정 ✅

#### Player.cs

| 위치 | 변경 전 | 변경 후 |
|---|---|---|
| `Idle()`, `GameOver()` | `"Idle"` | `"Wait1"` |
| `Move()` | `"Walk"` | `"Walk1"` |
| `UpdateMoving()` (멈춤/이동) | `"Idle"` / `"Walk"` | `"Wait1"` / `"Walk1"` |
| `Attack()` | `"Attack"` | `GetAttackAnimation()` |
| `DoSkill()` | `"Skill"` | `GetSkillAnimation()` |
| `Die()` | `"Dead1"` | `"Die"` |
| `Dash()` | `"Roll"` | `"Run1"` |
| `UpdateAttackEvents()` | `IsName("Attack")` | `IsAttackAnimationName(info)` |
| `UpdateSkillEvents()` | `IsName("Skill")` | `IsSkillAnimationName(info)` |
| `IsLookingLeft` | `== Define.LookLeft` | `== Define.LookRight` |
| `LookAt()` | `lookLeft ? LookLeft : LookRight` | `lookLeft ? LookRight : LookLeft` |

> **LookAt 반전 이유**: Shinabro 기본 방향이 왼쪽이라 X 반전 로직이 기존과 반대.

**추가된 필드/메서드:**

```csharp
private SimpleSpineSkinAssigner _skinAssigner;  // Awake에서 캐싱
private bool _skillAnimAlt;                      // Skill 1/2 교차용 토글

// 무기 타입 기반 공격 애니메이션 분기
string GetAttackAnimation()   // Bow→Shoot1, Staff/Scepter→Spell, TwoHand/DualHand/OneHand 분기
string GetSkillAnimation()    // Bow→Shoot1, Staff/Scepter→Spell, 근접→Cast

// 상태명 일괄 체크
bool IsAttackAnimationName(AnimatorStateInfo info)
bool IsSkillAnimationName(AnimatorStateInfo info)
```

#### SimpleSpineSkinAssigner.cs

```csharp
// 1. AssignSkins() public으로 변경 (런타임 파츠 교체 API용)
void AssignSkins()  →  public void AssignSkins()

// 2. Start() 추가 — 플레이 모드에서 스킨 적용
// OnEnable()은 Application.isPlaying이면 return하므로 런타임 스킨 미적용 → 캐릭터 투명
void Start()
{
    if (!Application.isPlaying) return;
    InitializeSkeleton();
    AssignSkins();
}
```

#### Character_Controller.controller

- `AttackSpeed` float 파라미터 추가 (default 1)
- StateMachine에 상태 추가 (기존 Wait1 하나만 등록된 미완성 상태였음):
  Walk1, Run1, Die, Attack_OneHand1/2, Attack_TwoHand1/2, Attack_DualHand1/2, Shoot1, Spell1/2, Cast1/2
- Attack/Skill 계열 상태에 `SpeedParameter: AttackSpeed` 연결

#### 런타임 파츠 교체 API

```csharp
var assigner = GetComponentInChildren<SimpleSpineSkinAssigner>();
assigner.rightHandWeaponSkin = "RIGHTHAND/Sword_OneHand_Epic1";
assigner.AssignSkins();
```

---

### 1-6. 프리팹 제작 절차 (에디터) ✅ 완료

1. 임시 씬에 `Simple Spine Character Sample.prefab` 배치
2. Inspector → `SimpleSpineSkinAssigner`에서 5개 슬롯 드롭다운으로 선택  
   (`[ExecuteInEditMode]`라 뷰포트에서 실시간 확인 가능)
3. Hierarchy에서 오브젝트를 Project 창 폴더로 드래그 → **Prefab Variant** 선택 저장
4. `Player.prefab` 열기
5. Player body 자식의 기존 CharacterMaker 프리팹 제거
6. 새 Variant를 Player body 자식으로 배치
7. Player body에 Animator 컴포넌트 없는지 확인

> **스케일 주의**: Player body localScale이 0.1이므로 Shinabro Variant의 localScale을 (10,10,10)으로 설정해야 실제 크기 1.0.  
> **기본 방향 주의**: Shinabro는 기본 방향이 왼쪽 — LookAt 반전으로 코드에서 처리 완료, 에디터 작업 불필요.

---

### 1-7. 미결 사항

- [x] `Dash()` 처리 방법 — `"Run1"` 재생으로 결정

---

## 2. Monster → 2D SD Monster Pack 교체

### 2-1. SD Monster Pack 에셋 특성

- **Unity 2D Animation (SpriteLibrary / SpriteResolver) 기반**
- 각 부위가 개별 PNG 파일 (body1.png, eye1.png 등)
- `.anim` 클립이 SpriteResolver를 통해 각 부위 스프라이트를 스왑하며 애니메이션 구현
- 사용 가능한 몬스터 15종: `Bat, Beholder, Bigguy, Crow, Ghost, Goblin, Orc, Rat, Skeleton, Slime, Spider, Worm, Zombie, Cyclope, Demon`

---

### 2-2. 현재 게임 몬스터 시스템

**로딩 경로:**
```
DbMonster.Resource  →  Resources/Prefabs/Characters/<Resource>
```

**현재 존재하는 몬스터 프리팹:**
```
Resources/Prefabs/Characters/
  Slime1.prefab ~ Slime5.prefab
  Mushroom1.prefab ~ Mushroom4.prefab
  BossSlime.prefab
  BossSlime2.prefab
```

**현재 프리팹 구조 (Slime1.prefab 기준):**
```
[Root] "Slime1"         → Poolable, SortingGroup
  ├── [Child 0] "Slime" → Animator  ← Monster.cs가 여기 GetOrAddComponent로 추가됨
  │     └── Body, leg1~4, Wings, Face  (SpriteRenderer 자식들)
  ├── "Square"           → 그림자 SpriteRenderer
  ├── "DamagePosition"   → root.Find()로 참조됨
  ├── "TargetingPosition"→ root.Find()로 참조됨
  └── "HpBar"            → SpriteRenderer (체력바)
```

**Monster.cs 인스턴스화 코드:**
```csharp
// FieldManager.cs:389
Manager.Resource.Instantiate("Characters/" + monsterMeta.Resource, 5, MonsterParent.transform)
    .transform.GetChild(0).gameObject.GetOrAddComponent<Monster>()
// → Root를 인스턴스화하고, GetChild(0) = 첫 번째 자식에 Monster.cs 추가
```

**Monster.cs Awake:**
```csharp
root = transform.parent;                        // root = 프리팹 루트
animator = transform.GetComponent<Animator>(); // 자신(첫 번째 자식)의 Animator
targetingPosition = root.Find("TargetingPosition");
_damagePosition = root.Find("DamagePosition");
```

---

### 2-3. 애니메이션 상태명 불일치

| Monster.cs 호출 | 역할 | SD Pack 상태명 | 불일치 |
|---|---|---|---|
| `"Attack"` | 일반 공격 | `"attack"` | ❌ 대소문자 다름 |
| `"Attack2"` | 보스 변형 공격 | 없음 (`"attack-smash"`, `"attack-bow"` 존재) | ❌ 없음 |
| `"Walk"` | 이동 + Idle 모두 | `"walk"` (비행형은 `"fly"`) | ❌ 대소문자, 일부 없음 |
| `"Die"` | 사망 | `"die"` | ❌ 대소문자 다름 |

**몬스터별 상태명 (SD Pack 컨트롤러 기준):**

| 몬스터 | idle | attack | walk/fly | die | 비고 |
|---|---|---|---|---|---|
| Bat | `idle` | `attack` | `fly` ⚠️ | `die` | walk 없음 |
| Beholder | `idle` | `attack` | `fly` ⚠️ | `die` | walk 없음 |
| Bigguy | `idle` | `attack-bow`, `attack-smash` | `walk` | `die` | 공격 2종 |
| Crow | `idle` | `attack` | `fly` ⚠️ | `die` | walk 없음 |
| Ghost | `idle` | `attack-bow`, `attack-smash` | `walk` | `die` | |
| Goblin | `idle` | `attack-bow`, `attack-smash` | `walk` | `die` | |
| Rat | `idle` | `attack` | `walk` | `die` | |
| Skeleton | `idle` | `attack-bow`, `attack-stab` | `walk` | `die` | |
| Slime | `idle` | `attack` | *(없음)* ⚠️ | `die` | walk 없음 |
| Spider | `idle` | `attack` | `walk` | `die` | |
| Worm | `idle` | `attack` | `walk` | `die` | |
| Zombie | `idle` | `attack-bow`, `attack-smash` | `walk` | `die` | |

> ⚠️ 표시: `"walk"` 호출 시 애니메이션 누락 → idle 포즈로 멈춤

---

### 2-4. SD Monster Pack 프리팹 구조

```
[Root] "Slime"        → Transform만 (게임 컴포넌트 없음)
  └── "Slime"         → Animator + SpriteLibrary + SpriteResolver
        └── "Body"    → SpriteRenderer + SpriteResolver
              ├── "Bubble1" → SpriteRenderer + SpriteResolver
              ├── "Bubble2" → SpriteRenderer + SpriteResolver
              └── "Face"    → SpriteRenderer + SpriteResolver
```

**→ Downloads 폴더의 프리팹은 게임에 직접 사용 불가**  
`Poolable`, `DamagePosition`, `TargetingPosition`, `HpBar` 없음.

---

### 2-5. 수정이 필요한 코드 (Monster.cs)

```csharp
// [1] Attack() — 일반 공격
animator.Play("Attack", 0, 0)   →   animator.Play("attack", 0, 0)

// [2] Attack() — 보스 변형 공격 (Attack2 없으므로 동일 상태 사용)
animator.Play(isTwo ? "Attack2" : "Attack", 0, 0)   →   animator.Play("attack", 0, 0)

// [3] Idle() — Walk 재생 중
animator.Play("Walk", 0, 0)   →   animator.Play("walk", 0, 0)
// (Bat/Beholder/Crow/Slime은 walk 없음 → idle 재생으로 대체)

// [4] Move()
animator.Play("Walk", 0, 0)   →   animator.Play("walk", 0, 0)

// [5] Die()
animator.Play("Die")   →   animator.Play("die")
```

---

### 2-6. 에디터 작업 절차 (몬스터 1종당)

1. 현재 프리팹 (`Slime1.prefab` 등) 복사
2. 기존 비주얼 자식 제거
3. SD Pack 해당 몬스터 비주얼을 자식으로 배치 (SpriteLibrary 포함)
4. `DamagePosition`, `TargetingPosition`, `HpBar` 위치 조정
5. `Resources/Prefabs/Characters/` 안에 저장
6. DbMonster Excel `Resource` 컬럼 값 업데이트

---

### 2-7. 미결 사항

- [ ] 현재 몬스터 ↔ SD Pack 매핑표 작성
  ```
  Slime1~5   → ?
  Mushroom1~4 → ?
  BossSlime  → ?
  BossSlime2 → ?
  ```
- [ ] `walk` 없는 몬스터(Bat, Beholder, Crow, Slime) 처리 방법
- [ ] 보스 `Attack2` 처리 방법

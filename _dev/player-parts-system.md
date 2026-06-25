# 플레이어 파츠 교체 시스템

> 작성일: 2026-06-23  
> 최종 업데이트: 2026-06-24
> 상태: 설계 완료 / Player 애니메이션 분기용 스킨 읽기 구현 / 외부 제어 API 미구현

---

## 개요

Shinabro 캐릭터(Spine 기반)는 5개 슬롯을 독립적으로 교체할 수 있다.  
현재 `SimpleSpineSkinAssigner`는 Start/OnEnable/OnValidate에서 내부적으로 `AssignSkins()`를 호출한다.

런타임 중 다른 시스템이 파츠를 바꾸려면 아직 아래 두 API 노출 작업이 필요하다. 현재는 `Player.cs` 내부에서 공격/스킬 애니메이션 분기용으로만 스킨 문자열을 읽는다.

---

## 스킨 슬롯 목록

| 필드명 | 카테고리 | 예시 값 |
|---|---|---|
| `bodySkin` | `BODY/` | `"BODY/knight_rare1"` |
| `hairSkin` | `HAIR/` | `"HAIR/hairA_a3"` |
| `headSkin` | `HEAD/` | `"HEAD/headA2"` |
| `rightHandWeaponSkin` | `RIGHTHAND/` | `"RIGHTHAND/Sword_OneHand_Common1"` |
| `leftHandWeaponSkin` | `LEFTHAND/` | `"LEFTHAND/Shield_Small_Common1"` |

전체 스킨 목록: `Character.json` (404개) 또는 `Character.atlas.txt` 참조

---

## 구현 시 필요한 코드 변경 (2곳)

### 1. `Assets/Downloads/Shinabro/MiniFantasyCharacters/Scripts/SimpleSpineSkinAssigner.cs`
```csharp
// 변경 전
void AssignSkins()

// 변경 후
public void AssignSkins()
```

### 2. `Assets/Scripts/Fight/Units/Player.cs`
```csharp
// _skinAssigner 필드 아래에 추가
public SimpleSpineSkinAssigner SkinAssigner => _skinAssigner;
```

## 현재 실제 코드 상태

- `Player.cs`는 `_skinAssigner = GetComponentInChildren<SimpleSpineSkinAssigner>()`로 Shinabro 파츠 정보를 읽는다.
- 공격 애니메이션은 `rightHandWeaponSkin`/`leftHandWeaponSkin` 문자열로 분기한다.
  - Bow → `Shoot1`
  - Scepter/Staff → `Spell1`/`Spell2`
  - TwoHand → `Attack_TwoHand1`/`Attack_TwoHand2`
  - 왼손 무기(Dagger/Sword/Spear) → `Attack_DualHand1`/`Attack_DualHand2`
  - 그 외 → `Attack_OneHand1`/`Attack_OneHand2`
- 스킬 애니메이션은 Bow → `Shoot1`, Scepter/Staff → `Spell1`/`Spell2`, 그 외 → `Cast1`/`Cast2`.
- `SimpleSpineSkinAssigner.AssignSkins()`는 아직 private이라 외부 시스템에서 직접 호출할 수 없다.
- `Player.SkinAssigner` public getter는 아직 없다.
- `_dev/asset-migration-analysis.md`의 과거 완료 기록보다 이 문서의 상태가 현재 코드 기준이다.

---

## 사용법

```csharp
var s = Manager.Player.SkinAssigner;

// 바꿀 슬롯만 지정 (나머지 슬롯은 유지됨)
s.rightHandWeaponSkin = "RIGHTHAND/Sword_OneHand_Common1";
s.leftHandWeaponSkin  = "LEFTHAND/Shield_Small_Common1";

// 마지막에 한 번만 호출
s.AssignSkins();
```

위 예시는 API 노출 작업 후 사용 가능하다.

---

## 원칙

- 각 컨텐츠가 자기 슬롯만 책임진다. 다른 슬롯을 건드리지 않는다.
- `AssignSkins()`는 슬롯 값을 모두 세팅한 후 마지막에 한 번만 호출한다.
- 저장/복원은 각 컨텐츠가 자기 방식으로 해결한다. 이 메커니즘과 무관하다.
- 스킨명이 존재하지 않으면 경고 로그만 출력하고 해당 슬롯을 무시한다. 크래시 없음.
- `Manager.Player`가 null일 수 있는 타이밍(씬 전환 등)에는 null 체크 필요.

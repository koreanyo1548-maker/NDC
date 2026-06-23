# 플레이어 파츠 교체 시스템

> 작성일: 2026-06-23  
> 상태: 설계 완료 / 미구현

---

## 개요

Shinabro 캐릭터(Spine 기반)는 5개 슬롯을 독립적으로 교체할 수 있다.  
`SimpleSpineSkinAssigner.AssignSkins()`를 호출하는 것만으로 즉시 비주얼이 바뀐다.

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

### 1. `SimpleSpineSkinAssigner.cs:114`
```csharp
// 변경 전
void AssignSkins()

// 변경 후
public void AssignSkins()
```

### 2. `Player.cs`
```csharp
// _skinAssigner 필드 아래에 추가
public SimpleSpineSkinAssigner SkinAssigner => _skinAssigner;
```

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

---

## 원칙

- 각 컨텐츠가 자기 슬롯만 책임진다. 다른 슬롯을 건드리지 않는다.
- `AssignSkins()`는 슬롯 값을 모두 세팅한 후 마지막에 한 번만 호출한다.
- 저장/복원은 각 컨텐츠가 자기 방식으로 해결한다. 이 메커니즘과 무관하다.
- 스킨명이 존재하지 않으면 경고 로그만 출력하고 해당 슬롯을 무시한다. 크래시 없음.
- `Manager.Player`가 null일 수 있는 타이밍(씬 전환 등)에는 null 체크 필요.

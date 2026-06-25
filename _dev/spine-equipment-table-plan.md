# Spine Equipment Table Plan

> 작성일: 2026-06-25  
> 상태: 다음 세션 구현 기준으로 확정된 설계 메모

## 목표

기존 장비 아이콘 리소스는 유지하고, 플레이어 Spine 파츠 교체용 Skin 이름을 장비 테이블에 별도 컬럼으로 추가한다.

기존 구조:

```text
DbWeapon.Resource / DbAccessory.Resource
→ UI 아이콘 Sprite 로드
→ 기존 2D SpriteRenderer 외형 교체에도 사용되던 값
```

신규 구조:

```text
DbWeapon.Resource
→ UI 아이콘용 Sprite 이름

DbWeapon.SpineRightHandSkin
→ 플레이어 오른손 Spine Skin 이름

DbAccessory.Resource
→ UI 아이콘용 Sprite 이름

DbAccessory.SpineLeftHandSkin
→ 플레이어 왼손 Spine Skin 이름
```

`Resource`를 Spine Skin 이름으로 대체하지 않는다. UI 아이콘 로딩이 `Manager.Resource.Load<Sprite>()`와 SpriteAtlas 기준으로 동작하기 때문에, `Resource`를 `RIGHTHAND/...` 같은 Spine Skin 이름으로 바꾸면 인벤토리/보상/소환/성장/각성 UI가 깨질 가능성이 높다.

## 확정 사항

1. 무기 아이콘은 현재 `DbWeapon.Resource`를 계속 사용한다.
2. 무기 장착 외형은 신규 `DbWeapon.SpineRightHandSkin`을 사용한다.
3. 반지/장신구 아이콘은 현재 `DbAccessory.Resource`를 계속 사용한다.
4. 반지/장신구 장착 외형은 신규 `DbAccessory.SpineLeftHandSkin`을 사용한다.
5. 반지는 Spine 왼손 방패 파츠로 표현한다.
6. 테이블에는 Spine atlas region 경로가 아니라 SkeletonData Skin 이름을 넣는다.

## 올바른 Spine Skin 이름

`SimpleSpineSkinAssigner`는 내부에서 `Skeleton.Data.FindSkin(skinName)`을 사용한다. 따라서 테이블 값은 `Character.json`의 `skins[].name` 기준이어야 한다.

올바른 값:

```text
RIGHTHAND/Sword_OneHand_Common1
RIGHTHAND/Spear_TwoHand_Epic3
LEFTHAND/Shield_Small_Common1
LEFTHAND/Shield_Large_Legendary3
```

잘못 쓰기 쉬운 값:

```text
RIGHTHAND/Sword/Sword_OneHand_Common1
LEFTHAND/Shield/Shield_Small_Common1
```

위 값들은 atlas region 경로에 가까우며, `FindSkin()` 대상 Skin 이름이 아니다.

## 확인된 Shinabro 파츠 수량

`Assets/Downloads/Shinabro/MiniFantasyCharacters/Character.json` 기준:

```text
전체 Skin: 404개
오른손 RIGHTHAND: 160개
왼손 LEFTHAND: 90개
```

오른손 타입:

```text
Bow: 20
Dagger: 20
Scepter: 20
Spear_OneHand: 20
Spear_TwoHand: 20
Staff: 20
Sword_OneHand: 20
Sword_TwoHand: 20
```

왼손 타입:

```text
Dagger: 20
Shield_Large: 15
Shield_Small: 15
Spear_OneHand: 20
Sword_OneHand: 20
```

반지/장신구는 우선 `LEFTHAND/Shield_Small_*` 또는 `LEFTHAND/Shield_Large_*`로 매핑한다.

## 데이터 파이프라인

현재 장비 데이터 흐름:

```text
Assets/Resources/Excels/Equipment.xlsx
→ Assets/Resources/Excels/EDbEquipments.asset
→ DataManageMenu.Save()
→ Assets/Resources/ExcelGenerated/Weapon.bytes
→ Assets/Resources/ExcelGenerated/Accessory.bytes
→ DbWeapon / DbAccessory 런타임 로드
```

관련 코드:

```text
Assets/Scripts/Data/Editor/EDbEquipment/EDbWeapon.cs
Assets/Scripts/Data/Editor/EDbEquipment/EDbAccessory.cs
Assets/Scripts/Data/DbEquipment/DbWeapon.cs
Assets/Scripts/Data/DbEquipment/DbAccessory.cs
Assets/Scripts/Data/Editor/Excel/EDbEquipments.cs
Assets/Scripts/Data/Editor/DataManageMenu.cs
```

`DataManageMenu.Save<EDbWeapon, DbWeapon>()`는 JSON serialize/deserialize로 같은 이름의 필드를 복사한 뒤 `.bytes`로 저장한다. 따라서 editor 모델과 runtime 모델 양쪽에 동일한 컬럼/필드를 추가해야 한다.

## 필요한 코드 변경

### 1. Weapon 모델

`EDbWeapon`, `DbWeapon`에 필드 추가:

```csharp
public string SpineRightHandSkin;
```

`DbWeapon`에는 필요하면 getter 추가:

```csharp
public string GetSpineRightHandSkin()
{
    return SpineRightHandSkin;
}
```

### 2. Accessory 모델

`EDbAccessory`, `DbAccessory`에 필드 추가:

```csharp
public string SpineLeftHandSkin;
```

`DbAccessory`에는 필요하면 getter 추가:

```csharp
public string GetSpineLeftHandSkin()
{
    return SpineLeftHandSkin;
}
```

### 3. SimpleSpineSkinAssigner 외부 적용 API

현재 `AssignSkins()`는 private이다. 런타임 장착 변경에서 호출할 수 있도록 public으로 바꾼다.

```csharp
public void AssignSkins()
```

### 4. Player에서 SkinAssigner 접근

`Player`에 getter 추가:

```csharp
public SimpleSpineSkinAssigner SkinAssigner => _skinAssigner;
```

### 5. 신규 런타임 적용 컴포넌트

예상 이름:

```text
SpineEquipmentSetter
```

역할:

```text
Awake/Start:
  현재 EquipController.data.Weapon / Accessory 값으로 양손 Skin 적용

Weapon.ValueChanged:
  DbWeapon.Get(id).SpineRightHandSkin → rightHandWeaponSkin 적용

Accessory.ValueChanged:
  DbAccessory.Get(id).SpineLeftHandSkin → leftHandWeaponSkin 적용

마지막:
  AssignSkins() 1회 호출
```

장착 값 처리:

```text
Weapon == 0 또는 SpineRightHandSkin 비어 있음
→ 기본 오른손 무기 Skin 적용 권장

Accessory == 0 또는 SpineLeftHandSkin 비어 있음
→ 빈 문자열 권장
```

반지 미장착 상태에서 방패가 보이면 어색할 수 있으므로, 왼손은 기본적으로 빈 문자열이 더 자연스럽다.

## 적용 흐름

무기:

```text
EquipController.data.Weapon.Value
→ DbWeapon.Get(id).SpineRightHandSkin
→ Manager.Player.SkinAssigner.rightHandWeaponSkin
→ AssignSkins()
```

장신구/반지:

```text
EquipController.data.Accessory.Value
→ DbAccessory.Get(id).SpineLeftHandSkin
→ Manager.Player.SkinAssigner.leftHandWeaponSkin
→ AssignSkins()
```

아이콘:

```text
DbWeapon.Resource
→ SpriteAtlas Sprite

DbAccessory.Resource
→ SpriteAtlas Sprite
```

## 구현 전 체크

1. 현재 실제 런타임 `Characters/Player` 프리팹에 Shinabro Spine 오브젝트와 `SimpleSpineSkinAssigner`가 포함되어 있는지 확인한다.
2. 포함되어 있지 않으면 Player 프리팹 교체/자식 배치가 먼저 필요하다.
3. `Player.cs`는 이미 `GetComponentInChildren<SimpleSpineSkinAssigner>()`로 SkinAssigner를 캐싱하고, 오른손/왼손 Skin 문자열로 공격 애니메이션을 분기한다.
4. SkinAssigner가 null이면 외형 적용과 무기별 애니메이션 분기가 모두 기본값으로 돌아간다.

## 다음 세션 작업 순서 제안

1. `EDbWeapon`, `DbWeapon`, `EDbAccessory`, `DbAccessory`에 Spine Skin 필드 추가.
2. `Equipment.xlsx`에 `SpineRightHandSkin`, `SpineLeftHandSkin` 컬럼 추가.
3. 우선 테스트용으로 일부 무기/장신구 행만 Skin 값 입력.
4. Unity 데이터 생성 메뉴로 `Weapon.bytes`, `Accessory.bytes` 재생성.
5. `SimpleSpineSkinAssigner.AssignSkins()` public 전환.
6. `Player.SkinAssigner` getter 추가.
7. `SpineEquipmentSetter` 작성 및 Player 프리팹/런타임에 연결.
8. Play Mode에서 무기 장착 변경 시 오른손, 반지 장착 변경 시 왼손 방패가 바뀌는지 확인.

## 가능성 평가

테이블 분리 방식:

```text
가능성: 8 / 10
```

이유:

- Shinabro Spine Skin 구조가 이미 오른손/왼손 교체에 적합하다.
- 테이블 파이프라인은 필드 추가에 대응하기 쉽다.
- 아이콘과 외형 Skin을 분리하면 기존 UI 영향이 작다.

감점 요인:

- 기존 35개 무기와 Shinabro Skin 사이에 자동 1:1 변환 규칙은 없다.
- `Scythe` 타입 Skin은 Shinabro 기본 파츠에 없다.
- 무기/반지별 매핑은 기획/아트 기준으로 테이블에 직접 넣어야 한다.
- Player 프리팹에 Shinabro Spine 오브젝트가 실제로 들어가 있어야 한다.

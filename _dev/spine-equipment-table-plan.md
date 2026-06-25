# Spine Equipment Table Plan

> 작성일: 2026-06-25
> 최신화: 2026-06-25
> 상태: 1차 구현 완료, Unity batchmode 컴파일/import 점검 완료

## 목표

기존 장비 아이콘 리소스는 그대로 유지하고, 플레이어 Spine 장비 외형에 사용할 Skin 이름을 장비 테이블의 별도 컬럼으로 분리한다.

기존 구조:

```text
DbWeapon.Resource / DbAccessory.Resource
-> UI 아이콘 Sprite 로드
-> 기존 2D SpriteRenderer 장비 외형 교체에도 함께 사용
```

신규 구조:

```text
DbWeapon.Resource
-> UI 아이콘용 Sprite 이름

DbWeapon.SpineRightHandSkin
-> 플레이어 오른손 Spine Skin 이름

DbAccessory.Resource
-> UI 아이콘용 Sprite 이름

DbAccessory.SpineLeftHandSkin
-> 플레이어 왼손 Spine Skin 이름
```

`Resource`를 Spine Skin 이름으로 바꾸지 않는다. `Resource`는 `Manager.Resource.Load<Sprite>()`와 SpriteAtlas 기반 UI에서 계속 사용되므로, 이 값을 `RIGHTHAND/...` 같은 Spine Skin 이름으로 바꾸면 인벤토리, 보상, 소환, 상점, 각성 UI가 깨질 수 있다.

## 완료된 작업

### 데이터 모델

다음 editor/runtime 모델에 Spine Skin 컬럼을 추가했다.

```text
Assets/Scripts/Data/Editor/EDbEquipment/EDbWeapon.cs
-> public string SpineRightHandSkin;

Assets/Scripts/Data/DbEquipment/DbWeapon.cs
-> public string SpineRightHandSkin;
-> public string GetSpineRightHandSkin()

Assets/Scripts/Data/Editor/EDbEquipment/EDbAccessory.cs
-> public string SpineLeftHandSkin;

Assets/Scripts/Data/DbEquipment/DbAccessory.cs
-> public string SpineLeftHandSkin;
-> public string GetSpineLeftHandSkin()
```

### 엑셀 및 생성 데이터

`Assets/Resources/Excels/Equipment.xlsx`에 다음 컬럼을 추가했다.

```text
Weapon 시트
Resource 바로 뒤 -> SpineRightHandSkin

Accessory 시트
Resource 바로 뒤 -> SpineLeftHandSkin
```

Unity import 및 데이터 생성 결과:

```text
Assets/Resources/Excels/EDbEquipments.asset
Assets/Resources/ExcelGenerated/Weapon.bytes
Assets/Resources/ExcelGenerated/Accessory.bytes
```

위 파일들에 새 컬럼과 실제 Skin 값이 반영되어 있다.

### Spine 적용 코드

`SimpleSpineSkinAssigner.AssignSkins()`를 외부에서 호출할 수 있게 public으로 변경했다.

```csharp
public void AssignSkins()
```

`Player`에서 SkinAssigner를 외부 컴포넌트가 접근할 수 있도록 getter를 추가했다.

```csharp
public SimpleSpineSkinAssigner SkinAssigner => _skinAssigner;
```

신규 컴포넌트:

```text
Assets/Scripts/Costume/SpineEquipmentSetter.cs
```

역할:

```text
Awake:
  Player.SkinAssigner 또는 GetComponentInChildren<SimpleSpineSkinAssigner>()로 SkinAssigner 확보
  현재 장착 무기/악세서리 값을 기반으로 Skin 적용
  Weapon, Accessory, WeaponCostume 변경 이벤트 구독

Weapon 변경:
  EquipController.data.Weapon.Value
  -> DbWeapon.Get(id).GetSpineRightHandSkin()
  -> SimpleSpineSkinAssigner.rightHandWeaponSkin
  -> AssignSkins()

Accessory 변경:
  EquipController.data.Accessory.Value
  -> DbAccessory.Get(id).GetSpineLeftHandSkin()
  -> SimpleSpineSkinAssigner.leftHandWeaponSkin
  -> AssignSkins()

WeaponCostume 변경:
  WeaponCostume이 장착되어 있으면 오른손 장비 Skin을 빈 문자열로 처리
  이후 Spine 코스튬 Skin 컬럼이 생기면 같은 위치에서 코스튬 Skin을 우선 적용
```

런타임 연결:

```text
Assets/Scripts/Scenes/FieldScene.cs

Manager.Resource.Instantiate("Characters/Player")
-> transform.GetChild(0).GetChild(0).gameObject
-> GetOrAddComponent<Player>()
-> GetOrAddComponent<SpineEquipmentSetter>()
-> Player.Spawn(...)
```

## 현재 연결 구조

전체 데이터/런타임 흐름:

```text
Equipment.xlsx
-> EDbEquipments.asset
-> DataManageMenu.Save<EDbWeapon, DbWeapon>()
-> Weapon.bytes
-> DbWeapon.Load()
-> DbWeapon.Get(id).SpineRightHandSkin
-> SpineEquipmentSetter.GetRightHandSkin()
-> Player.SkinAssigner.rightHandWeaponSkin
-> SimpleSpineSkinAssigner.AssignSkins()
-> Skeleton.Data.FindSkin(skinName)
-> Skeleton.SetSkin(combinedSkin)
```

```text
Equipment.xlsx
-> EDbEquipments.asset
-> DataManageMenu.Save<EDbAccessory, DbAccessory>()
-> Accessory.bytes
-> DbAccessory.Load()
-> DbAccessory.Get(id).SpineLeftHandSkin
-> SpineEquipmentSetter.GetLeftHandSkin()
-> Player.SkinAssigner.leftHandWeaponSkin
-> SimpleSpineSkinAssigner.AssignSkins()
-> Skeleton.Data.FindSkin(skinName)
-> Skeleton.SetSkin(combinedSkin)
```

프리팹 연결:

```text
Assets/Resources/Prefabs/Characters/Player.prefab
-> nested prefab:
   Assets/Downloads/Shinabro/MiniFantasyCharacters/Prefabs/Simple Spine Character Sample Variant 3 1.prefab
-> base prefab:
   Assets/Downloads/Shinabro/MiniFantasyCharacters/Prefabs/Simple Spine Character Sample.prefab
-> SimpleSpineSkinAssigner
-> Character_SkeletonData.asset
```

`Player.cs`는 프리팹에 직접 붙어 있지 않고 `FieldScene`에서 런타임에 `GetOrAddComponent<Player>()`로 붙는다. 따라서 `SpineEquipmentSetter`도 같은 런타임 GameObject에 붙도록 연결했다.

## 장착/코스튬 우선순위

현재 우선순위는 다음과 같다.

```text
오른손:
  WeaponCostume 장착됨
  -> 오른손 장비 Skin 숨김
  -> 현재는 빈 문자열
  -> 추후 Spine 코스튬 Skin 컬럼 추가 시 코스튬 Skin 우선 적용

  WeaponCostume 없음
  -> DbWeapon.SpineRightHandSkin 사용
  -> 값이 없거나 Weapon == 0이면 prefab 기본 오른손 Skin 유지

왼손:
  현재는 코스튬 컬럼 없음
  -> DbAccessory.SpineLeftHandSkin 사용
  -> 값이 없거나 Accessory == 0이면 빈 문자열
```

이 구조는 추후 코스튬도 무기/방패와 같은 슬롯 단위로 처리할 수 있게 만든 것이다. 동일 위치에 코스튬이 있으면 코스튬을 노출하고, 장착 장비는 숨기는 방식으로 확장하면 된다.

## 현재 매핑 규칙

Shinabro Spine에 Scythe 전용 Skin은 없다. 그래서 기존 35개 낫 무기는 우선 양손 무기 계열로 대체 매핑했다.

무기:

```text
홀수 Id -> RIGHTHAND/Sword_TwoHand_*
짝수 Id -> RIGHTHAND/Spear_TwoHand_*
```

악세서리:

```text
홀수 Id -> LEFTHAND/Shield_Small_*
짝수 Id -> LEFTHAND/Shield_Large_*
```

등급 변환:

```text
Normal    -> Common
Magic     -> Uncommon
Rare      -> Rare
Unique    -> Epic
Heroic    -> Epic
Legendary -> Legendary
Mythic    -> Legendary
```

번호 변환:

```text
오른손 무기 계열은 1~4 순환
왼손 방패 계열은 1~3 순환
```

## 실제 입력된 Skin 예시

무기:

```text
Id 1  -> RIGHTHAND/Sword_TwoHand_Common1
Id 2  -> RIGHTHAND/Spear_TwoHand_Common2
Id 6  -> RIGHTHAND/Spear_TwoHand_Uncommon1
Id 16 -> RIGHTHAND/Spear_TwoHand_Epic1
Id 26 -> RIGHTHAND/Spear_TwoHand_Legendary1
Id 35 -> RIGHTHAND/Sword_TwoHand_Legendary1
```

악세서리:

```text
Id 1  -> LEFTHAND/Shield_Small_Common1
Id 2  -> LEFTHAND/Shield_Large_Common2
Id 6  -> LEFTHAND/Shield_Large_Uncommon1
Id 16 -> LEFTHAND/Shield_Large_Epic1
Id 26 -> LEFTHAND/Shield_Large_Legendary1
Id 35 -> LEFTHAND/Shield_Small_Legendary2
```

## 사용 가능한 Spine Skin 목록

기준 파일:

```text
Assets/Downloads/Shinabro/MiniFantasyCharacters/Character.json
```

주의:

```text
아래 값은 SkeletonData Skin 이름이다.
atlas region 경로가 아니며, SimpleSpineSkinAssigner는 Skeleton.Data.FindSkin(skinName)으로 찾는다.
```

### 오른손 RIGHTHAND Skin

| 계열 | 개수 | 설명 | 사용 가능한 Skin |
|---|---:|---|---|
| Bow | 20 | 활. 공격 애니메이션은 Player.cs에서 Shoot1 계열로 분기 가능하다. | RIGHTHAND/Bow_Common1, RIGHTHAND/Bow_Common2, RIGHTHAND/Bow_Common3, RIGHTHAND/Bow_Common4, RIGHTHAND/Bow_Uncommon1, RIGHTHAND/Bow_Uncommon2, RIGHTHAND/Bow_Uncommon3, RIGHTHAND/Bow_Uncommon4, RIGHTHAND/Bow_Rare1, RIGHTHAND/Bow_Rare2, RIGHTHAND/Bow_Rare3, RIGHTHAND/Bow_Rare4, RIGHTHAND/Bow_Epic1, RIGHTHAND/Bow_Epic2, RIGHTHAND/Bow_Epic3, RIGHTHAND/Bow_Epic4, RIGHTHAND/Bow_Legendary1, RIGHTHAND/Bow_Legendary2, RIGHTHAND/Bow_Legendary3, RIGHTHAND/Bow_Legendary4 |
| Dagger | 20 | 단검. 왼손 단검과 조합하면 양손/쌍수 느낌으로 확장 가능하다. | RIGHTHAND/Dagger_Common1, RIGHTHAND/Dagger_Common2, RIGHTHAND/Dagger_Common3, RIGHTHAND/Dagger_Common4, RIGHTHAND/Dagger_Uncommon1, RIGHTHAND/Dagger_Uncommon2, RIGHTHAND/Dagger_Uncommon3, RIGHTHAND/Dagger_Uncommon4, RIGHTHAND/Dagger_Rare1, RIGHTHAND/Dagger_Rare2, RIGHTHAND/Dagger_Rare3, RIGHTHAND/Dagger_Rare4, RIGHTHAND/Dagger_Epic1, RIGHTHAND/Dagger_Epic2, RIGHTHAND/Dagger_Epic3, RIGHTHAND/Dagger_Epic4, RIGHTHAND/Dagger_Legendary1, RIGHTHAND/Dagger_Legendary2, RIGHTHAND/Dagger_Legendary3, RIGHTHAND/Dagger_Legendary4 |
| Scepter | 20 | 셉터/완드. Player.cs에서 Spell 계열 공격으로 분기 가능하다. | RIGHTHAND/Scepter_Common1, RIGHTHAND/Scepter_Common2, RIGHTHAND/Scepter_Common3, RIGHTHAND/Scepter_Common4, RIGHTHAND/Scepter_Uncommon1, RIGHTHAND/Scepter_Uncommon2, RIGHTHAND/Scepter_Uncommon3, RIGHTHAND/Scepter_Uncommon4, RIGHTHAND/Scepter_Rare1, RIGHTHAND/Scepter_Rare2, RIGHTHAND/Scepter_Rare3, RIGHTHAND/Scepter_Rare4, RIGHTHAND/Scepter_Epic1, RIGHTHAND/Scepter_Epic2, RIGHTHAND/Scepter_Epic3, RIGHTHAND/Scepter_Epic4, RIGHTHAND/Scepter_Legendary1, RIGHTHAND/Scepter_Legendary2, RIGHTHAND/Scepter_Legendary3, RIGHTHAND/Scepter_Legendary4 |
| Spear_OneHand | 20 | 한손 창. 방패나 왼손 무기와 조합하기 좋다. | RIGHTHAND/Spear_OneHand_Common1, RIGHTHAND/Spear_OneHand_Common2, RIGHTHAND/Spear_OneHand_Common3, RIGHTHAND/Spear_OneHand_Common4, RIGHTHAND/Spear_OneHand_Uncommon1, RIGHTHAND/Spear_OneHand_Uncommon2, RIGHTHAND/Spear_OneHand_Uncommon3, RIGHTHAND/Spear_OneHand_Uncommon4, RIGHTHAND/Spear_OneHand_Rare1, RIGHTHAND/Spear_OneHand_Rare2, RIGHTHAND/Spear_OneHand_Rare3, RIGHTHAND/Spear_OneHand_Rare4, RIGHTHAND/Spear_OneHand_Epic1, RIGHTHAND/Spear_OneHand_Epic2, RIGHTHAND/Spear_OneHand_Epic3, RIGHTHAND/Spear_OneHand_Epic4, RIGHTHAND/Spear_OneHand_Legendary1, RIGHTHAND/Spear_OneHand_Legendary2, RIGHTHAND/Spear_OneHand_Legendary3, RIGHTHAND/Spear_OneHand_Legendary4 |
| Spear_TwoHand | 20 | 양손 창. 현재 낫 대체 Skin으로 일부 사용 중이다. | RIGHTHAND/Spear_TwoHand_Common1, RIGHTHAND/Spear_TwoHand_Common2, RIGHTHAND/Spear_TwoHand_Common3, RIGHTHAND/Spear_TwoHand_Common4, RIGHTHAND/Spear_TwoHand_Uncommon1, RIGHTHAND/Spear_TwoHand_Uncommon2, RIGHTHAND/Spear_TwoHand_Uncommon3, RIGHTHAND/Spear_TwoHand_Uncommon4, RIGHTHAND/Spear_TwoHand_Rare1, RIGHTHAND/Spear_TwoHand_Rare2, RIGHTHAND/Spear_TwoHand_Rare3, RIGHTHAND/Spear_TwoHand_Rare4, RIGHTHAND/Spear_TwoHand_Epic1, RIGHTHAND/Spear_TwoHand_Epic2, RIGHTHAND/Spear_TwoHand_Epic3, RIGHTHAND/Spear_TwoHand_Epic4, RIGHTHAND/Spear_TwoHand_Legendary1, RIGHTHAND/Spear_TwoHand_Legendary2, RIGHTHAND/Spear_TwoHand_Legendary3, RIGHTHAND/Spear_TwoHand_Legendary4 |
| Staff | 20 | 지팡이. Player.cs에서 Spell 계열 공격으로 분기 가능하다. | RIGHTHAND/Staff_Common1, RIGHTHAND/Staff_Common2, RIGHTHAND/Staff_Common3, RIGHTHAND/Staff_Common4, RIGHTHAND/Staff_Uncommon1, RIGHTHAND/Staff_Uncommon2, RIGHTHAND/Staff_Uncommon3, RIGHTHAND/Staff_Uncommon4, RIGHTHAND/Staff_Rare1, RIGHTHAND/Staff_Rare2, RIGHTHAND/Staff_Rare3, RIGHTHAND/Staff_Rare4, RIGHTHAND/Staff_Epic1, RIGHTHAND/Staff_Epic2, RIGHTHAND/Staff_Epic3, RIGHTHAND/Staff_Epic4, RIGHTHAND/Staff_Legendary1, RIGHTHAND/Staff_Legendary2, RIGHTHAND/Staff_Legendary3, RIGHTHAND/Staff_Legendary4 |
| Sword_OneHand | 20 | 한손 검. 방패나 왼손 무기와 조합하기 좋다. | RIGHTHAND/Sword_OneHand_Common1, RIGHTHAND/Sword_OneHand_Common2, RIGHTHAND/Sword_OneHand_Common3, RIGHTHAND/Sword_OneHand_Common4, RIGHTHAND/Sword_OneHand_Uncommon1, RIGHTHAND/Sword_OneHand_Uncommon2, RIGHTHAND/Sword_OneHand_Uncommon3, RIGHTHAND/Sword_OneHand_Uncommon4, RIGHTHAND/Sword_OneHand_Rare1, RIGHTHAND/Sword_OneHand_Rare2, RIGHTHAND/Sword_OneHand_Rare3, RIGHTHAND/Sword_OneHand_Rare4, RIGHTHAND/Sword_OneHand_Epic1, RIGHTHAND/Sword_OneHand_Epic2, RIGHTHAND/Sword_OneHand_Epic3, RIGHTHAND/Sword_OneHand_Epic4, RIGHTHAND/Sword_OneHand_Legendary1, RIGHTHAND/Sword_OneHand_Legendary2, RIGHTHAND/Sword_OneHand_Legendary3, RIGHTHAND/Sword_OneHand_Legendary4 |
| Sword_TwoHand | 20 | 양손 검. 현재 낫 대체 Skin으로 일부 사용 중이다. | RIGHTHAND/Sword_TwoHand_Common1, RIGHTHAND/Sword_TwoHand_Common2, RIGHTHAND/Sword_TwoHand_Common3, RIGHTHAND/Sword_TwoHand_Common4, RIGHTHAND/Sword_TwoHand_Uncommon1, RIGHTHAND/Sword_TwoHand_Uncommon2, RIGHTHAND/Sword_TwoHand_Uncommon3, RIGHTHAND/Sword_TwoHand_Uncommon4, RIGHTHAND/Sword_TwoHand_Rare1, RIGHTHAND/Sword_TwoHand_Rare2, RIGHTHAND/Sword_TwoHand_Rare3, RIGHTHAND/Sword_TwoHand_Rare4, RIGHTHAND/Sword_TwoHand_Epic1, RIGHTHAND/Sword_TwoHand_Epic2, RIGHTHAND/Sword_TwoHand_Epic3, RIGHTHAND/Sword_TwoHand_Epic4, RIGHTHAND/Sword_TwoHand_Legendary1, RIGHTHAND/Sword_TwoHand_Legendary2, RIGHTHAND/Sword_TwoHand_Legendary3, RIGHTHAND/Sword_TwoHand_Legendary4 |

### 왼손 LEFTHAND Skin

| 계열 | 개수 | 설명 | 사용 가능한 Skin |
|---|---:|---|---|
| Dagger | 20 | 왼손 단검. 오른손 단검/검/창과 쌍수 연출 가능하다. | LEFTHAND/Dagger_Common1, LEFTHAND/Dagger_Common2, LEFTHAND/Dagger_Common3, LEFTHAND/Dagger_Common4, LEFTHAND/Dagger_Uncommon1, LEFTHAND/Dagger_Uncommon2, LEFTHAND/Dagger_Uncommon3, LEFTHAND/Dagger_Uncommon4, LEFTHAND/Dagger_Rare1, LEFTHAND/Dagger_Rare2, LEFTHAND/Dagger_Rare3, LEFTHAND/Dagger_Rare4, LEFTHAND/Dagger_Epic1, LEFTHAND/Dagger_Epic2, LEFTHAND/Dagger_Epic3, LEFTHAND/Dagger_Epic4, LEFTHAND/Dagger_Legendary1, LEFTHAND/Dagger_Legendary2, LEFTHAND/Dagger_Legendary3, LEFTHAND/Dagger_Legendary4 |
| Shield_Large | 15 | 큰 방패. 현재 악세서리/반지 대체 Skin으로 일부 사용 중이다. | LEFTHAND/Shield_Large_Common1, LEFTHAND/Shield_Large_Common2, LEFTHAND/Shield_Large_Common3, LEFTHAND/Shield_Large_Uncommon1, LEFTHAND/Shield_Large_Uncommon2, LEFTHAND/Shield_Large_Uncommon3, LEFTHAND/Shield_Large_Rare1, LEFTHAND/Shield_Large_Rare2, LEFTHAND/Shield_Large_Rare3, LEFTHAND/Shield_Large_Epic1, LEFTHAND/Shield_Large_Epic2, LEFTHAND/Shield_Large_Epic3, LEFTHAND/Shield_Large_Legendary1, LEFTHAND/Shield_Large_Legendary2, LEFTHAND/Shield_Large_Legendary3 |
| Shield_Small | 15 | 작은 방패. 현재 악세서리/반지 대체 Skin으로 일부 사용 중이다. | LEFTHAND/Shield_Small_Common1, LEFTHAND/Shield_Small_Common2, LEFTHAND/Shield_Small_Common3, LEFTHAND/Shield_Small_Uncommon1, LEFTHAND/Shield_Small_Uncommon2, LEFTHAND/Shield_Small_Uncommon3, LEFTHAND/Shield_Small_Rare1, LEFTHAND/Shield_Small_Rare2, LEFTHAND/Shield_Small_Rare3, LEFTHAND/Shield_Small_Epic1, LEFTHAND/Shield_Small_Epic2, LEFTHAND/Shield_Small_Epic3, LEFTHAND/Shield_Small_Legendary1, LEFTHAND/Shield_Small_Legendary2, LEFTHAND/Shield_Small_Legendary3 |
| Spear_OneHand | 20 | 왼손 한손 창. 쌍수 창 연출 가능하다. | LEFTHAND/Spear_OneHand_Common1, LEFTHAND/Spear_OneHand_Common2, LEFTHAND/Spear_OneHand_Common3, LEFTHAND/Spear_OneHand_Common4, LEFTHAND/Spear_OneHand_Uncommon1, LEFTHAND/Spear_OneHand_Uncommon2, LEFTHAND/Spear_OneHand_Uncommon3, LEFTHAND/Spear_OneHand_Uncommon4, LEFTHAND/Spear_OneHand_Rare1, LEFTHAND/Spear_OneHand_Rare2, LEFTHAND/Spear_OneHand_Rare3, LEFTHAND/Spear_OneHand_Rare4, LEFTHAND/Spear_OneHand_Epic1, LEFTHAND/Spear_OneHand_Epic2, LEFTHAND/Spear_OneHand_Epic3, LEFTHAND/Spear_OneHand_Epic4, LEFTHAND/Spear_OneHand_Legendary1, LEFTHAND/Spear_OneHand_Legendary2, LEFTHAND/Spear_OneHand_Legendary3, LEFTHAND/Spear_OneHand_Legendary4 |
| Sword_OneHand | 20 | 왼손 한손 검. 쌍수 검 연출 가능하다. | LEFTHAND/Sword_OneHand_Common1, LEFTHAND/Sword_OneHand_Common2, LEFTHAND/Sword_OneHand_Common3, LEFTHAND/Sword_OneHand_Common4, LEFTHAND/Sword_OneHand_Uncommon1, LEFTHAND/Sword_OneHand_Uncommon2, LEFTHAND/Sword_OneHand_Uncommon3, LEFTHAND/Sword_OneHand_Uncommon4, LEFTHAND/Sword_OneHand_Rare1, LEFTHAND/Sword_OneHand_Rare2, LEFTHAND/Sword_OneHand_Rare3, LEFTHAND/Sword_OneHand_Rare4, LEFTHAND/Sword_OneHand_Epic1, LEFTHAND/Sword_OneHand_Epic2, LEFTHAND/Sword_OneHand_Epic3, LEFTHAND/Sword_OneHand_Epic4, LEFTHAND/Sword_OneHand_Legendary1, LEFTHAND/Sword_OneHand_Legendary2, LEFTHAND/Sword_OneHand_Legendary3, LEFTHAND/Sword_OneHand_Legendary4 |

## 검증 결과

완료한 검증:

```text
Equipment.xlsx:
  Weapon 35행 모두 SpineRightHandSkin 값 존재
  Accessory 35행 모두 SpineLeftHandSkin 값 존재
  모든 값이 Character.json skins[].name에 존재

EDbEquipments.asset:
  SpineRightHandSkin / SpineLeftHandSkin 값 반영 확인

Weapon.bytes:
  SpineRightHandSkin 필드명 포함 확인
  RIGHTHAND/Sword_TwoHand_Common1 포함 확인
  RIGHTHAND/Spear_TwoHand_Legendary4 포함 확인

Accessory.bytes:
  SpineLeftHandSkin 필드명 포함 확인
  LEFTHAND/Shield_Small_Common1 포함 확인
  LEFTHAND/Shield_Large_Legendary1 포함 확인

Unity batchmode:
  C# 컴파일 에러 없음
  Excel import 실패 없음

git diff --check:
  공백 오류 없음
```

참고:

```text
Unity batchmode 로그에 named pipe 종료 관련 IOException이 한 번 출력될 수 있다.
이는 에디터 종료 과정의 내부 로그이며, CS 컴파일 에러나 Excel import 실패는 아니다.
```

## 남은 작업

1. Play Mode에서 실제 장착 변경 시 오른손/왼손 외형이 즉시 바뀌는지 육안 확인.
2. 기존 2D SpriteRenderer 장비/코스튬 표시와 Spine 장비 표시가 동시에 보이는지 확인.
3. Spine 코스튬 Skin 컬럼이 생기면 `SpineEquipmentSetter.GetRightHandCostumeSkin()` 및 왼손/몸통/머리 슬롯 우선순위 확장.
4. 현재 임시 매핑은 기획/아트 기준이 아니다. 실제 장비별 외형이 확정되면 `Equipment.xlsx` 값을 직접 조정해야 한다.

## 리스크

- Shinabro Skin에는 Scythe 전용 Skin이 없다. 현재 낫은 `Sword_TwoHand`와 `Spear_TwoHand`로 대체했다.
- 기존 35개 장비와 Shinabro Skin은 자동 1:1 매핑 규칙이 없다. 현재 값은 사용 가능한 Skin으로 채운 1차 매핑이다.
- `WeaponCostume`이 장착되면 현재 오른손 Spine 장비 Skin을 숨긴다. Spine 코스튬 Skin 자체는 아직 테이블에 없으므로, 코스튬의 Spine 외형은 다음 단계에서 연결해야 한다.

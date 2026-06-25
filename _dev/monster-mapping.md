# 몬스터 매핑표

> 작성일: 2026-06-22  
> 최종 업데이트: 2026-06-24
> 상태: 매핑 확정 / 컨트롤러 상태명 보정 완료 / 런타임 파츠 정렬 보정 완료 / 프리팹 배치와 플레이 검증 대기

---

## 1. 확정 매핑

| ID | 기존 이름 | SD Pack 매핑 | 월드 | 비고 |
|----|-----------|------------|------|------|
| 1 | Slime1 | Rat | 슬라임 | 일반 A (스테이지 1~9) |
| 2 | Slime2 | Spider | 슬라임 | 일반 B (스테이지 1~9) |
| 3 | Slime3 | Rat | 슬라임 | 일반 A (스테이지 11~19) |
| 4 | Slime4 | Spider | 슬라임 | 일반 B (스테이지 11~19) |
| 5 | Slime5 | Rat | 슬라임 | 스테이지 미사용 |
| 6 | BossSlime | Worm | 슬라임 | 보스 1차 (스테이지 10) |
| 28 | BossSlime2 | Ghost | 슬라임 | 보스 2차 (스테이지 20) |
| 7 | Mushroom1 | Bat | 버섯 | 일반 A (스테이지 22~29) |
| 8 | Mushroom2 | Crow | 버섯 | 일반 B (스테이지 22~29) |
| 9 | Mushroom3 | Bat | 버섯 | 일반 A (스테이지 32~39) |
| 10 | Mushroom4 | Crow | 버섯 | 일반 B (스테이지 32~39) |
| 11 | BossSpider | Beholder | 버섯 | 보스 1차 (스테이지 30) |
| 29 | BossSpider2 | Worm | 버섯 | 보스 2차 (스테이지 40) |
| 16 | Radish1 | Goblin | 무 | 일반 A (스테이지 42~49) |
| 17 | Radish2 | Skeleton | 무 | 일반 B (스테이지 42~49) |
| 18 | Radish3 | Goblin | 무 | 일반 A (스테이지 52~59) |
| 19 | Radish4 | Skeleton | 무 | 일반 B (스테이지 52~59) |
| 22 | BossRadish | Ghost | 무 | 보스 1차 (스테이지 50) |
| 27 | BossRadish2 | Beholder | 무 | 보스 2차 (스테이지 60) |
| 12 | Frog1 | Zombie | 개구리 | 일반 A (스테이지 62~69) |
| 14 | StandFrog1 | Rat | 개구리 | 일반 B (스테이지 62~69) |
| 13 | Frog2 | Zombie | 개구리 | 일반 A (스테이지 72~79) |
| 15 | StandFrog2 | Rat | 개구리 | 일반 B (스테이지 72~79) |
| 21 | BossFrog | Worm | 개구리 | 보스 1차 (스테이지 70) |
| 25 | BossFrog2 | Bigguy | 개구리 | 보스 2차 (스테이지 80) |
| 20 | Pig1 | Crow | 돼지 | 일반 A (스테이지 82~89) |
| 23 | Pig2 | Zombie | 돼지 | 일반 B (스테이지 82~89) |
| 24 | BossPig | Ghost | 돼지 | 보스 1차 (스테이지 90) |
| 26 | BossPig2 | Bigguy | 돼지 | 보스 2차 |
| 30 | AccessoryRadish | (미결) | 특수 | |
| 31 | WeaponStandFrog | (미결) | 특수 | |
| 32 | BossTraining | (미결) | 특수 | |

---

## 2. SD Pack 몬스터별 사용 현황

| SD Pack | 용도 | 등장 횟수 |
|---------|------|---------|
| Rat | 슬라임1/3, 개구리StandFrog1/2 | 4회 |
| Spider | 슬라임2/4 | 2회 |
| Bat | 버섯1/3 | 2회 |
| Crow | 버섯2/4, 돼지1 | 3회 |
| Goblin | 무1/3 | 2회 |
| Skeleton | 무2/4 | 2회 |
| Zombie | 개구리1/2, 돼지2 | 3회 |
| Worm | 슬라임보스1, 버섯보스2, 개구리보스1 | 3회 |
| Ghost | 슬라임보스2, 무보스1, 돼지보스1 | 3회 |
| Beholder | 버섯보스1, 무보스2 | 2회 |
| Bigguy | 개구리보스2, 돼지보스2 | 2회 |

---

## 3. walk 없는 몬스터 처리

### Bat
보유 상태: `idle`, `attack`, `die`, `sleep`, `hurt`, `laugh` (walk/fly 없음)  
→ **처리 완료**: Bat.controller에 `walk` 상태 추가, `idle.anim` 연결
→ 이유: 이동 중에도 가장 무난한 기본 자세 유지

### Beholder
보유 상태: `idle`, `attack`, `die`, `fly`, `hurt` (walk 없음)  
→ **처리 완료**: Beholder.controller에 `walk` 상태 추가 → `fly.anim` 연결

---

## 4. 이중 공격 몬스터 controller 작업

아래 몬스터들은 원본 기준 `attack` 상태가 없고 `attack-smash` / `attack-bow`(또는 `attack-stab`)로 분리되어 있었다.
현재 controller에는 코드가 호출하는 `attack` / `attack2` 상태를 추가해 둔 상태다.

| SD Pack | 추가할 상태 | 연결 클립 | 상태 |
|---------|-----------|---------|------|
| Goblin | `attack` | attack-smash.anim | 완료 |
| Zombie | `attack` | attack-smash.anim | 완료 |
| Ghost | `attack` | attack-smash.anim | 완료 |
| Skeleton | `attack` | attack-stab.anim | 완료 |
| Worm (보스) | `attack2` | attack.anim 재사용 | 완료 |
| Ghost (보스) | `attack2` | attack-bow.anim | 완료 |
| Beholder (보스) | `attack2` | attack과 동일 | 완료 |
| Bigguy (보스) | `attack` / `attack2` | attack-smash.anim / attack-bow.anim | 완료 |

## 5. Monster.cs 현재 코드 기준

- 기본 상태명은 SD Pack 기준 소문자다: `idle`, `walk`, `attack`, `die`.
- `Init()`과 `OnEnable()`에서 `ResetAnimatorToIdle()`을 호출한다.
- `ResetAnimatorToIdle()`은 `animator.Rebind()`, `animator.Play("idle", 0, 0)`, `animator.Update(0f)` 순서로 풀 재활용 상태를 초기화한다.
- Slime2가 풀 재활용 후 `die`로 시작하던 문제는 위 초기화로 임시 조치된 상태이며, 실제 플레이 검증이 아직 필요하다.
- 몬스터 루트에는 런타임에 `SortingGroup`이 보장된다.
- `MonsterPartSorter`가 루트에 런타임 부착되고, 실제 비주얼 루트(`Monster.cs`가 붙은 첫 번째 자식)의 하위 `SpriteRenderer`를 대상으로 local Z를 `sortingOrder`로 변환한다.
- `MonsterPartSorter`는 `LateUpdate()`마다 적용되어 Animator가 부위 local Z를 바꾸는 경우에도 프레임 후반에 정렬을 보정한다.
- 전역 GraphicsSettings의 Transparency Sort Axis는 변경하지 않는다.

---

## 6. SD Pack 내부 파츠 정렬 보정

**문제**
- SD Monster Pack은 부위별 local Z로 몸통/팔/무기 등의 내부 깊이를 표현한다.
- 프로젝트는 전역 투명 정렬이 Y축 기준이라, 몬스터 내부 부위가 에셋 의도와 다르게 섞일 수 있다.

**현재 조치**
- 몬스터 전체는 `SortingGroup`으로 하나의 외부 정렬 단위로 묶는다.
- 내부 부위는 `MonsterPartSorter`가 `sortingOrder = baseOrder + round(-relativeZ * zStep)`로 보정한다.
- 기본 `zStep`은 100이다.
- 이름 기반 부위 예외 처리는 아직 넣지 않았다. 아티스트가 설정한 local Z 의도를 우선하기 위해서다.

**검증 상태**
- Unity 2022.3.62f2 batchmode 컴파일 체크는 통과.
- Play Mode 시각 검증은 아직 필요하다.

---

## 7. 에디터 작업 순서 (매핑 확정 후)

1. walk/attack/attack2 상태가 실제 controller에 유지되어 있는지 확인 (3~4절)
2. 기존 프리팹(Slime1.prefab 등) 열기 → 기존 비주얼 자식 제거
3. SD Pack 해당 몬스터 비주얼을 자식으로 배치
4. `DamagePosition`, `TargetingPosition`, `HpBar` 위치 조정
5. `Resources/Prefabs/Characters/` 저장
6. Play Mode에서 일반 몬스터/보스/피격 이펙트/체력바 겹침을 확인

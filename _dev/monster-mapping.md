# 몬스터 매핑표

> 작성일: 2026-06-22  
> 상태: 매핑 확정 (외형은 추후 에디터에서 조정 가능)

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
→ **처리 필요**: Bat.controller에 `walk` 상태 추가, 연결할 클립 선택  
→ **후보**: idle (무난) / sleep (날개 접음) / laugh (코믹)  
→ **선택: ___________**

### Beholder
보유 상태: `idle`, `attack`, `die`, `fly`, `hurt` (walk 없음)  
→ **처리**: Beholder.controller에 `walk` 상태 추가 → `fly.anim` 연결 (권장)

---

## 4. 이중 공격 몬스터 controller 작업

아래 몬스터들은 `attack` 상태가 없고 `attack-smash` / `attack-bow`(또는 `attack-stab`)로 분리됨.  
에디터에서 각 controller에 상태 추가 필요.

| SD Pack | 추가할 상태 | 연결 클립 |
|---------|-----------|---------|
| Goblin | `attack` | attack-smash.anim |
| Zombie | `attack` | attack-smash.anim |
| Ghost | `attack` | attack-smash.anim |
| Skeleton | `attack` | attack-stab.anim |
| Worm (보스) | `attack2` | attack-bow.anim (없으면 attack 동일 클립) |
| Ghost (보스) | `attack2` | attack-bow.anim |
| Beholder (보스) | `attack2` | (attack과 동일, 단일 공격) |
| Bigguy (보스) | `attack` | attack-smash.anim / `attack2` → attack-bow.anim |

---

## 5. 에디터 작업 순서 (매핑 확정 후)

1. walk/attack 상태 누락 controller 수정 (3~4절)
2. 기존 프리팹(Slime1.prefab 등) 열기 → 기존 비주얼 자식 제거
3. SD Pack 해당 몬스터 비주얼을 자식으로 배치
4. `DamagePosition`, `TargetingPosition`, `HpBar` 위치 조정
5. `Resources/Prefabs/Characters/` 저장

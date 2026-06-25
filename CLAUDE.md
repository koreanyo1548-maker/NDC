# Mushroom Hero → 신규 게임 마이그레이션

## 프로젝트 목적

Mushroom Hero 코드베이스를 기반으로 신규 게임을 제작한다.
서버리스 구조로 전환하는 것이 방향이며, PlayFab 제거는 그 수단 중 하나다.

## 현재 단계

Phase 1 완료 — PlayFab 서버 의존 런타임을 로컬 저장/스텁 구조로 전환
Phase 2-B 진행 중 — Player/Monster 비주얼 에셋 교체 및 검증
현재 미완료 핵심: Player 런타임 파츠 외부 제어 API, Monster 프리팹/Play Mode 시각 검증, Bundle ID/광고/Firebase/IAP 계정값 교체
상세 계획 및 체크리스트: `_dev/new-game-migration-plan.md`

## MCP 사용 규칙

사용자의 명시적 지시가 있기 전까지 MCP 도구(coplay-mcp 등)를 호출하지 않는다.

## 세션 시작 규칙

새 세션 시작 시 아래 파일들을 반드시 읽을 것:
1. `_dev/new-game-migration-plan.md` — 현재 작업 계획 및 체크리스트
2. `_dev/history.md` — 의사결정 기록 및 직전 세션 요약
3. 현재 작업이 Player/Monster/Spine 관련이면 `_dev/player-parts-system.md`, `_dev/monster-mapping.md`, `_dev/asset-migration-analysis.md` 중 해당 문서를 함께 확인

## 현재 코드 기준 주의사항

- `PlayFabManager`, `PlayFabStore`, `PlayFabLeaderboard`, `PlayFabTitleData` 이름은 남아 있지만 서버 SDK 래퍼가 아니라 로컬 저장/스텁 호환 레이어로 사용 중이다.
- `Assets/Plugins/PlayFabSDK/`와 `PlayFabFunctions.cs`는 제거된 상태다.
- `IAPManager.cs`에는 `#if APPSFLYER_ENBALE` 블록 안에 `PlayFabClientAPI` 직접 호출이 남아 있다. 현재 심볼 오타로 비활성화되어 있지만, AppsFlyer를 되살릴 때 로컬 저장 또는 별도 분석 이벤트로 교체해야 한다.
- `IAPManager.cs`의 `Use Test Purchase Fallback`은 현재 코드상 IAP 연결 후 상품 없음/구매 불가 상황에서만 도달한다. `_isConnected == false`인 미초기화 상태는 초반 guard에서 바로 return한다.
- Player는 Shinabro Spine `SkeletonMecanim` 기반 애니메이션명으로 동작한다.
- `SimpleSpineSkinAssigner.AssignSkins()`는 아직 private이고, `Player.SkinAssigner` getter도 없다. 외부 런타임 파츠 교체가 필요하면 두 API를 먼저 노출해야 한다.
- Monster는 SD Pack 소문자 상태명(`idle`, `walk`, `attack`, `die`) 기준으로 동작하며, `Init()`/`OnEnable()`에서 `ResetAnimatorToIdle()`을 호출한다.
- Monster 루트에는 런타임에 `SortingGroup`과 `MonsterPartSorter`가 붙어 SD Pack 내부 local Z 기반 파츠 정렬을 보정한다. Play Mode 시각 검증은 아직 필요하다.

## 세션 종료 규칙

- 사용자가 대화를 마무리하는 시점에 `_dev/history.md` 업데이트를 자동으로 제안한다.
- 기록 대상: 큰 방향 결정과 그 이유, 다음 세션에 이어할 것
- 작은 단위 작업(파일 삭제 등)은 기록하지 않는다.

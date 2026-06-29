# 랭킹 시스템 구현 계획 — Supabase 기반

> 작성일: 2026-06-29  
> 상태: 계획 단계

---

## 1. 결정 사항

### 백엔드: Supabase (PostgreSQL)

| 항목 | 내용 |
|------|------|
| DB | Supabase Postgres (무료 500MB) |
| API | Supabase REST API (`UnityWebRequest`로 직접 호출) |
| 인증 | Supabase `anon key` + `service_role key` (어드민용) |
| 국적 | Unity `LocalizationSettings.SelectedLocale` 언어 코드로 대체 |
| 프리티어 정지 방지 | GitHub Actions cron으로 하루 1회 ping |

### 운영 방식

- **Supabase 대시보드** — 테이블 에디터에서 유저 정보 조회, 차단, 제거
- `is_banned = true` 설정 시 랭킹 조회 쿼리에서 자동 필터
- 특정 유저 제거: `is_hidden = true` 또는 직접 DELETE

### 랭킹 갱신 전략 (방치형 게임 특성 고려)

- 스코어 업로드: 기존 호출부(`UpdateStage`, `UpdateTraining`) 그대로 유지, 내부에 throttle 추가
  - 훈련장: 기록 갱신 시마다 호출되지만 **최소 5분 간격 throttle**
  - 스테이지: `UI_MainTop._GetStageRankingRoutine()`이 이미 15분 간격으로 호출 중 → 그대로 활용
- HUD 순위 추정: 랭킹 팝업 오픈 또는 앱 시작 시 top 200 스냅샷 로컬 캐싱 → 로컬 비교로 HUD 순위 표시
- 랭킹 팝업: 열 때 최신 데이터 조회

---

## 2. Supabase 테이블 스키마

```sql
-- 랭킹 메인 테이블
CREATE TABLE leaderboard (
  user_id        TEXT PRIMARY KEY,
  display_name   TEXT NOT NULL,
  language       TEXT DEFAULT 'ko',      -- Unity locale 코드 ("ko", "en", "ja" 등)
  stage_score    INTEGER DEFAULT 0,      -- 최고 스테이지
  training_score BIGINT DEFAULT 0,       -- 최고 훈련장 데미지 (BigInteger → BIGINT)
  level          INTEGER DEFAULT 1,
  title          INTEGER DEFAULT 0,
  profile        INTEGER DEFAULT 1,
  is_banned      BOOLEAN DEFAULT false,
  is_hidden      BOOLEAN DEFAULT false,
  updated_at     TIMESTAMPTZ DEFAULT now()
);

-- 조회용 인덱스
CREATE INDEX idx_training_score ON leaderboard (training_score DESC) WHERE is_banned = false AND is_hidden = false;
CREATE INDEX idx_stage_score    ON leaderboard (stage_score DESC)    WHERE is_banned = false AND is_hidden = false;

-- Row Level Security
ALTER TABLE leaderboard ENABLE ROW LEVEL SECURITY;

-- 일반 유저: 차단/숨김 제외한 행만 읽기 가능
CREATE POLICY "public read" ON leaderboard
  FOR SELECT USING (is_banned = false AND is_hidden = false);

-- 일반 유저: 자신의 행만 insert/update 가능
-- (anon key로 호출 시 user_id를 클라이언트가 제공 → 서버에서 검증 불가 이슈 있음)
-- → 단순 구조에서는 anon key upsert 허용하고 어뷰징은 운영으로 대응
CREATE POLICY "user upsert" ON leaderboard
  FOR INSERT WITH CHECK (true);
CREATE POLICY "user update" ON leaderboard
  FOR UPDATE USING (true);
```

### training_score 타입 주의

현재 `LeaderboardEntry.StatValue`는 `int`다. 훈련장 데미지는 `BigInteger`이므로:
- DB: `BIGINT` (최대 9.2 × 10^18)
- 서버 → 클라이언트: `long`으로 수신 후 `BigInteger`로 변환
- `LeaderboardEntry.StatValue`를 `long`으로 변경 필요 (→ `UI_Ranking_Item.cs`의 `Decode` 로직 제거 가능)

---

## 3. 재사용 가능 코드 현황

| 파일 | 재사용 여부 | 비고 |
|------|-----------|------|
| `UIs/Ranking/UI_Ranking.cs` | ✅ 100% | 변경 없음 |
| `UIs/Ranking/UI_Ranking_Item.cs` | ✅ 거의 그대로 | `StatValue` 타입 변경에 따라 `Decode()` 제거 가능 |
| `ThirdParty/LeaderboardEntry.cs` | 🔧 소폭 수정 | `StatValue: int → long` |
| `UIs/FieldMain/UI_MainTop.cs` | ✅ 100% | `SetRanking()`, 15분 업로드 루틴 그대로 |
| `ThirdParty/PlayFabTitleData.cs` | ✅ 100% | `CheckDontRank()` no-op 유지 |
| `ThirdParty/PlayFabLeaderboard.cs` | 🔧 내부 교체 | 시그니처 유지, 내부 Supabase REST 호출로 교체 |
| `ThirdParty/SupabaseClient.cs` | 🆕 신규 | `UnityWebRequest` 기반 REST 래퍼 |

---

## 4. 신규 작성: SupabaseClient.cs

역할: Supabase REST API 호출 공통 레이어

```
Assets/Scripts/ThirdParty/SupabaseClient.cs
```

제공할 메서드:

```csharp
// 내 스코어 업로드 (UPSERT)
public static IEnumerator UpsertScore(string userId, string displayName, string language,
    int stageScore, long trainingScore, int level, int title, int profile, Action onDone)

// 상위 N개 조회 (스테이지 or 훈련장)
public static IEnumerator GetTopN(RankingType type, int offset, int limit,
    Action<List<LeaderboardEntry>> onDone)

// 내 순위 조회 (내 스코어보다 높은 행 count + 1)
public static IEnumerator GetMyRank(RankingType type, long myScore, Action<int> onDone)
```

내부 구조:
- Supabase URL, anon key는 `Resources/SupabaseConfig.asset` (ScriptableObject)로 분리 — git에 커밋하지 않음
- 모든 요청: `Authorization: Bearer {anon_key}`, `apikey: {anon_key}` 헤더
- 응답: `JsonUtility` 또는 `Newtonsoft.Json`으로 파싱

---

## 5. PlayFabLeaderboard.cs 교체 내용

### UpdateStage / UpdateTraining (업로드)

```
기존: no-op
변경: SupabaseClient.UpsertScore() 호출
     단, UpdateTraining은 마지막 호출 후 5분 미경과 시 skip (throttle)
```

### GetStageLeaderboard / GetTrainingLeaderboard (목록 조회)

```
기존: 빈 리스트로 콜백
변경: SupabaseClient.GetTopN() → LeaderboardEntry 리스트 변환 → OnStageLoaded() 콜백
```

### GetMyStageLeaderboard / GetMyTrainingLeaderboard (내 순위)

```
기존: Position 9999로 콜백
변경: SupabaseClient.GetMyRank() → SetRanking() + OnMyStageLoaded() 콜백
     top 200 스냅샷도 함께 로컬 캐싱
```

### GetRankingInfo (유저 상세 정보)

```
기존: 0,0,0,0,0으로 콜백
변경: leaderboard 테이블에 level/title/profile 컬럼이 있으므로
     GetTopN 결과에 이미 포함 → 별도 조회 불필요, 로컬 캐시에서 반환
```

---

## 6. HUD 순위 추정 구조

```
앱 시작 / 포그라운드 복귀
  → GetMyTrainingLeaderboard() 호출
  → top 200 스냅샷 로컬 캐싱 (_cachedTop200)
  → 내 스코어와 비교 → SetRanking() 호출

훈련장 데미지 기록 갱신 (UpdateTraining 호출 시마다)
  → 캐싱된 _cachedTop200와 비교 → SetRanking() 즉시 업데이트 (서버 호출 없음)
  → throttle 통과 시에만 서버 UPSERT

15분마다 (기존 _GetStageRankingRoutine)
  → UpdateStage() → 서버 UPSERT + 스냅샷 갱신
```

---

## 7. 운영 기능

### Supabase 대시보드에서 직접 처리

```sql
-- 유저 차단 (랭킹에서 즉시 사라짐, 데이터 보존)
UPDATE leaderboard SET is_banned = true WHERE user_id = 'xxx';

-- 랭킹에서 숨김 (소프트 딜리트)
UPDATE leaderboard SET is_hidden = true WHERE user_id = 'xxx';

-- 언어별 유저 조회
SELECT user_id, display_name, training_score
FROM leaderboard WHERE language = 'ko' ORDER BY training_score DESC;

-- 특정 순위 유저 조회
SELECT *, RANK() OVER (ORDER BY training_score DESC) AS rank
FROM leaderboard WHERE is_banned = false AND is_hidden = false
LIMIT 50;
```

### 어드민 API (추후 필요 시)

- Supabase `service_role key`로 RLS 우회 가능
- 별도 어드민 웹페이지 필요 시 Supabase Edge Functions 또는 외부 Next.js 페이지에서 호출

---

## 8. 작업 순서

### Step 1 — Supabase 프로젝트 설정
- [ ] Supabase 프로젝트 생성
- [ ] 테이블 스키마 적용 (섹션 2 SQL)
- [ ] anon key / project URL 확보
- [ ] `Resources/SupabaseConfig.asset` ScriptableObject 작성 (`.gitignore`에 추가)

### Step 2 — SupabaseClient.cs 작성
- [ ] `UnityWebRequest` 기반 GET/POST/PATCH 공통 메서드
- [ ] `UpsertScore()` 구현
- [ ] `GetTopN()` 구현
- [ ] `GetMyRank()` 구현

### Step 3 — PlayFabLeaderboard.cs 교체
- [ ] `UpdateTraining()` — throttle + UpsertScore
- [ ] `UpdateStage()` — UpsertScore
- [ ] `GetStageLeaderboard()` / `GetTrainingLeaderboard()` — GetTopN + 콜백
- [ ] `GetMyStageLeaderboard()` / `GetMyTrainingLeaderboard()` — GetMyRank + 스냅샷 캐싱
- [ ] `GetRankingInfo()` — 로컬 캐시에서 반환

### Step 4 — LeaderboardEntry.cs 수정
- [ ] `StatValue: int → long`
- [ ] `UI_Ranking_Item.cs`의 `Decode()` 메서드 제거, `BigInteger` 직접 변환으로 교체

### Step 5 — 검증
- [ ] 에디터 Play Mode에서 UPSERT 동작 확인
- [ ] 랭킹 팝업 목록 조회 확인
- [ ] HUD 순위 추정 동작 확인
- [ ] throttle 동작 확인 (5분 미경과 시 UPSERT 스킵)
- [ ] 차단 유저 필터 확인

### Step 6 — 운영 설정
- [ ] GitHub Actions cron ping (7일 비활성 정지 방지)
- [ ] Supabase 대시보드 운영 SQL 검증

---

## 9. 미결 사항 / 추후 결정

- `is_banned` 처리를 클라이언트에서도 감지할지 (차단된 유저에게 알림 여부)
- 랭킹 팝업에서 타 유저 프로필 클릭 시 상세 보기 기능 추가 여부
- `training_score`의 BigInteger → long 변환 시 오버플로우 발생 가능 최대값 확인 필요
  - long 최대값: 약 9.2 × 10^18 → 현재 게임 데미지 스케일에서 충분한지 확인
- 어드민 웹 UI 필요 여부 (현재는 Supabase 대시보드 SQL로 대체)

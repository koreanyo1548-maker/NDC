# Shinabro 얼굴 스프라이트 교체 가이드

> 작성일: 2026-06-23  
> 대상 에셋: `Assets/Downloads/Shinabro/MiniFantasyCharacters/`

---

## 1. 구조 개요

Shinabro MiniFantasyCharacters는 Spine (SkeletonMecanim) 기반 에셋이다.

| 파일 | 역할 |
|------|------|
| `Character.png` | 2048×2048 텍스처 아틀라스 (모든 파츠) |
| `Character_2.png` | 발/신발 전용 보조 아틀라스 |
| `Character.atlas.txt` | 각 스프라이트의 픽셀 좌표 정의 |
| `Character.json` | Spine 스켈레톤 데이터 (뼈대/슬롯/스킨) |
| `Character_SkeletonData.asset` | Unity에서 생성된 Spine 에셋 |

**좌표계 주의**: `Character.atlas.txt`는 **좌상단 기준** (PIL/Pillow와 동일).  
Spine 에디터는 좌하단 기준이지만, 익스포트된 `.atlas.txt`는 좌상단으로 저장됨.

---

## 2. 얼굴 스프라이트 목록 (HEAD 슬롯)

`Character.atlas.txt` 기준. 모두 `Character.png` 안에 위치.

| 스프라이트 이름 | x | y | w | h | rotate |
|----------------|---|---|---|---|--------|
| HEAD/headA/head | 601 | 451 | 82 | 84 | — |
| HEAD/headA1/head | 685 | 430 | 82 | 84 | 90 |
| HEAD/headA2/head | 388 | 432 | 82 | 84 | 90 |
| HEAD/headA3/head | 911 | 433 | 82 | 84 | 90 |
| HEAD/headA4/head | 825 | 433 | 82 | 84 | 90 |
| HEAD/headA5/head | 302 | 437 | 82 | 84 | 90 |
| HEAD/headA6/head | 1940 | 442 | 82 | 84 | 90 |
| HEAD/headB/head | 30 | 470 | 83 | 84 | 90 |
| HEAD/headB1/head | 116 | 459 | 83 | 84 | 90 |
| HEAD/headB2/head | 1517 | 462 | 83 | 84 | 90 |
| HEAD/headB3/head | 1431 | 462 | 83 | 84 | 90 |
| HEAD/headB4/head | 1854 | 467 | 83 | 84 | 90 |
| HEAD/headB5/head | 1768 | 467 | 83 | 84 | 90 |
| HEAD/headB6/head | 216 | 467 | 83 | 84 | 90 |

> **headA만 rotate 없음** (atlas에 기재된 대로 그대로 붙여넣기 가능).  
> headA1~headB6은 rotate:90 — 아틀라스 안에서 90도 회전된 채로 저장됨.

---

## 3. 교체 절차

### 준비
- 소스 이미지: RGBA PNG, 배경 투명 권장
- headA 기준 크기: **82×84px**
- headA1~B6 기준 크기: **82×84** (rotate된 상태로 붙여야 하므로 **84×82** 로 소스 준비)

### 스크립트 (Python + Pillow)

```python
from PIL import Image
import shutil

# 백업
shutil.copy(
    'Assets/Downloads/Shinabro/MiniFantasyCharacters/Character.png',
    'Assets/Downloads/Shinabro/MiniFantasyCharacters/Character_backup.png'
)

atlas = Image.open('Assets/Downloads/Shinabro/MiniFantasyCharacters/Character.png').convert('RGBA')

# ── headA 교체 (rotate 없음) ──────────────────────────────────────────
face = Image.open('새_얼굴.png').convert('RGBA')
# 크기가 82×84가 아니면 리사이즈
if face.size != (82, 84):
    face = face.resize((82, 84), Image.LANCZOS)
atlas.paste(face, (601, 451), face)

# ── headA1 교체 예시 (rotate:90 — 시계방향 90도 회전해서 붙여야 함) ──
# face_rot = face.rotate(-90, expand=True)  # 82×84 → 84×82
# atlas.paste(face_rot, (685, 430), face_rot)

atlas.save('Assets/Downloads/Shinabro/MiniFantasyCharacters/Character.png')
```

> `rotate:90` 항목은 atlas 기준 w×h가 이미 회전된 치수임.  
> PIL에서 붙일 때 `-90도 회전`(시계방향)한 이미지를 paste해야 인게임에서 정상으로 보임.

### Unity 반영
스크립트 실행 후 Unity 에디터에서 `Character.png` 우클릭 → **Reimport** 또는 포커스 전환으로 자동 임포트.

---

## 4. Spine 버전 호환 픽스 (2026-06-23 적용)

### 문제
`Character.json`이 Spine **4.2.43**으로 익스포트됐으나, 프로젝트 런타임은 spine-unity **4.1**.  
Unity 임포트 시 `Data version: 4.2.43. Required version: 4.1.` 오류 발생.

### 조치
`Assets/Spine/Runtime/spine-unity/Asset Types/SkeletonDataCompatibility.cs` 44~45번째 줄 수정:

```csharp
// 변경 전
static readonly int[][] compatibleBinaryVersions = { new[] { 4, 1, 0 } };
static readonly int[][] compatibleJsonVersions = { new[] { 4, 1, 0 } };

// 변경 후
static readonly int[][] compatibleBinaryVersions = { new[] { 4, 1, 0 }, new[] { 4, 2, 0 } };
static readonly int[][] compatibleJsonVersions = { new[] { 4, 1, 0 }, new[] { 4, 2, 0 } };
```

### 주의
4.1 런타임이 4.2 전용 기능(새 물리, 새 제약 타입 등)을 쓰는 스켈레톤은 런타임 오류 가능.  
이상 발생 시 spine-unity 4.2 런타임 전체를 재설치해야 함.

---

## 5. 백업 위치

| 파일 | 경로 |
|------|------|
| 원본 아틀라스 백업 | `Assets/Downloads/Shinabro/MiniFantasyCharacters/Character_backup.png` |

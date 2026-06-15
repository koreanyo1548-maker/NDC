#!/usr/bin/env python3
"""
Mushroom Hero 프로젝트 대시보드 생성기
Usage: python _dev/generate_dashboard.py
Output: _dev/dashboard.html

동적 갱신 대상:
  - 마이그레이션 체크리스트 (_dev/new-game-migration-plan.md)
  - 빌드 설정 (ProjectSettings/ProjectSettings.asset)
  - SDK 버전 (gradle, xml, changelog)
  - Unity 패키지 버전 (Packages/manifest.json)
  - 게임 enum 전체 (Assets/Scripts/Data/Define.cs)
  - 씬 목록 (Assets/Scenes/*.unity)
"""

import json
import re
import sys
from pathlib import Path
from datetime import datetime

PROJECT_ROOT = Path(__file__).parent.parent
OUTPUT_FILE  = Path(__file__).parent / "dashboard.html"
DEFINE_CS    = PROJECT_ROOT / "Assets/Scripts/Data/Define.cs"
SCENES_DIR   = PROJECT_ROOT / "Assets/Scenes"


# ──────────────────────────────────────────────────────────────────────────────
# 공통 유틸
# ──────────────────────────────────────────────────────────────────────────────

def read_safe(path):
    try:
        return Path(path).read_text(encoding="utf-8")
    except Exception:
        return ""


# ──────────────────────────────────────────────────────────────────────────────
# Define.cs 파싱 — 모든 enum 추출
# ──────────────────────────────────────────────────────────────────────────────

def parse_enum(content, enum_name):
    """
    C# enum 블록을 파싱해 [{'name': str, 'value': str|None}] 반환.
    블록 내 빈 줄은 {'separator': True} 로 보존.
    """
    pattern = rf'public\s+enum\s+{re.escape(enum_name)}\s*\{{([^}}]+)\}}'
    m = re.search(pattern, content, re.DOTALL)
    if not m:
        return []

    members = []
    for raw_line in m.group(1).split('\n'):
        stripped = raw_line.strip()

        # 빈 줄 → 구분자
        if not stripped:
            if members and not members[-1].get('separator'):
                members.append({'separator': True})
            continue

        # 주석 줄 스킵
        if stripped.startswith('//') or stripped.startswith('/*') or stripped.startswith('*'):
            continue

        mm = re.match(r'(\w+)\s*(?:=\s*(-?\d+))?\s*,?', stripped)
        if mm:
            members.append({'name': mm.group(1), 'value': mm.group(2)})

    # 마지막 구분자 제거
    while members and members[-1].get('separator'):
        members.pop()

    return members


def parse_all_enums(content):
    """Define.cs 에서 모든 enum 이름 목록 추출"""
    return re.findall(r'public\s+enum\s+(\w+)', content)


# ──────────────────────────────────────────────────────────────────────────────
# 씬 목록
# ──────────────────────────────────────────────────────────────────────────────

# 씬별 설명 (코드에서 추론 불가 → 스크립트에서 관리)
SCENE_DESCRIPTIONS = {
    "Login":            ("게임 시작, 85+ DB 초기화, 로그인 처리", "운영"),
    "Field":            ("메인 게임플레이 — 스테이지·던전·UI 전체", "운영"),
    "Prologue":         ("프롤로그 시나리오", "운영"),
    "dev_Prologue":     ("개발용 프롤로그", "개발"),
    "CI":               ("CI/테스트", "개발"),
}

def get_scene_list():
    scenes = []
    if SCENES_DIR.exists():
        for f in sorted(SCENES_DIR.glob("*.unity")):
            name = f.stem
            desc, status = SCENE_DESCRIPTIONS.get(name, ("", "개발"))
            scenes.append({"name": name, "desc": desc, "status": status})
    return scenes


# ──────────────────────────────────────────────────────────────────────────────
# 프로젝트 설정
# ──────────────────────────────────────────────────────────────────────────────

def get_project_settings():
    content = read_safe(PROJECT_ROOT / "ProjectSettings/ProjectSettings.asset")
    patterns = {
        "productName":  r"productName:\s*(.+)",
        "companyName":  r"companyName:\s*(.+)",
        "bundleVersion":r"bundleVersion:\s*(.+)",
        "versionCode":  r"AndroidBundleVersionCode:\s*(\d+)",
        "minSdk":       r"AndroidMinSdkVersion:\s*(\d+)",
        "targetSdk":    r"AndroidTargetSdkVersion:\s*(\d+)",
    }
    result = {}
    for key, pat in patterns.items():
        mm = re.search(pat, content)
        result[key] = mm.group(1).strip() if mm else "N/A"
    return result


# ──────────────────────────────────────────────────────────────────────────────
# SDK 버전
# ──────────────────────────────────────────────────────────────────────────────

def get_sdk_versions():
    v = {}

    changelog = read_safe(PROJECT_ROOT / "Assets/GoogleMobileAds/CHANGELOG.md")
    m = re.search(r"Version\s+([\d.]+)", changelog)
    v["gma_unity"] = m.group(1) if m else "N/A"

    gma_xml = read_safe(PROJECT_ROOT / "Assets/GoogleMobileAds/Editor/GoogleMobileAdsDependencies.xml")
    m = re.search(r"play-services-ads:([\d.]+)", gma_xml)
    v["gma_android"] = m.group(1) if m else "N/A"
    m = re.search(r"user-messaging-platform:([\d.]+)", gma_xml)
    v["ump"] = m.group(1) if m else "N/A"

    fb_xml = read_safe(PROJECT_ROOT / "Assets/Firebase/Editor/AnalyticsDependencies.xml")
    m = re.search(r"firebase-analytics-unity:([\d.]+)", fb_xml)
    v["firebase_unity"] = m.group(1) if m else "N/A"
    m = re.search(r"firebase-analytics:([\d.]+)", fb_xml)
    v["firebase_android"] = m.group(1) if m else "N/A"

    gpgs_xml = read_safe(PROJECT_ROOT / "Assets/GooglePlayGames/com.google.play.games/Editor/GooglePlayGamesPluginDependencies.xml")
    m = re.search(r"gpgs-plugin-support:([\d.]+)", gpgs_xml)
    v["gpgs"] = m.group(1) if m else "N/A"

    gma_gradle = read_safe(PROJECT_ROOT / "Assets/Plugins/Android/GoogleMobileAdsPlugin.androidlib/build.gradle")
    m = re.search(r"compileSdkVersion\s+(\d+)", gma_gradle)
    v["gma_compile_sdk"] = m.group(1) if m else "N/A"
    m = re.search(r"buildToolsVersion\s+['\"]([^'\"]+)['\"]", gma_gradle)
    v["gma_build_tools"] = m.group(1) if m else "N/A"
    m = re.search(r"targetSdkVersion\s+(\d+)", gma_gradle)
    v["gma_target_sdk"] = m.group(1) if m else "N/A"

    return v


# ──────────────────────────────────────────────────────────────────────────────
# Unity 패키지
# ──────────────────────────────────────────────────────────────────────────────

def get_unity_packages():
    content = read_safe(PROJECT_ROOT / "Packages/manifest.json")
    try:
        return json.loads(content).get("dependencies", {})
    except Exception:
        return {}


# ──────────────────────────────────────────────────────────────────────────────
# 마이그레이션 플랜 파싱
# ──────────────────────────────────────────────────────────────────────────────

def parse_migration_plan(content):
    phases = {}
    current_phase = None
    for line in content.split("\n"):
        phase_match = re.match(r"###\s+Phase\s+(\d+)[:\s]*(.+)", line)
        if phase_match:
            key = f"Phase {phase_match.group(1)}"
            phases[key] = {"title": phase_match.group(2).strip(), "items": []}
            current_phase = key
            continue
        check_match = re.match(r"\s*[-*]\s*\[([ xX])\]\s*(.+)", line)
        if check_match and current_phase:
            done = check_match.group(1).lower() == "x"
            text = re.sub(r"`([^`]+)`", r"<code>\1</code>", check_match.group(2).strip())
            phases[current_phase]["items"].append({"done": done, "text": text})
    return phases


# ──────────────────────────────────────────────────────────────────────────────
# HTML 빌더 헬퍼
# ──────────────────────────────────────────────────────────────────────────────

def pct(done, total):
    return int(done / total * 100) if total > 0 else 0

def badge(status):
    cls = {"done": "badge-done", "progress": "badge-progress", "pending": "badge-pending"}.get(status, "badge-pending")
    label = {"done": "완료", "progress": "진행 중", "pending": "대기"}.get(status, status)
    return f'<span class="badge {cls}">{label}</span>'


# ──────────────────────────────────────────────────────────────────────────────
# 동적 섹션 생성
# ──────────────────────────────────────────────────────────────────────────────

def enum_to_tag_list(members, value_labels=None, color_map=None):
    """enum 멤버를 tag-list로 렌더링"""
    html = '<div class="tag-list">'
    for m in members:
        if m.get('separator'):
            html += '</div><div class="tag-list" style="margin-top:6px">'
            continue
        name = m['name']
        val  = m.get('value')
        label = (value_labels or {}).get(name, name)
        color = (color_map or {}).get(name, "")
        val_str = f" <small>= {val}</small>" if val is not None else ""
        html += f'<span class="tag {color}" title="{name}">{label}{val_str}</span>'
    html += '</div>'
    return html


def enum_to_table(members, headers=("이름", "값"), show_index=False):
    """enum 멤버를 2열 테이블로 렌더링"""
    rows = ""
    idx = 0
    for m in members:
        if m.get('separator'):
            rows += f'<tr><td colspan="3" class="sep-row"></td></tr>'
            continue
        val = m.get('value', str(idx) if show_index else "")
        rows += f'<tr><td><code>{m["name"]}</code></td><td>{val}</td></tr>'
        idx += 1
    return f'<table><tr><th>{headers[0]}</th><th>{headers[1]}</th></tr>{rows}</table>'


def build_currency_section(define_content):
    """CurrencyType enum → 재화 카드 목록"""
    members = parse_enum(define_content, "CurrencyType")

    # 그룹 레이블 (빈 줄 기준 그룹 순서에 맞게 지정)
    group_labels = [
        "💵 통화 (Money)",
        "🪨 성장석 (Stone)",
        "⚡ 기타 재화",
        "🎯 보유 장비",
        "📊 경험치",
        "🎫 패스 / 프리셋",
        "🗝️ 스킬 프리셋 슬롯",
        "🏰 던전 티켓",
        "🏰 광고 던전 티켓",
        "📿 소환 티켓",
        "📿 광고 소환 티켓",
        "📚 도감 / 책",
        "📚 책장",
        "🎨 어빌리티 프리셋",
        "🌙 오프라인 보상",
    ]

    groups = []
    current = []
    for m in members:
        if m.get('separator'):
            if current:
                groups.append(current)
            current = []
        else:
            current.append(m)
    if current:
        groups.append(current)

    html = ""
    for i, group in enumerate(groups):
        label = group_labels[i] if i < len(group_labels) else f"그룹 {i+1}"
        html += f'<div class="sub-title">{label}</div><div class="currency-grid">'
        for m in group:
            val = m.get('value', '')
            val_str = f'<div class="c-id">ID: {val}</div>' if val else ''
            html += f'<div class="currency-item"><div class="c-name">{m["name"]}</div>{val_str}</div>'
        html += '</div>'

    return html


def build_stat_section(define_content):
    members = [m for m in parse_enum(define_content, "StatType") if not m.get('separator') and m['name'] != 'None']
    html = '<div class="currency-grid">'
    for m in members:
        html += f'<div class="currency-item"><div class="c-name">{m["name"]}</div></div>'
    html += f'</div><p class="note-text">총 {len(members)}종 (Define.cs StatType)</p>'
    return html


def build_dungeon_section(define_content):
    members = [m for m in parse_enum(define_content, "FieldType") if not m.get('separator')]
    desc_map = {
        "Stage":       ("일반 스테이지", "골드·경험치·장비", "메인 진행"),
        "Awakening":   ("각성 던전", "각성석", "스테이지 몬스터 능력 % 적용"),
        "SkillGrowth": ("스킬 성장 던전", "스킬 성장석", "스테이지 % 적용"),
        "Promotion":   ("승급 던전", "코스튬·Attack/Hp 상승", "성공/실패 시나리오"),
        "Pet":         ("펫 던전", "펫 성장석", "스테이지 % 적용"),
        "BlackMarket": ("검은시장 던전", "검은시장 동전", "단일 스폰·스테이지별 진행"),
        "Dia":         ("다이아 던전", "다이아", "단일 스폰·스테이지별 진행"),
        "Training":    ("훈련장", "구슬 광석", "몬스터 ID 32 고정·피해량 단계"),
    }
    rows = ""
    for m in members:
        name = m['name']
        val  = m.get('value', '')
        kr, reward, note = desc_map.get(name, (name, "", ""))
        rows += f'<tr><td>{val}</td><td><code>{name}</code></td><td>{kr}</td><td>{reward}</td><td class="note">{note}</td></tr>'
    return f'<table><tr><th>ID</th><th>enum</th><th>던전 종류</th><th>주요 보상</th><th>비고</th></tr>{rows}</table>'


def build_grade_section(define_content):
    grades   = [m for m in parse_enum(define_content, "GradeType")    if not m.get('separator')]
    fullgrade = [m for m in parse_enum(define_content, "FullGradeType") if not m.get('separator')]
    color_map = {"Normal":"","Magic":"blue","Rare":"blue","Unique":"yellow",
                 "Heroic":"yellow","Legendary":"green","Mythic":"purple"}
    tags = '<div class="tag-list">'
    for m in grades:
        c = color_map.get(m['name'], "")
        tags += f'<span class="tag {c}">{m["name"]} (={m.get("value","")})</span>'
    tags += '</div>'
    return (f'<div class="info-box"><h4>등급 체계 GradeType — {len(grades)}단계</h4>{tags}'
            f'<p style="margin-top:12px;color:var(--text2)">FullGradeType: 각 등급 × 5단계 = 총 {len(fullgrade)}단계 (Normal1~Mythic5)</p></div>')


def build_equipment_section(define_content):
    equip_types = [m for m in parse_enum(define_content, "EquipmentType") if not m.get('separator')]
    costume_cat = [m for m in parse_enum(define_content, "CostumeCategoryType") if not m.get('separator')]
    costume_pos = [m for m in parse_enum(define_content, "CostumePositionType") if not m.get('separator')]

    equip_desc = {
        "Weapon":    ("무기", "공격력 증가, 성장/각성/소환 공통 시스템"),
        "Accessory": ("악세서리", "HP 증가, 성장/각성/소환 공통 시스템"),
        "Skill":     ("스킬", "스킬 슬롯 장착, SkillType으로 동작 정의"),
        "Pet":       ("펫", "전투 동행 유닛, 별도 던전(FieldType.Pet)에서 성장석 획득"),
    }

    rows = ""
    for m in equip_types:
        kr, note = equip_desc.get(m['name'], (m['name'], ""))
        rows += f'<tr><td><code>{m["name"]}</code></td><td>{kr}</td><td class="note">{note}</td></tr>'

    def tags(lst):
        return "".join(f'<span class="tag">{m["name"]}</span>' for m in lst)

    return f"""
    <table><tr><th>EquipmentType</th><th>종류</th><th>설명</th></tr>{rows}</table>
    <div class="two-col">
      <div class="info-box">
        <h4>펫 시스템 구조</h4>
        <p>장비 타입 중 하나 (EquipmentType.Pet)</p>
        <p>GradeType 공유 (Normal ~ Mythic)</p>
        <p>전용 스킬: Heal, AttackBuff (SkillType)</p>
        <p>전용 던전: FieldType.Pet → 펫 성장석</p>
        <p>각성 시스템: DbPetAwakening</p>
        <div class="tag-list" style="margin-top:8px">
          <span class="tag blue">PetManager</span>
          <span class="tag blue">PetController</span>
          <span class="tag blue">DbPet (CSV)</span>
          <span class="tag blue">DbPetAwakening</span>
        </div>
      </div>
      <div class="info-box">
        <h4>코스튬 시스템</h4>
        <h4 style="font-size:13px;margin-top:0">CostumeCategoryType</h4>
        <div class="tag-list">{tags(costume_cat)}</div>
        <h4 style="font-size:13px;margin-top:10px">CostumePositionType</h4>
        <div class="tag-list">{tags(costume_pos)}</div>
        <p style="margin-top:8px;color:var(--text2);font-size:12px">승급 던전(FieldType.Promotion) 보상으로 획득</p>
      </div>
    </div>"""


def build_skill_section(define_content):
    skill_types  = [m for m in parse_enum(define_content, "SkillType")       if not m.get('separator')]
    target_range = [m for m in parse_enum(define_content, "TargetRangeType") if not m.get('separator')]
    effect_pos   = [m for m in parse_enum(define_content, "EffectPositionType") if not m.get('separator')]

    def tags(lst):
        return "".join(f'<span class="tag">{m["name"]}</span>' for m in lst)

    return f"""
    <div class="two-col">
      <div class="info-box">
        <h4>SkillType — 스킬 동작 유형</h4>
        <div class="tag-list">{tags(skill_types)}</div>
        <p style="margin-top:10px;color:var(--text2);font-size:12px">Heal·AttackBuff = 펫 전용 스킬</p>
      </div>
      <div class="info-box">
        <h4>TargetRangeType — 타겟 방식</h4>
        <div class="tag-list">{tags(target_range)}</div>
        <h4 style="margin-top:12px">EffectPositionType</h4>
        <div class="tag-list">{tags(effect_pos)}</div>
      </div>
    </div>"""


def build_quest_section(define_content):
    members = [m for m in parse_enum(define_content, "QuestType") if not m.get('separator') and m['name'] != 'None']
    cycles  = [m for m in parse_enum(define_content, "QuestCycleType") if not m.get('separator')]

    tags_html = "".join(f'<span class="tag" style="font-size:11px">{m["name"]}</span>' for m in members)
    cycle_html = "".join(f'<span class="tag blue">{m["name"]}</span>' for m in cycles)

    return f"""
    <div class="info-box">
      <h4>QuestCycleType</h4>
      <div class="tag-list">{cycle_html}</div>
    </div>
    <div class="info-box">
      <h4>QuestType — {len(members)}종</h4>
      <div class="tag-list">{tags_html}</div>
    </div>"""


def build_iap_section(define_content):
    shop_cat  = [m for m in parse_enum(define_content, "ShopCategoryType") if not m.get('separator')]
    renewal   = [m for m in parse_enum(define_content, "RenewalType")      if not m.get('separator')]
    platform  = [m for m in parse_enum(define_content, "PlatformType")     if not m.get('separator')]
    ad_types  = [m for m in parse_enum(define_content, "eAdType")          if not m.get('separator')]
    ad_buff   = [m for m in parse_enum(define_content, "AdBuffType")       if not m.get('separator')]
    pass_type = [m for m in parse_enum(define_content, "PassType")         if not m.get('separator')]

    def tags(lst):
        return "".join(f'<span class="tag">{m["name"]}</span>' for m in lst)

    ad_rows = "".join(
        f'<tr><td><code>{m["name"]}</code></td><td>{m.get("value","")}</td></tr>'
        for m in ad_types
    )

    return f"""
    <div class="two-col">
      <div>
        <div class="info-box">
          <h4>ShopCategoryType — IAP 상품 카테고리</h4>
          <div class="tag-list">{tags(shop_cat)}</div>
        </div>
        <div class="info-box">
          <h4>RenewalType — 갱신 주기</h4>
          <div class="tag-list">{tags(renewal)}</div>
          <h4 style="margin-top:10px">PlatformType — 플랫폼 분기</h4>
          <div class="tag-list">{tags(platform)}</div>
          <h4 style="margin-top:10px">PassType — 패스 종류</h4>
          <div class="tag-list">{tags(pass_type)}</div>
        </div>
      </div>
      <div>
        <div class="info-box">
          <h4>eAdType — 광고 슬롯 (AdManager)</h4>
          <table><tr><th>슬롯</th><th>ID</th></tr>{ad_rows}</table>
          <h4 style="margin-top:10px">AdBuffType</h4>
          <div class="tag-list">{tags(ad_buff)}</div>
          <p style="margin-top:10px;color:var(--text2);font-size:12px">⚠️ Phase 2: AdMob App ID / Ad Unit ID 교체 필요</p>
        </div>
      </div>
    </div>"""


def build_setting_section(define_content):
    members = [m for m in parse_enum(define_content, "SettingType") if not m.get('separator')]
    tags_html = "".join(f'<span class="tag">{m["name"]}</span>' for m in members)
    return f'<div class="info-box"><h4>SettingType — {len(members)}종</h4><div class="tag-list">{tags_html}</div></div>'


def build_lock_section(define_content):
    members = [m for m in parse_enum(define_content, "LockType") if not m.get('separator')]
    cond    = [m for m in parse_enum(define_content, "LockConditionType") if not m.get('separator')]
    tags_html = "".join(f'<span class="tag" style="font-size:11px">{m["name"]}</span>' for m in members)
    cond_html = "".join(f'<span class="tag blue">{m["name"]}</span>' for m in cond)
    return f"""
    <div class="info-box">
      <h4>LockConditionType — 잠금 해제 조건</h4>
      <div class="tag-list">{cond_html}</div>
    </div>
    <div class="info-box">
      <h4>LockType — 잠금 항목 {len(members)}종</h4>
      <div class="tag-list">{tags_html}</div>
    </div>"""


def build_migration_html(phases):
    html = ""
    for phase_name, data in phases.items():
        items = data["items"]
        done  = sum(1 for i in items if i["done"])
        total = len(items)
        p     = pct(done, total)
        status = "done" if p == 100 else ("progress" if p > 0 else "pending")

        items_html = "".join(
            f'<div class="check-item {"done" if i["done"] else "pending"}">'
            f'{"✅" if i["done"] else "⬜"} {i["text"]}</div>'
            for i in items
        )
        html += f"""
        <div class="phase-card status-{status}">
          <div class="phase-header">
            <h3>{phase_name}: {data['title']}</h3>
            {badge(status)} <span class="phase-count">{done}/{total}</span>
          </div>
          <div class="progress-bar"><div class="progress-fill" style="width:{p}%"></div></div>
          <div class="check-list">{items_html}</div>
        </div>"""
    return html


def build_scene_html(scenes):
    rows = ""
    for s in scenes:
        status_badge = badge("done" if s["status"] == "운영" else "pending")
        rows += f'<tr><td>{s["name"]}.unity</td><td>{s["desc"]}</td><td>{status_badge}</td></tr>'
    return f'<table><tr><th>씬 파일</th><th>역할</th><th>상태</th></tr>{rows}</table>'


def build_sdk_rows(sdk):
    rows = [
        ("GMA Unity SDK",         sdk.get("gma_unity","N/A"),       "9.x 최신급"),
        ("GMA Android SDK",       sdk.get("gma_android","N/A"),     "Google Play Services"),
        ("GMA compileSdkVersion", sdk.get("gma_compile_sdk","N/A"), ""),
        ("GMA buildToolsVersion", sdk.get("gma_build_tools","N/A"), ""),
        ("GMA targetSdkVersion",  sdk.get("gma_target_sdk","N/A"),  ""),
        ("UMP",                   sdk.get("ump","N/A"),              "동의 관리"),
        ("Firebase Unity SDK",    sdk.get("firebase_unity","N/A"),  "Analytics 전용"),
        ("Firebase Analytics",    sdk.get("firebase_android","N/A"),""),
        ("GPGS (비활성)",          sdk.get("gpgs","N/A"),            "#if 비활성화 상태"),
    ]
    return "".join(
        f"<tr><td>{n}</td><td><code>{v}</code></td><td class='note'>{note}</td></tr>"
        for n, v, note in rows
    )


def build_pkg_rows(pkgs):
    targets = [
        ("com.unity.purchasing",           "Unity IAP"),
        ("com.unity.localization",         "Localization"),
        ("com.unity.mobile.notifications", "Mobile Notifications"),
        ("com.unity.postprocessing",       "Post Processing"),
        ("com.unity.textmeshpro",          "TextMeshPro"),
        ("com.unity.2d.animation",         "2D Animation"),
        ("com.unity.2d.psdimporter",       "PSD Importer"),
        ("com.unity.ai.navigation",        "AI Navigation"),
        ("com.unity.timeline",             "Timeline"),
        ("com.coffee.ui-particle",         "UI Particle (OpenUPM)"),
    ]
    return "".join(
        f"<tr><td>{label}</td><td><code>{pkg_id}</code></td><td>{pkgs.get(pkg_id,'N/A')}</td></tr>"
        for pkg_id, label in targets
    )


# ──────────────────────────────────────────────────────────────────────────────
# 메인 HTML 생성
# ──────────────────────────────────────────────────────────────────────────────

CSS = """
:root {
  --bg:#0f1117; --bg2:#1a1d27; --bg3:#22263a; --border:#2e3350;
  --accent:#4f8ef7; --green:#3ecf8e; --yellow:#f5a623; --red:#e05252;
  --purple:#c084fc; --text:#e2e8f0; --text2:#94a3b8; --code-bg:#1e2235;
}
* { box-sizing:border-box; margin:0; padding:0; }
body { font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;
       background:var(--bg); color:var(--text); font-size:14px; line-height:1.6; }
a { color:var(--accent); text-decoration:none; }
.layout { display:flex; min-height:100vh; }
nav { width:220px; min-width:220px; background:var(--bg2);
      border-right:1px solid var(--border); padding:24px 0;
      position:sticky; top:0; height:100vh; overflow-y:auto; }
nav .logo { padding:0 20px 20px; border-bottom:1px solid var(--border); margin-bottom:16px; }
nav .logo h1 { font-size:16px; color:var(--accent); }
nav .logo p  { font-size:11px; color:var(--text2); margin-top:2px; }
nav ul { list-style:none; }
nav ul li a { display:block; padding:7px 20px; color:var(--text2); font-size:13px; transition:all 0.15s; }
nav ul li a:hover { color:var(--text); background:var(--bg3); }
nav .nav-group { padding:14px 20px 6px; font-size:10px; color:var(--text2);
                 text-transform:uppercase; letter-spacing:0.08em; }
main { flex:1; padding:32px; max-width:1200px; }
section { margin-bottom:48px; }
.section-title { font-size:20px; font-weight:600; margin-bottom:20px;
                 padding-bottom:12px; border-bottom:1px solid var(--border);
                 display:flex; align-items:center; gap:10px; }
.section-title .icon { font-size:22px; }
.card-grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(220px,1fr)); gap:16px; margin-bottom:20px; }
.card { background:var(--bg2); border:1px solid var(--border); border-radius:10px; padding:20px; }
.card .card-label { font-size:12px; color:var(--text2); margin-bottom:6px; }
.card .card-value { font-size:22px; font-weight:700; }
.card .card-sub   { font-size:12px; color:var(--text2); margin-top:4px; }
.card.accent { border-color:var(--accent); }
.card.green  { border-color:var(--green);  }
.card.yellow { border-color:var(--yellow); }
.progress-bar  { background:var(--bg3); border-radius:4px; height:8px; margin:8px 0 12px; overflow:hidden; }
.progress-fill { height:100%; background:linear-gradient(90deg,var(--accent),var(--green)); border-radius:4px; }
.phase-card { background:var(--bg2); border:1px solid var(--border); border-radius:10px; padding:20px; margin-bottom:16px; }
.phase-card.status-done     { border-left:4px solid var(--green);  }
.phase-card.status-progress { border-left:4px solid var(--accent); }
.phase-card.status-pending  { border-left:4px solid var(--border); }
.phase-header { display:flex; align-items:center; gap:10px; margin-bottom:10px; flex-wrap:wrap; }
.phase-header h3 { font-size:15px; font-weight:600; }
.phase-count { font-size:12px; color:var(--text2); margin-left:auto; }
.badge { font-size:11px; padding:2px 8px; border-radius:20px; font-weight:600; }
.badge-done     { background:rgba(62,207,142,0.15); color:var(--green);  }
.badge-progress { background:rgba(79,142,247,0.15); color:var(--accent); }
.badge-pending  { background:rgba(100,116,139,0.15); color:var(--text2); }
.check-list { display:flex; flex-direction:column; gap:4px; }
.check-item { padding:4px 8px; border-radius:4px; font-size:13px; }
.check-item.done    { color:var(--text2); }
.check-item.pending { color:var(--text);  }
.check-item code { background:var(--code-bg); padding:1px 5px; border-radius:3px; font-size:12px; }
table { width:100%; border-collapse:collapse; background:var(--bg2);
        border:1px solid var(--border); border-radius:10px; overflow:hidden; margin-bottom:16px; }
th { background:var(--bg3); padding:10px 14px; text-align:left; font-size:12px;
     color:var(--text2); text-transform:uppercase; letter-spacing:0.05em; }
td { padding:9px 14px; border-top:1px solid var(--border); font-size:13px; }
td code { background:var(--code-bg); padding:2px 6px; border-radius:4px; font-size:12px; }
td.note { color:var(--text2); font-size:12px; }
td.sep-row { padding:2px; background:var(--bg3); }
tr:hover td:not(.sep-row) { background:var(--bg3); }
.info-box { background:var(--bg2); border:1px solid var(--border);
            border-radius:10px; padding:20px; margin-bottom:16px; }
.info-box h4 { font-size:14px; font-weight:600; margin-bottom:10px; color:var(--accent); }
.info-box p  { font-size:13px; color:var(--text2); margin-bottom:6px; }
.tag-list { display:flex; flex-wrap:wrap; gap:6px; margin-top:4px; }
.tag { background:var(--bg3); border:1px solid var(--border);
       padding:3px 10px; border-radius:20px; font-size:12px; color:var(--text2); }
.tag.green  { border-color:var(--green);  color:var(--green);  background:rgba(62,207,142,0.08); }
.tag.blue   { border-color:var(--accent); color:var(--accent); background:rgba(79,142,247,0.08); }
.tag.yellow { border-color:var(--yellow); color:var(--yellow); background:rgba(245,166,35,0.08); }
.tag.purple { border-color:var(--purple); color:var(--purple); background:rgba(192,132,252,0.08); }
.two-col { display:grid; grid-template-columns:1fr 1fr; gap:16px; margin-bottom:16px; }
.sub-title { font-size:15px; font-weight:600; margin:20px 0 10px; color:var(--text); }
.currency-grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(180px,1fr)); gap:10px; margin-bottom:16px; }
.currency-item { background:var(--bg2); border:1px solid var(--border); border-radius:8px; padding:10px 14px; }
.currency-item .c-name { font-size:13px; font-weight:500; }
.currency-item .c-id   { font-size:11px; color:var(--text2); }
.note-text { font-size:12px; color:var(--text2); margin-top:6px; }
footer { color:var(--text2); font-size:12px; text-align:center;
         padding:24px 0 40px; border-top:1px solid var(--border); margin-top:40px; }
@media (max-width:768px) { .two-col { grid-template-columns:1fr; } nav { display:none; } }
"""

JS = """
const sections = document.querySelectorAll('section[id]');
const navLinks = document.querySelectorAll('nav a');
const observer = new IntersectionObserver((entries) => {
  entries.forEach(e => {
    if (e.isIntersecting) {
      navLinks.forEach(l => l.style.color = '');
      const active = document.querySelector(`nav a[href="#${e.target.id}"]`);
      if (active) active.style.color = 'var(--accent)';
    }
  });
}, { threshold: 0.25 });
sections.forEach(s => observer.observe(s));
"""


def generate_html(migration_phases, settings, sdk, packages, scenes,
                  define_content, generated_at):

    all_items   = [i for p in migration_phases.values() for i in p["items"]]
    total_done  = sum(1 for i in all_items if i["done"])
    total_all   = len(all_items)
    overall_pct = pct(total_done, total_all)

    target_ok = int(settings.get("targetSdk", "0")) >= 35

    # 동적 게임 설계 섹션들
    currency_html = build_currency_section(define_content)
    stat_html     = build_stat_section(define_content)
    dungeon_html  = build_dungeon_section(define_content)
    grade_html    = build_grade_section(define_content)
    equipment_html= build_equipment_section(define_content)
    skill_html    = build_skill_section(define_content)
    quest_html    = build_quest_section(define_content)
    iap_html      = build_iap_section(define_content)
    setting_html  = build_setting_section(define_content)
    lock_html     = build_lock_section(define_content)
    scene_html    = build_scene_html(scenes)
    migration_html= build_migration_html(migration_phases)
    sdk_rows      = build_sdk_rows(sdk)
    pkg_rows      = build_pkg_rows(packages)

    # enum 목록 요약 (Define.cs 전체 enum 수)
    all_enums = parse_all_enums(define_content)
    enum_count = len(all_enums)

    return f"""<!DOCTYPE html>
<html lang="ko">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Mushroom Hero — 프로젝트 대시보드</title>
<style>{CSS}</style>
</head>
<body>
<div class="layout">

<nav>
  <div class="logo">
    <h1>🍄 Mushroom Hero</h1>
    <p>프로젝트 대시보드</p>
  </div>
  <ul>
    <li class="nav-group">관리</li>
    <li><a href="#summary">📊 요약</a></li>
    <li><a href="#migration">🔄 마이그레이션</a></li>
    <li><a href="#build">⚙️ 빌드 설정</a></li>
    <li><a href="#sdk">📦 SDK 버전</a></li>
    <li class="nav-group">게임 설계</li>
    <li><a href="#overview">🎮 게임 개요</a></li>
    <li><a href="#scenes">🎬 씬 구조</a></li>
    <li><a href="#currency">💰 재화 시스템</a></li>
    <li><a href="#grade">⭐ 등급 체계</a></li>
    <li><a href="#equipment">⚔️ 장비/펫</a></li>
    <li><a href="#dungeon">🏰 던전 시스템</a></li>
    <li><a href="#skill">✨ 스킬 시스템</a></li>
    <li><a href="#quest">📋 퀘스트</a></li>
    <li><a href="#iap">💳 IAP/광고</a></li>
    <li><a href="#lock">🔒 잠금 조건</a></li>
    <li><a href="#setting">⚙️ 게임 설정</a></li>
    <li><a href="#stat">📐 스탯 목록</a></li>
  </ul>
</nav>

<main>

<!-- 요약 -->
<section id="summary">
  <div class="section-title"><span class="icon">📊</span>프로젝트 요약</div>
  <p style="color:var(--text2);font-size:12px;margin-bottom:16px">
    생성: {generated_at} &nbsp;·&nbsp;
    Define.cs enum {enum_count}종 &nbsp;·&nbsp;
    씬 {len(scenes)}개
  </p>
  <div class="card-grid">
    <div class="card accent">
      <div class="card-label">전체 마이그레이션 진행률</div>
      <div class="card-value">{overall_pct}%</div>
      <div class="progress-bar"><div class="progress-fill" style="width:{overall_pct}%"></div></div>
      <div class="card-sub">{total_done} / {total_all} 항목 완료</div>
    </div>
    <div class="card green">
      <div class="card-label">앱 버전</div>
      <div class="card-value">{settings.get("bundleVersion","N/A")}</div>
      <div class="card-sub">코드: {settings.get("versionCode","N/A")}</div>
    </div>
    <div class="card {"green" if target_ok else "yellow"}">
      <div class="card-label">Target SDK</div>
      <div class="card-value">API {settings.get("targetSdk","N/A")}</div>
      <div class="card-sub">{"✅ Google Play 충족" if target_ok else "⚠️ API 35 이상 필요"}</div>
    </div>
    <div class="card">
      <div class="card-label">GMA Unity SDK</div>
      <div class="card-value">{sdk.get("gma_unity","N/A")}</div>
      <div class="card-sub">Android: {sdk.get("gma_android","N/A")}</div>
    </div>
  </div>
</section>

<!-- 마이그레이션 -->
<section id="migration">
  <div class="section-title"><span class="icon">🔄</span>마이그레이션 현황</div>
  {migration_html}
</section>

<!-- 빌드 설정 -->
<section id="build">
  <div class="section-title"><span class="icon">⚙️</span>빌드 설정</div>
  <div class="two-col">
    <table>
      <tr><th>항목</th><th>값</th></tr>
      <tr><td>앱 이름</td><td>{settings.get("productName","N/A")}</td></tr>
      <tr><td>회사명</td><td>{settings.get("companyName","N/A")}</td></tr>
      <tr><td>버전</td><td>{settings.get("bundleVersion","N/A")}</td></tr>
      <tr><td>버전 코드</td><td>{settings.get("versionCode","N/A")}</td></tr>
      <tr><td>Min SDK</td><td>API {settings.get("minSdk","N/A")}</td></tr>
      <tr><td>Target SDK</td><td>API {settings.get("targetSdk","N/A")} {"✅" if target_ok else "⚠️"}</td></tr>
    </table>
    <div class="info-box">
      <h4>Phase 2 교체 예정</h4>
      <p>⬜ 앱 이름 / 회사명</p>
      <p>⬜ Bundle ID</p>
      <p>⬜ AdMob App ID (AndroidManifest.xml)</p>
      <p>⬜ google-services.json</p>
      <p>⬜ Unity Project ID</p>
    </div>
  </div>
</section>

<!-- SDK 버전 -->
<section id="sdk">
  <div class="section-title"><span class="icon">📦</span>SDK / 패키지 버전</div>
  <div class="sub-title">Google SDK</div>
  <table><tr><th>SDK</th><th>버전</th><th>비고</th></tr>{sdk_rows}</table>
  <div class="sub-title">Unity 패키지 (주요)</div>
  <table><tr><th>패키지</th><th>ID</th><th>버전</th></tr>{pkg_rows}</table>
</section>

<!-- 게임 개요 -->
<section id="overview">
  <div class="section-title"><span class="icon">🎮</span>게임 개요</div>
  <div class="two-col">
    <div class="info-box">
      <h4>장르 / 구조</h4>
      <p>수집형 아이들 RPG</p>
      <p>스테이지 자동 전투 → 재화/장비 획득 → 성장 반복</p>
      <p>파워 세이브 모드 (배터리 절약, 보상 배치 누적)</p>
      <div class="tag-list" style="margin-top:8px">
        <span class="tag green">아이들 RPG</span>
        <span class="tag blue">수집형</span>
        <span class="tag yellow">오프라인 보상</span>
        <span class="tag">IAP</span>
        <span class="tag">AdMob</span>
        <span class="tag">로컬 저장</span>
      </div>
    </div>
    <div class="info-box">
      <h4>핵심 게임 루프</h4>
      <p>1. 스테이지 진입 (자동 전투)</p>
      <p>2. 몬스터 처치 → 골드 / 경험치 / 장비</p>
      <p>3. 보스 처치 → 다음 스테이지</p>
      <p>4. 재화로 장비 성장 / 소환</p>
      <p>5. 던전 → 특수 재화</p>
      <p>6. 오프라인 복귀 보상 수령</p>
    </div>
  </div>
  <div class="info-box">
    <h4>저장 / 서버 구조 (Phase 1 완료)</h4>
    <p>PlayFab 완전 제거 → 로컬 JSON 저장 (<code>save.json</code>)</p>
    <p>로그인: 디바이스 UUID 기반 자동 게스트</p>
    <p>랭킹: 스텁 처리 (Phase 4에서 Firebase 예정)</p>
  </div>
</section>

<!-- 씬 구조 -->
<section id="scenes">
  <div class="section-title"><span class="icon">🎬</span>씬 구조</div>
  {scene_html}
</section>

<!-- 재화 시스템 -->
<section id="currency">
  <div class="section-title"><span class="icon">💰</span>재화 시스템</div>
  <p class="note-text" style="margin-bottom:12px">소스: <code>Assets/Scripts/Data/Define.cs</code> CurrencyType enum</p>
  {currency_html}
</section>

<!-- 등급 체계 -->
<section id="grade">
  <div class="section-title"><span class="icon">⭐</span>등급 체계</div>
  {grade_html}
  <div class="info-box">
    <h4>적용 장비</h4>
    <div class="tag-list">
      <span class="tag blue">무기 (Weapon)</span>
      <span class="tag blue">악세서리 (Accessory)</span>
      <span class="tag blue">스킬 (Skill)</span>
      <span class="tag blue">목걸이 (Necklace)</span>
      <span class="tag blue">펫 (Pet)</span>
      <span class="tag blue">유물 (Relic)</span>
    </div>
  </div>
</section>

<!-- 장비 / 펫 -->
<section id="equipment">
  <div class="section-title"><span class="icon">⚔️</span>장비 / 펫 / 코스튬</div>
  <p class="note-text" style="margin-bottom:12px">소스: <code>Define.cs</code> EquipmentType / CostumeCategoryType</p>
  {equipment_html}
</section>

<!-- 던전 시스템 -->
<section id="dungeon">
  <div class="section-title"><span class="icon">🏰</span>던전 시스템</div>
  <p class="note-text" style="margin-bottom:12px">소스: <code>Define.cs</code> FieldType enum</p>
  {dungeon_html}
  <div class="info-box">
    <h4>공통 StageType</h4>
    <div class="tag-list">
      {"".join(f'<span class="tag">{m["name"]}</span>' for m in parse_enum(define_content, "StageType") if not m.get("separator"))}
    </div>
  </div>
</section>

<!-- 스킬 시스템 -->
<section id="skill">
  <div class="section-title"><span class="icon">✨</span>스킬 시스템</div>
  <p class="note-text" style="margin-bottom:12px">소스: <code>Define.cs</code> SkillType / TargetRangeType</p>
  {skill_html}
  <div class="info-box">
    <h4>소환 유형 SummonType</h4>
    <div class="tag-list">
      {"".join(f'<span class="tag blue">{m["name"]}</span>' for m in parse_enum(define_content, "SummonType") if not m.get("separator"))}
    </div>
  </div>
</section>

<!-- 퀘스트 -->
<section id="quest">
  <div class="section-title"><span class="icon">📋</span>퀘스트 시스템</div>
  <p class="note-text" style="margin-bottom:12px">소스: <code>Define.cs</code> QuestType / QuestCycleType</p>
  {quest_html}
</section>

<!-- IAP / 광고 -->
<section id="iap">
  <div class="section-title"><span class="icon">💳</span>IAP / 광고</div>
  <p class="note-text" style="margin-bottom:12px">소스: <code>Define.cs</code> ShopCategoryType / eAdType</p>
  {iap_html}
</section>

<!-- 잠금 조건 -->
<section id="lock">
  <div class="section-title"><span class="icon">🔒</span>잠금 조건 시스템</div>
  <p class="note-text" style="margin-bottom:12px">소스: <code>Define.cs</code> LockType / LockConditionType</p>
  {lock_html}
</section>

<!-- 게임 설정 -->
<section id="setting">
  <div class="section-title"><span class="icon">⚙️</span>게임 설정 (SettingType)</div>
  <p class="note-text" style="margin-bottom:12px">소스: <code>Define.cs</code> SettingType</p>
  {setting_html}
</section>

<!-- 스탯 목록 -->
<section id="stat">
  <div class="section-title"><span class="icon">📐</span>스탯 목록 (StatType)</div>
  <p class="note-text" style="margin-bottom:12px">소스: <code>Define.cs</code> StatType</p>
  {stat_html}
</section>

<footer>
  Mushroom Hero 프로젝트 대시보드 &nbsp;·&nbsp; 생성: {generated_at}<br>
  <code>python _dev/generate_dashboard.py</code> 로 재생성
</footer>

</main>
</div>
<script>{JS}</script>
</body>
</html>"""


# ──────────────────────────────────────────────────────────────────────────────
# 실행
# ──────────────────────────────────────────────────────────────────────────────

def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")

    print("Mushroom Hero 대시보드 생성 중...")

    define_content   = read_safe(DEFINE_CS)
    plan_content     = read_safe(PROJECT_ROOT / "_dev/new-game-migration-plan.md")
    migration_phases = parse_migration_plan(plan_content)
    settings         = get_project_settings()
    sdk              = get_sdk_versions()
    packages         = get_unity_packages()
    scenes           = get_scene_list()
    generated_at     = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    if not define_content:
        print("WARNING: Define.cs 를 찾을 수 없습니다 — 게임 설계 섹션이 비어있습니다.")

    html = generate_html(
        migration_phases, settings, sdk, packages,
        scenes, define_content, generated_at
    )

    OUTPUT_FILE.write_text(html, encoding="utf-8")

    done_count  = sum(1 for p in migration_phases.values() for i in p["items"] if i["done"])
    total_count = sum(len(p["items"]) for p in migration_phases.values())
    all_enums   = parse_all_enums(define_content)

    print(f"OK 생성 완료: {OUTPUT_FILE}")
    print(f"   마이그레이션 : {done_count} / {total_count} 항목")
    print(f"   Target SDK  : API {settings.get('targetSdk','N/A')}")
    print(f"   씬           : {len(scenes)}개")
    print(f"   Enum 종류    : {len(all_enums)}개 (Define.cs)")
    print(f"   GMA SDK     : {sdk.get('gma_unity','N/A')}")


if __name__ == "__main__":
    main()

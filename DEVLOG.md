# DEVLOG

## 2026-09-08 — 편집기 환경 구축

### 한 일
- Unity AI 패키지 제거 (NoSubscription 에러 유발, 미사용)
- Cursor ↔ Unity 연동, .csproj 생성, 자동완성 동작 확인

### 결정과 근거
- 편집기 연동에 비공식 패키지 com.unity.ide.cursor 채택.
  Unity 기본 "Cursor (internal)"은 .csproj를 생성하지 않고,
  공식 Visual Studio Editor 패키지는 Mac에 VS/VS Code가 없어 무효.
  대안이 없어 비공식 패키지를 수용. manifest.json에 git 의존성 1건 추가됨.

### 배운 것
- C#은 파일 하나에 문법 에러가 있으면 어셈블리 전체가 빌드에 실패한다.
  "스크립트를 붙일 수 없다" 류의 증상은 먼저 Console에서 컴파일 에러를 찾는다.

-------

# DEVLOG

## 2026-09-09 — 블록 A: 방향키 이동

### 한 일
- Player GameObject 생성(Square 스프라이트, Position 0,0,0)
- PlayerMovement.cs 작성. Update에서 Keyboard.current 입력을 읽어
  transform.position을 직접 이동
- Assets/Scripts, Assets/Sprites 폴더 구조 확보
- 커밋 및 push 완료

### 결정과 근거
- 입력 방식: 새 Input System(Keyboard.current) 사용.
  근거: Unity 6 2D(URP) 템플릿 기본값이며, 블록 B에서 재작업하지 않기 위함.
- moveSpeed = 11 (단위: 초당 월드 유닛)
  근거: 없음에 가까움. Play 모드에서 5, 20을 비교해 화면상 체감으로 고른 값.
  타일 크기(32px)와 카메라 Orthographic Size가 확정되지 않아 기준이 없다.
  블록 D 완료 후 "초당 이동 타일 수" 기준으로 재산정한다.
- 대각선 입력은 벡터 길이가 1을 넘을 때만 정규화.
  근거: 대각선 이동이 약 1.41배 빨라지는 문제를 막기 위함.

### 알게 된 것
- Play 모드에서 바꾼 Inspector 값은 Play 종료 시 폐기된다.
- [SerializeField]로 private 변수를 Inspector에 노출할 수 있다.

### 다음
- 블록 B: Rigidbody2D + Collider2D로 충돌 처리. PlayerMovement 재작성 예정.

------

# DEVLOG

## 2026-09-09 — 블록 A~D 완료, 에셋 조달

### 한 일
- 블록 A: Player(Square) 생성, 방향키 이동
- 블록 B: Rigidbody2D + Collider2D 기반 이동으로 재작성, 벽 충돌
- 블록 C: Cinemachine 카메라 추적
- 블록 D: Tilemap 2층 구성(Ground/Collision), Composite Collider 충돌
- 블록 E: 타일셋 구매

### 결정과 근거
- 입력: 새 Input System(Keyboard.current). Unity 6 2D 템플릿 기본값.
- moveSpeed = 11 (초당 월드 유닛). 근거 약함. Play 모드 체감으로 선택.
  타일 크기·카메라 크기 확정 후 "초당 이동 타일 수" 기준으로 재산정.
- 이동 방식: transform.position 직접 조작 → rb.linearVelocity로 교체.
  근거: 전자는 물리 엔진의 충돌 해소 결과를 매 프레임 덮어써 벽에서 진동이 발생.
- Collision Detection = Continuous.
  근거: Discrete는 물리 스텝 사이 이동을 무시. 대시·넉백 추가 시 관통 위험.
- Composite Collider 2D 사용(Composite Operation = Merge).
  근거: 타일당 콜라이더가 개별로 존재하면 이음매에 캐릭터가 걸림.
- 카메라: Lens Orthographic Size 6, Dead Zone 0.2, Damping 0.5
  근거: Size 6 = 세로 12유닛 = 세로 12타일. Dead Zone 0은 미세 흔들림이
  그대로 보이고, 과하면 반응이 늦음. Damping 1은 체감상 늦어 0.5로 조정.
- Order in Layer 배치 규칙: Ground 0 / Collision 1 / Player·NPC 10 / UI 20+
- 타일 규격: 1타일 = 1유닛. PPU를 타일 픽셀 크기와 일치시켜 유지.
  구매 에셋이 16x16이므로 임포트 시 PPU 16 설정.
- 타일셋 구매 결정($8).
  근거: 무료 체험판은 팩별로 다르지 않고 컬렉션 공용 샘플러 하나이며,
  무료 버전은 비상업 사용만 허용되어 포트폴리오 사용이 회색지대.

### 시행착오
- 타일 팔레트에서 스프라이트 파일명과 타일 애셋명이 충돌해 참조가 깨짐.
  스프라이트는 Sprite_ 접두사, 타일 애셋은 별도 폴더로 분리해 해결. 약 40분 소요.

### 에셋 출처

| 에셋명 | 제작자 | 출처 | 라이선스 | 표기 의무 |
|---|---|---|---|---|
| The Fan-tasy Tileset (Premium) 1.5.8 | Ventilatore | https://ventilatore.itch.io/the-fantasy-tileset | 상업·비상업 사용 가능, 재판매·재배포 금지 | 없음(권장) |

### 다음
- 블록 F: 타일셋 임포트, PPU 16 설정, 슬라이싱, 타일 애셋 교체
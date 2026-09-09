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
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
# Rhythm_game_demo

Unity 6.3 LTS / URP 2D. 월드를 탐색하다 보스전을 리듬게임으로 치르는 2D 어드벤처 데모.
포트폴리오용이며, 상세 상태와 결정 근거는 PROJECT_STATE.md와 DEVLOG.md에 있다.
작업 전 이 두 문서를 먼저 읽을 것.

## 기술 규격 (변경 금지)

- 타일 16px, 1타일 = 1월드유닛
- 스프라이트 임포트: PPU 16 / Filter Mode Point / Compression None / Generate Physics Shape 해제
- 건물·나무 등 오브젝트: Pivot Bottom
- Sprite Sort Point는 Player, NPC를 포함한 모든 월드 스프라이트에 Pivot을 적용한다
  (현재 Player·NPC는 임시 Square라 결과가 같지만, 캐릭터 스프라이트 교체 시 어긋난다)
- Order in Layer: Ground 0 / Detail 1 / Collision 2 / Player·NPC·건물 10 / UI 20+
- Y축 정렬: Renderer2D의 Transparency Sort Mode = Custom Axis (0,1,0)
- 카메라: Cinemachine, Orthographic Size 8
- 입력: 새 Input System. 이동 방향키, 상호작용 E, 리듬 판정 스페이스

## 콜라이더 규칙

- 건물: 밑동만 막는다. 높이 1~1.5, 폭은 스프라이트 폭에서 0.3~0.5를 뺀 값
  건물 전체를 덮으면 지붕 근처에서 멀찍이 막혀 어색하다
- 트리거 영역(NPC 상호작용, 보스 입구): Is Trigger 체크, 감점·충돌 없음
- 콜라이더 최종 크기는 Scene 뷰에서 눈으로 확인한 뒤 확정한다

## 금지 사항

- Conductor의 bpm(120), firstBeatOffset(0.3515) 변경 금지
- RhythmBattle의 perfectWindowMs(50), goodWindowMs(100), winHitRate(0.6) 변경 금지
  위 값들은 실측에 근거한 수치다. 근거는 DEVLOG.md와 PlayLogs/의 CSV에 있다.
- MapBuilder의 Build Base Tiles 실행 금지. 칠해둔 타일이 전부 지워진다
- PlayLogs/ 폴더 수정·삭제 금지. 기획서의 근거 원본이다
- 판정·오디오 관련 숫자는 사용자가 직접 정한다. 제안은 하되 임의로 바꾸지 말 것

## 작업 규칙

- 씬을 수정하는 작업은 한 번에 하나씩. 시작 전 커밋 상태를 확인할 것
- Unity 에디터가 열려 있어야 한다. Play 모드 중에는 설정을 바꾸지 말 것
- 배치의 미적 판단(어디가 보기 좋은가)은 사용자가 한다. 구조와 설정만 자동화 대상
- 커밋 메시지는 한글

## 에셋 원본 경로

/Users/relaxman/Documents/Assets/The Fan-tasy Tileset (Premium)/Art

하위 폴더: Buildings, Characters, Fences and Walls, Ground Tilesets,
Map Backgrounds, Props, Rock Slopes, Rocks, Shadows, Tall Grass Tilesets,
Trees and Bushes, Water and Sand

- 이 폴더는 읽기 전용으로 참조한다. 프로젝트 밖에 있다
- 필요한 파일만 Assets/Sprites 아래로 복사해서 쓴다
- 팩 전체를 Assets에 넣지 말 것 (임포트 부하, .meta 파일 폭증)
- 명명 규칙: Tileset_ = 지형 기초, Atlas_ = 격자 소품, 접두사 없음 = 개별 오브젝트

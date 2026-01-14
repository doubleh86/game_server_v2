## GameServer Framework

- AdminWeb : 운영툴
- ApiServer : 웹 API 서버
- DataTableLoader : 데이터 테이블 로더
- NotifyServer : 알림 서버
- Scheduler : 스케쥴 기반 컨텐츠 관리용 

간단 소개
- 소켓: SuperSocket
- API: ASP.NET Core (net10.0)
- 관리자 UI: Blazor (Blazor Bootstrap)
- 데이터 접근: Dapper
- 데이터베이스: MySQL(Shared DB), SQL Server(Data 테이블 + Game/Main DB 스키마)

ToDo
- 테이블 데이터 파일 저장 후 적용 방식으로 변경
- 현재 버전 저장 후 관리

- 이벤트 시스템 추가
  1. 전체 기간 
  2. 특정 요일 오픈 (24시간)
  3. 특정 시간대 오픈
  4. 특정 요일 / 특정 시간대

- 타임존 관련 시스템 확장
 

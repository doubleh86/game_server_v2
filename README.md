## GameServer Framework

간단 소개
- 소켓: SuperSocket
- API: ASP.NET Core (net9.0)
- 관리자 UI: Blazor (Blazor Bootstrap)
- 데이터 접근: Dapper
- 데이터베이스: MySQL(Shared DB), SQL Server(Data 테이블 + Game/Main DB 스키마)

이 저장소에는 여러 하위 프로젝트가 포함됩니다: ApiServer(HTTP), NotifyServer, Scheduler, AdminWeb, DbContext, NetworkProtocols 등

--------------------------------------------------------------------------------

시작하기(사용 방법)

사전 준비물
- .NET SDK 9.0+ 설치
- MySQL 8.0+ (Shared DB 용)
- SQL Server 2019+ (Data/Main DB 용) – Developer 또는 Docker 에디션 사용 가능
- Redis 6+ (로컬 개발 시 단일 인스턴스로 충분)

1) 데이터베이스 준비
- 로컬 인스턴스에 다음 데이터베이스를 생성합니다(이름은 설정에서 변경 가능):
  - MySQL: shared_db
  - SQL Server: data_db(정적 테이블), main_db(런타임 게임 테이블)
- Sql/ 디렉터리의 스크립트를 적용합니다:
  - Sql/SharedDB -> MySQL Shared DB 스키마(예: account_info, event_info, game_db_info)
  - Sql/DataDB -> SQL Server Data DB 스키마(예: ItemInfoTable, PlayerLevelTable, GameEventTable)
  - Sql/MainDB -> SQL Server Main DB 스키마(예: game_user, inventory_info, mail_info)
  - Sql/AdminDB -> 선택 사항(관리용 스키마)

2) 연결 정보 설정
- ApiServer/Settings/sqlSettings.json
  - SharedDbContext는 기본적으로 MySQL을 사용하도록 설정되어 있습니다:
    {
      "Ip": "localhost",
      "Port": 3306,
      "DatabaseName": "shared_db",
      "UserId": "root",
      "Password": "<your_mysql_password>",
      "IsMySql": "true"
    }
  - DataTableDbService는 SQL Server를 대상으로 합니다:
    {
      "Ip": "127.0.0.1",
      "Port": 1433,
      "DatabaseName": "data_db",
      "UserId": "SA",
      "Password": "<your_sa_password>",
      "modelAssembly": "DataTableLoader.Models, DataTableLoader"
    }
- ApiServer/Settings/redisSettings.json
  - 로컬 또는 원격 Redis 인스턴스 주소를 지정합니다.
- ApiServer/appsettings.json
  - 로깅(Serilog) 및 서비스 옵션(예: TimeZone)을 조정할 수 있습니다.

3) API 서버 빌드 및 실행
- 저장소 루트에서:
  dotnet build
  dotnet run --project ApiServer

Rider/VS에서 실행 시 ApiServer를 스타트업 프로젝트로 지정하세요. 부팅 시 서버는 다음을 수행합니다:
- appsettings.json 및 Settings/*.json 에서 설정 로드
- Redis, SQL 연결, 데이터 테이블 로더 초기화
- Shared DB에서 이벤트 로드

4) 헬스체크 및 인증 엔드포인트 테스트
- 헬스/테스트 엔드포인트:
  curl -X GET http://localhost:5000/auth/test

- 계정 조회/생성 및 토큰 발급:
  엔드포인트: POST /auth/get-account-info
  요청 바디 모델: NetworkProtocols.WebApi.Commands.Auth.AuthCommand.Request
  예시:
  curl -X POST http://localhost:5000/auth/get-account-info \
       -H "Content-Type: application/json" \
       -d '{
             "LoginId": "test_user_001",
             "AccountId": 0,
             "Sequence": 1,
             "SubSequence": 1
           }'

응답 예시(요약):
{
  "ResultCode": 0,
  "ServerTime": 1730000000000,
  "AccountId": 12345,
  "AccountType": 0,
  "Token": "<access_token>"
}

이후 게임용 엔드포인트에서도 AccountId, Token을 재사용할 수 있습니다(모델과 URL은 NetworkProtocols/WebApi/Commands/* 참고).

5) 구성 요소 위치 안내
- API 엔드포인트: ApiServer/Controllers
  - AuthController: /auth/test, /auth/get-account-info
- 요청/응답 모델: NetworkProtocols/WebApi/Commands
- 데이터베이스 컨텍스트: DbContext/* (SharedContext는 기본적으로 MySQL 사용)
- 설정 파일: ApiServer/Settings 및 ApiServer/appsettings*.json
- 데이터 테이블 로더: DataTableLoader/* (SQL Server Data DB에서 읽기)

6) 테스트 실행
- 저장소 루트에서:
  dotnet test
- 통합 테스트 헬퍼 예시: ApiServerTest/ApiTest/ApiTestHelper.cs

문제 해결 가이드
- MySQL 연결 오류: 3306 포트 개방 여부와 sqlSettings.json의 자격 증명을 확인하세요.
- SQL Server 연결 오류: Docker 사용 시 1433 포트 매핑 및 SA 비밀번호 설정을 확인하세요.
- Redis 연결 불가: ApiServer/Settings/redisSettings.json에서 올바른 주소로 수정하세요.
- 타임존 문제: ApiServer/appsettings.json의 "ServiceTimeZone" 확인, ServerFramework.CommonUtils.DateTimeHelper 참고.
- 데이터 테이블 비어 있음: DataHelper.ReloadTableData() 로그가 성공인지 확인하고 DataDB에 데이터가 존재하는지 확인하세요.

--------------------------------------------------------------------------------

ToDo
- 테이블 데이터 파일 저장 후 적용 방식으로 변경
- 현재 버전 저장 후 관리

- 이벤트 시스템 추가
  1. 전체 기간 
  2. 특정 요일 오픈 (24시간)
  3. 특정 시간대 오픈
  4. 특정 요일 / 특정 시간대

- 타임존 관련 시스템 확장
- 스테미너 시스템??
 
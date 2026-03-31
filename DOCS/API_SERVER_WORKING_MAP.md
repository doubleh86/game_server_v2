# API_SERVER_WORKING_MAP

## 1) 목적
`ApiServer` 프로젝트의 실행 구조, 코드 진입점, 의존성, 기능 확장 경로를 빠르게 파악하기 위한 작업 지도 문서.

## 2) 프로젝트 개요

- 프로젝트: `ApiServer/ApiServer.csproj`
- 프레임워크: `net10.0` (ASP.NET Core Web)
- 기본 엔드포인트: `http://0.0.0.0:18888` (`ApiServer/appsettings.json`)
- 주요 진입점: `ApiServer/Program.cs`

## 3) 실행 흐름

1. `WebApplication.CreateBuilder(args)`
2. `AddControllers()` 등록
3. Kestrel 제한 설정 (BodySize/Rate/KeepAlive)
4. DI 등록
   - `ApiServerService` (Singleton)
   - `EventService` (Singleton)
5. 앱 빌드 후 `MapControllers()`
6. `InitializeServices()` 실행
   - `ApiServerService.Initialize()`
   - 타임존 초기화
   - DataTable 로딩 (`DataHelper.Initialize`, `ReloadTableData`)
7. `InitializeEventService()` 실행
   - Shared DB에서 이벤트 목록 로드
   - `EventService.Initialize(eventList)`
8. `app.Run()`

## 4) 핵심 디렉터리 맵

- `ApiServer/Controllers`
  - API 라우팅 진입점
  - 컨트롤러:
    - `AuthController` (`/Auth/test`, `/Auth/get-account-info`)
    - `UserController` (`/User/get-user-info`)
    - `InventoryController` (`/Inventory/use-item`)
    - `ShopController` (`/Shop/shop-buy`)
    - `MailController` (`/Mail/get-mail-box`, `/Mail/open-mail`)
    - `CheatController` (`/Cheat/set-cheat-server`, `/Cheat/change-server-time`)

- `ApiServer/GameService/Handlers/GameHandlers`
  - 기능별 비즈니스 로직
  - `AuthHandler`, `UserHandler`, `InventoryHandler`, `ShopHandler`, `MailHandler`
  - 공통 초기화/모듈 조립: `BaseHandler`

- `ApiServer/GameService/GameModules`
  - DB 모듈 계층
  - `AssetInfoModule`, `GameUserModule`, `InventoryModule`, `MailModule`
  - 모듈 팩토리/매니저: `Manager/*`

- `ApiServer/Services`
  - `ApiServerService`: 설정/로깅/Redis/DB정보/gRPC 초기화
  - `EventService`: 이벤트 캐시/조회
  - `GRpcService`: Scheduler 구독 클라이언트
  - `RedisServiceFactory`

- `ApiServer/Utils`
  - 예외/유틸리티
  - `ApiServerException`
  - `GameUtils/RefreshDataHelper`

## 5) 주요 의존성

- 내부 프로젝트 참조:
  - `DataTableLoader`
  - `DbContext`
  - `NotifyServer`
- 외부 인프라:
  - Redis (`Settings/redisSettings.json`)
  - SQL DB (`Settings/sqlSettings.json`)
  - Scheduler gRPC (`ScheduleGRpcAddress`, 기본 `http://localhost:6000`)
- 공통 프레임워크:
  - `ServerFramework` (간접 의존, 실제 클래스 다수 사용)

## 6) 설정 파일 체크포인트

- `ApiServer/appsettings.json`
  - `ServiceTimeZone`
  - `ScheduleGRpcAddress`
  - `Serilog`
  - `Kestrel.Endpoints.Http.Url`

- `ApiServer/Settings/sqlSettings.json`
  - `SqlServerDbSettings.ConnectionInfos`
  - 핵심 키:
    - `SharedDbContext`
    - `DataTableDbService`
  - 샘플은 `sqlsettings.Default.json` 참고

- `ApiServer/Settings/redisSettings.json`
  - `RedisSettings.ConnectionInfos.SessionRedisService`
  - `RedLockConnections`
  - Lock/Session 만료 옵션
  - 샘플은 `redisSettings.Default.json` 참고

## 7) 코드 변경 작업 가이드

- 새 API 추가
1. `NetworkProtocols/WebApi/Commands/*` 요청/응답 계약 추가
2. `ApiServer/Controllers/*` 엔드포인트 추가
3. `ApiServer/GameService/Handlers/*` 비즈니스 처리 추가
4. 필요 시 `DbContext/*` 쿼리/프로시저 커맨드 추가
5. `ApiServerTest/ApiTest/*` 테스트 추가

- 기존 API 수정
1. 컨트롤러 입력/출력 모델 영향 범위 확인
2. Handler 내부 모듈 초기화/트랜잭션 흐름 확인
3. `GameResultCode` 및 에러 응답 일관성 유지

- 데이터 초기화/리로드 관련 수정
1. `Program.cs`의 `DataHelper.ReloadTableData()` 영향 확인
2. `DataTableLoader` 모델/매핑 동기화 확인

## 8) 테스트 맵

- API 통합 테스트: `ApiServerTest/ApiTest/*`
- Handler 테스트: `ApiServerTest/HandlerTest/*`
- 기본 대상 서버 URL: `http://127.0.0.1:18888` (`ApiServerTest/ApiTest/LoginInfo.cs`)

## 9) 로컬 실행 체크리스트

1. `Settings/sqlSettings.json`, `Settings/redisSettings.json` 실파일 준비
2. Redis 실행 (`127.0.0.1:6379`)
3. DB 접속 확인 (Shared/DataTable 관련 DB)
4. Scheduler 실행 여부 확인 (gRPC 구독 대상)
5. `ApiServer` 실행 후 `/Auth/test`로 헬스성 확인

## 10) 주의사항

- `ApiServerService.Initialize()`에서 Redis/gRPC/DB 초기화가 연쇄적으로 수행되므로 설정 누락 시 부팅 실패 가능
- Shared DB는 환경에 따라 MySQL/SQLServer 설정이 다를 수 있어 `IsMySql` 플래그 및 포트 확인 필요
- 타임존은 기본 `Asia/Tokyo`로 설정되어 있어 운영 기준과 다르면 이벤트/스케줄 판단에 영향 가능

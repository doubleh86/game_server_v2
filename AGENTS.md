# ApiServer Codex 작업 지침

## 프로젝트 범위
- 주요 프로젝트: `ApiServer/ApiServer.csproj`
- 솔루션: `RealTimeServer.sln`
- 테스트 프로젝트: `ApiServerTest/ApiServerTest.csproj`
- ApiServer가 사용하는 공유 프로젝트:
  - `NetworkProtocols`: 요청/응답 프로토콜, enum 정의
  - `DbContext`: SQL 접근, 저장 프로시저 래퍼, DB 결과 모델, `BaseModule`
  - `DataTableLoader`: `DataHelper` 기반 정적 게임 테이블 로딩
  - `NotifyServer`: Redis 세션 서비스 의존성

## 실행 환경
- 현재 프로젝트들은 `net10.0`을 대상으로 한다.
- 빌드나 테스트를 실행하려면 `net10.0`을 지원하는 .NET SDK가 필요하다.
- `**/sqlSettings.json`, `**/redisSettings.json`은 로컬 환경 전용 설정일 수 있으며 git에서 무시된다. DB/Redis 접속 정보가 들어갈 수 있으므로 함부로 수정하거나 커밋하지 않는다.
- `bin/`, `obj/`, `Logs/`, `.idea/`, `.vscode/`, `.DS_Store` 같은 생성물은 커밋하지 않는다.

## 자주 쓰는 명령
- ApiServer 빌드:
  - `dotnet build ApiServer/ApiServer.csproj`
- 전체 솔루션 빌드:
  - `dotnet build RealTimeServer.sln`
- ApiServer 실행:
  - `dotnet run --project ApiServer/ApiServer.csproj`
- ApiServer 테스트 실행:
  - `dotnet test ApiServerTest/ApiServerTest.csproj`

## 검증 시 주의점
- 현재 로컬 SDK가 .NET 9라면 `ApiServer.csproj`의 대상 프레임워크가 `net10.0`이므로 빌드가 실패한다.
- `ApiServerTest`에는 `ApiServerService`, Redis, SQL 설정, Shared DB context, 이벤트 데이터를 초기화하는 통합 테스트 성격의 테스트가 섞여 있다.
- 로컬 Redis/DB 설정이 준비되어 있지 않으면 `dotnet test`가 안전하거나 재현 가능하다고 가정하지 않는다.
- 호환되는 SDK가 설치되어 있다면 최소한 수정한 프로젝트의 빌드는 확인한다. DB/Redis가 없어 테스트를 실행하지 못했다면 그 이유를 명확히 남긴다.
- Controller 또는 Handler를 추가하거나 주요 동작을 변경할 때는 `ApiServerTest`에 대응 테스트 코드를 함께 추가한다.

## 런타임 흐름
- `Program.cs`에서 Controller, `ApiServerService`, `EventService`를 등록한다.
- 시작 시 `ApiServerService.Initialize()`를 호출한 뒤 `DataTableDbService` 설정으로 `DataHelper`를 초기화하고 테이블 데이터를 다시 로드한다.
- 이후 `SharedDbContextWrapper`를 통해 이벤트 데이터를 읽어 `EventService`를 초기화하고 앱을 실행한다.
- Controller는 `ApiControllerBase`를 상속한다.
- 인증이 필요한 게임 API는 Controller에서 `_Initialize(request)`를 호출해 다음 작업을 수행한다.
  - Redis에서 세션 데이터 조회
  - 토큰 검증
  - sequence/subSequence 검증
  - 세션 sequence 갱신
  - 계정 단위 분산락 획득
  - master/slave DB 접속 정보 반환
- Controller는 Handler를 만들고 `InitializeModulesAsync(...)`를 호출한 뒤 비즈니스 로직을 실행하고 `_OkResponse(...)`로 응답을 직렬화한다.

## 아키텍처 규칙
- Controller는 얇게 유지한다. 요청 흐름 제어, Handler 생성, 응답 매핑, 예외 매핑 정도만 담당한다.
- 새 인증형 게임 API Controller를 만들 때는 `ApiServer/Controllers/InventoryController.cs`의 구조를 기본 참고 대상으로 삼는다.
- 비즈니스 로직은 `ApiServer/GameService/Handlers` 아래 Handler에 둔다.
- DB 중심의 게임 기능은 `ApiServer/GameService/GameModules` 아래 Module에 둔다.
- 실제 DB 접근 구현과 저장 프로시저 래퍼는 `DbContext` 아래에 둔다.
- 요청/응답 프로토콜 클래스는 `NetworkProtocols/WebApi` 아래에 둔다.
- 정적 테이블 조회는 `DataHelper`와 `DataTableLoader.Models`의 테이블 모델을 사용한다.

## Handler와 Module 규칙
- 새 게임 Handler는 일반적으로 `BaseHandler`를 상속한다.
- Handler에서 게임 DB Module을 사용한다면 `_GetModule<T>()` 호출 전에 `InitializeModulesAsync(masterDbInfo, slaveDbInfo, isRefreshResponse)`를 먼저 호출해야 한다.
- 향후 Module 초기화 로직을 재정의할 경우 base 초기화 동작을 유지한다.
- `_GetRefreshDataHelper()`는 refresh 응답을 지원하는 API에서만 사용한다.
- 새 Module은 `IGameModule`을 구현해야 한다.
- 새 게임 DB Module을 만들 때는 `ApiServer/GameService/GameModules/InventoryModule.cs`의 구조를 기본 참고 대상으로 삼는다.
- 새 Module은 `GameDbModuleFactory`에 등록해야 한다.
- 새 Module이 새로운 Sub DB Context 타입을 필요로 하면 `DbContext/Common/Models/BaseModule.cs`의 `SubDbContextFactory.CreateSubDbContext`에도 등록해야 한다.

## 세션, 락, 응답 규칙
- 계정 세션 검증이 필요한 API에서 `_Initialize(request)`를 우회하지 않는다.
- 인증형 Controller action은 `InventoryController.UseInventoryItem`처럼 `_Initialize(request)`, Handler 생성, `InitializeModulesAsync(...)`, Handler 호출, `_OkResponse(...)`, 예외별 `_ErrorResponse(...)` 흐름을 따른다.
- `ApiControllerBase`의 sequence/subSequence 검증 흐름을 유지한다.
- 게임 상태를 변경하는 API에서는 계정 단위 분산락 흐름을 유지한다.
- 예상 가능한 게임/시스템 오류는 `GameResultCode`를 포함한 `ApiServerException`을 사용한다.
- Controller는 `ApiServerException`을 `_ErrorResponse(...)`로 매핑한다.
- 기존 Controller가 DB 예외를 별도 처리하고 있다면 같은 방식으로 DB 오류 result code에 매핑한다.
- 응답은 `JsonSerializer.Serialize`로 직접 직렬화한다. 응답 모델은 `NetworkProtocols`의 계약과 호환되게 유지한다.

## 데이터와 설정
- `ApiServerService.Initialize()`는 다음 파일을 읽는다.
  - `appsettings.json`
  - `Settings/redisSettings.json`
  - `Settings/sqlSettings.json`
- `Default`가 붙은 설정 파일은 예시/기본값 성격이다. 로컬 접속 정보가 들어간 파일보다 기본값 파일이나 문서를 수정하는 것을 우선한다.
- `appsettings.json`에는 `ServiceTimeZone`, Kestrel endpoint, Serilog, `ScheduleGRpcAddress` 등이 설정되어 있다.
- `SharedDbContext`는 `SharedDbContextWrapper.SetDefaultServerInfo`를 통해 전역 설정된다.

## 코딩 스타일
- 기존 C# 스타일을 따른다. file-scoped namespace, async `Task` 메서드, 도메인 오류용 `ApiServerException`, 작은 Handler/Module 구성을 유지한다.
- 프로젝트는 `<Nullable>warnings</Nullable>`를 사용하므로 nullable 경고를 고려한다.
- 게임/API 동작을 수정할 때 광범위한 리팩터링은 피한다.
- 기존 한국어 주석이나 region 이름은 필요한 경우가 아니면 유지한다.
- 관련 없는 포맷 변경은 만들지 않는다.

## 변경 작업 체크리스트
- 새 endpoint를 추가할 때:
  - `NetworkProtocols/WebApi/Commands`에 프로토콜 모델을 추가하거나 수정한다.
  - `ApiServer/Controllers`에 Controller action을 추가한다.
  - 비즈니스 로직은 Handler에 둔다.
  - DB 조회/변경은 Module을 통해 처리한다.
  - `ApiServerTest/ApiTest` 또는 `ApiServerTest/HandlerTest`에 대응 테스트 코드를 추가한다.
  - 로컬 DB/Redis 환경이 준비되어 있다면 추가한 테스트를 실행한다.
- 새 Handler를 추가할 때:
  - `ApiServerTest/HandlerTest`에 Handler 단위 테스트를 추가한다.
  - 기존 테스트 파일에 섞지 말고 Handler 이름에 대응되는 새 `.cs` 파일을 생성한다.
  - 예: `FooHandler`를 추가하면 `ApiServerTest/HandlerTest/FooHandlerTest.cs`를 만든다.
  - 기존 `HandlerTestBase` 초기화 흐름을 참고한다.
  - DB/Redis 의존성이 강한 경우에도 최소한 테스트 골격과 검증 의도를 남긴다.
- DB 기반 게임 기능을 추가할 때:
  - DbContext 저장 프로시저 래퍼를 추가하거나 확장한다.
  - Game Module을 추가하거나 확장한다.
  - 필요하면 Module/Context factory 등록을 갱신한다.
  - inventory, asset, user 상태가 바뀌면 refresh data 갱신을 처리한다.
- 보상 로직을 추가할 때:
  - 먼저 `RewardHandler`, `RewardTypeEnums`, `AssetTypeEnums`, `ItemTypeEnums`, 테이블 데이터 모델을 확인한다.

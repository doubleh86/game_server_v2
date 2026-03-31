# WORKING_MAP

## 1) 프로젝트 목적
이 저장소는 게임 서버 예제/실서비스 골격을 담은 멀티 프로젝트 솔루션이다.  
REST API, 실시간 Notify, 스케줄러, 어드민 웹, DB 컨텍스트, 프로토콜, 테스트 프로젝트가 분리되어 있다.

## 2) 솔루션 구성 맵

- `ApiServer`
  - 역할: 게임 REST API 서버
  - 주요 진입점: `ApiServer/Program.cs`
  - 컨트롤러: `Controllers/*` (`Auth`, `User`, `Inventory`, `Shop`, `Mail`, `Cheat`)
  - 비즈니스 처리: `GameService/Handlers/*`
  - 의존: `DbContext`, `DataTableLoader`, `NotifyServer`
  - 기본 포트: `18888` (`appsettings.json` Kestrel)

- `NotifyServer`
  - 역할: 실시간 소켓 서버(SuperSocket)
  - 주요 진입점: `NotifyServer/Program.cs`
  - 커맨드 처리: `NetworkCommand/Ping.cs`, `NetworkCommand/Test.cs`
  - 기본 포트: `18080` (`appsettings.json` serverOptions)

- `Scheduler`
  - 역할: 스케줄 루프 + gRPC 구독 엔드포인트 제공
  - 주요 진입점: `Scheduler/Program.cs`, `Scheduler/Main/ScheduleMain.cs`
  - gRPC 서비스: `Services/gRPCService/ScheduleGrpcServiceImpl.cs`
  - 기본 포트: `6000` (`appsettings.json` GrpcPort)

- `AdminWeb`
  - 역할: Blazor 기반 운영 툴
  - 주요 진입점: `AdminWeb/Program.cs`
  - 화면: `Components/Pages/*`
  - 서비스: `Services/*`
  - 개발 실행 URL: `http://localhost:5081`, `https://localhost:7146`

- `DbContext`
  - 역할: DB 접근 계층(Shared/Main/Admin/Game)
  - 구조: Context + Procedure/Query Command 단위 분리
  - 주요 폴더: `SharedContext`, `MainDbContext`, `AdminDbContext`, `GameDbContext`

- `DataTableLoader`
  - 역할: 데이터 테이블 모델/로더
  - 주요 위치: `Models/*`, `Utils/DataTableDbService.cs`
  - API/Admin/Scheduler 초기화 시 공통 사용

- `NetworkProtocols`
  - 역할: Socket/WebAPI/gRPC 계약(공유 DTO/Command)
  - gRPC proto: `gRPC/ScheduleSteam.proto`

- `ApiServerTest`
  - 역할: API/핸들러 테스트(NUnit)
  - 기본 API 대상 URL: `http://127.0.0.1:18888`

- `ClientTest`
  - 역할: TCP 클라이언트 시뮬레이션
  - 기본 Notify 대상 포트: `18080`

- `Sql`
  - 역할: DB 스키마/프로시저 스크립트
  - 구분: `AdminDB`, `SharedDB`, `MainDB`, `DataDB`

## 3) 런타임 의존 관계

- 외부 의존:
  - `../GSFramework/ServerFramework` 프로젝트 참조 필요
- 인프라 의존:
  - Redis (`Settings/redisSettings*.json`)
  - SQL Server + 일부 SharedDbContext는 MySQL 모드 지원(`IsMySql`)
- 서비스 간 연동:
  - `ApiServer` -> `Scheduler` gRPC 구독 (`ScheduleGRpcAddress`)
  - `ApiServer`/`NotifyServer` -> Redis 사용
  - `ApiServer`/`AdminWeb`/`Scheduler` -> DataTable 로딩 + DB Context 사용

## 4) 로컬 작업 시작 순서(권장)

1. DB 스키마 적용 (`Sql/*`)
2. Redis 실행 (기본 `127.0.0.1:6379`)
3. 설정 파일 준비
   - 각 프로젝트의 `Settings/*.Default.json`을 실제 `*.json`으로 복사/수정
4. `Scheduler` 실행 (gRPC 6000)
5. `NotifyServer` 실행 (TCP 18080)
6. `ApiServer` 실행 (HTTP 18888)
7. `AdminWeb` 실행 (운영 UI)
8. 필요 시 `ApiServerTest`, `ClientTest` 실행

## 5) 주요 설정 파일 체크포인트

- `ApiServer/appsettings.json`
  - `ServiceTimeZone`
  - `ScheduleGRpcAddress`
  - `Kestrel:Endpoints:Http:Url`
- `ApiServer/Settings/sqlSettings.json`
  - `SharedDbContext`, `DataTableDbService`
- `ApiServer/Settings/redisSettings.json`
  - 세션 채널, 락 설정
- `NotifyServer/appsettings.json`
  - `serverOptions.listeners.port`
- `Scheduler/appsettings.json`
  - `GrpcPort`, `ServiceTimeZone`
- `AdminWeb/Settings/sqlSettings.json`
  - `AdminDbContext`, `SharedDbContext`, `DataTableDbService`

## 6) 코드 작업 진입 가이드

- API 엔드포인트 추가:
  - `NetworkProtocols/WebApi/Commands/*` 계약 추가
  - `ApiServer/Controllers/*` 라우트 추가
  - `ApiServer/GameService/Handlers/*` 비즈니스 로직 구현
  - 필요 시 `DbContext/*` 쿼리/프로시저 커맨드 추가
- DB 연동 추가:
  - `Sql/*` 스키마/프로시저 반영
  - `DbContext` 결과 모델 + 커맨드 추가
- 데이터 테이블 추가:
  - `DataTableLoader/Models` 모델 추가
  - `DataTableDbService` 및 매핑 등록
- 실시간 패킷/명령 추가:
  - `NetworkProtocols/Socket/*` 계약 추가
  - `NotifyServer/NetworkCommand/*` 처리 구현
  - `ClientTest`로 회귀 확인
- 운영 UI 추가:
  - `AdminWeb/Components/Pages/*` + `Services/*` 확장

## 7) 테스트 맵

- API 통합 테스트: `ApiServerTest/ApiTest/*`
- 핸들러 단위 테스트: `ApiServerTest/HandlerTest/*`
- TCP 클라이언트 테스트: `ClientTest/*`

## 8) 주의사항

- 솔루션에 외부 경로 프로젝트(`ServerFramework`)가 포함되어 있어, 경로 누락 시 빌드 실패 가능
- `Settings/*.json` 실파일이 없으면 초기화 실패 가능
- 타임존 기본값이 프로젝트별로 다를 수 있으니(`Asia/Tokyo`, `Asia/Seoul`) 환경 기준 통일 필요

# ServerFramework Example (개인 프로젝트)

ServerFramework를 실제로 적용해보기 위한 **1인 제작 예제용 서버 프로젝트**입니다.  
프레임워크에서 제공하는 공통 기능(요청 파이프라인, 로깅/예외 처리, 표준 응답 규약, DB/Redis 연동, 분산락, gRPC 기반 등)을 “실제 API/운영/배치/테스트” 흐름에 연결해 **동작하는 형태로 검증**하는 목적을 갖습니다.

## 목적
- ServerFramework의 핵심 모듈을 단순 샘플이 아닌 **실행 가능한 서비스 구조**에서 검증
- REST API / 실시간 통신 / 운영툴 / 스케줄러를 분리해, 프레임워크가 다양한 프로세스에서 재사용 가능한지 확인
- 테스트 프로젝트를 통해 기능 재현 및 회귀 테스트 기반 마련

## 구성
- **ApiServer**: REST API 예제 서버(인증/유저/인벤토리/상점/메일 등)
- **NotifyServer**: 실시간 알림/통신 예제 서버
- **AdminWeb**: Blazor 기반 백오피스 예제
- **Scheduler**: 배치/스케줄 작업 예제
- **DbContext**: DB 접근 공통 레이어(예제 구현)
- **NetworkProtocols**: Socket/Web API/gRPC 프로토콜 계약 중앙 관리
- **DataTableLoader**: 게임 데이터 테이블 로딩/가공 도구 예제
- **ApiServerTest / ClientTest**: REST 및 TCP 통신 시나리오 테스트

## 특징
- 1인 개발로 설계/구현/테스트까지 일관된 기준으로 구성
- 공통 모듈(프로토콜/DB/테스트)을 분리해 “확장 가능한 형태”로 예제를 구성
- 실제 서버처럼 동작하는 구조로 프레임워크의 재사용성과 운영 관점의 유효성을 확인

# 프레임워크 위치 
 - https://github.com/doubleh86/server_framework
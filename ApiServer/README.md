## 구조
1. DB 처리는 Module을 만들어서 한다.
2. 비지니스 로직 처리는 Handler 를 만들어서 한다.
3. DbContext 도 가능하면 모듈에 맞춰서 생성
    - InventoryModule 은 InventoryDbContext 만 가지도록
4. Handler 는 여러개의 모듈을 가질 수 있다.
5. BaseHandler GameUserModule 에서 생성한다.
   - InitializeModulesAsync() 구현 시 base.InitializeModulesAsync() 호출 권장 
6. 모듈 추가는 InitializeModulesAsync() 구현에서 처리
7. SubDbContext 추가 시 BaseModule.cs 에 있는 SubDbContextFactory.CreateSubDbContext() 추가 필요.

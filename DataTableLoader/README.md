## Table 추가 방법
1. DB에 데이터 테이블 생성
2. 테이블과 연결된 클래스 생성 및 BaseData 상속
3. DataTableDbService에 DbSet 추가
4. _RegisterTableDbSet() 에 _tableMapping 추가
5. OnModelCreating() 에 entity 연결

## ModelAssembly
Model Data 가 있는 Assembly 위치 지정 필요
```json
{
    "DataTableDbService":
    {
        "Ip" : "127.0.0.1",
        "Port": 1433,
        "DatabaseName": "data_db",
        "UserId": "sa",
        "Password": "",
        "modelAssembly": "DataTableLoader.Models, DataTableLoader"
    }    
}


```
     
namespace DbContext.AdminDbContext.DbResultModel;

public class AdminUserDbModel
{
    public long uid { get; set; }
    public int admin_type { get; set; }
    public string user_id { get; set; }
    public string user_password { get; set; }
    public DateTime create_date { get; set; }
    public DateTime update_date { get; set; }
}
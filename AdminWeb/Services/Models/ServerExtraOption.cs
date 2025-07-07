namespace AdminWeb.Services.Models;

public class ServerExtraOption
{
    public bool UseDefaultSuperUser { get; set; } = false;
    public string ServerEnv { get; set; } = "Dev";
    public string DateTimeVisibleType { get; set; } = "yyyy-MM-dd HH:mm:ss";
}
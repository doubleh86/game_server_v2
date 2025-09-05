namespace AdminWeb.Services;

public class LanguageService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LanguageService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetAcceptLanguage()
    {
        var language = _httpContextAccessor.HttpContext?.Request.Headers.AcceptLanguage.ToString();
        if(string.IsNullOrEmpty(language) == true)
            return "en";
        
        return language.Split(',')[0];
    }
}
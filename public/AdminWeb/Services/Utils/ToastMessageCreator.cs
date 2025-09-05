using BlazorBootstrap;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.CommonUtils.Helper;

namespace AdminWeb.Services.Utils;

public static class ToastMessageCreator
{
    public static ToastMessage CreateToastWithTitle(ToastType type, string title, string message)
    {
        return new ToastMessage()
        {
            Type = type,
            Title = title,
            HelpText = $"{TimeZoneHelper.ServerTimeNow}",
            Message = message,
        };
    }

    public static ToastMessage CreateToastWithoutTitle(ToastType type, string message)
    {
        return new ToastMessage()
        {
            Type = type,
            Message = message,
        };
    }
}
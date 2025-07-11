using BlazorDownloadFile;

namespace AdminWeb.Services.Utils;

public static class FileDownloadHelper
{
    public static async Task<bool> DownloadFile(IBlazorDownloadFileService downloadService, string path, string fileName)
    {
        if (File.Exists(path) == false)
            return false;
        
        var fileBytes = await File.ReadAllBytesAsync(path);
        var downloadResult = await downloadService.DownloadFile(fileName, fileBytes, "application/octet-stream");
        
        return downloadResult.Succeeded;
        
    }
}
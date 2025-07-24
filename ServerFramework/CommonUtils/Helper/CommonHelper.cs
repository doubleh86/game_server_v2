using System.Text;
using Force.Crc32;

namespace ServerFramework.CommonUtils.Helper;

public static class CommonHelper
{
    public static int GetDbId(string accountId, int dbCount)
    {
        var crcCode = Crc32Algorithm.Compute(Encoding.ASCII.GetBytes(accountId));
            
        var index = (int)(crcCode % dbCount);
        return index;
    }
    
    public static List<T> Shuffle<T>(List<T> listData)
    {
        var rnd = new Random();
        return listData.OrderBy(a => rnd.Next()).ToList();
    }

    public static (int, int, int) ParseStringTimeToInt(string timeString)
    {
        var timeInt = int.Parse(timeString);
        if(timeInt <= 2359)
            timeInt *= 100;
        
        var hours = timeInt / 10000;
        var minutes = (timeInt / 100) % 100;
        var seconds = timeInt % 100;
        
        return (hours, minutes, seconds);
    }

    public static bool CheckOverlappedDate(DateTime startDate1, DateTime endDate1, DateTime startDate2, DateTime endDate2)
    {
        return startDate1 <= endDate2 && endDate1 >= startDate2;
    }
}
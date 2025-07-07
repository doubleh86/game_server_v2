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
}
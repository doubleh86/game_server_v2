using DbContext.SharedContext.DbResultModel;
using NetworkProtocols.Shared.Enums;
using NetworkProtocols.WebApi.ToClientModels;
using ServerFramework.CommonUtils.EventHelper;

namespace ApiServer.Services;

public class EventService : EventServiceBase<EventDbResult>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="upcomingHour"></param>
    /// <returns>(진행중, 오픈예정, 만료예정)</returns>
    public GameEventData GetEvents(int upcomingHour)
    {
        var result = new GameEventData();
        var (processList, upcomingOpenList, upcomingExpiredList) = _GetEventList(upcomingHour);

        result.ProcessList = processList.Select(x => x.ToClient(EventStatus.Active)).ToList();
        result.UpcomingList = upcomingOpenList.Select(x => x.ToClient(EventStatus.UpcomingOpen)).ToList();
        result.UpcomingExpireList = upcomingExpiredList.Select(x => x.ToClient(EventStatus.UpcomingExpired)).ToList();
        
        return result;
    }
}
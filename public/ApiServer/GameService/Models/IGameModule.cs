namespace ApiServer.GameService.Models;

public interface IGameModule : IDisposable
{
    long AccountId { get; set; }
}
namespace ApiServer.Handlers.Models;

public interface IGameModule : IDisposable
{
    long AccountId { get; set; }
}
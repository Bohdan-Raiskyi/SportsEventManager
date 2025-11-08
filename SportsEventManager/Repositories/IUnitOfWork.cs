namespace SportsEventManager.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IEventRepository Events { get; }
        IAthleteRepository Athletes { get; }
        ITeamRepository Teams { get; }
    }
}
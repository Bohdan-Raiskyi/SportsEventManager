using SportsEventManager.Models;

namespace SportsEventManager.Repositories
{
    public interface IMatchDetailsRepository
    {
        Task CreateAsync(MatchDetails details);
        Task<MatchDetails?> GetByEventIdAsync(long sqlEventId);
    }
}
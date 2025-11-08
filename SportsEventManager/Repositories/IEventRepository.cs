using SportsEventManager.Models;

namespace SportsEventManager.Repositories
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(long id);
        Task<IEnumerable<Event>> GetAllActiveAsync();
        Task CreateAsync(Event @event, long currentUserId);
        Task UpdateAsync(Event @event, long currentUserId);
        Task DeleteAsync(long id, long currentUserId);
    }
}
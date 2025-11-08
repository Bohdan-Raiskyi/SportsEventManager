using Microsoft.EntityFrameworkCore;
using SportsEventManager.Data;
using SportsEventManager.Models;
using SportsEventManager.Repositories;

namespace SportsEventManager.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly SportsDbContext _context;

        public EventRepository(SportsDbContext context)
        {
            _context = context;
        }

        public async Task<Event?> GetByIdAsync(long id)
        {
            return (await _context.Events
                .FromSqlInterpolated($"SELECT * FROM vw_Active_Events WHERE id = {id}")
                .ToListAsync())
                .FirstOrDefault();
        }

        public async Task<IEnumerable<Event>> GetAllActiveAsync()
        {
            return await _context.Events
                .FromSqlRaw("SELECT * FROM vw_Active_Events")
                .ToListAsync();
        }

        public async Task CreateAsync(Event @event, long currentUserId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"CALL sp_Create_Event({@event.EventName}, {@event.StartDate}, {@event.EndDate}, {currentUserId})"
            );
        }

        public async Task UpdateAsync(Event @event, long currentUserId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"CALL sp_Update_Event({@event.Id}, {@event.EventName}, {@event.StartDate}, {@event.EndDate}, {currentUserId})"
            );
        }
        public async Task DeleteAsync(long id, long currentUserId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"CALL sp_SoftDelete_Event({id}, {currentUserId})"
            );
        }
    }
}
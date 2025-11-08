using Microsoft.EntityFrameworkCore;
using SportsEventManager.Data;
using SportsEventManager.Models;

namespace SportsEventManager.Repositories;

public class AthleteRepository : IAthleteRepository
{
    private readonly SportsDbContext _context;

    public AthleteRepository(SportsDbContext context)
    {
        _context = context;
    }

    public async Task<Athlete?> GetByIdAsync(long id)
    {
        return (await _context.Athletes
            .FromSqlInterpolated($"SELECT * FROM vw_Active_Athletes WHERE id = {id}")
            .ToListAsync())
            .FirstOrDefault();
    }

    public async Task<IEnumerable<Athlete>> GetAllActiveAsync()
    {
        return await _context.Athletes
            .FromSqlRaw("SELECT * FROM vw_Active_Athletes")
            .ToListAsync();
    }

    public async Task DeleteAsync(long id, long currentUserId)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"CALL sp_SoftDelete_Athlete({id}, {currentUserId})"
        );
    }
}
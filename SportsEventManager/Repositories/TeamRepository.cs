using Microsoft.EntityFrameworkCore;
using SportsEventManager.Data;
using SportsEventManager.Models;

namespace SportsEventManager.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly SportsDbContext _context;

    public TeamRepository(SportsDbContext context)
    {
        _context = context;
    }

    public async Task<Team?> GetByIdAsync(long id)
    {
        // Читаємо з View 'vw_Active_Teams'
        return (await _context.Teams
            .FromSqlInterpolated($"SELECT * FROM vw_Active_Teams WHERE id = {id}")
            .ToListAsync())
            .FirstOrDefault();
    }

    public async Task<IEnumerable<Team>> GetAllActiveAsync()
    {
        // Читаємо з View 'vw_Active_Teams'
        return await _context.Teams
            .FromSqlRaw("SELECT * FROM vw_Active_Teams")
            .ToListAsync();
    }
}
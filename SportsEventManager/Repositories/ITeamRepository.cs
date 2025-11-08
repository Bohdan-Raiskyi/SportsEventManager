using SportsEventManager.Models;
namespace SportsEventManager.Repositories;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(long id); // З vw_Active_Teams
    Task<IEnumerable<Team>> GetAllActiveAsync(); // З vw_Active_Teams

}
using SportsEventManager.Models;
namespace SportsEventManager.Repositories;

public interface IAthleteRepository
{
    Task<Athlete?> GetByIdAsync(long id); // З vw_Active_Athletes
    Task<IEnumerable<Athlete>> GetAllActiveAsync(); // З vw_Active_Athletes
    Task DeleteAsync(long id, long currentUserId); // З sp_SoftDelete_Athlete
}
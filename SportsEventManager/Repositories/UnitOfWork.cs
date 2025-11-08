using SportsEventManager.Data;

namespace SportsEventManager.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SportsDbContext _context;

        // Кешовані екземпляри репозиторіїв
        private IEventRepository? _events;
        private IAthleteRepository? _athletes;
        private ITeamRepository? _teams;

        public UnitOfWork(SportsDbContext context)
        {
            _context = context;
        }

        // Використовуємо "ліниву" (lazy) ініціалізацію
        public IEventRepository Events
        {
            get { return _events ??= new EventRepository(_context); }
        }

        public IAthleteRepository Athletes
        {
            get { return _athletes ??= new AthleteRepository(_context); }
        }

        public ITeamRepository Teams
        {
            get { return _teams ??= new TeamRepository(_context); }
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
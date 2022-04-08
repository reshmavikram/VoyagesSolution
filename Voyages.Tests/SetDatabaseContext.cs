using Data.Solution.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Voyage.Tests
{
    public class SetDatabaseContext
    {

        public DatabaseContext SetDbContext()
        {
            DatabaseContext _context;
            var serviceProvider = new ServiceCollection().AddEntityFrameworkSqlServer().BuildServiceProvider();
            var builder = new DbContextOptionsBuilder<DatabaseContext>();

            //local database
            //builder.UseSqlServer($"Server=DESKTOP-GON49QI;Initial Catalog=Fleet1;Persist Security Info=False;User ID=sa;Password=pass123!@#;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;")
            //  .UseInternalServiceProvider(serviceProvider);

            //SFP-Dev-Db
            //builder.UseSqlServer($"Server=tcp:fleetperformance01.database.windows.net,1433;Initial Catalog=SFP-Dev-Db;Persist Security Info=False;User ID=SuperAdmin;Password=Scorpio@1234;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;")
            // .UseInternalServiceProvider(serviceProvider);

            //FleeetPerformance01
            builder.UseSqlServer($"Server=.;Initial Catalog=SFP-Dev-Db-Local;User ID=sa;Password=pass123!@#;MultipleActiveResultSets=False;Connection Timeout=30;")
              .UseInternalServiceProvider(serviceProvider);

            _context = new DatabaseContext(builder.Options);
            return _context;
        }
    }
}

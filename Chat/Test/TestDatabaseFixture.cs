using Microsoft.EntityFrameworkCore;

public class TestDatabaseFixture
{
    private string? ConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

    private static readonly object _lock = new();
    private static bool _databaseInitialized;

    public TestDatabaseFixture()
    {
        if (ConnectionString == null)
        {
            throw new InvalidOperationException("Connection string is null");
        }
        lock (_lock)
        {
            if (!_databaseInitialized)
            {
                using (var context = CreateContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    context.AddRange(
                        new User { Id = Guid.NewGuid().ToString(), FirstName = "TestFirstName", LastName = "TestLastName" },
                        new User { Id = Guid.NewGuid().ToString(), FirstName = "TestFirstName2", LastName = "TestLastName2" },
                        new User { Id = Guid.NewGuid().ToString(), FirstName = "TestFirstName3", LastName = "TestLastName3" }
                    );
                    context.SaveChanges();
                }

                _databaseInitialized = true;
            }
        }
    }

    public ApplicationDbContext CreateContext()
        => new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString)
                .Options);
}
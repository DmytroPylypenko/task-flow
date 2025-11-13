using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;

namespace TaskFlow.Api.Tests.Data;

/// <summary>
/// A factory for creating new ApplicationDbContext instances
/// using an in-memory database. This ensures every test
/// runs against a clean, isolated database.
/// </summary>
public static class DbContextFactory
{
    public static ApplicationDbContext Create()
    {
        // 1. Define the in-memory database options
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            // Use a unique database name for each test run to guarantee isolation
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // 2. Create and return a new DbContext instance
        var context = new ApplicationDbContext(options);

        // 3. (Optional but recommended) Ensure the database schema is created
        context.Database.EnsureCreated();

        return context;
    }
}
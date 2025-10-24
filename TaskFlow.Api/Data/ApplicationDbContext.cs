using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    {
        
    }
}
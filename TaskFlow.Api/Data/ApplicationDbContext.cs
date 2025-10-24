using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Models;
using Task = TaskFlow.Api.Models.Task;

namespace TaskFlow.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<Column> Columns { get; set; }
    public DbSet<Task> Tasks { get; set; }
}
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Server.Entities.Configurations;
using Shared.Models;

namespace Server.Entities;

public class ApiContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Stock> Stocks { get; set; }
    
    public ApiContext() {}
    
    public ApiContext(DbContextOptions<ApiContext> options) : base(options){}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(UserEfConfiguration).GetTypeInfo().Assembly);
    }
}
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Server.Entities.Configurations;
using Shared.Models;
using Shared.Models.Aggregates;

namespace Server.Entities;

public class ApiContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Stock> Stocks { get; set; }
    public virtual DbSet<Aggregate> Aggregates { get; set; }
    
    public virtual DbSet<Watchlist> Watchlists { get; set; }
    
    public ApiContext() {}
    
    public ApiContext(DbContextOptions<ApiContext> options) : base(options){}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(UserEfConfiguration).GetTypeInfo().Assembly);
    }
}
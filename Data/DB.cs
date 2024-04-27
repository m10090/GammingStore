using gammingStore.Models;
using Microsoft.EntityFrameworkCore;

namespace gammingStore.Data;

public class DB : DbContext {
  protected override void
  OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
    optionsBuilder.UseSqlite("Data Source=Data.db");
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
  }

  public DbSet<User> users { set; get; } = null!;
  public DbSet<Product> products { set; get; } = null!;
  public DbSet<TranscationHistory> Historys { set; get; } = null!;
}

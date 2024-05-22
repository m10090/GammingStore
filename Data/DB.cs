using gammingStore.Models;
using Microsoft.EntityFrameworkCore;

namespace gammingStore.Data;

public class DB : DbContext
{
    protected override void
    OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Data.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
    }

    public override int SaveChanges()
    {
        ExecuteIsDeletedTrigger();
        return base.SaveChanges();
    }

    private void ExecuteIsDeletedTrigger()
    {
        this.Database.ExecuteSqlRaw(@"
      CREATE TRIGGER IF NOT EXISTS IsDeletedTrigger
      AFTER INSERT ON products
      BEGIN
        UPDATE products
        SET IsDeleted = 1
        WHERE Stock <= 0;
      END
    ");
    }
    public void ResetContext()
    {
        this.Dispose();
        var newContext = new DB();
    }

    public DbSet<User> users { set; get; } = null!;
    public DbSet<Product> products { set; get; } = null!;
    public DbSet<TranscationHistory> historys { set; get; } = null!;
}

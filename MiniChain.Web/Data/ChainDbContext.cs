using Microsoft.EntityFrameworkCore;

namespace MiniChain.Web.Data;

public class ChainDbContext(DbContextOptions<ChainDbContext> options) : DbContext(options)
{
    public DbSet<BlockEntity> Blocks => Set<BlockEntity>();
    public DbSet<TransactionEntity> Transactions => Set<TransactionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlockEntity>()
            .HasMany(b => b.Transactions)
            .WithOne()
            .HasForeignKey(b => b.BlockId);
    }
}
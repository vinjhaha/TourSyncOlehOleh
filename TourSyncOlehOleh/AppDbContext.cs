using Microsoft.EntityFrameworkCore;

namespace AplikasiKasir
{
    public class AppDbContext : DbContext
    {
        public DbSet<Barang> Barangs { get; set; }
        public DbSet<Transaksi> Transaksis { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=ecommerce.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Barang>()
                .Property(b => b.Id)
                .ValueGeneratedNever();
        }
    }
}

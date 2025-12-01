using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Peca> Pecas { get; set; }
        public DbSet<RegistroPonto> RegistrosPonto { get; set; }
        public DbSet<LogAuditoria> LogsAuditoria { get; set; }
     
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoItem> PedidoItens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            modelBuilder.Entity<Peca>()
                .Property(p => p.Preco)
                .HasPrecision(10, 2);

            // Configuração para garantir precisão nos valores do pedido também
            modelBuilder.Entity<Pedido>()
                .Property(p => p.ValorTotal)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PedidoItem>()
                .Property(p => p.PrecoUnitario)
                .HasPrecision(10, 2);
        }
    }
}
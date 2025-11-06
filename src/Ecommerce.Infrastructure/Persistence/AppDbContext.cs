using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        // Construtor necessário para a Injeção de Dependência
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Mapeia suas entidades para tabelas do banco
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Peca> Pecas { get; set; }
        public DbSet<RegistroPonto> RegistrosPonto { get; set; }
        public DbSet<LogAuditoria> LogsAuditoria { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aqui podemos adicionar configurações (ex: IDs únicos, tamanhos de campo)
            // Por exemplo, garantir que o Email do usuário seja único
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            // Configurar Preço da Peça (RN03)
            modelBuilder.Entity<Peca>()
                .Property(p => p.Preco)
                .HasPrecision(10, 2); // Ex: 12345678,99
        }
    }
}
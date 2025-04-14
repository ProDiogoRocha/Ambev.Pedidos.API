using Ambev.Pedidos.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ambev.Pedidos.API.DbContexts
{
    public class PedidosDbContext : DbContext
    {
        public PedidosDbContext(DbContextOptions<PedidosDbContext> options)
            : base(options)
        {
        }

        public DbSet<PedidoComErro> PedidosComErro { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PedidoComErro>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(p => p.RevendaId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.JsonPedido)
                    .IsRequired();
            });
        }
    }
}

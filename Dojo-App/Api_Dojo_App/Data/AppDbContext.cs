using Api_Dojo_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Api_Dojo_App.Data

{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoItem> PedidoItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }



        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Dos FK desde PedidoItem hacia Color (primario y secundario).
            // Restrict para evitar rutas de borrado en cascada múltiples y que
            // un Color no se pueda borrar mientras esté referenciado en un pedido.
            modelBuilder.Entity<PedidoItem>()
                .HasOne(pi => pi.PrimaryColor)
                .WithMany()
                .HasForeignKey(pi => pi.PrimaryColorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PedidoItem>()
                .HasOne(pi => pi.SecondaryColor)
                .WithMany()
                .HasForeignKey(pi => pi.SecondaryColorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Los PedidoItem son hechos históricos (transacciones que ocurrieron):
            // nunca deben borrarse por rebote al eliminar catálogo. Restrict hace
            // que la BD rechace el borrado de un producto/variante/campaña con
            // pedidos asociados, en lugar de mutilar el histórico en silencio.
            // (El borrado de productos del catálogo se hace por soft-delete: Product.IsActive)
            modelBuilder.Entity<ProductVariant>()
                .HasOne(v => v.Product)
                .WithMany(p => p.ProductVariants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PedidoItem>()
                .HasOne(pi => pi.ProductVariant)
                .WithMany()
                .HasForeignKey(pi => pi.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Campaign)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Campaign)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

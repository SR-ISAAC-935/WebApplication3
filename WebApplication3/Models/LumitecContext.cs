using Microsoft.EntityFrameworkCore;

namespace WebApplication3.Models;

public partial class LumitecContext : DbContext
{
    public LumitecContext()
    {
    }

    public LumitecContext(DbContextOptions<LumitecContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Abono> Abonos { get; set; }

    public virtual DbSet<Consumidore> Consumidores { get; set; }

    public virtual DbSet<DetailSalesClient> DetailSalesClients { get; set; }

    public virtual DbSet<DetalleListaNegra> DetalleListaNegras { get; set; }

    public virtual DbSet<EstadoProducto> EstadoProductos { get; set; }

    public virtual DbSet<EstadosDeudore> EstadosDeudores { get; set; }

    public virtual DbSet<ListaNegra> ListaNegras { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<SalesClientResume> SalesClientResumes { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("workstation id=Lumitec.mssql.somee.com;packet size=4096;user id=Lumitec_SQLLogin_1;pwd=uzaz6jxprp;data source=Lumitec.mssql.somee.com;persist security info=False;initial catalog=Lumitec;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Abono>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Abonos__3213E83F39FFC12A");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AbonoDeuda)
                .HasColumnType("money")
                .HasColumnName("Abono deuda");
            entity.Property(e => e.FacturaDeuda).HasColumnName("Factura Deuda");
            entity.Property(e => e.FechaAbono)
                .HasColumnType("datetime")
                .HasColumnName("Fecha Abono");
            entity.Property(e => e.IdUsuario).HasColumnName("ID Usuario");

            entity.HasOne(d => d.FacturaDeudaNavigation).WithMany(p => p.Abonos)
                .HasForeignKey(d => d.FacturaDeuda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_deudas");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Abonos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Abono");
        });

        modelBuilder.Entity<Consumidore>(entity =>
        {
            entity.HasKey(e => e.IdConsumidor).HasName("PK__Consumid__9F510D9639E38DAB");

            entity.Property(e => e.NombreConsumidor).HasMaxLength(45);
        });

        modelBuilder.Entity<DetailSalesClient>(entity =>
        {
            entity.HasKey(e => e.IdSale).HasName("PK__DetailSa__2071DEA3043F5B76");

            entity.ToTable("DetailSalesClient");

            entity.Property(e => e.IdSale).HasColumnName("ID_Sale");
            entity.Property(e => e.IdProduct).HasColumnName("ID_Product");
            entity.Property(e => e.IdVenta).HasColumnName("ID_Venta");
            entity.Property(e => e.Price).HasColumnType("money");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.DetailSalesClients)
                .HasForeignKey(d => d.IdProduct)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetailSales_Products");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetailSalesClients)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetailSales_Sales");
        });

        modelBuilder.Entity<DetalleListaNegra>(entity =>
        {
            entity.HasKey(e => e.IdDeuda).HasName("PK__DetalleL__7F8C86B128308E54");

            entity.ToTable("DetalleListaNegra");

            entity.Property(e => e.FechaCompra)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fechaCompra");
            entity.Property(e => e.IdEstado).HasDefaultValue(1);
            entity.Property(e => e.Precio).HasColumnType("money");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.DetalleListaNegras)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleListaNegra_EstadoProducto");

            entity.HasOne(d => d.IdListadoNavigation).WithMany(p => p.DetalleListaNegras)
                .HasForeignKey(d => d.IdListado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Detalle_Lista");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleListaNegras)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Detalle_Producto");
        });

        modelBuilder.Entity<EstadoProducto>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__EstadoPr__FBB0EDC1B967456E");

            entity.ToTable("EstadoProducto");

            entity.Property(e => e.Descripcion).HasMaxLength(50);
        });

        modelBuilder.Entity<EstadosDeudore>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__EstadosD__FBB0EDC1ABFF7B22");

            entity.Property(e => e.Descripcion).HasMaxLength(100);
        });

        modelBuilder.Entity<ListaNegra>(entity =>
        {
            entity.HasKey(e => e.IdListado).HasName("PK__ListaNeg__9C1022A85968EFE1");

            entity.ToTable("ListaNegra");

            entity.Property(e => e.Deuda).HasColumnType("money");
            entity.Property(e => e.FechaVenta).HasColumnType("datetime");

            entity.HasOne(d => d.IdConsumidorNavigation).WithMany(p => p.ListaNegras)
                .HasForeignKey(d => d.IdConsumidor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Consumidor");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.ListaNegras)
                .HasForeignKey(d => d.IdEstado)
                .HasConstraintName("FK_ListaNegra_EstadosDeudores");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.IdProduct).HasName("PK__Products__522DE496C4200F8A");

            entity.Property(e => e.IdProduct).HasColumnName("ID_Product");
            entity.Property(e => e.ProductBuyed)
                .HasColumnType("money")
                .HasColumnName("Product_buyed");
            entity.Property(e => e.ProductName)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("Product_Name");
            entity.Property(e => e.ProductPrices)
                .HasColumnType("money")
                .HasColumnName("Product_Prices");
            entity.Property(e => e.ProductProvider)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("Product_Provider");
            entity.Property(e => e.ProductStock).HasColumnName("Product_Stock");
        });

        modelBuilder.Entity<SalesClientResume>(entity =>
        {
            entity.HasKey(e => e.IdSales).HasName("PK__SalesCli__853C82F8EA2B42AC");

            entity.ToTable("SalesClientResume");

            entity.Property(e => e.IdSales).HasColumnName("ID_Sales");
            entity.Property(e => e.IdConsumidor).HasColumnName("ID_Consumidor");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.Total).HasColumnType("money");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.SalesClientResumes)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesClientResume_Consumidores");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__Usuarios__3717C9828326C28E");

            entity.Property(e => e.IdUser).HasColumnName("idUser");
            entity.Property(e => e.Passwords).HasMaxLength(75);
            entity.Property(e => e.Users).HasMaxLength(30);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

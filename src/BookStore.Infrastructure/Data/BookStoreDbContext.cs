using BookStore.Domain.Constants;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Data;

public sealed class BookStoreDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public BookStoreDbContext(DbContextOptions<BookStoreDbContext> options) : base(options)
    {
    }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

        builder.Entity<Author>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Biography).HasMaxLength(2000);
        });

        builder.Entity<Category>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Book>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(240).IsRequired();
            entity.Property(x => x.Isbn).HasColumnName("ISBN").HasMaxLength(20).IsRequired();
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.CoverImageUrl).HasMaxLength(600);
            entity.HasIndex(x => x.Isbn).IsUnique();
            entity.HasOne(x => x.Author).WithMany(x => x.Books).HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Category).WithMany(x => x.Books).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Cart>(entity =>
        {
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(x => x.UserId).IsUnique();
        });

        builder.Entity<CartItem>(entity =>
        {
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.CartId, x.BookId }).IsUnique();
            entity.HasOne(x => x.Cart).WithMany(x => x.Items).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Book).WithMany().HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Order>(entity =>
        {
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.OrderNumber).HasMaxLength(40).IsRequired();
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            entity.HasIndex(x => x.OrderNumber).IsUnique();
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Book).WithMany().HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Restrict);
        });

        Seed(builder);
    }

    private static void Seed(ModelBuilder builder)
    {
        var adminRoleId = "8f58f6ec-f1ba-4af9-b79a-2f120ef66f4b";
        var customerRoleId = "43ecf861-2863-45e1-b85e-4912382afc3d";
        var adminUserId = "22186ea4-f7e2-4d3d-99e9-63b41fa45fdc";
        var customerUserId = "0a3f1ad5-e17e-4373-b506-e3a2399f83bb";
        var createdAt = new DateTime(2026, 6, 12, 0, 0, 0, DateTimeKind.Utc);

        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole { Id = adminRoleId, Name = SystemRoles.Admin, NormalizedName = SystemRoles.Admin.ToUpperInvariant() },
            new ApplicationRole { Id = customerRoleId, Name = SystemRoles.Customer, NormalizedName = SystemRoles.Customer.ToUpperInvariant() });

        var hasher = new PasswordHasher<ApplicationUser>();
        var admin = new ApplicationUser
        {
            Id = adminUserId,
            FirstName = "Coforge",
            LastName = "Admin",
            Email = "admin@bookstore.local",
            NormalizedEmail = "ADMIN@BOOKSTORE.LOCAL",
            UserName = "admin@bookstore.local",
            NormalizedUserName = "ADMIN@BOOKSTORE.LOCAL",
            EmailConfirmed = true,
            SecurityStamp = "d3e9d24a-7782-4391-a912-2b7d61f4ae02",
            CreatedAt = createdAt,
            IsActive = true
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin@12345");

        var customer = new ApplicationUser
        {
            Id = customerUserId,
            FirstName = "Demo",
            LastName = "Customer",
            Email = "customer@bookstore.local",
            NormalizedEmail = "CUSTOMER@BOOKSTORE.LOCAL",
            UserName = "customer@bookstore.local",
            NormalizedUserName = "CUSTOMER@BOOKSTORE.LOCAL",
            EmailConfirmed = true,
            SecurityStamp = "0f1b47a0-aa61-4750-a50c-ed86cb75be23",
            CreatedAt = createdAt,
            IsActive = true
        };
        customer.PasswordHash = hasher.HashPassword(customer, "Customer@12345");

        builder.Entity<ApplicationUser>().HasData(admin, customer);
        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string> { UserId = adminUserId, RoleId = adminRoleId },
            new IdentityUserRole<string> { UserId = customerUserId, RoleId = customerRoleId });

        builder.Entity<Author>().HasData(
            new Author { Id = 1, Name = "Robert C. Martin", Biography = "Author of Clean Code and software craftsmanship books.", CreatedAt = createdAt, IsActive = true },
            new Author { Id = 2, Name = "Martin Fowler", Biography = "Author and speaker on software architecture and refactoring.", CreatedAt = createdAt, IsActive = true },
            new Author { Id = 3, Name = "Andrew Hunt", Biography = "Co-author of The Pragmatic Programmer.", CreatedAt = createdAt, IsActive = true });

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Software Engineering", Description = "Professional programming and design books.", IsActive = true },
            new Category { Id = 2, Name = "Architecture", Description = "System design and enterprise architecture.", IsActive = true },
            new Category { Id = 3, Name = "Career", Description = "Career growth and pragmatic practices.", IsActive = true });

        builder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "Clean Code", Isbn = "9780132350884", Price = 42.99m, StockQuantity = 30, AuthorId = 1, CategoryId = 1, Description = "A handbook of agile software craftsmanship.", CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9780132350884-L.jpg", CreatedAt = createdAt, IsActive = true },
            new Book { Id = 2, Title = "Refactoring", Isbn = "9780134757599", Price = 55.00m, StockQuantity = 20, AuthorId = 2, CategoryId = 1, Description = "Improving the design of existing code.", CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9780134757599-L.jpg", CreatedAt = createdAt, IsActive = true },
            new Book { Id = 3, Title = "The Pragmatic Programmer", Isbn = "9780201616224", Price = 39.50m, StockQuantity = 25, AuthorId = 3, CategoryId = 3, Description = "A practical guide to becoming a better programmer.", CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9780201616224-L.jpg", CreatedAt = createdAt, IsActive = true });
    }
}

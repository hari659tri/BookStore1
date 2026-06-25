using BookStore.Models;

namespace BookStore.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Programming" },
                    new Category { Name = "Fiction" },
                    new Category { Name = "Science" }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            if (!context.Books.Any())
            {
                context.Books.AddRange(
                    new Book
                    {
                        Title = "C# Basics",
                        Author = "Harikesh",
                        Price = 499,
                        Description = "Learn C#",
                        ImageUrl = "",
                        CategoryId = 1
                    },
                    new Book
                    {
                        Title = "ASP.NET Core",
                        Author = "Microsoft",
                        Price = 799,
                        Description = "Learn Razor Pages",
                        ImageUrl = "",
                        CategoryId = 1
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
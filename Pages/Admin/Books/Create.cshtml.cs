using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Pages.Admin.Books
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet() { }

        public IActionResult OnPost(string Title, string Author, decimal Price, string Description)
        {
            Console.WriteLine("POST HIT");

            var book = new Book
            {
                Title = Title,
                Author = Author,
                Price = Price,
                Description = Description
            };

            _context.Books.Add(book);
            _context.SaveChanges();

            Console.WriteLine("BOOK SAVED");

            return RedirectToPage("Index");
        }
    }
}
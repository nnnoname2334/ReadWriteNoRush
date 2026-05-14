using System.Collections.Generic;
using System.Linq;

namespace ReadWriteNoRush.Services
{
    internal class BookService
    {
        
        public static List<Books> GetAll()
        {
            return Core.Context.Books
                .Include("Users")
                .Include("Genres")
                .Where(b => !b.IsFrozen)
                .ToList();
        }

       
        public static List<Books> Search(string query)
        {
            query = query.ToLower();
            return Core.Context.Books
                .Include("Users")
                .Include("Genres")
                .Where(b => !b.IsFrozen &&
                       (b.Title.ToLower().Contains(query) ||
                        b.Users.DisplayName.ToLower().Contains(query)))
                .ToList();
        }

        
        public static double GetAvgRating(int bookId)
        {
            var reviews = Core.Context.Reviews
                .Where(r => r.BookId == bookId)
                .ToList();

            if (!reviews.Any()) return 0;
            return reviews.Average(r => r.Rating);
        }
        // Добавь внутрь класса BookService

        public static List<Books> GetByAuthor(int authorId, bool includeFrozen = false)
        {
            return Core.Context.Books
                .Include("Genres")
                .Where(b => b.AuthorId == authorId && (includeFrozen || !b.IsFrozen))
                .ToList();
        }

        public static List<Books> GetFrozenByAuthor(int authorId)
        {
            return Core.Context.Books
                .Include("Genres")
                .Where(b => b.AuthorId == authorId && b.IsFrozen)
                .ToList();
        }

        public static void Add(Books book)
        {
            Core.Context.Books.Add(book);
            Core.Context.SaveChanges();
        }

        public static void Update()
        {
            Core.Context.SaveChanges();
        }

        public static bool ApplyForUnfreezeBook(int userId, int bookId, string reason)
        {
            bool pending = Core.Context.UnfreezeApplications
                .Any(a => a.UserId == userId && a.BookId == bookId && a.Status == "Pending");
            if (pending) return false;

            Core.Context.UnfreezeApplications.Add(new UnfreezeApplications
            {
                UserId = userId,
                BookId = bookId,
                Reason = reason,
                Status = "Pending",
                CreatedAt = System.DateTime.Now
            });
            Core.Context.SaveChanges();
            return true;
        }
    }
}
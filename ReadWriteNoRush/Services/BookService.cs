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
    }
}
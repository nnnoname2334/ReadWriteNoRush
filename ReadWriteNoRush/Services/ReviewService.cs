using System.Collections.Generic;
using System.Linq;

namespace ReadWriteNoRush.Services
{
    internal class ReviewService
    {
        public static List<Reviews> GetByBook(int bookId)
        {
            return Core.Context.Reviews
                .Include("Users")
                .Where(r => r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public static bool Add(int bookId, int userId, string text, int rating)
        {
            bool already = Core.Context.Reviews
                .Any(r => r.BookId == bookId && r.UserId == userId);
            if (already) return false;

            Core.Context.Reviews.Add(new Reviews
            {
                BookId = bookId,
                UserId = userId,
                ReviewText = text,
                Rating = rating,
                CreatedAt = System.DateTime.Now
            });
            Core.Context.SaveChanges();
            return true;
        }

        public static void Freeze(int reviewId)
        {
            // у Reviews нет IsFrozen — замораживаем через жалобу-пометку,
            // поэтому просто удаляем отзыв (поведение как у заморозки)
            var r = Core.Context.Reviews.Find(reviewId);
            if (r != null)
            {
                Core.Context.Reviews.Remove(r);
                Core.Context.SaveChanges();
            }
        }
    }
}
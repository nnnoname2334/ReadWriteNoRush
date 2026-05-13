using System.Linq;

namespace ReadWriteNoRush.Services
{
    internal class ComplaintService
    {
        public static bool AddOnBook(int userId, int bookId, string reason)
        {
            bool exists = Core.Context.Complaints
                .Any(c => c.UserId == userId && c.BookId == bookId);
            if (exists) return false;

            Core.Context.Complaints.Add(new Complaints
            {
                UserId = userId,
                BookId = bookId,
                Reason = reason,
                CreatedAt = System.DateTime.Now
            });
            Core.Context.SaveChanges();
            return true;
        }

        public static bool AddOnReview(int userId, int reviewId, string reason)
        {
            bool exists = Core.Context.Complaints
                .Any(c => c.UserId == userId && c.ReviewId == reviewId);
            if (exists) return false;

            Core.Context.Complaints.Add(new Complaints
            {
                UserId = userId,
                ReviewId = reviewId,
                Reason = reason,
                CreatedAt = System.DateTime.Now
            });
            Core.Context.SaveChanges();
            return true;
        }
    }
}
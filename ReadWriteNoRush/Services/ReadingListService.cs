using System.Collections.Generic;
using System.Linq;

namespace ReadWriteNoRush.Services
{
    internal class ReadingListService
    {
        public static List<ReadingLists> GetByStatus(int userId, int statusId)
        {
            return Core.Context.ReadingLists
                .Include("Books")
                .Include("Books.Users")
                .Include("Books.Genres")
                .Where(r => r.UserId == userId && r.StatusId == statusId)
                .ToList();
        }

        public static void AddOrMove(int userId, int bookId, int statusId)
        {
            var existing = Core.Context.ReadingLists
                .FirstOrDefault(r => r.UserId == userId && r.BookId == bookId);

            if (existing != null)
            {
                existing.StatusId = statusId;
            }
            else
            {
                Core.Context.ReadingLists.Add(new ReadingLists
                {
                    UserId = userId,
                    BookId = bookId,
                    StatusId = statusId,
                    AddedAt = System.DateTime.Now
                });
            }
            Core.Context.SaveChanges();
        }

        public static void Remove(int userId, int bookId)
        {
            var item = Core.Context.ReadingLists
                .FirstOrDefault(r => r.UserId == userId && r.BookId == bookId);
            if (item == null) return;
            Core.Context.ReadingLists.Remove(item);
            Core.Context.SaveChanges();
        }

        public static int? GetStatus(int userId, int bookId)
        {
            return Core.Context.ReadingLists
                .FirstOrDefault(r => r.UserId == userId && r.BookId == bookId)?.StatusId;
        }
    }
}
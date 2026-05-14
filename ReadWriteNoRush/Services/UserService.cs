using System.Linq;

namespace ReadWriteNoRush.Services
{
    internal class UserService
    {
        public static bool ApplyForAuthor(int userId)
        {
            bool pending = Core.Context.RoleApplications
                .Any(a => a.UserId == userId && a.Status == "Pending");
            if (pending) return false;

            Core.Context.RoleApplications.Add(new RoleApplications
            {
                UserId = userId,
                Status = "Pending",
                CreatedAt = System.DateTime.Now
            });
            Core.Context.SaveChanges();
            return true;
        }

        public static bool ApplyForUnfreeze(int userId, string reason)
        {
            bool pending = Core.Context.UnfreezeApplications
                .Any(a => a.UserId == userId && a.BookId == null && a.Status == "Pending");
            if (pending) return false;

            Core.Context.UnfreezeApplications.Add(new UnfreezeApplications
            {
                UserId = userId,
                Reason = reason,
                Status = "Pending",
                CreatedAt = System.DateTime.Now
            });
            Core.Context.SaveChanges();
            return true;
        }
    }
}
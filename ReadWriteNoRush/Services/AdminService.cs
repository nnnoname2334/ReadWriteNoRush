using System.Collections.Generic;
using System.Linq;

namespace ReadWriteNoRush.Services
{
    internal class AdminService
    {
        // Жалобы
        public static List<Complaints> GetComplaints()
        {
            return Core.Context.Complaints
                .Include("Users")
                .Include("Books")
                .Include("Reviews")
                .ToList();
        }

        public static void DeleteComplaint(int complaintId)
        {
            var c = Core.Context.Complaints.Find(complaintId);
            if (c != null) Core.Context.Complaints.Remove(c);
            Core.Context.SaveChanges();
        }

        // Заявки на разморозку
        public static List<UnfreezeApplications> GetUnfreezeApps()
        {
            return Core.Context.UnfreezeApplications
                .Include("Users")
                .Include("Books")
                .Where(a => a.Status == "Pending")
                .ToList();
        }

        public static void ResolveUnfreeze(int appId, bool approve)
        {
            var app = Core.Context.UnfreezeApplications
                .Include("Users")
                .Include("Books")
                .FirstOrDefault(a => a.UnfreezeAppId == appId);
            if (app == null) return;

            app.Status = approve ? "Approved" : "Rejected";

            if (approve)
            {
                if (app.BookId == null)
                    app.Users.IsFrozen = false;
                else if (app.Books != null)
                    app.Books.IsFrozen = false;
            }

            Core.Context.SaveChanges();
        }

        // Заявки на роль автора
        public static List<RoleApplications> GetRoleApps()
        {
            return Core.Context.RoleApplications
                .Include("Users")
                .Where(a => a.Status == "Pending")
                .ToList();
        }

        public static void ResolveRoleApp(int appId, bool approve)
        {
            var app = Core.Context.RoleApplications
                .Include("Users")
                .FirstOrDefault(a => a.ApplicationId == appId);
            if (app == null) return;

            app.Status = approve ? "Approved" : "Rejected";
            if (approve) app.Users.RoleId = 2; // Автор
            Core.Context.SaveChanges();
        }

        // Замороженные
        public static List<Books> GetFrozenBooks()
        {
            return Core.Context.Books
                .Include("Users")
                .Where(b => b.IsFrozen)
                .ToList();
        }

        public static List<Users> GetFrozenUsers()
        {
            return Core.Context.Users
                .Include("Roles")
                .Where(u => u.IsFrozen)
                .ToList();
        }

        // Все пользователи
        public static List<Users> GetAllUsers()
        {
            return Core.Context.Users
                .Include("Roles")
                .OrderBy(u => u.Login)
                .ToList();
        }

        public static void SetRole(int userId, int roleId)
        {
            var user = Core.Context.Users.Find(userId);
            if (user != null) user.RoleId = roleId;
            Core.Context.SaveChanges();
        }

        public static void SetPassword(int userId, string newPassword)
        {
            var user = Core.Context.Users.Find(userId);
            if (user != null) user.PasswordHash = newPassword;
            Core.Context.SaveChanges();
        }

        public static void FreezeUser(int userId)
        {
            var user = Core.Context.Users.Find(userId);
            if (user != null) user.IsFrozen = true;
            Core.Context.SaveChanges();
        }

        public static void UnfreezeUser(int userId)
        {
            var user = Core.Context.Users.Find(userId);
            if (user != null) user.IsFrozen = false;
            Core.Context.SaveChanges();
        }
    }
}
using System.Linq;
using ReadWriteNoRush.Helpers;

namespace ReadWriteNoRush.Services
{
    internal class AuthService
    {
        
        public static bool SignIn(string login, string passwordHash)
        {
            var user = Core.Context.Users
                .Include("Roles")
                .FirstOrDefault(u => u.Login == login && u.PasswordHash == passwordHash);

            if (user == null)
                return false;

            AppSession.CurrentUser = user;
            return true;
        }

        
        public static bool SignUp(string login, string email, string displayName, string passwordHash)
        {
            
            bool exists = Core.Context.Users
                .Any(u => u.Login == login || u.Email == email);

            if (exists)
                return false;

            var newUser = new Users
            {
                Login = login,
                Email = email,
                DisplayName = displayName,
                PasswordHash = passwordHash,
                RoleId = 1,
                IsFrozen = false,
                CreatedAt = System.DateTime.Now
            };

            Core.Context.Users.Add(newUser);
            Core.Context.SaveChanges();
            return true;
        }

        
        public static void SignOut()
        {
            AppSession.LogOut();
        }
    }
}
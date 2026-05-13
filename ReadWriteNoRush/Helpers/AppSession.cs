namespace ReadWriteNoRush.Helpers
{
    internal static class AppSession
    {
        public static Users CurrentUser { get; set; }

        public static bool IsLoggedIn => CurrentUser != null;
        public static bool IsAdmin => CurrentUser?.Roles?.RoleName == "Администратор";
        public static bool IsAuthor => CurrentUser?.Roles?.RoleName == "Автор";
        public static bool IsReader => CurrentUser?.Roles?.RoleName == "Читатель";
        public static bool IsFrozen => CurrentUser?.IsFrozen == true;

        public static void LogOut()
        {
            CurrentUser = null;
        }
    }
}
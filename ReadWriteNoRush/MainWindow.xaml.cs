using System.Windows;
using System.Windows.Controls;
using ReadWriteNoRush.Helpers;
using ReadWriteNoRush.Views;

namespace ReadWriteNoRush
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ConfigureSidebar();
            MainFrame.Navigate(new CatalogPage());
        }

        private void ConfigureSidebar()
        {
            BtnAuthor.Visibility = AppSession.IsAuthor ? Visibility.Visible : Visibility.Collapsed;
            BtnAdmin.Visibility = AppSession.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnFrozen.Visibility = AppSession.IsFrozen ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnCatalog_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new CatalogPage());

        private void BtnLists_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new ReadingListPage());

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new ProfilePage());

        private void BtnAuthor_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new AuthorPage());

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new AdminPage());

        private void BtnFrozen_Click(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new FrozenPage());

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AppSession.LogOut();
            new AuthWindow().Show();
            this.Close();
        }
    }
}
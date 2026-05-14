using System.Windows;
using System.Windows.Controls;
using ReadWriteNoRush.Helpers;
using ReadWriteNoRush.Services;

namespace ReadWriteNoRush.Views
{
    public partial class FrozenPage : Page
    {
        public FrozenPage()
        {
            InitializeComponent();
            TxtReason.Text = AppSession.IsFrozen
                ? "Ваш аккаунт был заморожен администратором."
                : "";
        }

        private void BtnUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Укажите причину оспаривания:");
            if (dialog.ShowDialog() != true) return;
            string reason = dialog.Answer;
            if (string.IsNullOrWhiteSpace(reason)) return;

            bool ok = UserService.ApplyForUnfreeze(AppSession.CurrentUser.UserId, reason);
            MessageBox.Show(ok
                ? "Заявка отправлена. Ожидайте решения администратора."
                : "У вас уже есть активная заявка на разморозку.");
        }
    }
}
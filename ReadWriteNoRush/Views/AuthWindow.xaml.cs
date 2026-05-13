using System.Windows;
using ReadWriteNoRush.Services;
using ReadWriteNoRush.Helpers;

namespace ReadWriteNoRush.Views
{
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
        }

        private void BtnSignIn_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                TxtError.Text = "Заполните все поля.";
                return;
            }

            bool success = AuthService.SignIn(login, password);

            if (!success)
            {
                TxtError.Text = "Неверный логин или пароль.";
                return;
            }

            var main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtRegLogin.Text.Trim();
            string name = TxtRegName.Text.Trim();
            string email = TxtRegEmail.Text.Trim();
            string password = TxtRegPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(name) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TxtError.Text = "Заполните все поля.";
                return;
            }

            bool success = AuthService.SignUp(login, email, name, password);

            if (!success)
            {
                TxtError.Text = "Логин или email уже заняты.";
                return;
            }

            TxtError.Text = "";
            MessageBox.Show("Регистрация прошла успешно! Войдите в аккаунт.", "Успех");
            SwitchToLogin_Click(null, null);
        }

        private void SwitchToRegister_Click(object sender, RoutedEventArgs e)
        {
            PanelSignIn.Visibility = Visibility.Collapsed;
            PanelSignUp.Visibility = Visibility.Visible;
            TxtError.Text = "";
        }

        private void SwitchToLogin_Click(object sender, RoutedEventArgs e)
        {
            PanelSignIn.Visibility = Visibility.Visible;
            PanelSignUp.Visibility = Visibility.Collapsed;
            TxtError.Text = "";
        }
    }
}
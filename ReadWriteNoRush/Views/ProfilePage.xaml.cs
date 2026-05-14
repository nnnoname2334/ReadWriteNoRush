using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReadWriteNoRush.Helpers;
using ReadWriteNoRush.Services;

namespace ReadWriteNoRush.Views
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadProfile();
            LoadReviews();
        }

        private void LoadProfile()
        {
            var user = Core.Context.Users
                .Include("Roles")
                .FirstOrDefault(u => u.UserId == AppSession.CurrentUser.UserId);

            if (user == null) return;

            TxtName.Text = user.DisplayName;
            TxtLogin.Text = user.Login;
            TxtEmail.Text = user.Email;
            TxtRole.Text = user.Roles?.RoleName ?? "—";

            // Кнопка заявки — только для читателя
            BtnApplyAuthor.Visibility = AppSession.IsReader
                ? Visibility.Visible : Visibility.Collapsed;

            // Заморозка
            if (user.IsFrozen)
            {
                PanelFrozen.Visibility = Visibility.Visible;
                var unfreezeApp = Core.Context.UnfreezeApplications
                    .Where(a => a.UserId == user.UserId && a.BookId == null)
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefault();
                TxtFrozenReason.Text = unfreezeApp != null
                    ? $"Причина: {unfreezeApp.Reason}"
                    : "Причина не указана.";
            }
        }

        private void LoadReviews()
        {
            ReviewsPanel.Children.Clear();
            var reviews = Core.Context.Reviews
                .Include("Books")
                .Where(r => r.UserId == AppSession.CurrentUser.UserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            if (!reviews.Any())
            {
                ReviewsPanel.Children.Add(new TextBlock
                {
                    Text = "Вы ещё не оставляли отзывов.",
                    Foreground = Brushes.Gray
                });
                return;
            }

            foreach (var r in reviews)
            {
                var card = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(14),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var sp = new StackPanel();

                sp.Children.Add(new TextBlock
                {
                    Text = r.Books?.Title ?? "—",
                    FontWeight = FontWeights.Bold,
                    FontSize = 13
                });

                sp.Children.Add(new TextBlock
                {
                    Text = $"⭐ {r.Rating}/10",
                    Foreground = Brushes.OrangeRed,
                    Margin = new Thickness(0, 4, 0, 0)
                });

                sp.Children.Add(new TextBlock
                {
                    Text = r.ReviewText,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                    Margin = new Thickness(0, 4, 0, 0)
                });

                sp.Children.Add(new TextBlock
                {
                    Text = r.CreatedAt.ToString("dd.MM.yyyy"),
                    Foreground = Brushes.Gray,
                    FontSize = 11,
                    Margin = new Thickness(0, 4, 0, 0)
                });

                card.Child = sp;
                ReviewsPanel.Children.Add(card);
            }
        }

        private void BtnApplyAuthor_Click(object sender, RoutedEventArgs e)
        {
            bool ok = UserService.ApplyForAuthor(AppSession.CurrentUser.UserId);
            MessageBox.Show(ok
                ? "Заявка отправлена! Ожидайте решения администратора."
                : "У вас уже есть активная заявка.");
        }

        private void BtnUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Укажите причину оспаривания заморозки:");
            if (dialog.ShowDialog() != true) return;
            string reason = dialog.Answer;
            if (string.IsNullOrWhiteSpace(reason)) return;

            bool ok = UserService.ApplyForUnfreeze(AppSession.CurrentUser.UserId, reason);
            MessageBox.Show(ok
                ? "Заявка на разморозку отправлена."
                : "У вас уже есть активная заявка на разморозку.");
        }
    }
}
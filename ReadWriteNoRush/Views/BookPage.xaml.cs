using ReadWriteNoRush.Helpers;
using ReadWriteNoRush.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReadWriteNoRush.Views
{
    public partial class BookPage : Page
    {
        private int _bookId;
        private Books _book;

        public BookPage(int bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            LoadBook();
            LoadReviews();
        }

        private void LoadBook()
        {
            _book = Core.Context.Books
                .Include("Users")
                .Include("Genres")
                .FirstOrDefault(b => b.BookId == _bookId);

            if (_book == null) return;

            TxtTitle.Text = _book.Title;
            TxtAuthor.Text = "Автор: " + (_book.Users?.DisplayName ?? "—");
            TxtGenres.Text = "Жанры: " + string.Join(", ", _book.Genres.Select(g => g.GenreName));
            TxtContent.Text = _book.Content;
            TxtDesc.Text = _book.Description;
            // Загрузка обложки
            try
            {
                if (!string.IsNullOrEmpty(_book.CoverPath))
                {
                    var uri = new Uri($"pack://application:,,,/Assets/{_book.CoverPath}",
                        UriKind.Absolute);
                    ImgCover.Source = new System.Windows.Media.Imaging.BitmapImage(uri);
                }
                else
                {
                    var uri = new Uri("pack://application:,,,/Assets/no_cover.png",
                        UriKind.Absolute);
                    ImgCover.Source = new System.Windows.Media.Imaging.BitmapImage(uri);
                }
            }
            catch
            {
                var uri = new Uri("pack://application:,,,/Assets/no_cover.png",
                    UriKind.Absolute);
                ImgCover.Source = new System.Windows.Media.Imaging.BitmapImage(uri);
            }

            double rating = BookService.GetAvgRating(_bookId);
            TxtRating.Text = rating > 0 ? $"⭐ {rating:F1} / 10" : "Нет оценок";

            // Кнопка заморозки — только для админа
            BtnFreezeBook.Visibility = AppSession.IsAdmin
                ? Visibility.Visible : Visibility.Collapsed;

            // Форма отзыва — скрыть если уже оставил или автор/админ
            bool alreadyReviewed = Core.Context.Reviews
                .Any(r => r.BookId == _bookId && r.UserId == AppSession.CurrentUser.UserId);
            bool isAuthorOrAdmin = AppSession.IsAdmin || AppSession.IsAuthor;

            PanelAddReview.Visibility = (alreadyReviewed || isAuthorOrAdmin)
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LoadReviews()
        {
            ReviewsPanel.Children.Clear();
            var reviews = ReviewService.GetByBook(_bookId);

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

                // Заголовок отзыва
                var header = new StackPanel { Orientation = Orientation.Horizontal };
                header.Children.Add(new TextBlock
                {
                    Text = r.Users?.DisplayName ?? "—",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 10, 0)
                });
                header.Children.Add(new TextBlock
                {
                    Text = $"⭐ {r.Rating}/10",
                    Foreground = Brushes.OrangeRed
                });
                sp.Children.Add(header);

                sp.Children.Add(new TextBlock
                {
                    Text = r.ReviewText,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 0),
                    Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60))
                });

                sp.Children.Add(new TextBlock
                {
                    Text = r.CreatedAt.ToString("dd.MM.yyyy"),
                    Foreground = Brushes.Gray,
                    FontSize = 11,
                    Margin = new Thickness(0, 4, 0, 6)
                });

                // Кнопки жалобы и заморозки отзыва
                var btnPanel = new StackPanel { Orientation = Orientation.Horizontal };

                var btnComplain = new Button
                {
                    Content = "⚠ Жалоба",
                    Height = 26,
                    Padding = new Thickness(10, 0, 10, 0),
                    Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Tag = r.ReviewId,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                btnComplain.Click += BtnComplainReview_Click;
                btnPanel.Children.Add(btnComplain);

                if (AppSession.IsAdmin)
                {
                    var btnFreeze = new Button
                    {
                        Content = "❄ Заморозить",
                        Height = 26,
                        Padding = new Thickness(10, 0, 10, 0),
                        Background = new SolidColorBrush(Color.FromRgb(142, 68, 173)),
                        Foreground = Brushes.White,
                        BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand,
                        Tag = r.ReviewId
                    };
                    btnFreeze.Click += BtnFreezeReview_Click;
                    btnPanel.Children.Add(btnFreeze);
                }

                sp.Children.Add(btnPanel);
                card.Child = sp;
                ReviewsPanel.Children.Add(card);
            }

            if (!reviews.Any())
                ReviewsPanel.Children.Add(new TextBlock
                {
                    Text = "Отзывов пока нет.",
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 10)
                });
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
            => NavigationService.GoBack();

        private void BtnAddReview_Click(object sender, RoutedEventArgs e)
        {
            string text = TxtReviewText.Text.Trim();
            int rating = int.Parse(((ComboBoxItem)CmbRating.SelectedItem).Content.ToString());

            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Введите текст отзыва.");
                return;
            }

            bool ok = ReviewService.Add(_bookId, AppSession.CurrentUser.UserId, text, rating);
            if (!ok)
            {
                MessageBox.Show("Вы уже оставляли отзыв на эту книгу.");
                return;
            }

            PanelAddReview.Visibility = Visibility.Collapsed;
            LoadReviews();
        }

        private void BtnComplainBook_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Укажите причину жалобы на книгу:");
            if (dialog.ShowDialog() != true) return;
            string reason = dialog.Answer;
            if (string.IsNullOrWhiteSpace(reason)) return;

            bool ok = ComplaintService.AddOnBook(AppSession.CurrentUser.UserId, _bookId, reason);
            MessageBox.Show(ok ? "Жалоба отправлена." : "Вы уже жаловались на эту книгу.");
        }

        private void BtnComplainAuthor_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Укажите причину жалобы на автора:");
            if (dialog.ShowDialog() != true) return;
            string reason = dialog.Answer;
            if (string.IsNullOrWhiteSpace(reason)) return;

            // Жалоба на автора = жалоба на книгу с пометкой об авторе
            bool ok = ComplaintService.AddOnBook(AppSession.CurrentUser.UserId, _bookId,
                "[Жалоба на автора] " + reason);
            MessageBox.Show(ok ? "Жалоба отправлена." : "Вы уже жаловались.");
        }

        private void BtnComplainReview_Click(object sender, RoutedEventArgs e)
        {
            int reviewId = (int)((Button)sender).Tag;
            var dialog = new InputDialog("Укажите причину жалобы на комментарий:");
            if (dialog.ShowDialog() != true) return;
            string reason = dialog.Answer;
            if (string.IsNullOrWhiteSpace(reason)) return;

            bool ok = ComplaintService.AddOnReview(
                AppSession.CurrentUser.UserId, reviewId, reason);
            MessageBox.Show(ok ? "Жалоба отправлена." : "Вы уже жаловались на этот отзыв.");
        }

        private void BtnFreezeBook_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Заморозить книгу?", "Подтверждение",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            _book.IsFrozen = true;
            Core.Context.SaveChanges();
            MessageBox.Show("Книга заморожена.");
            NavigationService.GoBack();
        }

        private void BtnFreezeReview_Click(object sender, RoutedEventArgs e)
        {
            int reviewId = (int)((Button)sender).Tag;
            if (MessageBox.Show("Удалить отзыв?", "Подтверждение",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            ReviewService.Freeze(reviewId);
            LoadReviews();
        }
    }
}
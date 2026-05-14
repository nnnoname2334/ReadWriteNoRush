using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReadWriteNoRush.Helpers;
using ReadWriteNoRush.Services;

namespace ReadWriteNoRush.Views
{
    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();
            LoadBooks();
        }

        private void LoadBooks()
        {
            RenderPublished();
            RenderFrozen();
        }

        private void RenderPublished()
        {
            PublishedPanel.Children.Clear();
            var books = BookService.GetByAuthor(AppSession.CurrentUser.UserId);

            if (!books.Any())
            {
                PublishedPanel.Children.Add(new TextBlock
                {
                    Text = "У вас нет опубликованных книг.",
                    Foreground = Brushes.Gray
                });
                return;
            }

            foreach (var book in books)
            {
                var card = MakeCard(book, false);
                PublishedPanel.Children.Add(card);
            }
        }

        private void RenderFrozen()
        {
            FrozenPanel.Children.Clear();
            var books = BookService.GetFrozenByAuthor(AppSession.CurrentUser.UserId);

            if (!books.Any())
            {
                FrozenPanel.Children.Add(new TextBlock
                {
                    Text = "Нет замороженных книг.",
                    Foreground = Brushes.Gray
                });
                return;
            }

            foreach (var book in books)
            {
                var card = MakeCard(book, true);
                FrozenPanel.Children.Add(card);
            }
        }

        private Border MakeCard(Books book, bool isFrozen)
        {
            var card = new Border
            {
                Width = 170,
                Height = 220,
                Margin = new Thickness(0, 0, 12, 12),
                Background = isFrozen
                    ? new SolidColorBrush(Color.FromRgb(240, 240, 255))
                    : Brushes.White,
                CornerRadius = new CornerRadius(8),
                BorderBrush = new SolidColorBrush(
                    isFrozen ? Color.FromRgb(142, 68, 173) : Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10)
            };

            var sp = new StackPanel();

            sp.Children.Add(new TextBlock
            {
                Text = book.Title,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = 55
            });

            var genres = string.Join(", ", book.Genres.Select(g => g.GenreName));
            sp.Children.Add(new TextBlock
            {
                Text = genres,
                Foreground = Brushes.SteelBlue,
                FontSize = 10,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0)
            });

            if (isFrozen)
            {
                sp.Children.Add(new TextBlock
                {
                    Text = "❄ Заморожена",
                    Foreground = new SolidColorBrush(Color.FromRgb(142, 68, 173)),
                    FontSize = 11,
                    Margin = new Thickness(0, 6, 0, 0)
                });

                var btnUnfreeze = new Button
                {
                    Content = "Оспорить",
                    Height = 28,
                    Margin = new Thickness(0, 8, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(142, 68, 173)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = book.BookId
                };
                btnUnfreeze.Click += BtnUnfreezeBook_Click;
                sp.Children.Add(btnUnfreeze);
            }
            else
            {
                var btnEdit = new Button
                {
                    Content = "Редактировать",
                    Height = 28,
                    Margin = new Thickness(0, 8, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(74, 144, 217)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = book.BookId
                };
                btnEdit.Click += BtnEdit_Click;
                sp.Children.Add(btnEdit);
            }

            card.Child = sp;
            return card;
        }

        private void BtnAddBook_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new AddEditBookPage(null));

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;
            NavigationService.Navigate(new AddEditBookPage(bookId));
        }

        private void BtnUnfreezeBook_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;
            var dialog = new InputDialog("Укажите причину оспаривания заморозки:");
            if (dialog.ShowDialog() != true) return;
            string reason = dialog.Answer;
            if (string.IsNullOrWhiteSpace(reason)) return;

            bool ok = BookService.ApplyForUnfreezeBook(
                AppSession.CurrentUser.UserId, bookId, reason);
            MessageBox.Show(ok
                ? "Заявка отправлена."
                : "У вас уже есть активная заявка по этой книге.");
        }
    }
}
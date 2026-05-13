using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReadWriteNoRush.Helpers;
using ReadWriteNoRush.Services;

namespace ReadWriteNoRush.Views
{
    public partial class ReadingListPage : Page
    {
        private int _currentStatusId = 4; // По умолчанию "Прочитано"
        private List<ReadingLists> _allItems;

        public ReadingListPage()
        {
            InitializeComponent();
            LoadGenres();
            LoadBooks();
            HighlightActiveTab();
        }

        private void LoadGenres()
        {
            var genres = Core.Context.Genres.ToList();
            CmbGenre.Items.Add(new ComboBoxItem { Content = "Все жанры", IsSelected = true });
            foreach (var g in genres)
                CmbGenre.Items.Add(new ComboBoxItem { Content = g.GenreName, Tag = g.GenreId });
            CmbGenre.SelectedIndex = 0;
        }

        private void LoadBooks()
        {
            _allItems = ReadingListService.GetByStatus(
                AppSession.CurrentUser.UserId, _currentStatusId);
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allItems == null) return;

            var items = _allItems.AsEnumerable();

            string query = TxtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(query))
                items = items.Where(i =>
                    i.Books.Title.ToLower().Contains(query) ||
                    i.Books.Users.DisplayName.ToLower().Contains(query));

            if (CmbGenre.SelectedItem is ComboBoxItem gi && gi.Tag is int genreId)
                items = items.Where(i => i.Books.Genres.Any(g => g.GenreId == genreId));

            if (CmbSort.SelectedIndex == 1)
                items = items.OrderByDescending(i =>
                    BookService.GetAvgRating(i.BookId));
            else
                items = items.OrderBy(i => i.Books.Title);

            RenderBooks(items.ToList());
        }

        private void RenderBooks(List<ReadingLists> items)
        {
            BooksPanel.Children.Clear();

            if (!items.Any())
            {
                BooksPanel.Children.Add(new TextBlock
                {
                    Text = "В этом списке пока нет книг.",
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(8)
                });
                return;
            }

            foreach (var item in items)
            {
                var book = item.Books;
                double rating = BookService.GetAvgRating(book.BookId);

                var card = new Border
                {
                    Width = 170,
                    Height = 260,
                    Margin = new Thickness(8),
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(8),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
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

                sp.Children.Add(new TextBlock
                {
                    Text = book.Users?.DisplayName ?? "—",
                    Foreground = Brushes.Gray,
                    FontSize = 11,
                    Margin = new Thickness(0, 4, 0, 0)
                });

                sp.Children.Add(new TextBlock
                {
                    Text = rating > 0 ? $"⭐ {rating:F1}" : "Нет оценок",
                    Foreground = Brushes.OrangeRed,
                    FontSize = 12,
                    Margin = new Thickness(0, 4, 0, 0)
                });

                // Переместить в другой список
                var cmb = new ComboBox
                {
                    Height = 28,
                    Margin = new Thickness(0, 8, 0, 4),
                    Tag = book.BookId
                };
                var statuses = Core.Context.ReadingListStatuses.ToList();
                foreach (var s in statuses)
                    cmb.Items.Add(new ComboBoxItem
                    {
                        Content = s.StatusName,
                        Tag = s.StatusId,
                        IsSelected = s.StatusId == _currentStatusId
                    });
                cmb.SelectionChanged += CmbMoveStatus_Changed;
                sp.Children.Add(cmb);

                // Кнопка открыть
                var btnOpen = new Button
                {
                    Content = "Открыть",
                    Height = 28,
                    Background = new SolidColorBrush(Color.FromRgb(74, 144, 217)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = book.BookId,
                    Margin = new Thickness(0, 4, 0, 0)
                };
                btnOpen.Click += (s, e) =>
                    NavigationService.Navigate(new BookPage((int)((Button)s).Tag));
                sp.Children.Add(btnOpen);

                card.Child = sp;
                BooksPanel.Children.Add(card);
            }
        }

        private void CmbMoveStatus_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cmb &&
                cmb.SelectedItem is ComboBoxItem item &&
                item.Tag is int newStatusId &&
                cmb.Tag is int bookId)
            {
                if (newStatusId == _currentStatusId) return;
                ReadingListService.AddOrMove(
                    AppSession.CurrentUser.UserId, bookId, newStatusId);
                LoadBooks();
            }
        }

        private void HighlightActiveTab()
        {
            var active = new SolidColorBrush(Color.FromRgb(74, 144, 217));
            var normal = new SolidColorBrush(Color.FromRgb(240, 240, 240));

            BtnAbandoned.Background = _currentStatusId == 1 ? active : normal;
            BtnPlanned.Background = _currentStatusId == 2 ? active : normal;
            BtnReading.Background = _currentStatusId == 3 ? active : normal;
            BtnRead.Background = _currentStatusId == 4 ? active : normal;

            BtnAbandoned.Foreground = _currentStatusId == 1 ? Brushes.White : Brushes.Black;
            BtnPlanned.Foreground = _currentStatusId == 2 ? Brushes.White : Brushes.Black;
            BtnReading.Foreground = _currentStatusId == 3 ? Brushes.White : Brushes.Black;
            BtnRead.Foreground = _currentStatusId == 4 ? Brushes.White : Brushes.Black;
        }

        private void BtnStatus_Click(object sender, RoutedEventArgs e)
        {
            _currentStatusId = int.Parse(((Button)sender).Tag.ToString());
            HighlightActiveTab();
            LoadBooks();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
            => ApplyFilters();

        private void CmbGenre_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();
    }
}
using ReadWriteNoRush.Helpers;
using ReadWriteNoRush.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReadWriteNoRush.Views
{
    public partial class CatalogPage : Page
    {
        private List<Books> _allBooks;

        public CatalogPage()
        {
            InitializeComponent();
            LoadGenres();
            LoadBooks();
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
            _allBooks = BookService.GetAll();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allBooks == null) return; 

            var books = _allBooks.AsEnumerable();

            // Поиск
            string query = TxtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(query))
                books = books.Where(b =>
                    b.Title.ToLower().Contains(query) ||
                    b.Users.DisplayName.ToLower().Contains(query));

            // Фильтр по жанру
            if (CmbGenre.SelectedItem is ComboBoxItem gi && gi.Tag is int genreId)
                books = books.Where(b => b.Genres.Any(g => g.GenreId == genreId));

            // Сортировка
            if (CmbSort.SelectedIndex == 1)
                books = books.OrderByDescending(b => BookService.GetAvgRating(b.BookId));
            else
                books = books.OrderBy(b => b.Title);

            RenderBooks(books.ToList());
        }

        private void RenderBooks(List<Books> books)
        {
            BooksPanel.Children.Clear();

            foreach (var book in books)
            {
                double rating = BookService.GetAvgRating(book.BookId);

                var card = new Border
                {
                    Width = 160,
                    Height = 280,
                    Margin = new Thickness(8),
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(8),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand,
                    Tag = book
                };

                var sp = new StackPanel { Margin = new Thickness(10) };

                // Обложка
                var img = new System.Windows.Controls.Image
                {
                    Height = 80,
                    Stretch = System.Windows.Media.Stretch.UniformToFill,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                try
                {
                    if (!string.IsNullOrEmpty(book.CoverPath))
                    {
                        var uri = new Uri($"pack://application:,,,/Assets/{book.CoverPath}",
                            UriKind.Absolute);
                        img.Source = new System.Windows.Media.Imaging.BitmapImage(uri);
                    }
                }
                catch { }
                sp.Children.Add(img);

                sp.Children.Add(new TextBlock
                {
                    Text = book.Title,
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    MaxHeight = 60
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
                    Margin = new Thickness(0, 6, 0, 0)
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

                var btnOpen = new Button
                {
                    Content = "Открыть",
                    Height = 28,
                    Margin = new Thickness(0, 8, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(74, 144, 217)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Tag = book
                };
                btnOpen.Click += BtnOpen_Click;
                sp.Children.Add(btnOpen);

                var btnAdd = new Button
                {
                    Content = "+ В список",
                    Height = 28,
                    Margin = new Thickness(0, 4, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(39, 174, 96)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Tag = book.BookId
                };
                btnAdd.Click += BtnAddToList_Click;
                sp.Children.Add(btnAdd);

                card.Child = sp; // <- теперь в самом конце
                BooksPanel.Children.Add(card);

                btnAdd.Visibility = AppSession.IsFrozen ? Visibility.Collapsed : Visibility.Visible;

            }
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            var book = (Books)((Button)sender).Tag;
            NavigationService.Navigate(new BookPage(book.BookId));
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
            => ApplyFilters();

        private void CmbGenre_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;

            var menu = new ContextMenu();
            var statuses = Core.Context.ReadingListStatuses.ToList();

            foreach (var s in statuses)
            {
                var item = new MenuItem { Header = s.StatusName, Tag = s.StatusId };
                item.Click += (ms, me) =>
                {
                    int statusId = (int)((MenuItem)ms).Tag;
                    ReadingListService.AddOrMove(
                        AppSession.CurrentUser.UserId, bookId, statusId);
                    MessageBox.Show("Книга добавлена в список «" +
                        ((MenuItem)ms).Header + "».");
                };
                menu.Items.Add(item);
            }

            menu.IsOpen = true;
        }
    }
}
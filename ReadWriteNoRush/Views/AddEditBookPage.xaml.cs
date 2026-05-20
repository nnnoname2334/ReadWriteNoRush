using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ReadWriteNoRush.Helpers;
using ReadWriteNoRush.Services;

namespace ReadWriteNoRush.Views
{
    public partial class AddEditBookPage : Page
    {
        private int? _bookId;
        private Books _book;
        private List<CheckBox> _genreCheckBoxes = new List<CheckBox>();

        public AddEditBookPage(int? bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            TxtPageTitle.Text = bookId == null ? "Добавить книгу" : "Редактировать книгу";
            LoadGenres();
            if (bookId != null) LoadBook();
        }

        private void LoadGenres()
        {
            var genres = Core.Context.Genres.ToList();
            foreach (var g in genres)
            {
                var cb = new CheckBox
                {
                    Content = g.GenreName,
                    Tag = g.GenreId,
                    Margin = new Thickness(0, 0, 12, 8)
                };
                _genreCheckBoxes.Add(cb);
                GenresPanel.Children.Add(cb);
            }
        }

        private void LoadBook()
        {
            _book = Core.Context.Books
                .Include("Genres")
                .FirstOrDefault(b => b.BookId == _bookId);

            if (_book == null) return;

            TxtTitle.Text = _book.Title;
            TxtDesc.Text = _book.Description;
            TxtContent.Text = _book.Content;
            TxtCover.Text = _book.CoverPath ?? "";
            var bookGenreIds = _book.Genres.Select(g => g.GenreId).ToList();
            foreach (var cb in _genreCheckBoxes)
                if (bookGenreIds.Contains((int)cb.Tag))
                    cb.IsChecked = true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {

            string title = TxtTitle.Text.Trim();
            string desc = TxtDesc.Text.Trim();
            string content = TxtContent.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Введите название книги.");
                return;
            }

            var selectedGenres = _genreCheckBoxes
                .Where(cb => cb.IsChecked == true)
                .Select(cb => Core.Context.Genres.Find((int)cb.Tag))
                .ToList();

            if (_bookId == null)
            {
                var newBook = new Books
                {
                    Title = title,
                    Description = desc,
                    Content = content,
                    AuthorId = AppSession.CurrentUser.UserId,
                    IsFrozen = false,
                    CreatedAt = System.DateTime.Now

                };
                newBook.CoverPath = TxtCover.Text.Trim();
                foreach (var g in selectedGenres)
                    newBook.Genres.Add(g);

                BookService.Add(newBook);
                MessageBox.Show("Книга добавлена!");
            }
            else
            {
                _book.CoverPath = TxtCover.Text.Trim();
                _book.Title = title;
                _book.Description = desc;
                _book.Content = content;

                _book.Genres.Clear();
                foreach (var g in selectedGenres)
                    _book.Genres.Add(g);

                BookService.Update();
                MessageBox.Show("Книга обновлена!");
            }

            NavigationService.GoBack();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
            => NavigationService.GoBack();
    }
}
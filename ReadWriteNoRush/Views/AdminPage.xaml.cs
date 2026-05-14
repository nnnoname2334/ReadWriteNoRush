using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReadWriteNoRush.Services;

namespace ReadWriteNoRush.Views
{
    public partial class AdminPage : Page
    {
        private int _currentTab = 0;

        public AdminPage()
        {
            InitializeComponent();
            HighlightTab();
            LoadTab();
        }

        private void BtnTab_Click(object sender, RoutedEventArgs e)
        {
            _currentTab = int.Parse(((Button)sender).Tag.ToString());
            HighlightTab();
            LoadTab();
        }

        private void HighlightTab()
        {
            var active = new SolidColorBrush(Color.FromRgb(74, 144, 217));
            var normal = new SolidColorBrush(Color.FromRgb(240, 240, 240));

            BtnTabComplaints.Background = _currentTab == 0 ? active : normal;
            BtnTabUnfreeze.Background = _currentTab == 1 ? active : normal;
            BtnTabRoles.Background = _currentTab == 2 ? active : normal;
            BtnTabFrozen.Background = _currentTab == 3 ? active : normal;
            BtnTabUsers.Background = _currentTab == 4 ? active : normal;

            BtnTabComplaints.Foreground = _currentTab == 0 ? Brushes.White : Brushes.Black;
            BtnTabUnfreeze.Foreground = _currentTab == 1 ? Brushes.White : Brushes.Black;
            BtnTabRoles.Foreground = _currentTab == 2 ? Brushes.White : Brushes.Black;
            BtnTabFrozen.Foreground = _currentTab == 3 ? Brushes.White : Brushes.Black;
            BtnTabUsers.Foreground = _currentTab == 4 ? Brushes.White : Brushes.Black;
        }

        private void LoadTab()
        {
            ContentPanel.Children.Clear();
            switch (_currentTab)
            {
                case 0: LoadComplaints(); break;
                case 1: LoadUnfreeze(); break;
                case 2: LoadRoleApps(); break;
                case 3: LoadFrozen(); break;
                case 4: LoadUsers(); break;
            }
        }

        // ── Жалобы ──────────────────────────────────────────
        private void LoadComplaints()
        {
            var list = AdminService.GetComplaints();
            AddTitle("Жалобы");
            if (!list.Any()) { AddEmpty("Жалоб нет."); return; }

            foreach (var c in list)
            {
                string target = c.BookId != null
                    ? $"Книга: {c.Books?.Title}"
                    : $"Отзыв #{c.ReviewId}";

                var card = MakeCard();
                var sp = (StackPanel)card.Child;

                sp.Children.Add(Bold($"От: {c.Users?.DisplayName} | {target}"));
                sp.Children.Add(Gray(c.Reason));

                var btns = Row();
                btns.Children.Add(MakeBtn("Принять (удалить)", "#27AE60", c.ComplaintId,
                    (id) => { AdminService.DeleteComplaint(id); LoadTab(); }));
                btns.Children.Add(MakeBtn("Отклонить", "#E74C3C", c.ComplaintId,
                    (id) => { AdminService.DeleteComplaint(id); LoadTab(); }));
                sp.Children.Add(btns);

                ContentPanel.Children.Add(card);
            }
        }

        // ── Заявки на разморозку ─────────────────────────────
        private void LoadUnfreeze()
        {
            var list = AdminService.GetUnfreezeApps();
            AddTitle("Заявки на разморозку");
            if (!list.Any()) { AddEmpty("Заявок нет."); return; }

            foreach (var a in list)
            {
                string target = a.BookId == null
                    ? $"Аккаунт: {a.Users?.DisplayName}"
                    : $"Книга: {a.Books?.Title}";

                var card = MakeCard();
                var sp = (StackPanel)card.Child;

                sp.Children.Add(Bold(target));
                sp.Children.Add(Gray(a.Reason));

                var btns = Row();
                btns.Children.Add(MakeBtn("Одобрить", "#27AE60", a.UnfreezeAppId,
                    (id) => { AdminService.ResolveUnfreeze(id, true); LoadTab(); }));
                btns.Children.Add(MakeBtn("Отклонить", "#E74C3C", a.UnfreezeAppId,
                    (id) => { AdminService.ResolveUnfreeze(id, false); LoadTab(); }));
                sp.Children.Add(btns);

                ContentPanel.Children.Add(card);
            }
        }

        // ── Заявки на роль автора ────────────────────────────
        private void LoadRoleApps()
        {
            var list = AdminService.GetRoleApps();
            AddTitle("Заявки на роль Автора");
            if (!list.Any()) { AddEmpty("Заявок нет."); return; }

            foreach (var a in list)
            {
                var card = MakeCard();
                var sp = (StackPanel)card.Child;

                sp.Children.Add(Bold($"{a.Users?.DisplayName} (@{a.Users?.Login})"));
                sp.Children.Add(Gray($"Подана: {a.CreatedAt:dd.MM.yyyy}"));

                var btns = Row();
                btns.Children.Add(MakeBtn("Одобрить", "#27AE60", a.ApplicationId,
                    (id) => { AdminService.ResolveRoleApp(id, true); LoadTab(); }));
                btns.Children.Add(MakeBtn("Отклонить", "#E74C3C", a.ApplicationId,
                    (id) => { AdminService.ResolveRoleApp(id, false); LoadTab(); }));
                sp.Children.Add(btns);

                ContentPanel.Children.Add(card);
            }
        }

        // ── Замороженные ─────────────────────────────────────
        private void LoadFrozen()
        {
            AddTitle("Замороженные книги");
            var books = AdminService.GetFrozenBooks();
            if (!books.Any()) AddEmpty("Нет замороженных книг.");
            foreach (var b in books)
            {
                var card = MakeCard();
                var sp = (StackPanel)card.Child;
                sp.Children.Add(Bold(b.Title));
                sp.Children.Add(Gray($"Автор: {b.Users?.DisplayName}"));
                ContentPanel.Children.Add(card);
            }

            AddTitle("Замороженные пользователи");
            var users = AdminService.GetFrozenUsers();
            if (!users.Any()) AddEmpty("Нет замороженных пользователей.");
            foreach (var u in users)
            {
                var card = MakeCard();
                var sp = (StackPanel)card.Child;
                sp.Children.Add(Bold($"{u.DisplayName} (@{u.Login})"));
                sp.Children.Add(Gray($"Роль: {u.Roles?.RoleName}"));

                var btns = Row();
                btns.Children.Add(MakeBtn("Разморозить", "#27AE60", u.UserId,
                    (id) => { AdminService.UnfreezeUser(id); LoadTab(); }));
                sp.Children.Add(btns);

                ContentPanel.Children.Add(card);
            }
        }

        // ── Пользователи ─────────────────────────────────────
        private void LoadUsers()
        {
            AddTitle("Все пользователи");
            var roles = Core.Context.Roles.ToList();
            var users = AdminService.GetAllUsers();

            foreach (var u in users)
            {
                var card = MakeCard();
                var sp = (StackPanel)card.Child;

                sp.Children.Add(Bold($"{u.DisplayName} (@{u.Login})"));
                sp.Children.Add(Gray($"{u.Email} | {u.Roles?.RoleName}"));

                var btns = Row();

                // Сменить роль
                var cmb = new ComboBox
                {
                    Width = 130,
                    Height = 28,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                foreach (var r in roles)
                    cmb.Items.Add(new ComboBoxItem
                    {
                        Content = r.RoleName,
                        Tag = r.RoleId,
                        IsSelected = r.RoleId == u.RoleId
                    });
                var uid = u.UserId;
                cmb.SelectionChanged += (s, e) =>
                {
                    if (cmb.SelectedItem is ComboBoxItem item && item.Tag is int rid)
                    {
                        AdminService.SetRole(uid, rid);
                        LoadTab();
                    }
                };
                btns.Children.Add(cmb);

                // Сменить пароль
                btns.Children.Add(MakeBtn("Сменить пароль", "#8E44AD", u.UserId, (id) =>
                {
                    var d = new InputDialog("Новый пароль:");
                    if (d.ShowDialog() == true && !string.IsNullOrWhiteSpace(d.Answer))
                    {
                        AdminService.SetPassword(id, d.Answer);
                        MessageBox.Show("Пароль изменён.");
                    }
                }));

                // Заморозить
                if (!u.IsFrozen)
                    btns.Children.Add(MakeBtn("Заморозить", "#E74C3C", u.UserId,
                        (id) => { AdminService.FreezeUser(id); LoadTab(); }));
                else
                    btns.Children.Add(MakeBtn("Разморозить", "#27AE60", u.UserId,
                        (id) => { AdminService.UnfreezeUser(id); LoadTab(); }));

                sp.Children.Add(btns);
                ContentPanel.Children.Add(card);
            }
        }

        // ── Хелперы ──────────────────────────────────────────
        private void AddTitle(string text)
        {
            ContentPanel.Children.Add(new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 8, 0, 10)
            });
        }

        private void AddEmpty(string text)
        {
            ContentPanel.Children.Add(new TextBlock
            {
                Text = text,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 16)
            });
        }

        private Border MakeCard()
        {
            var card = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 10),
                Child = new StackPanel()
            };
            return card;
        }

        private TextBlock Bold(string text) => new TextBlock
        {
            Text = text,
            FontWeight = FontWeights.Bold,
            TextWrapping = TextWrapping.Wrap
        };

        private TextBlock Gray(string text) => new TextBlock
        {
            Text = text,
            Foreground = Brushes.Gray,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 0)
        };

        private StackPanel Row() => new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0)
        };

        private Button MakeBtn(string text, string hex, int id,
            System.Action<int> action)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            var btn = new Button
            {
                Content = text,
                Height = 28,
                Padding = new Thickness(10, 0, 10, 0),
                Margin = new Thickness(0, 0, 8, 0),
                Background = new SolidColorBrush(color),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = id
            };
            btn.Click += (s, e) => action((int)((Button)s).Tag);
            return btn;
        }
    }
}
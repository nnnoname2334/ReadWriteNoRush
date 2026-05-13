using System.Windows;

namespace ReadWriteNoRush.Views
{
    public partial class InputDialog : Window
    {
        public string Answer => TxtInput.Text.Trim();

        public InputDialog(string prompt)
        {
            InitializeComponent();
            TxtPrompt.Text = prompt;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
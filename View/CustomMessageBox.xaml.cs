using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.Sharp;

namespace TamAnh_EMR_System.View
{
    public partial class CustomMessageBox : Window
    {
        public string Message { get; set; }
        public string Title { get; set; }

        public IconChar Icon { get; set; }
        public Brush HeaderColor { get; set; }
        public Brush IconBackground { get; set; }

        public CustomMessageBox(string message, string title, string type = "info")
        {
            InitializeComponent();

            Message = message;
            Title = title;

            SetStyle(type);
            GenerateButtons(type);

            DataContext = this;
        }

        // 🎨 STYLE
        private void SetStyle(string type)
        {
            switch (type.ToLower())
            {
                case "success":
                    HeaderColor = Brushes.SeaGreen;
                    IconBackground = Brushes.SeaGreen;
                    Icon = IconChar.Bug;
                    break;

                case "warning":
                    HeaderColor = Brushes.DarkOrange;
                    IconBackground = Brushes.DarkOrange;
                    Icon = IconChar.Bug;
                    break;

                case "error":
                    HeaderColor = Brushes.IndianRed;
                    IconBackground = Brushes.IndianRed;
                    Icon = IconChar.Bug;
                    break;

                case "question":
                    HeaderColor = Brushes.DodgerBlue;
                    IconBackground = Brushes.DodgerBlue;
                    Icon = IconChar.Bug;
                    break;

                default:
                    HeaderColor = Brushes.Teal;
                    IconBackground = Brushes.Teal;
                    Icon = IconChar.Bug;
                    break;
            }
        }

        // 🔘 BUTTON
        private void GenerateButtons(string type)
        {
            ButtonPanel.Children.Clear();

            switch (type.ToLower())
            {
                case "question":
                    AddButton("Yes", true, Brushes.SeaGreen);
                    AddButton("No", false, Brushes.Gray);
                    break;

                case "warning":
                    AddButton("OK", true, Brushes.DarkOrange);
                    break;

                case "error":
                    AddButton("OK", true, Brushes.IndianRed);
                    break;

                default:
                    AddButton("OK", true, Brushes.SeaGreen);
                    break;
            }
        }

        private void AddButton(string text, bool result, Brush color)
        {
            var btn = new Button
            {
                Content = text,
                Width = 80,
                Height = 30,
                Margin = new Thickness(5),
                Background = color,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            btn.Click += (s, e) =>
            {
                DialogResult = result;
                Close();
            };

            ButtonPanel.Children.Add(btn);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // 🚀 GỌI NHANH
        public static bool? Show(string message, string title = "Thông báo", string type = "info")
        {
            var box = new CustomMessageBox(message, title, type);
            return box.ShowDialog();
        }
    }
}
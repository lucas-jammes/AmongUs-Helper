using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AmongUs_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Character_LeftClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image img && FindName($"{img.Name}_Label") is Label associatedLabel)
            {
                int state = img.Tag is int tag ? tag : 0;

                switch (state)
                {
                    case 0:
                        associatedLabel.Foreground = Brushes.LimeGreen;
                        associatedLabel.Content = "SAFE";
                        break;
                    case 1:
                        associatedLabel.Foreground = Brushes.Red;
                        associatedLabel.Content = "SUS";
                        break;
                    case 2:
                        associatedLabel.Foreground = Brushes.White;
                        associatedLabel.Content = associatedLabel.Name.Replace("_Label", "");
                        break;
                }

                img.Tag = (state + 1) % 3;
            }
        }

        private void Character_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image img)
            {
                if (FindName($"{img.Name}_Label") is Label associatedLabel)
                {
                    bool isAlive = img.Opacity == 1.0;

                    img.Opacity = isAlive ? 0.15 : 1.0;
                    associatedLabel.Foreground = isAlive ? Brushes.DarkSlateGray : Brushes.White;
                    associatedLabel.Content = isAlive ? "DEAD" : associatedLabel.Name.Replace("_Label", "");
                }
            }
        }

        private void Refresh_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (object element in MainGrid.Children)
            {
                // Reset labels to default state
                if (element is Label label && label.Name != "WindowTitle")
                {
                    label.Foreground = Brushes.White;
                    label.Content = label.Name.Replace("_Label", "");
                }
                // Reset character images to default state
                if (element is Border border && border.Child is Image img)
                {
                    img.Tag = 0;
                    img.Opacity = 1.0;
                }
            }
        }


        // Close when close_button is clicked
        private void Close_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        // Drag Window by clicking on the top bar
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}

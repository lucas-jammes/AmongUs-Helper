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

        // Drag Window by clicking on the top bar
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Character_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image img && img.Tag != null)
            {
                // Get the associated label name
                string labelName = $"{img.Name}_Label";

                // Find the label in the window
                if (FindName(labelName) is Label associatedLabel)
                {
                    // Get current state of the image
                    if (int.TryParse(img.Tag.ToString(), out int state))
                    {
                        // Update the image source based on the new state
                        switch (state)
                        {
                            case 0:
                                associatedLabel.Foreground = Brushes.LimeGreen;
                                break;
                            case 1:
                                associatedLabel.Foreground = Brushes.Red;
                                break;
                            case 2:
                                associatedLabel.Foreground = Brushes.DarkSlateGray;
                                break;
                            case 3:
                                associatedLabel.Foreground = Brushes.White;
                                break;
                        }

                        // Increment the state
                        state = (state + 1) % 4;
                        img.Tag = state;
                    }
                    else
                    {
                        _ = MessageBox.Show("Invalid state value.");
                    }
                }
            }
        }

        // Close when close_button is clicked
        private void Close_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Refresh_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (object? element in MainGrid.Children)
            {
                if (element is Label label && label.Name != "WindowTitle")
                {
                    label.Foreground = Brushes.White;
                    label.Content = label.Name.Replace("_Label", ""); // Remet le nom du perso
                }
            }
        }
    }
}

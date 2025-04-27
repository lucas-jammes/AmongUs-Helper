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
            // Check if the sender is an Image and if its Label exists
            if (sender is not Image img || FindName($"{img.Name}_Label") is not Label associatedLabel)
                return;

            // If the character is dead, reset the state
            if (img.Opacity != 1.0)
            {
                ResetCharacterState(img, associatedLabel);
                return;
            }

            // Read Tag value to determine the state
            int state = img.Tag is int tag ? tag : 0;

            // Update the state and label based on the current state
            (Brush color, string content) = state switch
            {
                0 => (Brushes.LimeGreen, "SAFE"),
                1 => (Brushes.Red, "SUS"),
                2 => (Brushes.White, associatedLabel.Name.Replace("_Label", "")),
                _ => (Brushes.White, associatedLabel.Name.Replace("_Label", "")),
            };

            // Assign the new color and content to the label
            associatedLabel.Foreground = color;
            associatedLabel.Content = content;

            // Increment the state
            img.Tag = (state + 1) % 3;
        }

        private void ResetCharacterState(Image img, Label label)
        {
            // Reset the character state to default values
            img.Opacity = 1.0;
            img.Tag = 0;
            label.Foreground = Brushes.White;
            label.Content = label.Name.Replace("_Label", "");
        }


        private void Character_RightClick(object sender, MouseButtonEventArgs e)
        {
            // Check if the sender is an Image and if its Label exists
            if (sender is not Image img || FindName($"{img.Name}_Label") is not Label label)
                return;

            // Verify if the character is alive
            bool isAlive = img.Opacity == 1.0;

            // Change opacity, foreground color, and label content based on the alive state
            img.Opacity = isAlive ? 0.15 : 1.0;
            label.Foreground = isAlive ? Brushes.DarkSlateGray : Brushes.White;
            label.Content = isAlive ? "DEAD" : label.Name.Replace("_Label", "");
        }

        private void Refresh_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var element in MainGrid.Children)
            {
                switch (element)
                {
                    case Label label when label.Name != "WindowTitle":
                        label.Foreground = Brushes.White;
                        label.Content = label.Name.Replace("_Label", "");
                        break;

                    case Border { Child: Image img }:
                        img.Tag = 0;
                        img.Opacity = 1.0;
                        break;
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

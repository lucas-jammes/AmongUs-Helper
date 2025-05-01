using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Sus_Companion.Properties;

namespace Sus_Companion
{
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
            {
                return;
            }

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
            {
                return;
            }

            // Verify if the character is alive
            bool isAlive = img.Opacity == 1.0;

            // Change opacity, foreground color, and label content based on the alive state
            img.Opacity = isAlive ? 0.15 : 1.0;
            label.Foreground = isAlive ? Brushes.DarkSlateGray : Brushes.White;
            label.Content = isAlive ? "DEAD" : label.Name.Replace("_Label", "");
        }

        private void Refresh_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Refresh the state of all characters
            foreach (object? element in MainGrid.Children)
            {
                // Check if the element is a Label or an Image
                switch (element)
                {
                    // Reset the state of Labels
                    case Label label when label.Name != "WindowTitle":
                        label.Foreground = Brushes.White;
                        label.Content = label.Name.Replace("_Label", "");
                        break;

                    // Reset the state of Images
                    case Border { Child: Image img }:
                        img.Tag = 0;
                        img.Opacity = 1.0;
                        break;
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Save the window position before closing
            Settings.Default.WindowTop = Top;
            Settings.Default.WindowLeft = Left;
            Settings.Default.Save();
            base.OnClosing(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Check if the window position is not default (0,0)
            if (Settings.Default.WindowTop != 0 || Settings.Default.WindowLeft != 0)
            {
                // Set the window position to the saved values
                Top = Settings.Default.WindowTop;
                Left = Settings.Default.WindowLeft;
            }
        }

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow the window to be dragged when the top bar is clicked
            DragMove();
        }

        private void GitHub_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Open the GitHub page in the default web browser
            _ = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/lucas-jammes/SusCompanion",
                UseShellExecute = true
            });
        }

        private void Close_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Close the application when the close button is clicked
            Close();
        }
    }
}

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

        private void MouseEnter_HoverEffect(object sender, MouseEventArgs e)
        {
            // Check if the sender is an Image and if its Label exists
            if (sender is Image img)
            {
                // Set the hex code based on the image name - these are the exact hex codes of each characters
                string hexCode = img.Name switch
                {
                    "Red" => "#C61111",
                    "Blue" => "#132ED2",
                    "Green" => "#00FF00",
                    "Pink" => "#EE54BB",
                    "Orange" => "#F07D0D",
                    "Yellow" => "#F6F657",
                    "Black" => "#3F474E",
                    "White" => "#D7E1F1",
                    "Purple" => "#6B2FBC",
                    "Brown" => "#71491E",
                    "Cyan" => "#38E2DD",
                    "Lime" => "#50F039",
                    "Maroon" => "#6B2B3C",
                    "Rose" => "#ECC0D3",
                    "Banana" => "#FFFEBE",
                    "Tan" => "#928776",
                    "Coral" => "#EC7578",
                    _ => "#000000"
                };

                // Create or retrieve ScaleTransform
                if (img.RenderTransform is not ScaleTransform scale)
                {
                    scale = new ScaleTransform(1.0, 1.0);
                    img.RenderTransform = scale;
                    img.RenderTransformOrigin = new Point(0.5, 0.5);
                }

                // Scale to 1.1 in 0.15 seconds
                DoubleAnimation animX = new(1.1, TimeSpan.FromMilliseconds(150));
                DoubleAnimation animY = new(1.1, TimeSpan.FromMilliseconds(150));
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, animY);

                // Opacity to 0.9 in 0.15 seconds
                DoubleAnimation animOpacity = new(0.9, TimeSpan.FromMilliseconds(150));
                img.BeginAnimation(Image.OpacityProperty, animOpacity);

                // Create or retrieve DropShadowEffect
                if (img.Effect is not DropShadowEffect shadow)
                {
                    shadow = new DropShadowEffect { Color = Colors.Black, ShadowDepth = 0, BlurRadius = 0 };
                    img.Effect = shadow;
                }

                // Color to the specified hex code in 0.1 seconds
                ColorAnimation animColor = new((Color)ColorConverter.ConvertFromString(hexCode), TimeSpan.FromMilliseconds(100));
                shadow.BeginAnimation(DropShadowEffect.ColorProperty, animColor);

                // Blur to 15 in 0.1 seconds
                DoubleAnimation animBlur = new(15, TimeSpan.FromMilliseconds(100));
                shadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, animBlur);
            }
        }

        private void MouseLeave_HoverEffect(object sender, MouseEventArgs e)
        {
            // Check if the sender is an Image and if its Label exists
            if (sender is Image img && img.RenderTransform is ScaleTransform scale && img.Effect is DropShadowEffect shadow)
            {
                // Scale back to 1.0 in 0.15 seconds
                DoubleAnimation animX = new(1.0, TimeSpan.FromMilliseconds(150));
                DoubleAnimation animY = new(1.0, TimeSpan.FromMilliseconds(150));
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, animY);

                // Opacity back to 1.0 in 0.15 seconds
                DoubleAnimation animOpacity = new(1.0, TimeSpan.FromMilliseconds(150));
                img.BeginAnimation(Image.OpacityProperty, animOpacity);

                // Color back to black in 0.15 seconds
                ColorAnimation animColor = new(Colors.Black, TimeSpan.FromMilliseconds(150));
                shadow.BeginAnimation(DropShadowEffect.ColorProperty, animColor);

                // Blur back to 0 in 0.15 seconds
                DoubleAnimation animBlur = new(0, TimeSpan.FromMilliseconds(150));
                shadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, animBlur);
            }
        }
    }
}

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Sus_Companion.Properties;

namespace Sus_Companion
{
    public partial class MainWindow : Window
    {
        // Enable sound by default
        private bool IsSoundEnabled = true;

        public MainWindow()
        {
            InitializeComponent();

            // Update the Stats label located at the bottom of the window
            UpdateStats();
        }

        /// <summary>
        /// Methods concerning the window properties and settings
        /// </summary>
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Save the window position before closing
            Settings.Default.WindowTop = Top;
            Settings.Default.WindowLeft = Left;
            Settings.Default.Save();
            base.OnClosing(e);
        }

        /// <summary>
        /// Methods concerning the characters
        /// </summary>
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

            // Update the Stats label located at the bottom of the window
            UpdateStats();
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


            if (isAlive)
            {
                // Play the sound for dead character
                PlaySound("assets/sounds/dead.wav", 0.2);
                img.Opacity = isAlive ? 0.15 : 1.0;
                label.Foreground = isAlive ? Brushes.DarkSlateGray : Brushes.White;
                label.Content = isAlive ? "DEAD" : label.Name.Replace("_Label", "");
            }
            // Change opacity, foreground color, and label content based on the alive state


            // Update the Stats label located at the bottom of the window
            UpdateStats();
        }

        /// <summary>
        /// Methods concerning TopBar content, including buttons
        /// </summary>
        private async void Refresh_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Refresh the state of all characters
            foreach (object? element in MainGrid.Children)
            {
                // Check if the element is a Label or an Image
                switch (element)
                {
                    // Reset the state of Labels
                    case Label label when label.Name is not "WindowTitle" and not "Stats":
                        label.Foreground = Brushes.White;
                        label.Content = label.Name.Replace("_Label", "");
                        break;

                    case Label label when label.Name == "Stats":
                        // Reset the state of Stats label
                        UpdateStats();
                        break;

                    // Reset the state of Images
                    case Border { Child: Image img }:
                        img.Tag = 0;
                        img.Opacity = 1.0;
                        break;
                }
            }

            // Start the refresh animation, 
            Refresh_Button.IsEnabled = false;
            Refresh_Button_Animation();

            await Task.Delay(500);

            Refresh_Button.IsEnabled = true;
        }

        private void Refresh_Button_Animation()
        {
            // Rotate the refresh button 360 degrees when clicked
            DoubleAnimation rotationAnimation = new()
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(0.5),
                RepeatBehavior = new RepeatBehavior(1)
            };

            RefreshRotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);
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

        private void Sound_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Toggle the sound on and off
            IsSoundEnabled = !IsSoundEnabled;

            if (IsSoundEnabled)
            {
                SoundPath.Data = Geometry.Parse("M3 11V13 M6 8V16 M9 10V14 M12 7V17 M15 4V20 M18 9V15 M21 11V13");
                Sound_Button.Opacity = 1.0;
            }
            else
            {
                SoundPath.Data = Geometry.Parse("M3 11V13 M6 11V13 M9 11V13 M12 10V14 M15 11V13 M18 11V13 M21 11V13");
                Sound_Button.Opacity = 0.5;
            }
        }

        /// <summary>
        /// Miscellaneous methods
        /// </summary>
        private void UpdateStats()
        {
            // Get all Image controls inside MainGrid
            IEnumerable<Image> images = MainGrid.Children
                .OfType<Border>()
                .Select(b => b.Child)
                .OfType<Image>();

            int aliveCount = 0;
            int safeCount = 0;
            int susCount = 0;
            int deadCount = 0;

            foreach (Image img in images)
            {
                // Check if the image is dead (opacity < 1.0) and define his state to 0
                bool isDead = img.Opacity < 1.0;
                int state = img.Tag as int? ?? 0;

                // Increment the respective counters based on the image Tag (state)
                if (isDead)
                {
                    deadCount++;
                }
                else
                {
                    aliveCount++;

                    if (state == 1)
                    {
                        safeCount++;
                    }
                    else if (state == 2)
                    {
                        susCount++;
                    }
                }
            }

            // Build the stats display
            Span span = new();
            span.Inlines.Add(new Run($"{aliveCount} Alive") { Foreground = Brushes.White });
            span.Inlines.Add(new Run("   -   "));
            span.Inlines.Add(new Run($"{safeCount} Safe") { Foreground = Brushes.LimeGreen });
            span.Inlines.Add(new Run("   -   "));
            span.Inlines.Add(new Run($"{susCount} Sus") { Foreground = Brushes.Red });
            span.Inlines.Add(new Run("   -   "));
            span.Inlines.Add(new Run($"{deadCount} Dead") { Foreground = Brushes.DarkSlateGray });

            Stats.Content = span;
        }

        private void PlaySound(string soundFile, double volume)
        {
            // Skip if sound is disabled
            if (!IsSoundEnabled)
            {
                return;
            }

            // Play kill sound effect
            if (File.Exists(soundFile))
            {
                MediaPlayer player = new();
                Uri uri = new(new FileInfo(soundFile).FullName, UriKind.Absolute);
                player.Open(uri);
                player.Volume = volume;
                player.Play();

                player.MediaEnded += (s, e) => player.Close();
            }
        }
    }
}

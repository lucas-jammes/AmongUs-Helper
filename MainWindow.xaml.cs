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

        // List to keep track of active MediaPlayer instances
        private readonly List<MediaPlayer> _activePlayers = new();

        public MainWindow()
        {
            InitializeComponent();
        }


        #region Window lifecycle methods

        /// <summary>
        /// Sets the window position based on saved settings.
        /// </summary>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Update the Stats label located at the bottom of the window
            UpdateStats();

            // Check if the window position is not default (0,0)
            if (Settings.Default.WindowTop != 0 || Settings.Default.WindowLeft != 0)
            {
                // Set the window position to the saved values
                Top = Settings.Default.WindowTop;
                Left = Settings.Default.WindowLeft;
            }

            // Enable or disable sound based on saved settings and set the icon accordingly
            IsSoundEnabled = Properties.Settings.Default.IsSoundEnabled;
            SoundPath.Data = Geometry.Parse(IsSoundEnabled
                ? "M3 11V13 M6 8V16 M9 10V14 M12 7V17 M15 4V20 M18 9V15 M21 11V13"
                : "M3 11V13 M6 11V13 M9 11V13 M12 10V14 M15 11V13 M18 11V13 M21 11V13");
            Sound_Button.Opacity = IsSoundEnabled ? 1.0 : 0.5;
        }

        /// <summary>
        /// Sets the window position before closing.
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Save the window position before closing
            Settings.Default.WindowTop = Top;
            Settings.Default.WindowLeft = Left;
            Settings.Default.Save();
            base.OnClosing(e);
        }

        #endregion

        #region Character interaction methods

        /// <summary>
        /// Handles a left click on a character to cycle its state between SAFE, SUS, and ALIVE (the character's name).
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


        /// <summary>
        /// Handles a right click on a character to toggle its DEAD/ALIVE (character's name) state.
        /// </summary>
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
                PlaySound("dead.wav");
                img.Opacity = isAlive ? 0.15 : 1.0;
                label.Foreground = isAlive ? Brushes.DarkSlateGray : Brushes.White;
                label.Content = isAlive ? "DEAD" : label.Name.Replace("_Label", "");
            }
            // Change opacity, foreground color, and label content based on the alive state


            // Update the Stats label located at the bottom of the window
            UpdateStats();
        }

        /// <summary>
        /// Resets the visual state of a character image and its associated label.
        /// </summary>
        /// <param name="img">The character image control</param>
        /// <param name="label">The label control associated with the character</param>
        private void ResetCharacterState(Image img, Label label)
        {
            // Reset the character state to default values
            img.Opacity = 1.0;
            img.Tag = 0;
            label.Foreground = Brushes.White;
            label.Content = label.Name.Replace("_Label", "");
        }

        #endregion

        #region TopBar and button interaction methods

        /// <summary>
        /// Handles a left click on the refresh button. Starts animation and triggers refresh logic.
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

        /// <summary>
        /// Starts a single 360-degree rotation animation on the refresh icon
        /// </summary>
        private void Refresh_Button_Animation()
        {
           DoubleAnimation rotationAnimation = new()
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(0.5),
                RepeatBehavior = new RepeatBehavior(1)
            };

            RefreshRotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);
        }

        /// <summary>
        /// Handles a mouse click on the top bar to allow window dragging.
        /// </summary>
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// Opens the GitHub repository in the default web browser.
        /// </summary>
        private void GitHub_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Open the GitHub page in the default web browser
            _ = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/lucas-jammes/SusCompanion",
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Closes the application window.
        /// </summary>
        private void Close_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Close the application when the close button is clicked
            Close();
        }

        /// <summary>
        /// Toggles sound on or off and updates the sound icon path.
        /// </summary>
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

            // Save sound settings for future sessions
            Properties.Settings.Default.IsSoundEnabled = IsSoundEnabled;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Updates the Stats label with counts of ALIVE, SAFE, SUS, and DEAD characters.
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

        /// <summary>
        /// Plays a sound file at the specified volume if sound is enabled.
        /// </summary>
        /// <param name="soundFile">The file path to the sound file</param>
        /// <param name="volume">The volume level (0.0 to 1.0)</param>
        private void PlaySound(string resourceName, double volume = 0.2)
        {
            // Skip if sound is not enabled
            if (!IsSoundEnabled)
            {
                return;
            }

            string resourcePath = $"Sus_Companion.assets.sounds.{resourceName}"; // adapte ton namespace

            using var stream = GetType().Assembly.GetManifestResourceStream(resourcePath);
            if (stream == null)
            {
                MessageBox.Show($"Resource {resourcePath} not found.");
                return;
            }

            // Créer un fichier temporaire avec extension .wav
            string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
            using (var fileStream = File.Create(tempFile))
            {
                stream.CopyTo(fileStream);
            }

            var player = new MediaPlayer();
            player.Open(new Uri(tempFile, UriKind.Absolute));
            player.Volume = volume;
            player.Play();

            _activePlayers.Add(player);
            player.MediaEnded += (s, e) =>
            {
                player.Close();
                _activePlayers.Remove(player);

                try { File.Delete(tempFile); } catch { /* ignore delete failure */ }
            };
        }

        #endregion
    }
}

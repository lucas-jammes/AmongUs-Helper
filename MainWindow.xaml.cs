using System.IO;
using System.Media;
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

        // Cache the loaded sound files
        private readonly Dictionary<string, byte[]> _soundCache = new();

        // List to keep track of active MediaPlayer instances
        private readonly List<MediaPlayer> _activePlayers = [];

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

            // Preload sound files from embedded resources
            PreloadSounds();

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
            Settings.Default.WindowTop = Top;
            Settings.Default.WindowLeft = Left;
            Settings.Default.Save();
            base.OnClosing(e);
        }

        private void PreloadSounds()
        {
            string[] soundNames = { "select.wav", "dead.wav", "refresh.wav", "sound-on.wav", "sound-off.wav", "alive.wav" };

            foreach (var name in soundNames)
            {
                string resourcePath = $"Sus_Companion.assets.sounds.{name}";
                using Stream? stream = GetType().Assembly.GetManifestResourceStream(resourcePath);
                if (stream != null)
                {
                    using MemoryStream ms = new();
                    stream.CopyTo(ms);
                    _soundCache[name] = ms.ToArray();
                }
                else
                {
                    _ = MessageBox.Show($"Sound resource {resourcePath} not found.");
                }
            }
        }

        #endregion

        #region Character interaction methods

        /// <summary>
        /// Handles a left click on a character to cycle its state between SAFE, SUS, and ALIVE (the character's name).
        /// </summary>
        private void Character_LeftClick(object sender, MouseButtonEventArgs e)
        {
            // Check if the sender is an Image and if its Label exists
            if (sender is not Image img)
            {
                return;
            }

            if (FindName($"{img.Name}_Label") is not Label label)
            {
                return;
            }

            // If the character is dead, set it to ALIVE state
            if (img.Opacity < 1.0)
            {
                img.Opacity = 1.0;
                img.Tag = 0;
                label.Foreground = Brushes.White;
                label.Content = img.Name;
                UpdateStats();
                return;
            }

            // Else, cycle through the states
            int state = img.Tag is int t ? t : 0;
            state = (state + 1) % 3;
            img.Tag = state;

            // Set the label content and color based on the state
            switch (state)
            {
                case 1:
                    label.Foreground = Brushes.LimeGreen;
                    label.Content = "SAFE";
                    PlaySound("select.wav", 0.3);
                    break;
                case 2:
                    label.Foreground = Brushes.Red;
                    label.Content = "SUS";
                    PlaySound("select.wav", 0.3);
                    break;
                default:
                    label.Foreground = Brushes.White;
                    label.Content = img.Name;
                    PlaySound("alive.wav", 0.3);
                    break;
            }

            UpdateStats();
        }

        /// <summary>
        /// Handles a right click on a character to toggle its DEAD/ALIVE (character's name) state.
        /// </summary>
        private void Character_RightClick(object sender, MouseButtonEventArgs e)
        {
            // Check if the sender is an Image and if its Label exists
            if (sender is not Image img)
            {
                return;
            }

            if (FindName($"{img.Name}_Label") is not Label label)
            {
                return;
            }

            // Check if the character is dead
            bool isDead = img.Opacity < 1.0;

            if (isDead)
            {
                // Set character to ALIVE state
                img.Opacity = 1.0;
                img.Tag = 0;
                label.Foreground = Brushes.White;
                label.Content = img.Name;
            }
            else
            {
                // Play the kill sound effect at 20% volume
                PlaySound("dead.wav", 0.2);

                // Set character to DEAD state
                img.Opacity = 0.15;
                label.Foreground = Brushes.DarkSlateGray;
                label.Content = "DEAD";
            }

            UpdateStats();
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
                switch (element)
                {
                    // Reset character Labels to their original state
                    case Label label when label.Name is not "WindowTitle" and not "Stats":
                        label.Foreground = Brushes.White;
                        label.Content = label.Name.Replace("_Label", "");
                        break;

                    // Reset the Stats Label
                    case Label label when label.Name == "Stats":
                        UpdateStats();
                        break;

                    // Reset the Tag only (keep opacity < 1.0 to let the animation play)
                    case Border { Child: Image img }:
                        img.Tag = 0;
                        break;
                }
            }

            // Disable the button during animation
            Refresh_Button.IsEnabled = false;

            // Start the reset and the refresh icon animations
            RefreshButtonAnimation();
            ResetCharactersAnimation();

            // Play the refresh sound effect at 30% volume
            PlaySound("refresh.wav", 0.3);

            // Delay according to the animation duration
            await Task.Delay(500);

            // Enable the button after the animation
            Refresh_Button.IsEnabled = true;
        }

        /// <summary>
        /// Starts a single 360-degree rotation animation on the refresh icon
        /// </summary>
        private void RefreshButtonAnimation()
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
            Close();
        }

        /// <summary>
        /// Toggles sound on or off and updates the sound icon path.
        /// </summary>
        private void Sound_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsSoundEnabled)
            {
                PlaySound("sound-off.wav", 0.2);

                IsSoundEnabled = false;

                // Update the sound icon and visual state
                SoundPath.Data = Geometry.Parse("M3 11V13 M6 11V13 M9 11V13 M12 10V14 M15 11V13 M18 11V13 M21 11V13");
                Sound_Button.Opacity = 0.5;
            }
            else
            {
                IsSoundEnabled = true;

                PlaySound("sound-on.wav", 0.2);

                // Update the sound icon and visual state
                SoundPath.Data = Geometry.Parse("M3 11V13 M6 8V16 M9 10V14 M12 7V17 M15 4V20 M18 9V15 M21 11V13");
                Sound_Button.Opacity = 1.0;
            }

            // Persist the sound setting for future sessions
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
        /// Plays a sound from embedded resources with optional volume control. 
        /// Supports overlapping sounds without interrupting others.
        /// </summary>
        /// <param name="resourceName">The embedded sound file name (e.g., "dead.wav")</param>
        /// <param name="volume">Playback volume from 0.0 to 1.0</param>
        private void PlaySound(string resourceName, double volume)
        {
            if (!IsSoundEnabled) return;

            string resourcePath = $"Sus_Companion.assets.sounds.{resourceName}";
            using Stream? stream = GetType().Assembly.GetManifestResourceStream(resourcePath);

            if (stream == null)
            {
                _ = MessageBox.Show($"Resource {resourcePath} not found.");
                return;
            }

            // Load the embedded resource into memory
            MemoryStream memoryStream = new();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            // Save the memory content into a temporary WAV file
            string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
            File.WriteAllBytes(tempFile, memoryStream.ToArray());

            // Create a new MediaPlayer instance for concurrent playback
            MediaPlayer player = new();

            // Set the volume after the media is opened to ensure it takes effect
            player.MediaOpened += (s, e) =>
            {
                player.Volume = volume;
            };

            player.Open(new Uri(tempFile, UriKind.Absolute));
            player.Play();

            // Cleanup after playback ends
            player.MediaEnded += (s, e) =>
            {
                player.Close();
                try { File.Delete(tempFile); } catch { }
            };
        }

        /// <summary>
        ///  Resets the animation of all characters to their initial state.
        /// </summary>
        private void ResetCharactersAnimation()
        {
            IEnumerable<Image> images = MainGrid.Children
                .OfType<Border>()
                .Select(b => b.Child)
                .OfType<Image>();

            foreach (Image img in images)
            {
                if (img.Opacity < 1.0)
                {
                    // Set character to Alive state
                    img.Opacity = 1.0;

                    // Start Fade In animation (opacity from 0.15 to 1.0 in 0.5 seconds)
                    DoubleAnimation fadeIn = new()
                    {
                        From = 0.15,
                        To = 1.0,
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                        FillBehavior = FillBehavior.Stop
                    };

                    // Begin the animation
                    img.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                }
            }

            UpdateStats();
        }

        #endregion
    }
}

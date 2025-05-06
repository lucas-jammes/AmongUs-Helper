# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),  
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).  

---

## [1.8.0] 2025-05-06

### Added  

- Added new sound effects concerning:
  - Character selection.
  - Character default state.
  - GitHub/Support buttons.
  - Close button.
- Added a *Support the app* badge on GitHub to **encourage voluntary support**.  
- Included a subtle **PayPal support button** in the app (non-intrusive).  

### Changed 

- Updated UI to better **balance visibility of the bottom right buttons** without drawing too much attention.  
- Improved **tooltip text** for clarity and friendliness.  

---

## [1.7.0] - 2025-05-03

### Added

- **Rounded corners** on the application window for a more modern look.
- **Sound effect** when clicking the Refresh button.

### Changed

- Increased **Window Title** font size
- Increased **Stats Label** font size

---

## [1.6.0] – 2025-05-01

### Added

- Added **smooth fade-in animation** when reviving DEAD characters.

### Changed

- Increased Stats Label size from `28px` to `32px`.
- Set Label's background opacity to `75%`.

---

## [1.5.1] - 2025-05-01

### Fixed

- Hotfix: Fixed right-click **not restoring ALIVE state** from DEAD characters.

---

## [1.5.0] 2025-05-01

### Added

- Added **stats panel** to display counts of Alive, Safe, Sus, and Dead players.
- Added **kill sound effect** triggered when marking a character as Dead.
- Added **sound toggle button** to enable or disable sound playback.
- Added sound **toggle persistence** between sessions, restoring icon and button opacity on startup.
- Added refresh button with **360-degree rotation animation**.

### Changed

- Added **XML documentation summaries and param tags** for all methods in MainWindow.xaml.cs.
- Grouped methods using #region for better organization and readability.

---

## [1.4.0] - 2025-04-28  

### Added  

- Added **hover animation** (scale and shadow glow) on all crewmate images for better interactivity. 

### Changed  

- Refactored XAML for improved **readability and consistency** across image components.  
- Resized labels to **match images alignment**.

---

## [1.3.1] - 2025-04-28

### Added

- Replaced old PNG buttons with **scalable SVG assets** for higher visual quality.
- Added informations about **author**, **version** and **copyrights** to the application properties.

### Changed

- Build process updated to generate a **single standalone executable** for easier distribution.

### Removed

- Removed all unnecessary PNG files for a **cleaner assets folder**.

---

## [1.3.0] - 2025-04-28  

### Added  

- Added a clickable **GitHub button** in the bottom right corner to directly access the project repository.  

### Changed  

- Removed **borders around crewmates** (`BorderThickness` to `0`) for a cleaner visual design.  

---

## [1.2.0] - 2024-04-28  
  
### Added  

- **Renamed** the application ~~AmongUs Helper~~ → **Sus Companion**.    
- Added **saving** of the window position (`Top`, `Left`) when closing the app.   
- Added **automatic restoring** of the window position when reopening the app.   
  
### Fixed  

- Optimized most of the functions.  
- Improved `README.md` readability.  

---

## [1.1.0] - 2025-04-27
  
### Added  

- Added **right-click support** to mark characters as **DEAD** (opacity reduced, label updated).  

### Removed  

- Removed the possibility to mark characters as **DEAD** with left click → **use right click** instead.  

### Fixed  

- Fixed a bug where the **state tags** (`Tag` property) were **not correctly reset** after refreshing.  

# [1.0.0] - 2025-04-27

### Added

- Basic WPF UI with **18 crewmate images**.
- Ability to mark characters as `SAFE`, `SUS`, `DEAD` or `ALIVE` by left-clicking on them
- **Refresh button** to reset all character states
- **Close button** to exit the application
- **Window dragging** by clicking the top bar

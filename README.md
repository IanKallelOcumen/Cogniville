# 🧠 Cogniville

**Cogniville** is a polished, "juicy" educational RPG where players explore a vibrant world of intellect and defeat enemies by solving math challenges. The project features a robust UI system with high-quality animations, a 3D-style "Book" selection menu, and a turn-based battle system utilizing `TextMeshPro`.

## 🌟 Key Features

* **Math-Based Combat:** Turn-based RPG logic where answering questions correctly deals damage, and wrong answers hurt the player. Supports multiple difficulties (Easy/Medium/Hard/Boss) and operation types.
* **"Juicy" UI:** Extensive use of procedural animation. Buttons squish, backgrounds wiggle, titles breathe, and screens shake. All menu buttons share the same look, size, and hover/press feedback.
* **World Progression:** A saved-data system (`PlayerPrefs`) that locks/unlocks districts of Cogniville. Includes a 3D-style "Book" selection menu and a 2D "World Carousel."
* **Smooth Transitions:** Custom scene fading and camera panning logic using a custom easing library.
* **Leaderboard:** Global high scores; syncs with Firebase (optional) when enabled, or stays local.
* **Results screen:** After completing a level, players see stats (accuracy, time, etc.) before returning to the menu.
* **Roles & sessions:** **Principal** (bypass code) adds teachers (first name → code). **Teachers** log in with first name + code and run sessions, adding students by last name (each gets a code). **Students** join a session on the name screen with last name + code when a session is active.
* **Firebase backend (optional):** REST API (no SDK) for leaderboard and teacher sessions on the Spark (free) plan; Anonymous auth. See `Assets/Scripts/Backend/README_FIREBASE_SPARK.md`.

## 🛠 Project Requirements

1. **Unity Version:** 2021.3 or newer recommended.
2. **TextMeshPro:** Essential. The `QuizBattle.cs` and `WorldItem.cs` scripts rely on `TextMeshProUGUI`.
3. **DOTween (Optional):** Not required! This project uses a custom `Easing.cs` library for all animations, keeping it dependency-free.

## 📂 Scene Setup Guide

### 1. The Battle Scene (The Arena)

To create a fight level:

1. Create a Canvas with **TextMeshPro** elements.
2. Attach `QuizBattle.cs` to an empty GameObject.
3. **Inspector Setup:**
* **Enemies List:** Create "Profiles" for each enemy (Name, HP, Sprites, Difficulty).
* **UI References:** Drag in your Hearts (Images), Question Text (TMP), and Answer Buttons.
* **Sprites:** Assign Player and Enemy distinct sprites.


4. **Note:** The script handles Math generation automatically based on the chosen difficulty.

### 2. The World Select Scene (The Map)

To create the level selector:

1. Create a `Canvas`.
2. Create a parent GameObject for the slider (e.g., "WorldContainer").
3. Attach `WorldSliderController.cs` to the container.
4. Add child GameObjects with `WorldItem.cs` for each level/district.
* **Unique IDs:** Give each `WorldItem` a unique `World Save ID` (e.g., `District_Fire_Unlocked`) so the game remembers progress.



## 📜 Script Architecture

### ⚔️ Core Gameplay

* **`QuizBattle.cs`**: The heart of the combat. Generates math questions, manages HP state, handles turn animations (attacks/hurt), and determines Win/Loss.
* **`GameProgressManager.cs`**: A static helper class that handles saving and loading unlocked worlds using `PlayerPrefs`.
* **`WorldItem.cs`**: Represents a single level node. Handles locking logic (grayed out if locked), floating animations, and scene loading.
* **`WorldSliderController.cs`**: Manages the navigation between World Items, including "Snapping" to the selected world and animating the camera/container.

### 🎨 Visual Polish ("The Juice")

* **`Easing.cs`**: A static math library providing Cubic and Linear easing functions for smooth code-based animations.
* **`BackgroundWiggle.cs`**: Makes backgrounds drift or spin. Includes a `Kick()` function to make the background "jump" when a button is pressed.
* **`UIFloat.cs`**: Makes UI elements (like logos) bob up and down; hover scale (e.g. 1.1) for buttons. Can be "Bumped" for a reaction effect.
* **`ButtonFeedback.cs`**: Unified button press feedback (scale on press, optional background kick). Works with `UIFloat` for hover.
* **`ButtonPressAnimator.cs`**: Alternative tactile feedback by shrinking buttons slightly when clicked.
* **`SceneFader.cs`**: A singleton that creates a black overlay to fade scenes in and out smoothly.

### 🖥️ Menu & Navigation

* **`MainMenuController.cs`**: Manages all UI panels (Main, Play, Login, Panel Name, Panel Session, Panel Principal, Book Select, Leaderboard, About, Results). Includes "Focus" mode (zoom in on the selected book), sound toggle, and role-based flows (Principal / Teacher / Student).
* **`BookSelector.cs`**: Handles the logic for selecting "Chapters" or Modes presented as books. Includes unlock animations (shaking/popping).
* **`LeaderboardPanel.cs`**: Displays global high scores; loads from Firebase when enabled, otherwise local.
* **`ResultsPanel.cs`**: Shows post-level stats (accuracy, time, etc.) when returning from a battle.
* **`UIFadeIn.cs`**: Simple utility to make panels slide up and fade in when enabled.
* **`BackButtonHook.cs`**: Automatically binds the Android "Back" button (or Escape key) to the Menu's "Back" function.
* **Sound:** Sound toggle (music on/off) and `SoundToggleSkin.cs` for visual state.

### 👤 Login, roles & sessions

* **Principal access:** From the main menu, use **Login** → **Principal or Teacher?** → **Principal**. Enter the **bypass code** (no stored principal code) to open the principal panel. There you add teachers by **first name**; each gets a **random code** to log in.
* **Teacher login:** **Login** → **Teacher** → enter **first name** and **code** (from the principal). Successful login opens the **Session** panel. Teacher can add **students** by **last name**; each student gets a **code** to join the session.
* **Student join:** On **Panel Name** (Play flow), when a session is active and the teacher has added students, the student enters **last name** and **code** to join; otherwise a single name field is used. All input fields use the same size and autocapitalization (Name).
* **`GameDataManager.cs`**: Holds principal bypass validation, teacher list (first name + code), session state, and session students (last name + code). **`PrincipalPanel.cs`** and **`SessionPanel.cs`** drive the principal and session UI.

### 🎨 UI consistency & tools

* **Global button styling:** All buttons get the same size (260×56), sprite, hover (UIFloat 1.1), and press feedback (ButtonFeedback scale 1) at runtime. Principal panel and login panels are normalized to match.
* **Large text:** Inputs and button labels use font size 32 / 30; placeholders use normal style (no bold/italic). Panel Login error text is created at runtime if missing so validation messages show.
* **Panel Login:** "Principal or Teacher?" is shown first (no teacher form flash); background stays visible when switching to login. LoginButton is resolved under Panel Login first to avoid wrong binding with duplicate names.
* **Editor tools (Tools → Cogniville):** **Setup Student Join UI** (Panel Name + Session), **Check for conflicting names in hierarchy** (duplicate names that can break Find() / assigned graphics), **Fix Missing Script References**, **Import Firebase SDK packages**, **Apply standard UI (Play look)**, and font/button scale helpers.

### 🔧 Backend & Firebase

* **Firebase (optional):** REST API only (no SDK required). **`FirebaseBackend.cs`** uses Anonymous auth and Firestore for **leaderboard** and **teacher sessions**. **`FirebaseConfig`** asset (Api Key, Project Id) in `Resources`; untick **Use Firebase** for local-only. Spark (free) plan; see `Assets/Scripts/Backend/README_FIREBASE_SPARK.md` for setup and rules.
* **`GameDataManager.cs`**: Persists player name, teachers, session state, results; calls `FirebaseBackend` when config is enabled.

### ⚙️ Utilities

* **`UIFirstAid.cs`**: A failsafe script. Drop this in any scene to automatically ensure an `EventSystem` is present so buttons always click.

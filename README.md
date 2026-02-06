# 🧠 Cogniville

**Cogniville** is a polished, "juicy" educational RPG where players explore a vibrant world of intellect and defeat enemies by solving math challenges. The project features a robust UI system with high-quality animations, a 3D-style "Book" selection menu, and a turn-based battle system utilizing `TextMeshPro`.

## 🌟 Key Features

* **Math-Based Combat:** Turn-based RPG logic where answering questions correctly deals damage, and wrong answers hurt the player. Supports multiple difficulties (Easy/Medium/Hard/Boss) and operation types (, , ).
* **"Juicy" UI:** Extensive use of procedural animation. Buttons squish, backgrounds wiggle, titles breathe, and screens shake.
* **World Progression:** A saved-data system (`PlayerPrefs`) that locks/unlocks districts of Cogniville. Includes a 3D-style "Book" selection menu and a 2D "World Carousel."
* **Smooth Transitions:** Custom scene fading and camera panning logic using a custom easing library.

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
* **`UIFloat.cs`**: Makes UI elements (like logos) bob up and down. Can be "Bumped" for a reaction effect.
* **`ButtonPressAnimator.cs`**: Adds tactile feedback by shrinking buttons slightly when clicked.
* **`SceneFader.cs`**: A singleton that creates a black overlay to fade scenes in and out smoothly.

### 🖥️ Menu & Navigation

* **`MainMenuController.cs`**: Manages the primary UI panels (Main, About, Settings). Includes logic to "Focus" (zoom in) on specific interactive elements like Books.
* **`BookSelector.cs`**: Handles the logic for selecting "Chapters" or Modes presented as books. Includes unlock animations (shaking/popping).
* **`UIFadeIn.cs`**: Simple utility to make panels slide up and fade in when enabled.
* **`BackButtonHook.cs`**: Automatically binds the Android "Back" button (or Escape key) to the Menu's "Back" function.

### ⚙️ Utilities

* **`UIFirstAid.cs`**: A failsafe script. Drop this in any scene to automatically ensure an `EventSystem` is present so buttons always click.

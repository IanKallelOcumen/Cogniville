# Simple Screen Setup Guide

## What You Need To Do

You need to create 5 new screens. Each screen is a folder with buttons and text boxes inside it.

### STEP 1: Create ScreenManager

1. Right-click on `System` folder in Hierarchy
2. Click `Create Empty`
3. Name it: `ScreenManager`
4. Click the `+` button on the right of ScreenManager
5. Search for `ScreenManager` script and add it

### STEP 2: Create GameDataManager

1. Right-click on `System` folder
2. Click `Create Empty`
3. Name it: `GameDataManager`
4. Click the `+` button
5. Search for `GameDataManager` script and add it

### STEP 3: Create LoginScreen (Screen 1)

1. Right-click on `Canvas`
2. Click `Create Empty`
3. Name it: `LoginScreen`
4. Click the `+` button
5. Add script: `LoginScreen`
6. Click the `+` button again
7. Add component: `CanvasGroup` (search for it)

Now add children to LoginScreen:
- Right-click `LoginScreen` → `UI` → `Image` (this is the purple background)
- Right-click `LoginScreen` → `UI` → `Input Field - TextMesh Pro` (this is email box)
- Right-click `LoginScreen` → `UI` → `Input Field - TextMesh Pro` (this is password box)
- Right-click `LoginScreen` → `UI` → `Button - TextMesh Pro` (this is Login button)
- Right-click `LoginScreen` → `UI` → `Button - TextMesh Pro` (this is Signup button)
- Right-click `LoginScreen` → `UI` → `Button - TextMesh Pro` (this is Guest button)

Name them:
- Image → name it `BG`
- First input field → `EmailInput`
- Second input field → `PasswordInput`
- First button → `LoginButton`
- Second button → `SignupButton`
- Third button → `PlayAsGuestButton`

### STEP 4: Create EnterNameScreen (Screen 2)

1. Right-click on `Canvas`
2. Click `Create Empty`
3. Name it: `EnterNameScreen`
4. Add script: `EnterNameScreen`
5. Add component: `CanvasGroup`

Add children:
- Right-click → `UI` → `Image` (background) → name it `BG`
- Right-click → `UI` → `Text - TextMesh Pro` (text) → name it `PromptText`
- Right-click → `UI` → `Input Field - TextMesh Pro` → name it `NameInput`
- Right-click → `UI` → `Button - TextMesh Pro` → name it `EnterButton`
- Right-click → `UI` → `Button - TextMesh Pro` → name it `BackButton`

### STEP 5: Create CurrentSessionScreen (Screen 3)

1. Right-click on `Canvas`
2. Click `Create Empty`
3. Name it: `CurrentSessionScreen`
4. Add script: `CurrentSessionScreen`
5. Add component: `CanvasGroup`

Add children:
- Right-click → `UI` → `Image` → name it `BG`
- Right-click → `UI` → `Text - TextMesh Pro` → name it `SessionTitle`
- Right-click → `UI` → `Image` → name it `SessionIcon`
- Right-click → `UI` → `Text - TextMesh Pro` → name it `TeacherName`
- Right-click → `UI` → `Image` → name it `QuizCard1`
- Right-click → `UI` → `Image` → name it `QuizCard2`
- Right-click → `UI` → `Button - TextMesh Pro` → name it `EndSessionButton`
- Right-click → `UI` → `Button - TextMesh Pro` → name it `BackButton`

### STEP 6: Create ResultsScreen (Screen 4)

1. Right-click on `Canvas`
2. Click `Create Empty`
3. Name it: `ResultsScreen`
4. Add script: `ResultsScreen`
5. Add component: `CanvasGroup`

Add children:
- Right-click → `UI` → `Image` → name it `BG`
- Right-click → `UI` → `Text - TextMesh Pro` → name it `ScoreText`
- Right-click → `UI` → `Text - TextMesh Pro` → name it `AccuracyText`
- Right-click → `UI` → `Text - TextMesh Pro` → name it `TimeText`
- Right-click → `UI` → `Text - TextMesh Pro` → name it `CorrectAnswersText`
- Right-click → `UI` → `Image` → name it `GraphImage`
- Right-click → `UI` → `Raw Image` → name it `ChartRawImage`
- Right-click → `UI` → `Button - TextMesh Pro` → name it `ContinueButton`

### STEP 7: Create LeaderboardScreen (Screen 5)

1. Right-click on `Canvas`
2. Click `Create Empty`
3. Name it: `LeaderboardScreen`
4. Add script: `LeaderboardScreen`
5. Add component: `CanvasGroup`

Add children:
- Right-click → `UI` → `Image` → name it `BG`
- Right-click → `UI` → `Scroll View`
  - This creates: ScrollView > Viewport > Content
- Right-click → `UI` → `Button - TextMesh Pro` → name it `RefreshButton`
- Right-click → `UI` → `Button - TextMesh Pro` → name it `BackButton`

---

## STEP 8: Connect Everything (The Important Part!)

1. Click on `ScreenManager` in Hierarchy
2. Look at the right side (Inspector)
3. You will see empty boxes for:
   - Login Screen
   - Enter Name Screen
   - Current Session Screen
   - Results Screen
   - Leaderboard Screen

4. For each box, drag the matching screen from the Hierarchy:
   - Drag `LoginScreen` panel into "Login Screen" box
   - Drag `EnterNameScreen` panel into "Enter Name Screen" box
   - Drag `CurrentSessionScreen` panel into "Current Session Screen" box
   - Drag `ResultsScreen` panel into "Results Screen" box
   - Drag `LeaderboardScreen` panel into "Leaderboard Screen" box

---

## That's It!

When you're done, your Canvas should look like:
```
Canvas
├── BG (your game background)
├── PanelMain (your current main menu - leave it)
├── PanelAbout (leave it)
├── PanelBooks (leave it)
├── PanelLeaderboard (leave it)
├── LoginScreen (NEW)
├── EnterNameScreen (NEW)
├── CurrentSessionScreen (NEW)
├── ResultsScreen (NEW)
└── LeaderboardScreen (NEW)
```

And your System folder should have:
```
System
├── ScreenManager (NEW)
├── GameDataManager (NEW)
├── EventSystem (leave it)
```

That's all! You're done setting up!

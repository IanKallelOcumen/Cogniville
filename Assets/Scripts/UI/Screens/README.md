# UI Screens Setup Guide

## Overview
These screens replace your React UIs with optimized Unity Canvas implementations. They use the existing animation system and follow your optimized code patterns.

## Screens Included

### 1. **LoginScreen** 
- Email and Password input fields
- Sign up / Play as Guest buttons
- Purple background with cyan accents
- Validates input before allowing login

### 2. **EnterNameScreen**
- Simple name input
- Enter button (validates non-empty name)
- Back button to return to login
- Stores player name to PlayerPrefs

### 3. **CurrentSessionScreen**
- Shows active session/teacher info
- Session cards display
- End Session button
- Displays teacher name and session status

### 4. **ResultsScreen**
- Shows game statistics:
  - Score
  - Accuracy %
  - Time Taken
  - Correct Answers / Total
- Graph visualization placeholder
- Navigation buttons (Next, Retry, Home)

### 5. **LeaderboardScreen**
- Scrollable ranked player list
- Displays Name, Rank, Score
- Sample data included
- Home and Back buttons

## Setup Instructions

### Step 1: Create Canvas Structure
```
Canvas (Main)
├── LoginScreen (Panel)
├── EnterNameScreen (Panel)
├── CurrentSessionScreen (Panel)
├── ResultsScreen (Panel)
└── LeaderboardScreen (Panel)
```

### Step 2: Assign to ScreenManager
1. Create empty GameObject: `ScreenManager`
2. Add `ScreenManager.cs` component
3. Assign each screen to the corresponding public field

### Step 3: Connect ButtonFeedback
Add `ButtonFeedback` component to all buttons for unified feedback:
- Email/Password inputs
- Login button
- Signup button
- All navigation buttons

### Step 4: Design Each Screen

#### LoginScreen
- Background: Purple (#8B00FF)
- InputFields: Cyan border with white text
- Buttons: Cyan with white text
- Add TextMeshPro components for title

#### EnterNameScreen
- Background: Purple
- InputField: Large centered white field
- Button: Cyan "ENTER" button
- Add back button (small, top-left)

#### CurrentSessionScreen
- Background: Purple
- Session Card: Orange/Coral color
- Teacher name display
- "END" button in bright color
- Show 2 quiz placeholder cards

#### ResultsScreen
- Background: Dark gray
- Stats displayed in readable format
- Graph area for chart visualization
- 3 navigation buttons: Next, Retry, Home

#### LeaderboardScreen
- Background: Purple/Gray
- ScrollRect with leaderboard entries
- Entry template prefab with Name, Rank, Score
- Book-themed visual (optional)

### Step 5: Configure InputFields (LoginScreen)
```csharp
EmailInput:
  - Content Type: Email
  - Line Type: Single Line
  
PasswordInput:
  - Content Type: Password
  - Line Type: Single Line
```

## Usage Examples

### Show a Screen
```csharp
ScreenManager.Instance.ShowScreen("Login");
ScreenManager.Instance.ShowScreen("EnterName");
ScreenManager.Instance.ShowScreen("Results");
```

### Get Current Screen
```csharp
UIScreen current = ScreenManager.Instance.GetCurrentScreen();
```

### Set Results Before Showing
```csharp
ResultsScreen results = ScreenManager.Instance.GetScreen("Results") as ResultsScreen;
results.SetResults(score: 850, correctAnswers: 17, totalQuestions: 20, timeTaken: 45.5f);
ScreenManager.Instance.ShowScreen("Results");
```

### Set Teacher Info
```csharp
CurrentSessionScreen session = ScreenManager.Instance.GetScreen("CurrentSession") as CurrentSessionScreen;
session.SetTeacher("Mrs. Reyes");
```

## Integration with QuizBattle

After a quiz completes:
```csharp
// In QuizBattle.cs CheckGameState()
void CheckGameState()
{
    if (playerHP <= 0) 
    {
        // Game over - show results
        ResultsScreen results = ScreenManager.Instance.GetScreen("Results") as ResultsScreen;
        results.SetResults(
            score: currentScore,
            correctAnswers: correctCount,
            totalQuestions: totalQuestions,
            timeTaken: Time.time - sessionStart
        );
        ScreenManager.Instance.ShowScreen("Results");
    }
    else if (currentEnemyHP <= 0)
    {
        // Next enemy or victory
        if (currentEnemyIndex < enemies.Count - 1)
            LoadEnemy(currentEnemyIndex + 1);
        else
        {
            // Victory - show results
            ScreenManager.Instance.ShowScreen("Results");
        }
    }
}
```

## Styling Tips

### Color Scheme
- **Primary Purple**: #8B00FF
- **Accent Cyan**: #00FFFF
- **Highlight Orange**: #FF6B00
- **Text White**: #FFFFFF

### Font Recommendations
- **Headers**: Bold, 40-48pt
- **Labels**: Regular, 28-32pt
- **Input Fields**: Regular, 24-28pt
- **Small Text**: Regular, 18-20pt

### Button Style
- All buttons use `ButtonFeedback` component
- Enable `enableSquish = true` for press feedback
- Use `bumpTarget` to bump title/header on press
- Use `bg` for background kick effect

## Troubleshooting

**"Screen not found" error**
- Make sure screen is assigned in ScreenManager inspector
- Check spelling of screen name matches registration

**Buttons not responding**
- Ensure EventSystem exists in scene
- Check `ButtonFeedback` is added to button
- Verify UIFirstAid script is handling setup

**Input fields not working**
- Make sure `TMP_InputField` (not regular InputField)
- Check canvas has `GraphicRaycaster`
- Verify interactable is true

## Next Steps
1. Build out Canvas UI with all prefabs
2. Test transitions between screens
3. Connect to QuizBattle for score passing
4. Add teacher/world selection
5. Implement database for leaderboards

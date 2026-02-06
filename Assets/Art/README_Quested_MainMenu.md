# Quested — Main Menu (Mobile Landscape) Setup Guide

This README walks you **from zero to working** for a landscape, mobile‑friendly main menu in Unity, using:
- A background image (dungeon)
- A **title logo** that gently **floats**
- Menu buttons (**Play**, **About**, **Leaderboard**, **Exit**) with **Sprite Swap** states
- A **Sound** toggle that **persists**
- **About** and **Leaderboard** sub‑panels with Back
- Android back‑button support and iOS‑friendly Exit behavior
- Safe Area handling for notches/cutouts

> Scripts included (zipped): `SafeArea.cs`, `TitleFloat.cs`, `ButtonBumpTitle.cs`, `MainMenuController.cs`

Downloads:
- Scripts: **Quested_MainMenu_Scripts.zip** — [Download](sandbox:/mnt/data/Quested_MainMenu_Scripts.zip)
- Backgrounds: **bg_dungeon_1920x1080.jpg** — [Download](sandbox:/mnt/data/bg_dungeon_1920x1080.jpg), **bg_dungeon_1280x720.jpg** — [Download](sandbox:/mnt/data/bg_dungeon_1280x720.jpg)

---

## 0) Requirements

- **Unity** 2021 LTS or newer (2D or 2D URP template)
- **TextMeshPro** (TMP) Essentials imported
- Target: **Mobile (Android/iOS)** — **Landscape** orientation

---

## 1) Create the Project

1. Unity Hub → **New** → Template: **2D** (URP or Core 2D) → Name: `Quested`.
2. Open the project.
3. `Window → TextMeshPro → Import TMP Essential Resources`.
4. `Edit → Project Settings → Player`:
   - **Active Input Handling**: **Both** (recommended) or Input System.
   - **Default Orientation**: **Landscape Left** and **Landscape Right** enabled.

---

## 2) Import Assets & Scripts

1. Download the files above.
2. In Unity, create folders:
   ```
   Assets/
     Art/
       Backgrounds/
       UI/
     Prefabs/
       UI/
     Scenes/
     Scripts/
       UI/
   ```
3. Drag the **background JPG**(s) into `Assets/Art/Backgrounds/`.
4. Unzip **Quested_MainMenu_Scripts.zip** and drag all `.cs` into `Assets/Scripts/UI/`.

---

## 3) Create Scenes

1. `File → New Scene` (2D) → save as `Assets/Scenes/MainMenu.unity`.
2. Create another scene `Assets/Scenes/Game.unity` (empty placeholder for now).
3. `File → Build Settings`:
   - Click **Add Open Scenes** (MainMenu) → ensure it’s **index 0**.
   - Open `Game.unity`, **Add Open Scenes** → **index 1**.
   - **Platform**: switch to **Android** or **iOS** when ready to build.

---

## 4) Canvas & Scaling (MainMenu)

1. In **MainMenu** scene, **Create → UI → Canvas**. Unity adds **EventSystem** (keep it).
2. Select **Canvas**:
   - **Canvas Scaler**: *Scale With Screen Size*
   - **Reference Resolution**: **1920 × 1080**
   - **Match**: **0.5**

> This keeps UI the same physical size across resolutions (ideal for mobile).

---

## 5) Background Setup

1. Under **Canvas**, add **UI → Image** → rename to **`BG`**.
2. In **Inspector**:
   - **Source Image** = `bg_dungeon_1920x1080` (or 1280×720).
   - Anchors: **Stretch** full (Alt+Shift while clicking the stretch preset).
   - Add **Aspect Ratio Fitter** → **Aspect Mode = Envelope Parent** → **Aspect Ratio = 16:9**.

This guarantees full‑bleed background on any landscape screen (no black bars).

---

## 6) Panels & Layout

Under **Canvas**, create this structure exactly:

```
Canvas
├─ BG (Image)
├─ PanelMain (active)
│  ├─ TitleLogo (UI → Image)        ← your game title image
│  ├─ SoundRow (Empty + Horizontal Layout Group)
│  │   ├─ SoundLabel (TMP)          ← text “Sound”
│  │   └─ SoundToggle (UI → Toggle)
│  ├─ PlayButton (UI → Button + TMP label)
│  ├─ AboutButton (UI → Button + TMP label)
│  ├─ LeaderboardButton (UI → Button + TMP label)
│  └─ ExitButton (UI → Button + TMP label)
├─ PanelAbout (inactive)
│  ├─ AboutBodyText (TMP)
│  └─ AboutBackButton (Button)
└─ PanelLeaderboard (inactive)
   ├─ Scroll View
   │   └─ Viewport
   │       └─ Content               ← rows are spawned here
   └─ LeaderboardBackButton (Button)
```

**Tips**
- Buttons ≥ **64 px** tall at 1920×1080 reference, spacing **20–32 px**.
- Add **Vertical Layout Group** on `PanelMain` for neat stacking (optional).
- Anchor **TitleLogo** at **Top‑Center**, **Preserve Aspect** checked.

---

## 7) Safe Area (Notches & Cutouts)

1. Add **`SafeArea`** (script) to `PanelMain`, `PanelAbout`, `PanelLeaderboard` (or a common parent RectTransform that wraps them).
2. The script automatically constrains anchors inside `Screen.safeArea` on devices with notches/rounded corners.

---

## 8) Title Float (Floating Logo)

1. Select `TitleLogo` → **Add Component → TitleFloat**.
2. Tweak in Inspector:
   - **AmplitudeY**: 8–14 px (gentle bob)
   - **SpeedY**: 0.5–0.7
   - **RotAmplitude**: 1–2°
   - **ScaleAmplitude**: 0.008–0.012
3. When any menu button is pressed, we’ll make it “hop” (next step).

---

## 9) Button → Title “Bump” Feedback

1. On **each** button (`PlayButton`, `AboutButton`, `LeaderboardButton`, `ExitButton`, `AboutBackButton`, `LeaderboardBackButton`):
   - **Add Component → ButtonBumpTitle** (it auto‑finds `TitleFloat` in the scene).
2. That’s it — tapping any menu button gives the logo a quick, buoyant hop.

*Optional:* Adjust bump in `TitleFloat` (Bump section) — `bumpHeight`, `bumpOutTime`, `bumpBackTime`, `bumpRotate`.

---

## 10) Button Visual States (Sprite Swap)

If you have a button spritesheet with **normal / hover / pressed** frames:

1. Import the PNG to `Assets/Art/UI/Buttons/`.
2. Select it → **Sprite Mode = Multiple** → **Sprite Editor → Slice** (grid or auto). Name slices:
   - `btn_normal`, `btn_hover`, `btn_pressed`, (optional `btn_disabled`).
3. For each Button:
   - Select the **Image** on the same GameObject → **Source Image = btn_normal**.
   - **Image Type**: **Sliced** (if you set borders) or **Simple**.
   - On the **Button** component:
     - **Transition = Sprite Swap**
     - **Highlighted = btn_hover**
     - **Pressed = btn_pressed**
     - **Selected = btn_hover**
     - **Disabled = (optional)**
     - **Color Multiplier = 1** (don’t tint your art)
     - **Fade Duration ≈ 0.1**
> Note: Mobile doesn’t have hover; you’ll mainly see the **pressed** state.

---

## 11) Leaderboard Row Prefab

1. **Hierarchy → Create Empty** → name **`RowPrefab`**.
2. Add **Horizontal Layout Group** (Spacing ~20, Child Force Expand Width/Height ✔).
3. Add three children **TextMeshPro – Text**:
   - Name them **`RankText`**, **`NameText`**, **`ScoreText`**.
4. Drag `RowPrefab` into `Assets/Prefabs/UI/` to create the prefab; delete it from the scene.
5. The sample menu script spawns rows under `PanelLeaderboard/Scroll View/Viewport/Content`.

---

## 12) Main Menu Controller — Wiring

1. **Create Empty** in Hierarchy → name **`System`**.
2. Add **`MainMenuController`** component.
3. Drag references in the Inspector:
   - **Panels**: `PanelMain`, `PanelAbout`, `PanelLeaderboard`
   - **UI**: `SoundToggle`, `ExitButton`
   - **Leaderboard**: `Content` (inside Scroll View) and `RowPrefab` (prefab you made)
   - **Scenes**: `gameSceneName = "Game"` (exact scene name)
   - **Audio (optional)**: `MasterMixer` (see below) and a `MusicSource` (AudioSource with looping music)
4. Hook up **Button OnClick** to `System (MainMenuController)`:
   - `PlayButton` → `OnPlay`
   - `AboutButton` → `OnAbout`
   - `LeaderboardButton` → `OnLeaderboard`
   - `ExitButton` → `OnExit`
   - `AboutBackButton` → `OnBack`
   - `LeaderboardBackButton` → `OnBack`

**Behavior notes**
- Sound toggle is persisted via `PlayerPrefs` (`Quested_Sound` key).
- iOS: `ExitButton` is hidden automatically.
- Android: **Back** key navigates Back/Exit from the menu.

---

## 13) Optional: Audio Mixer (Cleaner Mute)

1. **Create → Audio Mixer**, name it `MasterMixer`.
2. Select the **Master** group → right‑click **Volume** slider → **Expose ‘Volume (of Master)’**.
3. Rename the exposed parameter to **`MasterVolume`** (exact).
4. On your **MusicSource** (AudioSource), set **Output** to the Master group.
5. Drag `MasterMixer` into `MainMenuController.masterMixer`.

The Sound toggle now sets `MasterVolume` to **0 dB** (on) / **−80 dB** (mute).

---

## 14) Mobile Build Settings

**Android**
- `File → Build Settings → Android → Switch Platform`
- **Player Settings**:
  - **Scripting Backend**: IL2CPP
  - **Target Architectures**: ARM64 (and ARMv7 if you need legacy)
  - **Minimum API Level**: 23+
  - **Orientation**: Landscape Left/Right
- Scenes In Build: `MainMenu` (0), `Game` (1)

**iOS**
- Switch Platform → iOS
- **Player Settings**:
  - **Scripting Backend**: IL2CPP
  - **Target**: ARM64
  - **Orientation**: Landscape Left/Right
- Build → open in Xcode → set team/provisioning → Run

---

## 15) Recommended Folder Structure

```
Assets/
  Art/
    Backgrounds/
    UI/
      Buttons/
  Prefabs/
    UI/
  Scenes/
    MainMenu.unity
    Game.unity
  Scripts/
    UI/
      SafeArea.cs
      TitleFloat.cs
      ButtonBumpTitle.cs
      MainMenuController.cs
```

---

## 16) Test Checklist

- [ ] **BG fills** the screen without black bars (Aspect Ratio Fitter = Envelope Parent, 16:9).
- [ ] **TitleLogo floats** smoothly (TitleFloat on it).
- [ ] **Buttons bump** the title on press (ButtonBumpTitle on each button).
- [ ] **Sound** toggle mutes/unmutes and **remembers** state.
- [ ] **About** and **Leaderboard** panels open/close with **Back**.
- [ ] **Leaderboard** shows sample rows.
- [ ] **Play** loads the Game scene.
- [ ] **Exit**:
  - Editor: stops Play Mode
  - Android/PC: quits app
  - iOS: hidden
- [ ] **Android Back** button goes Back/Exit.

---

## 17) Troubleshooting

- **No text / TMP errors** → `Window → TextMeshPro → Import TMP Essential Resources`.
- **Buttons don’t change sprites** → Set **Button → Transition = Sprite Swap**, **Color Multiplier = 1**, assign sprites to Highlighted/Pressed.
- **Hover never shows on phone** → Normal; touch devices don’t have hover.
- **Buttons don’t receive input** → Ensure there’s **EventSystem** in scene; if using Input System, it should be **Input System UI Input Module**.
- **Play button does nothing** → Make sure your **Game** scene is **added** to Build Settings and the **name matches** `gameSceneName`.
- **Title doesn’t float** → Ensure `TitleFloat` is on the **TitleLogo** (not on the Canvas), and that its anchored position isn’t constrained by a layout you don’t want.
- **Images look tinted** → Button **Color Multiplier** must be `1`; remove color tints on Image components.
- **SafeArea not working** → Ensure `SafeArea` is on a **RectTransform** that directly wraps your panel contents.

---

## 18) Next Steps (Optional Polish)

- Add subtle **button press nudge** (Y‑offset) via animation or script.
- Add **SFX** for hover/press routed to the AudioMixer.
- Skin fonts/colors to your “Quested” style.
- Replace sample leaderboard with a **real backend** call.

---

## Credits

- “Dungeon” background provided as processed 16:9 JPGs (1920×1080, 1280×720).
- Scripts authored for the Quested menu scaffold.

Happy building! 🎮

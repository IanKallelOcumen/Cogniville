# Firebase Backend (Spark Plan — Free)

Cogniville uses **Firebase REST API** (no SDK) so it works on the **Spark (free)** plan. Leaderboard and teacher sessions sync to **Cloud Firestore**; auth uses **Anonymous** sign-in to get a token.

---

## Option A: REST-only (current, no SDK)

Sections 1–6 below describe the **REST-only** setup: no Unity SDK, no `google-services.json`. You configure **Api Key** and **Project Id** in the **FirebaseConfig** asset. This keeps the project on Spark with minimal dependencies.

## Option B: Official Firebase Unity SDK (google-services.json + SDK)

If you prefer the official Firebase Unity SDK (e.g. for Analytics or other SDK features):

1. **Add your Firebase config file**
   - In the Unity **Project** window, move your downloaded **google-services.json** (e.g. from `Downloads`) into the **Assets** folder. You can place it anywhere inside Assets (e.g. `Assets/` or `Assets/Firebase/`).
   - **Note:** Do not commit `google-services.json` if it contains secrets; add it to `.gitignore` or use a placeholder in the repo.

2. **Import the Firebase Unity SDK**
   - Unzip the Firebase Unity SDK (e.g. `firebase_unity_sdk_13.8.0.zip`) somewhere on your machine.
   - In Unity: **Assets > Import Package > Custom Package**.
   - From the unzipped SDK folder, select **FirebaseAnalytics.unitypackage** (and any other Firebase packages you need, e.g. Auth, Firestore).
   - In the **Import Unity Package** window, click **Import**.

3. **Using both:** The existing **FirebaseBackend** (REST) and **FirebaseConfig** asset still work for leaderboard/sessions. The SDK can be used in parallel for Analytics or other products; the REST setup does not require the SDK.

---

## 1. Create a Firebase project (Spark)

1. Go to [Firebase Console](https://console.firebase.google.com).
2. **Add project** (or use an existing one). No Blaze upgrade needed.
3. In **Build > Authentication**:
   - Click **Get started**
   - Open **Sign-in method**
   - Enable **Anonymous**
   - Save
4. In **Build > Firestore Database**:
   - Click **Create database**
   - Start in **production mode** (we use Auth token; rules below will allow reads/writes for authenticated users)
   - Pick a region and enable
5. In **Firestore > Rules**, paste the rules from **`Assets/Scripts/Backend/firestore.rules`** (or the block below). They allow only authenticated users, **leaderboard: read + create only** (no update/delete), **sessions: read/write** with field validation:

```text
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /leaderboard/{entry} {
      allow read: if request.auth != null;
      allow create: if request.auth != null
        && request.resource.data.keys().hasAll(['playerName', 'score', 'teacherName', 'timestamp'])
        && request.resource.data.playerName is string
        && request.resource.data.playerName.size() <= 200
        && request.resource.data.score is int
        && request.resource.data.score >= 0
        && request.resource.data.score <= 999999
        && request.resource.data.teacherName is string
        && request.resource.data.teacherName.size() <= 200
        && request.resource.data.timestamp is timestamp;
      allow update, delete: if false;
    }
    match /sessions/{session} {
      allow read: if request.auth != null;
      allow create, update: if request.auth != null
        && request.resource.data.keys().hasAll(['teacherName', 'isActive', 'updatedAt'])
        && request.resource.data.teacherName is string
        && request.resource.data.teacherName.size() <= 200
        && request.resource.data.isActive is bool
        && request.resource.data.updatedAt is timestamp;
      allow delete: if request.auth != null;
    }
  }
}
```

6. In **Project settings** (gear) > **General**:
   - Under **Your apps**, add a **Web** app if you don’t have one (nickname e.g. "Cogniville").
   - Copy the **Web API Key** and **Project ID**.

---

## 2. Unity setup

### 2.1 Firebase config asset (already in project)

A default **FirebaseConfig** asset is at `Assets/Resources/FirebaseConfig.asset`. It loads automatically at runtime.

1. In Unity menu: **Cogniville > Firebase > Select Firebase Config** to select it (or find `Assets/Resources/FirebaseConfig` in the Project window).
2. In the Inspector set:
   - **Api Key**: Web API Key from Firebase Console (Project settings > General > Your apps > Web API Key).
   - **Project Id**: Project ID from Firebase.
   - **Use Firebase**: tick to enable cloud (untick = local-only mode).
3. **Cogniville > Firebase > Open Firebase Console** opens the Firebase website.

### 2.2 FirebaseBackend in the scene

**FirebaseBackend** is added automatically to the same GameObject as **GameDataManager** if it isn't already in the scene. You can also add it yourself (e.g. **Add Component > Firebase Backend**). If your config is not in **Resources**, assign the **Firebase Config** asset to the **Config** field. If it's in `Resources/FirebaseConfig`, the script will load it when **Config** is empty.

1. Open **MainMenu** (or the scene that has **GameDataManager**).
2. Find or create an object that persists (e.g. same GameObject as **GameDataManager**, or an empty “Backend”).
3. **Add Component > Firebase Backend** (script `FirebaseBackend`).
4. If your config is not in **Resources**, assign the **Firebase Config** asset to the **Config** field.  
   If it’s in `Resources/FirebaseConfig`, the script will load it automatically when **Config** is empty.

### 2.3 GameDataManager

**GameDataManager** should be in the same (or a persistent) scene. It already uses **FirebaseBackend** when available:

- **Leaderboard**: new scores are sent to Firestore; opening the leaderboard panel refreshes from Firestore.
- **Sessions**: starting/ending a teacher session updates Firestore.

No extra wiring is required once **FirebaseBackend** is in the scene and config is set.

---

## 3. What runs on Firebase (Spark)

| Feature            | Firestore collection | Usage                    |
|--------------------|----------------------|--------------------------|
| Global leaderboard | `leaderboard`        | Add on score; query top N|
| Teacher sessions  | `sessions`           | One doc per teacher; `isActive` |

- **Auth**: Anonymous (no email). One anonymous user per device; token used for Firestore.
- **Spark limits**: 50K reads / 20K writes per day; 1 GiB storage. Enough for a small/medium class.

---

## 3.1 Supported scale (small testing site)

The app and Spark plan are set up for a **small testing site** with:

| Role    | Typical scale | Notes |
|---------|----------------|--------|
| Teachers | 10+ | Principal adds by first name; each gets a code. Stored in PlayerPrefs; session state in Firestore. |
| Students | 50+ | Students join sessions with last name + code (no per-student Firebase Auth). Leaderboard and session writes stay well under Spark limits. |
| Leaderboard | Top 50 | Leaderboard fetches top 50 entries from Firestore. |

No configuration changes needed for 50+ students and 10+ teachers. If you grow beyond that, monitor Firestore usage in the Firebase Console; Spark quotas are 50K reads / 20K writes per day.

---

## 4. Disable Firebase

- Untick **Use Firebase** on the config, or clear **Api Key** / **Project Id**.
- The game falls back to local-only (PlayerPrefs + in-memory leaderboard).

---

## 5. Security and reliability

- **Firestore rules:** Leaderboard allows only **create** and **read** (no update/delete). Sessions allow read/write with validated field types and sizes (strings ≤ 200 chars, score 0–999999). Copy the full rules from `Assets/Scripts/Backend/firestore.rules` into Firebase Console.
- **Input validation:** `FirebaseBackend` truncates player/teacher names to 200 characters and clamps score to 0–999999 before sending. JSON escaping covers quotes, newlines, tabs, and control characters.
- **Auth:** Anonymous auth is retried once on failure. All Firestore requests use a 15s timeout and send the Bearer token.
- **API key:** The Web API key is used in the client (Unity). In Firebase Console > Project Settings > API key, you can **restrict** the key (e.g. by HTTP referrer for web, or by app bundle ID when building) to reduce abuse. For a small testing site this is optional but recommended for production.

---

## 6. Troubleshooting

- **“Auth failed”**: In Firebase Console, ensure **Anonymous** sign-in is enabled under Authentication.
- **403 / Permission denied**: Check Firestore rules and that the client uses the Auth token (Bearer).
- **Leaderboard empty**: First open the leaderboard after at least one game has been played and Firebase is configured; data is in Firestore and loads when the panel opens.

---

## 7. Prompt for Gemini (Firebase help)

Copy and paste the block below when asking Gemini for Firebase-related help so it has full context:

```
I'm working on a Unity game called Cogniville (C#). We use Firebase on the **Spark (free) plan** with **REST API only** (no Firebase SDK in Unity).

- **Auth:** Anonymous sign-in via Identity Toolkit REST (accounts:signUp) to get an idToken. One anonymous user per device; the token is sent as Bearer for Firestore.
- **Firestore:** Two collections:
  - `leaderboard` — global high scores (add on game end; query top N when opening leaderboard).
  - `sessions` — teacher session state (one doc per teacher, e.g. isActive, startedAt).
- **Unity:** We have a FirebaseConfig asset (apiKey, projectId, useFirebase). FirebaseBackend.cs uses UnityWebRequest to call Firebase REST; it runs Anonymous auth in Awake and then uses the idToken for Firestore reads/writes. GameDataManager and LeaderboardPanel use FirebaseBackend when config.useFirebase is true.
- **Rules:** Only authenticated users (request.auth != null) can read/write leaderboard and sessions. We stay on Spark (no Blaze).

When I ask about Firebase, assume this setup: REST only, Spark, Anonymous auth, Firestore for leaderboard + sessions. Help me with rules, REST URLs, or Unity C# (UnityWebRequest) as needed.
```

# Firebase Backend (Spark Plan — Free)

Cogniville uses **Firebase REST API** (no SDK) so it works on the **Spark (free)** plan. Leaderboard and teacher sessions sync to **Cloud Firestore**; auth uses **Anonymous** sign-in to get a token.

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
5. In **Firestore > Rules**, set rules so only authenticated users can read/write:

```text
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /leaderboard/{entry} {
      allow read, create: if request.auth != null;
    }
    match /sessions/{session} {
      allow read, write: if request.auth != null;
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

## 4. Disable Firebase

- Untick **Use Firebase** on the config, or clear **Api Key** / **Project Id**.
- The game falls back to local-only (PlayerPrefs + in-memory leaderboard).

---

## 5. Troubleshooting

- **“Auth failed”**: In Firebase Console, ensure **Anonymous** sign-in is enabled under Authentication.
- **403 / Permission denied**: Check Firestore rules and that the client uses the Auth token (Bearer).
- **Leaderboard empty**: First open the leaderboard after at least one game has been played and Firebase is configured; data is in Firestore and loads when the panel opens.

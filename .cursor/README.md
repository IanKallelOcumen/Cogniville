# Cursor rules for Cogniville

Rules in `rules/` give the AI project context so it can work faster and stay consistent.

- **cogniville-project.mdc** — Always applied: project structure, panel names, UI font/buttons, teacher flow.
- **unity-scripts.mdc** — When editing `Assets/Scripts/**/*.cs` or `Assets/Scenes/**/*.unity`: singletons, scene refs, FindObjectByType, TMP_InputField.
- **editor-tools.mdc** — Always applied: use Tools > Cogniville for bulk font/scale; when to grep vs search; don’t commit Logs/debug.

To add more rules: create `.mdc` files in `rules/` with YAML frontmatter (`description`, optional `globs`, `alwaysApply`). Keep each rule short and focused.

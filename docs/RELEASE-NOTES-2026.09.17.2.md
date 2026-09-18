## Downloads

- **[Download the app package (ZIP)](https://github.com/Kaurus2/eq-legends-achievement-companion/releases/download/v2026.09.17.2/EQ-Legends-Achievement-Companion-v2026.09.17.2.zip)**
- **[Read the App Guide (PDF)](https://github.com/Kaurus2/eq-legends-achievement-companion/releases/download/v2026.09.17.2/EQ-Legends-Achievement-Companion-App-Guide.pdf)**

## Group kill tracking fix

Saved mob names were working for personal kills, but group kills credited to another player were skipped. The tracker now accepts a matching group kill when your log records party experience shortly before the death line at the same timestamp. Each experience signal is used once. Unrelated kills without that signal remain excluded.

This fixes the reported Phoboplasm / Cubic case when Phoboplasm is in your saved mob list. Counts remain estimates; export achievements in game to confirm actual credit. Existing kills are not replayed when you install the update.

All features from v2026.09.17.1 remain included: tile time estimates, multiple mob-name entry, completion effects, Banestrike labels, and the illustrated guide with clickable contents.

229 automated checks and native app integration checks passed. Tests cover group credit, no-credit kills, stale signals, duplicate prevention and monitor resets.

Use Start update in the app, or extract the full ZIP for a new installation. The standalone EXE is for the in-app updater. Download the PDF separately for the revised guide; the updater replaces only the executable. Your saved mob names and notes stay in place.

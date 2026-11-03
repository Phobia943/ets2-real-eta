## Download und Start

**Empfohlen:** `ETS2-Fahrzeit-v0.1.0-win-x64.zip` herunterladen, vollständig entpacken und `ETS2-Fahrzeit.exe` starten. Alternativ gibt es die EXE einzeln.

- Windows x64; die .NET-Laufzeit ist enthalten. Kein Installer und keine separate .NET-Installation nötig.
- ETS2 mit aktivierter RenCloud-Telemetrie (Formatrevision 12) und einer Navi-Route erforderlich. ETS2LA selbst ist nicht erforderlich.
- Falls das Telemetrie-Plugin fehlt: die [kurze Einrichtung im README](https://github.com/Phobia943/ets2-real-eta#telemetrie-einmalig-einrichten) befolgen.

## Enthalten

- Restfahrzeit in echten Minuten, ungefähre Ankunftszeit und verbleibende Kilometer.
- Anpassung an das tatsächliche Vorankommen nach etwa 30 Sekunden; gleitender Durchschnitt bis 90 Sekunden.
- Behandlung von Spielpausen, Stillstand, Routenwechseln und Verbindungsabbrüchen.
- Separates kleines Fenster mit optionalem „Immer im Vordergrund“; kein Eingriff in Lenkung oder Spielsteuerung.

15 automatische Prüfungen bestanden; Zugriff auf laufende ETS2/TruckersMP-Telemetrie lokal geprüft. Die Ankunftszeit ist eine Schätzung, insbesondere zukünftige Staus, Fähren, Stadtfahrten und Mautstopps können abweichen.

Die EXE ist nicht digital signiert. Prüfsummen stehen in `SHA256SUMS.txt`. Die ausführliche Anleitung und Lizenzhinweise liegen im ZIP und im Repository.

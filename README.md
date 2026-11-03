# ETS2 · Echte Fahrzeit

**Wie lange dauert die restliche Strecke in echten Minuten?** Dieses kleine Windows-Fenster zeigt die geschätzte Restfahrzeit, die ungefähre Ankunftszeit und die verbleibenden Navi-Kilometer live an.

[**Windows-Version herunterladen**](https://github.com/Phobia943/ets2-real-eta/releases/latest) · [Fehler melden](https://github.com/Phobia943/ets2-real-eta/issues)

## In drei Schritten starten

1. Unter **Releases** die Datei **ETS2-Fahrzeit-v0.1.0-win-x64.zip** herunterladen. Die automatisch angebotenen „Source code“-Archive sind für Entwickler.
2. ZIP vollständig entpacken und **ETS2-Fahrzeit.exe** doppelklicken. Alternativ kann die einzelne EXE heruntergeladen werden. Eine separate .NET-Installation ist nicht nötig.
3. ETS2 starten, den Lkw laden und ein Ziel im Navi setzen. Die Telemetrie muss eingerichtet sein; siehe unten.

Unterstützte Plattform: **Windows x64** mit ETS2. Die Verbindung wurde auch während TruckersMP geprüft. ETS2LA ist **nicht** erforderlich. Die Anzeige benötigt weder ein GitHub-Konto noch eine Internetverbindung. Die EXE ist nicht digital signiert; Prüfsummen liegen beim Download.

## Telemetrie einmalig einrichten

Die Anzeige liest Daten des **RenCloud SCS SDK Plugins, Formatrevision 12**. Wenn ETS2LA oder eine andere Anwendung dieses Plugin bereits verwendet, ist in der Regel nichts weiter zu tun. Ein beliebiges anderes Telemetrie-Plugin reicht nicht unbedingt aus.

Falls die Anzeige dauerhaft auf Spieldaten wartet:

1. ETS2 vollständig beenden.
2. Das Plugin von der [offiziellen RenCloud-Release-Seite](https://github.com/RenCloud/scs-sdk-plugin/releases/latest) herunterladen und entpacken.
3. Die **64-Bit-Version** von `scs-telemetry.dll` aus dem Paket in den ETS2-Ordner `bin\win_x64\plugins` kopieren. Den Ordner `plugins` bei Bedarf erstellen. In Steam findet sich der Spielordner über **Verwalten → Lokale Dateien durchsuchen**. Eine vorhandene Installation nicht blind durch eine zweite Version ersetzen.
4. ETS2 neu starten und den Hinweis zu SDK-Funktionen bestätigen, falls er erscheint. Lkw laden und eine Navi-Route wählen.

Das Telemetrie-Plugin wird separat bezogen und ist nicht im Download dieser Anzeige enthalten.

## Anzeige bedienen

- **Große Zahl:** geschätzte Restfahrzeit in echten Minuten bzw. Stunden.
- **Ankunft:** ungefähre lokale Uhrzeit deines PCs.
- **Kilometer:** verbleibende Strecke laut Spiel-Navi, keine realen Straßenkilometer.
- **Immer im Vordergrund:** hält das Fenster über anderen normalen Fenstern.

Das Fenster startet ohne Fokusübernahme, bevorzugt auf einem zweiten Bildschirm. Wenn es zunächst verdeckt ist, über die Taskleiste öffnen. Im exklusiven Vollbild kann es verdeckt bleiben; dann einen zweiten Bildschirm oder Fenstermodus verwenden. Beim Schließen wird die Fensterposition gespeichert. Es gibt keinen Autostart und keine globalen Tastenkürzel.

## So entsteht die Schätzung

Die erste Schätzung teilt die Navi-Zeit durch den aktuellen lokalen Zeitmaßstab. Nach mindestens 30 Sekunden verwertbarer Fahrt nutzt die Anzeige deinen tatsächlichen Routenfortschritt pro echter Sekunde. Dafür werden bis zu 90 Sekunden betrachtet; die Anzeige wird geglättet und einmal pro Sekunde aktualisiert.

Kurze Stopps fließen in den Durchschnitt ein. Bei längeren Stopps zeigt sie die Restfahrzeit ab Weiterfahrt; die zusätzliche Wartezeit ist unbekannt. Spielpausen werden nicht als langsame Fahrt gezählt. Größere Routensprünge setzen die Lernwerte zurück. Fehlende oder veraltete Daten blenden die Schätzung aus. Ohne Navi-Route wird keine Ankunft behauptet.

**Die Zeit ist eine Schätzung.** Zukünftige Stadtfahrten, andere Kartenmaßstäbe, Fähren, Staus und Mautstopps sind nicht vorhersagbar. Besonders die anfängliche Navi-Schätzung kann bei Maßstabswechseln abweichen. Die langfristige Genauigkeit wurde noch nicht gegen eine vollständig abgeschlossene Fahrt vermessen.

## Daten und Unabhängigkeit

Die Anwendung öffnet den dokumentierten Telemetriespeicher ausschließlich mit Leserechten. Sie sendet keine Lenk-, Gas-, Brems- oder Tasteneingaben, verändert weder ETS2LA noch das Spiel und überträgt keine Daten ins Internet. Gespeichert wird nur die Fensterposition unter `%LOCALAPPDATA%\ETS2-Fahrzeit\window.json`.

## Selbst bauen

Unter Windows mit dem **.NET 10 SDK**:

```powershell
dotnet run --project Tests -c Release
./Publish.ps1
```

Das Skript erzeugt unter `artifacts` eine portable Windows-x64-EXE mit enthaltener .NET-Laufzeit, eine ZIP-Datei samt Anleitung/Lizenzen und SHA-256-Prüfsummen. GitHub Actions führt dieselben Prüfungen und den Paketbau für `main` und Pull Requests aus. Für Änderungen am Versionsnamen sind Projektdatei, Paketskript und Downloadhinweise gemeinsam anzupassen.

## Quellen und Lizenz

- [Öffentliches Telemetrie-Datenformat](https://github.com/RenCloud/scs-sdk-plugin/blob/master/scs-telemetry/inc/scs-telemetry-common.hpp)
- [SCS-Navigationseinheiten](https://github.com/RenCloud/scs-sdk-plugin/blob/master/scs_sdk/include/common/scssdk_telemetry_truck_common_channels.h)
- [SCS-Zeit- und Kartenmaßstab](https://github.com/RenCloud/scs-sdk-plugin/blob/master/scs_sdk/readme.txt)

Eigener Quellcode: [MIT](LICENSE). Die portable Version enthält Microsoft .NET; Lizenzhinweise liegen dem ZIP bei und sind in der EXE eingebettet. Unabhängiges Projekt, nicht mit SCS Software oder TruckersMP verbunden.

Der portable Paketbau verwendet .NET 10.0.9 passend zu den beigefügten Lizenzhinweisen. Bei Laufzeit-Updates sind auch diese Hinweise zu aktualisieren.

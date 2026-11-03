namespace RealEta;

public record Estimate(double? Seconds, string Status, string Method, double? Km, double? SpeedKph);

public sealed class Estimator
{
    private readonly Queue<(double Duration, double Metres)> history = new();
    private Sample? previous;
    private double previousWall, lastChanged, elapsed, distance, stoppedFor;
    private double? smoothed;

    public Estimate Update(Sample? s, double now)
    {
        if (s is null || !s.Active)
        {
            Reset();
            return new(null, "Warte auf ETS2 / Telemetrie", "Spiel starten und Lkw laden", null, null);
        }
        if (previous is null || previous.Timestamp != s.Timestamp) lastChanged = now;
        if (now - lastChanged > 4 && !s.Paused)
        {
            Reset();
            // Keep the last timestamp so a frozen mapping cannot look fresh again.
            previous = s; previousWall = now;
            return new(null, "Keine aktuellen Spieldaten", "Verbindung wird automatisch erneut geprüft", null, null);
        }
        var dt = previous is null ? 0 : now - previousWall;
        var delta = previous is null ? 0 : previous.Distance - s.Distance;
        var validDt = dt > 0 && dt < 5;
        var routeChanged = previous is not null && (s.Timestamp < previous.Timestamp
            || delta < -250 || delta > Math.Max(500, Math.Max(Math.Abs(previous.Speed), Math.Abs(s.Speed)) * Math.Max(previous.Scale, s.Scale) * dt * 2.5 + 100));
        if (routeChanged || (previous is not null && dt >= 5)) ClearHistory();

        if (s.Distance < 1 || s.NavigationSeconds <= 0)
        {
            ClearHistory(); previous = s; previousWall = now;
            return new(null, "Keine aktive Navi-Route", "Ziel im Spiel auswählen", s.Distance / 1000d, s.Speed * 3.6);
        }

        if (!routeChanged && validDt && !s.Paused && previous is { Paused: false })
        {
            history.Enqueue((dt, delta)); elapsed += dt; distance += delta;
            while (history.Count > 1 && elapsed - history.Peek().Duration >= 90)
            {
                var old = history.Dequeue(); elapsed -= old.Duration; distance -= old.Metres;
            }
            stoppedFor = Math.Abs(s.Speed) < .5 ? stoppedFor + dt : 0;
        }
        previous = s; previousWall = now;
        if (s.Paused)
            return new(smoothed ?? s.NavigationSeconds / s.Scale, "Spiel pausiert", "Restzeit bleibt stehen · Ankunft ab Weiterfahrt", s.Distance / 1000d, s.Speed * 3.6);

        var learned = elapsed >= 30 && distance > 100;
        var target = learned ? s.Distance / (distance / elapsed) : s.NavigationSeconds / s.Scale;
        var method = learned ? $"Aus deinem Fortschritt der letzten {Math.Round(elapsed):0} s"
            : $"Navi-Schätzung · lernt dein Tempo ({Math.Min(30, elapsed):0}/30 s)";
        if (stoppedFor >= 10)
            return new(smoothed ?? target, "Du stehst · Ankunft verschiebt sich", "Restfahrzeit ab Weiterfahrt · Wartezeit noch offen", s.Distance / 1000d, s.Speed * 3.6);
        if (elapsed >= 30 && distance <= 100)
            return new(null, "Zu wenig Fortschritt für eine Schätzung", "Bei Weiterfahrt wird neu berechnet", s.Distance / 1000d, s.Speed * 3.6);

        // Smooth the estimate, not a wall-clock countdown: stopping must not consume route time.
        smoothed = smoothed is null || routeChanged ? target
            : smoothed + (target - smoothed) * (1 - Math.Exp(-Math.Max(0, dt) / 12));
        return new(smoothed, routeChanged ? "Route geändert · neu berechnet" : "Live · geschätzte Restfahrzeit", method, s.Distance / 1000d, s.Speed * 3.6);
    }

    private void ClearHistory()
    {
        history.Clear(); elapsed = 0; distance = 0; stoppedFor = 0; smoothed = null;
    }
    public void Reset() { ClearHistory(); previous = null; previousWall = 0; }
}

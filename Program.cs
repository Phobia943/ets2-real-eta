using System.Diagnostics;
using System.Text.Json;

namespace RealEta;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--probe")
        {
            using var probe = new Telemetry();
            File.WriteAllText(args[1], JsonSerializer.Serialize(probe.Read(), new JsonSerializerOptions { WriteIndented = true }));
            return;
        }
        using var mutex = new Mutex(true, @"Local\ETS2-RealEta-Window", out bool first);
        if (!first) return;
        ApplicationConfiguration.Initialize();
        Application.Run(new EtaWindow());
    }
}

public sealed class EtaWindow : Form
{
    private readonly Telemetry telemetry = new();
    private readonly Estimator estimator = new();
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 1000 };
    private readonly Label state, remaining, arrival, route, method;
    private readonly CheckBox pin;
    private readonly string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ETS2-Fahrzeit", "window.json");

    // Do not steal focus from a running game on startup.
    protected override bool ShowWithoutActivation => true;

    public EtaWindow()
    {
        Text = "ETS2 · Echte Fahrzeit";
        ClientSize = new Size(430, 290);
        MinimumSize = new Size(446, 329);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.FromArgb(20, 26, 35);
        ForeColor = Color.FromArgb(234, 240, 247);
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.Manual;
        var screen = Screen.AllScreens.FirstOrDefault(x => !x.Primary) ?? Screen.PrimaryScreen!;
        Location = new Point(screen.WorkingArea.Right - Width - 24, screen.WorkingArea.Top + 30);
        state = LabelAt("Verbinde mit ETS2 …", 24, 18, 390, 26, 10, Color.FromArgb(92, 216, 185));
        remaining = LabelAt("— Min", 20, 48, 390, 72, 36, ForeColor, FontStyle.Bold);
        arrival = LabelAt("Voraussichtliche Ankunft —", 24, 126, 390, 26, 12, ForeColor);
        route = LabelAt("Warte auf Routendaten", 24, 160, 390, 25, 10, Color.FromArgb(188, 201, 218));
        method = LabelAt("", 24, 199, 390, 42, 9, Color.FromArgb(147, 165, 188));
        pin = new CheckBox { Text = "Immer im Vordergrund", Location = new Point(24, 251), AutoSize = true, TabIndex = 0 };
        pin.CheckedChanged += (_, _) => TopMost = pin.Checked;
        Controls.Add(pin);
        try
        {
            if (File.Exists(settingsPath))
            {
                var p = JsonSerializer.Deserialize<Placement>(File.ReadAllText(settingsPath));
                if (p is not null && Screen.AllScreens.Any(s => s.WorkingArea.Contains(new Rectangle(p.X, p.Y, Width, Height))))
                    Location = new Point(p.X, p.Y);
                // Never place a restored always-on-top window over the driver's game on startup.
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { }
        timer.Tick += (_, _) => RefreshEstimate();
        Shown += (_, _) => { RefreshEstimate(); timer.Start(); };
        FormClosed += (_, _) =>
        {
            timer.Stop(); timer.Dispose(); telemetry.Dispose();
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
                File.WriteAllText(settingsPath, JsonSerializer.Serialize(new Placement(Left, Top)));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        };
    }

    private Label LabelAt(string text, int x, int y, int width, int height, float size, Color color, FontStyle style = FontStyle.Regular)
    {
        var label = new Label { Text = text, Location = new Point(x, y), Size = new Size(width, height), Font = new Font("Segoe UI", size, style), ForeColor = color };
        Controls.Add(label); return label;
    }

    private void RefreshEstimate()
    {
        var e = estimator.Update(telemetry.Read(), clock.Elapsed.TotalSeconds);
        state.Text = e.Status; method.Text = e.Method;
        remaining.Text = FormatDuration(e.Seconds);
        Text = $"ETS2 · {remaining.Text} · Echte Fahrzeit";
        arrival.Text = e.Seconds is double sec && sec < 7 * 86400
            ? $"Ankunft ca. {DateTime.Now.AddSeconds(sec):HH:mm} Uhr" : "Voraussichtliche Ankunft —";
        if (e.Status == "Spiel pausiert") arrival.Text = "Ankunft ab Weiterfahrt";
        route.Text = e.Km is double km ? $"{km:N0} km verbleibend   ·   {Math.Abs(e.SpeedKph ?? 0):N0} km/h" : "Warte auf Routendaten";
    }

    public static string FormatDuration(double? seconds)
    {
        if (seconds is null || !double.IsFinite(seconds.Value) || seconds < 0) return "— Min";
        var min = (int)Math.Ceiling(Math.Min(seconds.Value / 60, 7 * 24 * 60));
        if (min < 1) return "< 1 Min";
        return min < 60 ? $"≈ {min} Min" : $"≈ {min / 60} h {min % 60:00}";
    }
    private record Placement(int X, int Y);
}

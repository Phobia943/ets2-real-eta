using RealEta;

int passed = 0;
void Check(bool condition, string name) { if (!condition) throw new Exception(name); Console.WriteLine("PASS " + name); passed++; }
Sample S(int t, float distance = 100000, float speed = 20, bool pause = false) => new(true, pause, (ulong)(t + 1), 19, speed, distance, 19000);

var e = new Estimator();
Check(e.Update(S(0), 0).Seconds == 1000, "Navi seconds converted to real seconds");
Estimate result = e.Update(S(0), 0);
for (int t = 1; t <= 90; t++) result = e.Update(S(t, 100000 - t * 380), t);
Check(Math.Abs(result.Seconds!.Value - 65800d / 380) < 25, "Actual map progress is learned and smoothed");
var beforePause = result.Seconds;
for (int t = 91; t <= 180; t++) result = e.Update(S(90, 65800, 0, true), t);
Check(result.Status == "Spiel pausiert" && result.Seconds == beforePause, "Pause freezes remaining time");
result = e.Update(S(181, 65000), 181);
Check(result.Seconds < 1000, "Pause does not inflate learned journey time");
result = e.Update(S(182, 900000), 182);
Check(result.Status.Contains("Route geändert") && result.Seconds == 1000, "Reroute resets history");
result = e.Update(S(183, 5000), 183);
Check(result.Status.Contains("Route geändert"), "Teleport resets history");
for (int t = 184; t <= 200; t++) result = e.Update(S(t, 5000, 0), t);
Check(result.Status.StartsWith("Du stehst"), "Stop does not count down to arrival");
Check(e.Update(S(201, 0), 201).Seconds is null, "No route is not reported as arrived");
Check(e.Update(null, 202).Seconds is null, "Disconnect clears ETA");
e = new Estimator(); e.Update(S(1), 1);
for (int t = 2; t <= 12; t++) result = e.Update(S(1), t);
Check(result.Seconds is null && result.Status.Contains("aktuellen"), "Frozen telemetry stays unavailable");
Check(e.Update(S(13), 13).Seconds is not null, "Connection recovers");
var bytes = new byte[1200];
void U(int o, uint v) => BitConverter.GetBytes(v).CopyTo(bytes, o);
void F(int o, float v) => BitConverter.GetBytes(v).CopyTo(bytes, o);
bytes[0] = 1; U(40, 12); U(52, 1); F(700,19); F(948,20); F(1060,100000); F(1064,19000);
Check(Telemetry.Decode(bytes)?.Distance == 100000, "Telemetry fields and offsets");
F(1060, float.NaN); Check(Telemetry.Decode(bytes) is null, "Reject non-finite route");
F(1060, 1); U(40, 99); Check(Telemetry.Decode(bytes) is null, "Reject unknown telemetry layout");
Check(EtaWindow.FormatDuration(3601) == "≈ 1 h 01", "Hour formatting");
Console.WriteLine($"{passed} checks passed.");

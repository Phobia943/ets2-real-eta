using System.IO.MemoryMappedFiles;

namespace RealEta;

public record Sample(bool Active, bool Paused, ulong Timestamp, float Scale,
    float Speed, float Distance, float NavigationSeconds);

// Public RenCloud SDK revision 12 layout; units from the SCS SDK.
// Only a read-only view is opened. No ETS2LA or game input channel is used.
public sealed class Telemetry : IDisposable
{
    private MemoryMappedFile? map;
    private MemoryMappedViewAccessor? view;
    private readonly byte[] buffer = new byte[1200];

    public Sample? Read()
    {
        try
        {
            map ??= MemoryMappedFile.OpenExisting(@"Local\SCSTelemetry", MemoryMappedFileRights.Read);
            view ??= map.CreateViewAccessor(0, buffer.Length, MemoryMappedFileAccess.Read);
            view.ReadArray(0, buffer, 0, buffer.Length);
            return Decode(buffer);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Dispose();
            return null;
        }
    }

    public static Sample? Decode(byte[] bytes)
    {
        if (bytes.Length < 1068 || BitConverter.ToUInt32(bytes, 40) != 12
            || BitConverter.ToUInt32(bytes, 52) is not (1 or 2)) return null;
        var s = new Sample(bytes[0] != 0, bytes[4] != 0, BitConverter.ToUInt64(bytes, 8),
            BitConverter.ToSingle(bytes, 700), BitConverter.ToSingle(bytes, 948),
            BitConverter.ToSingle(bytes, 1060), BitConverter.ToSingle(bytes, 1064));
        if (!float.IsFinite(s.Scale) || s.Scale < 1 || s.Scale > 100
            || !float.IsFinite(s.Speed) || Math.Abs(s.Speed) > 150
            || !float.IsFinite(s.Distance) || s.Distance < 0 || s.Distance > 100_000_000
            || !float.IsFinite(s.NavigationSeconds) || s.NavigationSeconds < 0) return null;
        return s;
    }

    public void Dispose()
    {
        view?.Dispose(); map?.Dispose(); view = null; map = null;
    }
}

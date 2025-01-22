using System.Globalization;
using System.Runtime.InteropServices;
using ManagedBass;
using NWaves.Signals;
using NWaves.Utils;

namespace TagTool.App.Core.Services;

public class BassAudioCaptureService : IDisposable
{
    private static readonly string _outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MBass");
    private readonly int _deviceId;
    private readonly int _handle;
    private readonly int _frequency;
    private readonly WaveFileWriter _writer;
    private byte[]? _buffer;

    public string OutputFilePath { get; }
        = Path.Combine(_outputPath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.CurrentCulture) + ".wav");

    /// <summary>
    ///     Initializes the audio capture service, and starts capturing the specified device ID
    /// </summary>
    /// <param name="deviceIdId"></param>
    /// <param name="frequency"></param>
    public BassAudioCaptureService(int deviceIdId = 1, int frequency = 44100)
    {
        Directory.CreateDirectory(_outputPath);
        // Output all devices, then select one
        // for (var i = 0; Bass.RecordGetDeviceInfo(i, out var info); ++i)
        // {
        //     Debug.WriteLine($"{i}\t\t{info.Type}: {info.Name}");
        // }

        Bass.Init();
        Bass.RecordInit(_deviceId);

        var info = Bass.RecordingInfo;
        _handle = Bass.RecordStart(info.Frequency, info.Channels, BassFlags.RecordPause, AudioChunkCaptured);
        _deviceId = deviceIdId;
        _frequency = frequency;
        _writer = new WaveFileWriter(new FileStream(OutputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read), new WaveFormat());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        // Bass.CurrentRecordingDevice = _deviceId;
        Bass.RecordFree();

        _writer.Dispose();
    }

    /// <summary>
    ///     Call back from the audio recording, to process each chunk of audio data
    /// </summary>
    /// <param name="handle">The device handle</param>
    /// <param name="buffer">The chunk of audio data</param>
    /// <param name="length">The length of the audio data chunk</param>
    /// <param name="user"></param>
    private bool AudioChunkCaptured(int handle, IntPtr buffer, int length, IntPtr user)
    {
        if (_buffer is null || _buffer.Length < length)
        {
            _buffer = new byte[length];
        }

        Marshal.Copy(buffer, _buffer, 0, length);

        _writer.Write(_buffer, length);

        return true;
    }

    public void Start() => Bass.ChannelPlay(_handle);

    public void Stop() => Bass.ChannelStop(_handle);

    public bool IsActive() => Bass.ChannelIsActive(_handle) == PlaybackState.Playing;

    public double GetVoiceIntensity()
    {
        if (_buffer is null)
        {
            return 0d;
        }

        var sampleCount = _buffer.Length / 2;

        var signal = new DiscreteSignal(_frequency, sampleCount);
        using var reader = new BinaryReader(new MemoryStream(_buffer));

        for (var i = 0; i < sampleCount; i++)
        {
            signal[i] = reader.ReadInt16() / 32768f;
        }

        return Scale.ToDecibel(signal.Rms() * 1.2);
    }
}

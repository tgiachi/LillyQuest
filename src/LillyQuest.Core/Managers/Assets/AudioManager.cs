using System.Buffers.Binary;
using System.Numerics;
using System.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Types;
using MP3Sharp;
using NVorbis;
using Serilog;
using Silk.NET.OpenAL;

namespace LillyQuest.Core.Managers.Assets;

/// <summary>
/// Manages audio device initialization and audio asset loading.
/// </summary>
public class AudioManager : IAudioManager, IDisposable
{
    private const int DefaultReadBufferSize = 4096;

    private readonly ILogger _logger = Log.ForContext<AudioManager>();
    private readonly Dictionary<string, uint> _soundBuffers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, uint> _musicBuffers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<uint>> _soundSourcesByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<uint, string> _soundSourceToName = new();
    private readonly List<uint> _activeSoundSources = new();
    private uint _musicSource;

    private AudioContext? _audioContext;
    private AL _al;
    private bool _isInitialized;
    private Vector3 _listenerPosition;

    /// <summary>
    /// Gets or sets the listener position.
    /// </summary>
    public Vector3 ListenerPosition
    {
        get => _listenerPosition;
        set
        {
            _listenerPosition = value;

            if (_isInitialized)
            {
                _al.SetListenerProperty(ListenerVector3.Position, value);
            }
        }
    }

    /// <summary>
    /// Releases resources used by the audio manager.
    /// </summary>
    public void Dispose()
    {
        Shutdown();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes the OpenAL device and context.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _audioContext = new();
        _al = AL.GetApi();
        _al.SetListenerProperty(ListenerVector3.Position, _listenerPosition);
        _isInitialized = true;
        _logger.Information("AudioManager initialized.");
    }

    /// <summary>
    /// Loads a music track from raw data buffer.
    /// </summary>
    /// <param name="name">The music name.</param>
    /// <param name="data">The audio data buffer.</param>
    public void LoadMusicFromBuffer(string name, Span<byte> data)
    {
        LoadAudioFromBuffer(name, data, _musicBuffers);
    }

    /// <summary>
    /// Loads a music track from file.
    /// </summary>
    /// <param name="name">The music name.</param>
    /// <param name="filePath">The file path.</param>
    public void LoadMusicFromFile(string name, string filePath)
    {
        LoadAudioFromFile(name, filePath, _musicBuffers);
    }

    /// <summary>
    /// Loads a sound from raw data buffer.
    /// </summary>
    /// <param name="name">The sound name.</param>
    /// <param name="data">The audio data buffer.</param>
    public void LoadSoundFromBuffer(string name, Span<byte> data)
    {
        LoadAudioFromBuffer(name, data, _soundBuffers);
    }

    /// <summary>
    /// Loads a sound from file.
    /// </summary>
    /// <param name="name">The sound name.</param>
    /// <param name="filePath">The file path.</param>
    public void LoadSoundFromFile(string name, string filePath)
    {
        LoadAudioFromFile(name, filePath, _soundBuffers);
    }

    /// <summary>
    /// Plays a loaded music track.
    /// </summary>
    /// <param name="name">The music name.</param>
    /// <param name="isLooping">True to loop the music.</param>
    public void PlayMusic(string name, bool isLooping = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureInitialized();

        if (!_musicBuffers.TryGetValue(name, out var buffer))
        {
            throw new KeyNotFoundException($"Music not found: {name}");
        }

        StopMusic();

        _musicSource = _al.GenSource();
        _al.SetSourceProperty(_musicSource, SourceInteger.Buffer, (int)buffer);
        _al.SetSourceProperty(_musicSource, SourceBoolean.Looping, isLooping);
        _al.SourcePlay(_musicSource);
    }

    /// <summary>
    /// Plays a loaded sound.
    /// </summary>
    /// <param name="name">The sound name.</param>
    /// <param name="isLooping">True to loop the sound.</param>
    public void PlaySound(string name, float gain = 1f, bool isLooping = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureInitialized();
        CleanupStoppedSources();

        if (!_soundBuffers.TryGetValue(name, out var buffer))
        {
            throw new KeyNotFoundException($"Sound not found: {name}");
        }

        var source = _al.GenSource();
        _al.SetSourceProperty(source, SourceInteger.Buffer, (int)buffer);
        _al.SetSourceProperty(source, SourceBoolean.Looping, isLooping);
        _al.SetSourceProperty(source, SourceFloat.Gain, gain);
        _al.SourcePlay(source);

        _activeSoundSources.Add(source);
        _soundSourceToName[source] = name;

        if (!_soundSourcesByName.TryGetValue(name, out var sources))
        {
            sources = new();
            _soundSourcesByName[name] = sources;
        }

        sources.Add(source);
    }

    /// <summary>
    /// Plays a loaded sound at the given world position.
    /// </summary>
    /// <param name="name">The sound name.</param>
    /// <param name="position">The emitter position.</param>
    /// <param name="gain">The gain multiplier.</param>
    /// <param name="isLooping">True to loop the sound.</param>
    public void PlaySound3D(string name, Vector3 position, float gain = 1f, bool isLooping = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureInitialized();
        CleanupStoppedSources();

        if (!_soundBuffers.TryGetValue(name, out var buffer))
        {
            throw new KeyNotFoundException($"Sound not found: {name}");
        }

        var source = _al.GenSource();
        _al.SetSourceProperty(source, SourceInteger.Buffer, (int)buffer);
        _al.SetSourceProperty(source, SourceBoolean.Looping, isLooping);
        _al.SetSourceProperty(source, SourceFloat.Gain, gain);
        _al.SetSourceProperty(source, SourceVector3.Position, position);
        _al.SourcePlay(source);

        _activeSoundSources.Add(source);
        _soundSourceToName[source] = name;

        if (!_soundSourcesByName.TryGetValue(name, out var sources))
        {
            sources = new();
            _soundSourcesByName[name] = sources;
        }

        sources.Add(source);
    }

    /// <summary>
    /// Sets the listener orientation.
    /// </summary>
    /// <param name="forward">Listener forward direction.</param>
    /// <param name="up">Listener up direction.</param>
    public void SetListenerOrientation(Vector3 forward, Vector3 up)
    {
        if (!_isInitialized)
        {
            return;
        }

        unsafe
        {
            Span<float> orientation = stackalloc float[6]
            {
                forward.X, forward.Y, forward.Z,
                up.X, up.Y, up.Z
            };

            fixed (float* ptr = orientation)
            {
                _al.SetListenerProperty(ListenerFloatArray.Orientation, ptr);
            }
        }
    }

    /// <summary>
    /// Shuts down the OpenAL device and releases resources.
    /// </summary>
    public void Shutdown()
    {
        if (!_isInitialized)
        {
            return;
        }

        StopAll();

        foreach (var buffer in _soundBuffers.Values)
        {
            _al.DeleteBuffer(buffer);
        }

        foreach (var buffer in _musicBuffers.Values)
        {
            _al.DeleteBuffer(buffer);
        }

        _soundBuffers.Clear();
        _musicBuffers.Clear();

        _audioContext?.Dispose();
        _audioContext = null;
        _isInitialized = false;
        _logger.Information("AudioManager shutdown completed.");
    }

    /// <summary>
    /// Stops all currently playing audio.
    /// </summary>
    public void StopAll()
    {
        foreach (var source in _activeSoundSources)
        {
            _al.SourceStop(source);
            _al.DeleteSource(source);
        }

        _activeSoundSources.Clear();
        _soundSourceToName.Clear();
        _soundSourcesByName.Clear();

        StopMusic();
    }

    /// <summary>
    /// Stops the currently playing music.
    /// </summary>
    public void StopMusic()
    {
        if (_musicSource == 0)
        {
            return;
        }

        _al.SourceStop(_musicSource);
        _al.DeleteSource(_musicSource);
        _musicSource = 0;
    }

    /// <summary>
    /// Stops all sources for the specified sound.
    /// </summary>
    /// <param name="name">The sound name.</param>
    public void StopSound(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureInitialized();

        if (!_soundSourcesByName.TryGetValue(name, out var sources))
        {
            return;
        }

        foreach (var source in sources)
        {
            _al.SourceStop(source);
            _al.DeleteSource(source);
            _soundSourceToName.Remove(source);
            _activeSoundSources.Remove(source);
        }

        sources.Clear();
    }

    private void CleanupStoppedSources()
    {
        for (var i = _activeSoundSources.Count - 1; i >= 0; i--)
        {
            var source = _activeSoundSources[i];
            _al.GetSourceProperty(source, GetSourceInteger.SourceState, out var state);

            if ((SourceState)state != SourceState.Stopped)
            {
                continue;
            }

            _al.DeleteSource(source);
            _activeSoundSources.RemoveAt(i);

            if (_soundSourceToName.TryGetValue(source, out var name) &&
                _soundSourcesByName.TryGetValue(name, out var sources))
            {
                sources.Remove(source);

                if (sources.Count == 0)
                {
                    _soundSourcesByName.Remove(name);
                }
            }

            _soundSourceToName.Remove(source);
        }
    }

    private static DecodedAudio DecodeAudio(byte[] data, AudioFormatType format)
    {
        return format switch
        {
            AudioFormatType.WAV => DecodeWav(data),
            AudioFormatType.OGG => DecodeOgg(data),
            AudioFormatType.MP3 => DecodeMp3(data)
        };
    }

    private static DecodedAudio DecodeMp3(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var mp3Stream = new MP3Stream(stream);
        using var output = new MemoryStream();

        var buffer = new byte[DefaultReadBufferSize];
        int bytesRead;

        while ((bytesRead = mp3Stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, bytesRead);
        }

        if (mp3Stream.Frequency <= 0 || mp3Stream.ChannelCount <= 0)
        {
            throw new InvalidDataException("MP3 header information is invalid.");
        }

        return new(output.ToArray(), mp3Stream.Frequency, mp3Stream.ChannelCount, 16);
    }

    private static DecodedAudio DecodeOgg(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new VorbisReader(stream, false);

        var channels = (short)reader.Channels;
        var sampleRate = reader.SampleRate;
        var sampleBuffer = new float[DefaultReadBufferSize * reader.Channels];

        using var output = new MemoryStream();

        while (true)
        {
            var samplesRead = reader.ReadSamples(sampleBuffer, 0, sampleBuffer.Length);

            if (samplesRead == 0)
            {
                break;
            }

            var pcmBuffer = new byte[samplesRead * sizeof(short)];

            for (var i = 0; i < samplesRead; i++)
            {
                var sample = Math.Clamp(sampleBuffer[i], -1f, 1f);
                var pcmValue = (short)(sample * short.MaxValue);
                BinaryPrimitives.WriteInt16LittleEndian(pcmBuffer.AsSpan(i * 2, 2), pcmValue);
            }

            output.Write(pcmBuffer, 0, pcmBuffer.Length);
        }

        return new(output.ToArray(), sampleRate, channels, 16);
    }

    private static DecodedAudio DecodeWav(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, Encoding.ASCII);

        var riff = new string(reader.ReadChars(4));

        if (!riff.Equals("RIFF", StringComparison.Ordinal))
        {
            throw new InvalidDataException("Invalid WAV header.");
        }

        reader.ReadInt32();

        var wave = new string(reader.ReadChars(4));

        if (!wave.Equals("WAVE", StringComparison.Ordinal))
        {
            throw new InvalidDataException("Invalid WAV format.");
        }

        short channels = 0;
        short bitsPerSample = 0;
        var sampleRate = 0;
        byte[]? pcmData = null;

        while (stream.Position + 8 <= stream.Length)
        {
            var chunkId = new string(reader.ReadChars(4));
            var chunkSize = reader.ReadInt32();

            if (chunkId.Equals("fmt ", StringComparison.Ordinal))
            {
                var audioFormat = reader.ReadInt16();
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt16();
                bitsPerSample = reader.ReadInt16();

                if (chunkSize > 16)
                {
                    reader.ReadBytes(chunkSize - 16);
                }

                if (audioFormat != 1)
                {
                    throw new NotSupportedException("Only PCM WAV files are supported.");
                }
            }
            else if (chunkId.Equals("data", StringComparison.Ordinal))
            {
                pcmData = reader.ReadBytes(chunkSize);
            }
            else
            {
                reader.ReadBytes(chunkSize);
            }
        }

        if (pcmData == null)
        {
            throw new InvalidDataException("WAV data chunk not found.");
        }

        return new(pcmData, sampleRate, channels, bitsPerSample);
    }

    private static AudioFormatType DetectFormatFromData(byte[] data)
    {
        if (data.Length >= 12 &&
            data[0] == (byte)'R' &&
            data[1] == (byte)'I' &&
            data[2] == (byte)'F' &&
            data[3] == (byte)'F' &&
            data[8] == (byte)'W' &&
            data[9] == (byte)'A' &&
            data[10] == (byte)'V' &&
            data[11] == (byte)'E')
        {
            return AudioFormatType.WAV;
        }

        if (data.Length >= 4 &&
            data[0] == (byte)'O' &&
            data[1] == (byte)'g' &&
            data[2] == (byte)'g' &&
            data[3] == (byte)'S')
        {
            return AudioFormatType.OGG;
        }

        if (data.Length >= 3 &&
            data[0] == (byte)'I' &&
            data[1] == (byte)'D' &&
            data[2] == (byte)'3')
        {
            return AudioFormatType.MP3;
        }

        if (data.Length >= 2 && data[0] == 0xFF && (data[1] & 0xE0) == 0xE0)
        {
            return AudioFormatType.MP3;
        }

        return AudioFormatType.Unknown;
    }

    private void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("AudioManager is not initialized.");
        }
    }

    private static BufferFormat GetBufferFormat(short channels, short bitsPerSample)
    {
        return (channels, bitsPerSample) switch
        {
            (1, 8)  => BufferFormat.Mono8,
            (1, 16) => BufferFormat.Mono16,
            (2, 8)  => BufferFormat.Stereo8,
            (2, 16) => BufferFormat.Stereo16,
            _       => throw new NotSupportedException("Unsupported audio channel configuration.")
        };
    }

    private static AudioFormatType GetFormatFromPath(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".wav" => AudioFormatType.WAV,
            ".ogg" => AudioFormatType.OGG,
            ".mp3" => AudioFormatType.MP3,
            _      => AudioFormatType.Unknown
        };
    }

    private void LoadAudioFromBuffer(string name, Span<byte> data, Dictionary<string, uint> targetBuffers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureInitialized();

        if (targetBuffers.ContainsKey(name))
        {
            _logger.Warning("Audio with name {AudioName} already loaded.", name);

            return;
        }

        if (data.Length == 0)
        {
            throw new ArgumentException("Audio data cannot be empty.", nameof(data));
        }

        var bufferData = data.ToArray();
        var format = DetectFormatFromData(bufferData);
        LoadAudioInternal(name, bufferData, format, targetBuffers);
    }

    private void LoadAudioFromFile(string name, string filePath, Dictionary<string, uint> targetBuffers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        EnsureInitialized();

        if (targetBuffers.ContainsKey(name))
        {
            _logger.Warning("Audio with name {AudioName} already loaded.", name);

            return;
        }

        if (!File.Exists(filePath))
        {
            _logger.Error("Audio file not found: {FilePath}", filePath);

            throw new FileNotFoundException("Audio file not found.", filePath);
        }

        var data = File.ReadAllBytes(filePath);
        var format = GetFormatFromPath(filePath);
        LoadAudioInternal(name, data, format, targetBuffers);
    }

    private void LoadAudioInternal(string name, byte[] data, AudioFormatType format, Dictionary<string, uint> targetBuffers)
    {
        var decoded = DecodeAudio(data, format);
        var bufferFormat = GetBufferFormat(decoded.Channels, decoded.BitsPerSample);
        var buffer = _al.GenBuffer();
        _al.BufferData(buffer, bufferFormat, decoded.Data, decoded.SampleRate);
        targetBuffers[name] = buffer;
        _logger.Information("Audio {AudioName} loaded successfully.", name);
    }
}

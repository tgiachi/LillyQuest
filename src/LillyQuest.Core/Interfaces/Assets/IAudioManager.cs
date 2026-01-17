using System.Numerics;

namespace LillyQuest.Core.Interfaces.Assets;

public interface IAudioManager
{
    /// <summary>
    /// Gets or sets the listener position.
    /// </summary>
    Vector3 ListenerPosition { get; set; }

    /// <summary>
    /// Initializes the audio system.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Loads a music track from a raw buffer.
    /// </summary>
    void LoadMusicFromBuffer(string name, Span<byte> data);

    /// <summary>
    /// Loads a music track from file.
    /// </summary>
    void LoadMusicFromFile(string name, string filePath);

    /// <summary>
    /// Loads a sound from a raw buffer.
    /// </summary>
    void LoadSoundFromBuffer(string name, Span<byte> data);

    /// <summary>
    /// Loads a sound from file.
    /// </summary>
    void LoadSoundFromFile(string name, string filePath);

    /// <summary>
    /// Plays a loaded music track.
    /// </summary>
    void PlayMusic(string name, bool isLooping = true);

    /// <summary>
    /// Plays a loaded sound.
    /// </summary>
    void PlaySound(string name, float gain = 1f, bool isLooping = false);

    /// <summary>
    /// Plays a loaded sound at the given world position.
    /// </summary>
    void PlaySound3D(string name, Vector3 position, float gain = 1f, bool isLooping = false);

    /// <summary>
    /// Sets the listener orientation.
    /// </summary>
    void SetListenerOrientation(Vector3 forward, Vector3 up);

    /// <summary>
    /// Shuts down the audio system.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Stops all currently playing audio.
    /// </summary>
    void StopAll();

    /// <summary>
    /// Stops the currently playing music.
    /// </summary>
    void StopMusic();

    /// <summary>
    /// Stops all sources for the specified sound.
    /// </summary>
    void StopSound(string name);
}

namespace LillyQuest.Core.Managers.Assets;

/// <summary>
/// Represents decoded PCM audio data.
/// </summary>
public readonly record struct DecodedAudio(byte[] Data, int SampleRate, short Channels, short BitsPerSample);

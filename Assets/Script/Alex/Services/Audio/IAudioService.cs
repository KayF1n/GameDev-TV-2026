using System;
using System.Collections.Generic;

public interface IAudioService {
    void SetVolume(AudioChannelType channel, float normalizedVolume);
    float GetVolume(AudioChannelType channel);
    List<AudioChannelType> GetSupportedChannelsTypes();
    void StopCurrentMusic();
    void StartMusicPlaylist(MusicPlaylist mainMenu);
}

public enum MusicPlaylist {
    MainMenu,
    GameLoop,
    BossFight
}
public enum AudioChannelType {
    Master,
    Music,
    SFX,
    Ambience
}

public class AudioSystem : IDataLoader, IDataSaveable {
    private readonly IAudioService _audioService;
    private readonly IDataRepository<AudioSettings> _dataRepository;

    public AudioSystem(IAudioService audioService, IDataRepository<AudioSettings> dataRepository) {
        _audioService = audioService;
        _dataRepository = dataRepository;
    }
    public void Load() {
        AudioSettings audioSettings = _dataRepository.Load();
        if (audioSettings == null) {
            audioSettings = new AudioSettings();
        }
        ApplyAudioData(audioSettings);
    }

    public void Save() {
        AudioSettings audioSettings = CollectAudioData();
        _dataRepository.Save(audioSettings);
    }

    private void ApplyAudioData(AudioSettings data) {
        foreach (var channel in data.channels) {
            _audioService.SetVolume(channel.ChannelType, channel.Volume);
        }
    }

    private AudioSettings CollectAudioData() {
        var settings = new AudioSettings();
        foreach (var channelType in _audioService.GetSupportedChannelsTypes()) {
            settings.channels.Add(new AudioChannel {
                ChannelType = channelType,
                Volume = _audioService.GetVolume(channelType)
            });
        }
        return settings;
    }
}

[Serializable]
[DataSource(DataSourceType.PlayerPrefs, "audio_settings")]
public class AudioSettings {
    public List<AudioChannel> channels = new();
}

[Serializable]
public class AudioChannel {
    public string name;
    public AudioChannelType ChannelType;
    public float Volume;
}
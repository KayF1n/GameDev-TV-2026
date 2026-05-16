using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class FmodAudioService : IAudioService {
    private readonly Dictionary<AudioChannelType, string> busNames = new() {
        { AudioChannelType.Master, "bus:/" },
        { AudioChannelType.Music, "bus:/Music" },
        { AudioChannelType.SFX, "bus:/SFX" },
        { AudioChannelType.Ambience, "bus:/Ambience" }
    };

    private Dictionary<AudioChannelType, FMOD.Studio.Bus> buses = new();

    private Dictionary<string, EventInstance> loopedInstances = new();
    private Dictionary<string, EventInstance> musicInstances = new();

    private Dictionary<EventReference, EventDescription> eventDescriptionCache = new();

    public string SaveKey => "audio_settings";

    private AudioStateConfig _config;
    private EventInstance _currentMusic;
    private EventReference _currentMusicRef;

    public FmodAudioService(AudioStateConfig config) {
        InitDictionary();
        _config = config;
        StartMusicPlaylist(MusicPlaylist.MainMenu);
        SetVolume(AudioChannelType.Master, 0.5f);
    }

    private void InitDictionary() {
        foreach (var kvp in busNames) {
            buses[kvp.Key] = RuntimeManager.GetBus(kvp.Value);
        }
    }

    #region IAudioService Implementation (??????E??Q??????)

    public float GetVolume(AudioChannelType channel) {
        if (!buses.TryGetValue(channel, out var bus)) {
            Debug.LogError($"Bus for channel {channel} not found!");
            return 0f;
        }
        bus.getVolume(out float volume);
        return volume;
    }

    public void SetVolume(AudioChannelType channel, float normalizedVolume) {
        if (!buses.TryGetValue(channel, out var bus)) {
            Debug.LogError($"Bus for channel {channel} not found!");
            return;
        }
        bus.setVolume(Mathf.Clamp01(normalizedVolume));
    }

    public List<AudioChannelType> GetSupportedChannelsTypes() {
        return new List<AudioChannelType>(buses.Keys);
    }

    public void StopCurrentMusic() {
        if (_currentMusic.isValid()) {
            _currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _currentMusic.release();
        }
    }

    public void StartMusicPlaylist(MusicPlaylist playlist) {
        var eventRef = _config.GetMusicEvent(playlist);
        if (!eventRef.IsNull) {
            PlayMusic(eventRef);
        }
    }

    public void PlayMusic(EventReference musicEvent) {
        if (IsSameEvent(musicEvent, _currentMusicRef)) {
            return;
        }

        if (_currentMusic.isValid()) {
            StopCurrentMusic();
        }

        _currentMusicRef = musicEvent;
        _currentMusic = RuntimeManager.CreateInstance(musicEvent);
        _currentMusic.start();
    }

    #endregion

    #region One-Shot Sounds
    public void PlaySound(string eventPath) {
        RuntimeManager.PlayOneShot(eventPath);
    }

    public void PlayOneShot(EventReference eventRef, Vector3 position) {
        if (eventRef.IsNull) {
            Debug.LogWarning("FmodAudioService: Event reference is null!");
            return;
        }
        RuntimeManager.PlayOneShot(eventRef, position);
    }

    public void PlayOneShotWithParameter(EventReference eventRef, Vector3 position, string paramName, float paramValue) {
        if (eventRef.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(eventRef);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.setParameterByName(paramName, paramValue);
        instance.start();
        instance.release();
    }

    #endregion

    #region Looped Sounds

    public EventInstance PlayLooped(string key, EventReference eventRef, Transform parent = null) {
        if (loopedInstances.ContainsKey(key)) {
            Debug.LogWarning($"FmodAudioService: Sound with key '{key}' is already playing!");
            return loopedInstances[key];
        }

        if (eventRef.IsNull) {
            Debug.LogWarning("FmodAudioService: Event reference is null!");
            return default;
        }

        EventInstance instance = RuntimeManager.CreateInstance(eventRef);

        if (parent != null) {
            RuntimeManager.AttachInstanceToGameObject(instance, parent);
        }

        instance.start();
        loopedInstances[key] = instance;

        return instance;
    }

    public void StopLooped(string key, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT) {
        if (loopedInstances.TryGetValue(key, out EventInstance instance)) {
            instance.stop(stopMode);
            instance.release();
            loopedInstances.Remove(key);
        }
    }

    public bool IsLoopedPlaying(string key) {
        if (loopedInstances.TryGetValue(key, out EventInstance instance)) {
            instance.getPlaybackState(out PLAYBACK_STATE state);
            return state == PLAYBACK_STATE.PLAYING;
        }
        return false;
    }

    public void SetLoopedParameter(string key, string paramName, float value) {
        if (loopedInstances.TryGetValue(key, out EventInstance instance)) {
            instance.setParameterByName(paramName, value);
        }
    }

    public void StopAllLooped(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT) {
        foreach (var kvp in loopedInstances) {
            kvp.Value.stop(stopMode);
            kvp.Value.release();
        }
        loopedInstances.Clear();
    }

    #endregion

    #region Advanced Music Control

    public void PlayMusicWithKey(string key, EventReference eventRef, bool stopOthers = true) {
        if (stopOthers) {
            StopAllMusic();
        }

        if (musicInstances.ContainsKey(key)) {
            Debug.LogWarning($"FmodAudioService: Music with key '{key}' is already playing!");
            return;
        }

        if (eventRef.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(eventRef);
        instance.start();
        musicInstances[key] = instance;
    }

    public void StopMusicWithKey(string key, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT) {
        if (musicInstances.TryGetValue(key, out EventInstance instance)) {
            instance.stop(stopMode);
            instance.release();
            musicInstances.Remove(key);
        }
    }

    public void StopAllMusic(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT) {
        if (_currentMusic.isValid()) {
            _currentMusic.stop(stopMode);
            _currentMusic.release();
        }

        foreach (var kvp in musicInstances) {
            kvp.Value.stop(stopMode);
            kvp.Value.release();
        }
        musicInstances.Clear();
    }

    public void SetMusicParameter(string key, string paramName, float value) {
        if (musicInstances.TryGetValue(key, out EventInstance instance)) {
            instance.setParameterByName(paramName, value);
        }
    }

    public void SetCurrentMusicParameter(string paramName, float value) {
        if (_currentMusic.isValid()) {
            _currentMusic.setParameterByName(paramName, value);
        }
    }

    #endregion

    #region Global Parameters

    public void SetGlobalParameter(string paramName, float value) {
        RuntimeManager.StudioSystem.setParameterByName(paramName, value);
    }

    public float GetGlobalParameter(string paramName) {
        RuntimeManager.StudioSystem.getParameterByName(paramName, out float value);
        return value;
    }

    #endregion

    #region Bus Control

    public float GetBusVolume(AudioChannelType channel) {
        return GetVolume(channel);
    }

    public void SetBusVolume(AudioChannelType channel, float volume) {
        SetVolume(channel, volume);
    }

    public void SetBusMute(AudioChannelType channel, bool mute) {
        if (!buses.TryGetValue(channel, out var bus)) {
            Debug.LogError($"Bus for channel {channel} not found!");
            return;
        }
        bus.setMute(mute);
    }

    public void SetBusPaused(AudioChannelType channel, bool paused) {
        if (!buses.TryGetValue(channel, out var bus)) {
            Debug.LogError($"Bus for channel {channel} not found!");
            return;
        }
        bus.setPaused(paused);
    }

    public bool GetBusMute(AudioChannelType channel) {
        if (!buses.TryGetValue(channel, out var bus)) {
            Debug.LogError($"Bus for channel {channel} not found!");
            return false;
        }
        bus.getMute(out bool mute);
        return mute;
    }

    public bool GetBusPaused(AudioChannelType channel) {
        if (!buses.TryGetValue(channel, out var bus)) {
            Debug.LogError($"Bus for channel {channel} not found!");
            return false;
        }
        bus.getPaused(out bool paused);
        return paused;
    }

    #endregion

    #region VCA Control

    public void SetVCAVolume(string vcaPath, float volume) {
        VCA vca = RuntimeManager.GetVCA(vcaPath);
        if (vca.isValid()) {
            vca.setVolume(Mathf.Clamp01(volume));
        } else {
            Debug.LogError($"VCA '{vcaPath}' not found!");
        }
    }

    public float GetVCAVolume(string vcaPath) {
        VCA vca = RuntimeManager.GetVCA(vcaPath);
        if (vca.isValid()) {
            vca.getVolume(out float volume);
            return volume;
        }
        Debug.LogError($"VCA '{vcaPath}' not found!");
        return 0f;
    }

    #endregion

    #region Snapshots

    public EventInstance StartSnapshot(EventReference snapshotRef) {
        if (snapshotRef.IsNull) return default;

        EventInstance instance = RuntimeManager.CreateInstance(snapshotRef);
        instance.start();
        return instance;
    }

    public void StopSnapshot(EventInstance instance, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT) {
        if (instance.isValid()) {
            instance.stop(stopMode);
            instance.release();
        }
    }

    #endregion

    #region  Utility

    public bool EventExists(EventReference eventRef) {
        if (eventRef.IsNull) return false;

        if (!eventDescriptionCache.TryGetValue(eventRef, out EventDescription description)) {
            description = RuntimeManager.GetEventDescription(eventRef);
            eventDescriptionCache[eventRef] = description;
        }

        return description.isValid();
    }

    public int GetEventLength(EventReference eventRef) {
        EventDescription description = RuntimeManager.GetEventDescription(eventRef);
        if (description.isValid()) {
            description.getLength(out int length);
            return length;
        }
        return 0;
    }

    public int GetActiveLoopedCount() {
        return loopedInstances.Count;
    }

    public int GetActiveMusicCount() {
        return musicInstances.Count;
    }

    public List<string> GetActiveLoopedKeys() {
        return new List<string>(loopedInstances.Keys);
    }

    #endregion

    #region Cleanup

    public void ClearAll() {
        StopAllLooped(FMOD.Studio.STOP_MODE.IMMEDIATE);
        StopAllMusic(FMOD.Studio.STOP_MODE.IMMEDIATE);
        eventDescriptionCache.Clear();
    }

    public void Dispose() {
        ClearAll();
    }

    #endregion

    #region Private Helpers

    private bool IsSameEvent(EventReference a, EventReference b) {
        return a.Guid == b.Guid && a.Guid != default;
    }

    #endregion
}

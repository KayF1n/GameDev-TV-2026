using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioStateConfig", menuName = "Audio/FMODMusicConfig")]
public class AudioStateConfig : ScriptableObject {
    [Serializable]
    public class MusicEntry {
        public MusicPlaylist playlist;
        public FMODUnity.EventReference eventReference;
    }

    public List<MusicEntry> musicEvents = new();

    private Dictionary<MusicPlaylist, FMODUnity.EventReference> _musicCache;

    public FMODUnity.EventReference GetMusicEvent(MusicPlaylist playlist) {
        if (_musicCache == null) {
            _musicCache = new Dictionary<MusicPlaylist, FMODUnity.EventReference>();
            foreach (var entry in musicEvents) {
                _musicCache[entry.playlist] = entry.eventReference;
            }
        }

        return _musicCache.TryGetValue(playlist, out var eventRef)
            ? eventRef
            : default;
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveLoadService : ISaveLoadService {
    private readonly List<IDataLoader> _loaders = new();
    private readonly List<IDataSaveable> _saveables = new();

    public SaveLoadService(IEnumerable<IDataLoader> loaders, IEnumerable<IDataSaveable> saveables) {
        _loaders = loaders.ToList();
        _saveables = saveables.ToList();
    }

    public void RegisterReceiver(IDataLoader receiver) {
        if (!_loaders.Contains(receiver)) {
            _loaders.Add(receiver);
        }
    }

    public void RegisterSubmitter(IDataSaveable submitter) {
        if (!_saveables.Contains(submitter)) {
            _saveables.Add(submitter);
        }
    }

    public void LoadAll() {
        int succeeded = 0;
        foreach (var loader in _loaders) {
            try {
                loader.Load();
                succeeded++;
            } catch (Exception ex) {
                Debug.LogError($"Failed to load data for key '{loader}': {ex.Message}");
            }
        }
        Debug.Log($"Loaded {succeeded} resources");
    }

    public void SaveAll() {
        int succeeded = 0;
        foreach (var saveable in _saveables) {
            try {
                saveable.Save();
                succeeded++;
            } catch (Exception ex) {
                Debug.LogError($"Failed to save data for '{saveable}': {ex.Message}");
            }
        }
        Debug.Log($"Saved {succeeded} resources");
    }
}
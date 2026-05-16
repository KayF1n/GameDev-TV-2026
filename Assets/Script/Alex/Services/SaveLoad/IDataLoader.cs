using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public enum DataSourceType {
    PlayerPrefs,
    Resources,
    FileSystem
}

public interface ISaveLoadService {
    void LoadAll();
    void SaveAll();
}

public interface IDataLoader {
    void Load();
}

public interface IDataSaveable {
    void Save();
}

public interface IDataRepository<T> where T : class {
    T Load();
    void Save(T data);
}

public class JsonDataRepository<T> : IDataRepository<T> where T : class {
    private readonly string _filePath;
    public JsonDataRepository(string filePath) {
        _filePath = Path.Combine(Application.persistentDataPath, filePath);
    }

    public T Load() {
        if (!File.Exists(_filePath)) return null;

        var json = File.ReadAllText(_filePath);
        return JsonUtility.FromJson<T>(json);
    }

    public void Save(T data) {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_filePath, json);
    }
}

public class ResourcesRepository<T> : IDataRepository<T> where T : class {
    private readonly string _path;
    public ResourcesRepository(string path) { _path = path; }

    public T Load() {
        TextAsset textAsset = Resources.Load<TextAsset>(_path);
        if (textAsset != null && !string.IsNullOrEmpty(textAsset.text)) {
            return JsonUtility.FromJson<T>(textAsset.text);
        }
        return default;
    }

    public void Save(T data) {
        Debug.LogWarning("Save not supported for Resources");
    }
}

public class PlayerPrefsRepository<T> : IDataRepository<T> where T : class {
    private readonly string _key;
    public PlayerPrefsRepository(string key) { _key = key; }
    public T Load() {
        string json = PlayerPrefs.GetString(_key, "");
        return string.IsNullOrEmpty(json) ? default : JsonUtility.FromJson<T>(json);
    }
    public void Save(T data) {
        PlayerPrefs.SetString(_key, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class DataSourceAttribute : Attribute {
    public DataSourceType SourceType { get; }
    public string Key { get; }

    public DataSourceAttribute(DataSourceType sourceType, string key) {
        SourceType = sourceType;
        Key = key;
    }
}
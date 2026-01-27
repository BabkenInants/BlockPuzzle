using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Core;
using Saves;

namespace Managers
{
    public class SavesManager : MonoBehaviour
    {
        [SerializeField] private Settings settings;
        [SerializeField] private BlockSpawner blockSpawner;
        private List<ISavable> _savables;
        private string _filePath;
        private bool _gameIsOver;

        private void Awake()
        {
            _filePath = Path.Combine(Application.persistentDataPath, settings.savesFolder);
            if(!Directory.Exists(_filePath)) Directory.CreateDirectory(_filePath);
            _filePath = Path.Combine(_filePath, settings.saveFileName);
        }

        private void Start() => Load();

        private void FindAllSavables()
        {
            if (_savables != null) return;
            _savables = new List<ISavable>();
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (mb is ISavable savable)
                    _savables.Add(savable);
        }

        private void OnEnable()
        {
            GameEvents.SaveGame += Save;
            GameEvents.LoadGame += Load;
            GameEvents.OnGameOver += HandleGameOver;
            GameEvents.SaveGameForRestart += SaveForRestart;
        }

        private void OnDisable()
        {
            GameEvents.SaveGame -= Save;
            GameEvents.LoadGame -= Load;
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.SaveGameForRestart -= SaveForRestart;
        }

        private void SaveForRestart()
        {
            _gameIsOver = true;
            Save();
        }

        private void HandleGameOver() => _gameIsOver = true;

        private void Save()
        {
            FindAllSavables();
            if (_savables == null)
            {
                Debug.LogError("No savables found");
                return;
            }
            var saveData = new SaveData { GameIsOver = _gameIsOver };
            foreach (ISavable savable in _savables)
                savable.Save(saveData);
            string jsonString;
            try
            {
                jsonString = JsonUtility.ToJson(saveData);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to convert to json in save func: " + e);
                return;
            }
            string tempPath = _filePath + ".tmp";
            try
            {
                File.WriteAllText(tempPath, jsonString);
                if (File.Exists(_filePath))
                    File.Delete(_filePath);
                File.Move(tempPath, _filePath);
                Debug.Log("Successfully saved");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save to json file" + e);
            }
        }

        private void Load()
        {
            FindAllSavables();
            if (!File.Exists(_filePath))
            {
                Debug.LogError("No saves found");
                blockSpawner?.SpawnBlocks();
                return;
            }
            if (_savables == null)
            {
                Debug.LogError("No savables found");
                blockSpawner?.SpawnBlocks();
                return;
            }

            string jsonString;
            try
            {
                jsonString = File.ReadAllText(_filePath);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to read from file" + e);
                blockSpawner?.SpawnBlocks();
                return;
            }
        
            SaveData saveData;
            try
            {
                saveData = JsonUtility.FromJson<SaveData>(jsonString);
                if (saveData == null)
                {
                    Debug.LogError("Failed to read data");
                    blockSpawner?.SpawnBlocks();
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load save: " + e);
                blockSpawner?.SpawnBlocks();
                return;
            }
            foreach (ISavable savable in _savables)
                savable.Load(saveData);
            blockSpawner?.SpawnBlocks();
            Debug.Log("Loaded save");
        }
    }
}

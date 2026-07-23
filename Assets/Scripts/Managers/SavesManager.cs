using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Core;
using Saves;
using UnityEngine.SceneManagement;
using YG;
using YG.Insides;

namespace Managers
{
    public class SavesManager : MonoBehaviour
    {
        [SerializeField] private Settings settings;
        [SerializeField] private BlockSpawner blockSpawner;
        private List<ISavable> _savables;
        private bool _gameIsOver;

        private void Start() => Load();

        private void FindAllSavables()
        {
            if (_savables != null) return;
            _savables = new List<ISavable>();
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (mb is ISavable savable)
                    _savables.Add(savable);
        }

        #region Events
        
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

        #endregion

        #region Save/Load

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

            YG2.saves.json = jsonString;
            YG2.SaveProgress();
        }

        private void Load()
        {
            SaveData saveData = null;
            FindAllSavables();
            YGInsides.LoadProgress();
            if (string.IsNullOrEmpty(YG2.saves.json))
            {
                Debug.LogError("No saves found");
                saveData = new SaveData { GameIsOver = true };
            }
            
            if (_savables == null)
            {
                Debug.LogError("No savables found");
                blockSpawner?.SpawnBlocks();
                return;
            }

            if (saveData == null)
            {
                string jsonString = YG2.saves.json;
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
            }

            foreach (ISavable savable in _savables)
                savable.Load(saveData);
            blockSpawner?.SpawnBlocks();
        }
        
        #endregion
    }
}

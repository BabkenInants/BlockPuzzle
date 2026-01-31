using System.Collections;
using Core;
using Saves;
using Tutorial;
using UnityEngine;

namespace Managers
{
    public class TutorialManager : MonoBehaviour, ISavable
    {
        [SerializeField] private TutorialExample[] examples;
        [SerializeField] private Transform firstCell;
        private bool _nextExample;
        private bool _completedTutorial;

        private void NextExample() => _nextExample = true;
        
        private IEnumerator StartTutorial()
        {
            GameEvents.RaiseStartTutorial();
            yield return null;
            foreach (TutorialExample example in examples)
            {
                example.firstCellPosition = firstCell.position;
                GameEvents.RaiseLoadTutorialExample(example);
                while (!_nextExample) yield return null;
                _nextExample = false;
            }
            GameEvents.RaiseFinishTutorial();
            _completedTutorial = true;
            GameEvents.RaiseSaveGame();
        }

        private void OnEnable()
        {
            GameEvents.OnTutorialExampleCompleted += NextExample;
        }

        private void OnDisable()
        {
            GameEvents.OnTutorialExampleCompleted -= NextExample;
        }

        public void Save(SaveData saveData)
        {
            if(!_completedTutorial) return;
            saveData.CompletedTutorial = _completedTutorial;
        }

        public void Load(SaveData saveData)
        {
            _completedTutorial = saveData.CompletedTutorial;
            if(!saveData.CompletedTutorial) StartCoroutine(StartTutorial());
        }
    }
}
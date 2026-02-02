using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Saves;

namespace Managers
{
    public class AudioManager : MonoBehaviour, ISavable
    {
        private bool _sfxOn = true;
        private Queue<AudioSource> _audioSourcesPool = new Queue<AudioSource>();
        
        private void PlaySfx(AudioClip clip)
        {
            if(!_sfxOn || !clip) return;
            StartCoroutine(PlayAndEnqueueAtTheEnd(GetFreeSource(), clip));
        }

        #region Object Pooling

        private IEnumerator PlayAndEnqueueAtTheEnd(AudioSource source, AudioClip clip)
        { 
            source.gameObject.SetActive(true);
            source.clip = clip;
            source.Play();
            yield return new WaitForSeconds(source.clip.length + .1f);
            source.clip = null;
            source.gameObject.SetActive(false);
            _audioSourcesPool.Enqueue(source);
        }
        
        private void AddSource()
        {
            var sourceObj = new GameObject("SFX" + _audioSourcesPool.Count);
            sourceObj.transform.SetParent(transform);
            var source = sourceObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.Stop();
            sourceObj.SetActive(false);
            _audioSourcesPool.Enqueue(source);
        }

        private AudioSource GetFreeSource()
        {
            if(_audioSourcesPool.Count == 0) AddSource();
            AudioSource obj = _audioSourcesPool.Dequeue();
            return obj;
        }

        #endregion

        #region Events

        private void SetSfxState(bool state) => _sfxOn = state;

        private void OnEnable()
        {
            GameEvents.SetSfxState += SetSfxState;
            GameEvents.PlaySfx += PlaySfx;
        }

        private void OnDisable()
        {
            GameEvents.SetSfxState -= SetSfxState;
            GameEvents.PlaySfx -= PlaySfx;
        }
        
        #endregion
        
        #region Saves
        
        public void Save(SaveData data){}

        public void Load(SaveData data) => _sfxOn = data.SfxIsOn;
        
        #endregion
    }
}

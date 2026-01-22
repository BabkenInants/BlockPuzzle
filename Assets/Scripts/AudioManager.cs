using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public bool sfxOn { get; private set; } = true;
    
    private void PlaySfx(AudioClip clip)
    {
        if(!sfxOn || !clip) return;
    
        var temp = new GameObject(clip.name + "_SFX");
        temp.transform.SetParent(transform);
    
        var tempSource = temp.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.loop = false;
        tempSource.playOnAwake = false;
        tempSource.Play();
    
        temp.AddComponent<AutoDestroyer>().lifeTime = clip.length + 5f;
    }
    
    private void SetSfxState(bool state) => sfxOn = state;

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
}

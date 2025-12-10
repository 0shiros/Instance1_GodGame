using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    private float timeBetweenMusics;
    
    public static AudioManager instance;

    void Awake()
    {
        if (instance != null) {
            instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
        
        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = s.audioMixerGroup;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake = s.playOnAwake;
            
        }
    }
    
    #region music
    
    private void Start() {
        PlaySound("Music");
    }
    private void Update() {
        timeBetweenMusics += Time.deltaTime;
        if (timeBetweenMusics >= GetLength("Music")) {
            PlaySound("Music");
        }
    }
    #endregion
    
    #region sfx
    public void PlaySound (string pName) {
        try {
            Sound s = Array.Find(sounds, sound => sound.name == pName);
            s.source.Play();
        }
        catch {
            Debug.LogWarning(pName + " sound not found");
        }
    }

    public void PlayOverlap(string pName) {
        try {
            Sound s = Array.Find(sounds, sound => sound.name == pName);
            s.source.PlayOneShot(s.source.clip, s.source.volume);
        }
        catch {
            Debug.LogWarning(pName + " sound not found");
        }
    }

    public void PlayDelay(string pName,float pDelay) {
        StartCoroutine(PlayDelayCoroutine(pName, pDelay));
    }

    private IEnumerator PlayDelayCoroutine(string pName, float pDelay) {
        yield return new WaitForSeconds(pDelay);
        PlayOverlap(pName);
    }

    public void StopSound(string pName) {
        try {
            Sound s = Array.Find(sounds, sound => sound.name == pName);
            s.source.Stop();
        }
        catch {
            Debug.LogWarning(pName + " sound not found");
        }
    }

    public void stopAllSounds() {
        foreach (Sound s in sounds)
        {
            if (s.source.isPlaying)
            {
                s.source.Stop();
            }
        }
    }
    #endregion
    
    #region effects
    public void ChangeVolume(string pName, float pVolume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == pName);
        s.source.volume = pVolume;
    }
    
    public void ChangePitch(string pName, float pPitch) { //pas utilisé
        Sound s = Array.Find(sounds, sound => sound.name == pName);
        s.source.pitch = pPitch;
    }
    
    public void FadeVolume(string pName, float pDuration, float pTargetVolume) {
        StartCoroutine(FadeVolumeCoroutine(pName, pDuration, pTargetVolume));
    }

    private IEnumerator FadeVolumeCoroutine(string pName, float pDuration, float pTargetVolume) {
        try {
            Sound s = Array.Find(sounds, sound => sound.name == pName);
            while (Mathf.Approximately(s.source.volume, pTargetVolume)) {
                s.source.DOFade(pTargetVolume, pDuration);
            }
        }
        catch {
            Debug.LogWarning(pName + " sound not found");
        }
        yield return null;
    }
    #endregion
    
    #region parameters
    public float GetLength(string pName) {
        Sound s = Array.Find(sounds, sound => sound.name == pName);
        return s.source.clip.length;
    }
    #endregion

    //placer dans nimporte quel scrypt avec le bon nom dans les "" pour jouer un son
    //AudioManager.instance.X(Y);
    //X est le nom de la fonction appelée
    //Y sont les paramètres de X
}

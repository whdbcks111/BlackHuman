using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField]
    public AudioSource BgmAudioSource, SeAudioSource;
    [SerializeField]
    private BackgroundMusic[] _musics;
    [SerializeField]
    private SerializableDictionary<string, AudioClip> _sounds;
    private int _index = 0;

    private void Awake() {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update() {
        if(_musics.Length == 0) return;
        var curMusic = _musics[_index % _musics.Length];
        if(BgmAudioSource.clip != curMusic.OpenClip && BgmAudioSource.clip != curMusic.LoopClip) 
        {
            BgmAudioSource.clip = curMusic.OpenClip;
            BgmAudioSource.loop = false;
            BgmAudioSource.Play();
        }
        else if(BgmAudioSource.clip == curMusic.OpenClip && !BgmAudioSource.isPlaying)
        {
            BgmAudioSource.clip = curMusic.LoopClip;
            BgmAudioSource.loop = true;
            BgmAudioSource.Play();
        }
    }

    public void NextMusic() 
    {
        if(_musics.Length == 0) return;
        _index = (_index + 1) % _musics.Length;
    }

    public void PlayOneShot(string name, float vol = 1f)
    {
        SeAudioSource.PlayOneShot(_sounds[name], vol);
    }

    [Serializable]
    public class BackgroundMusic 
    {
        public AudioClip OpenClip, LoopClip;
    }
}
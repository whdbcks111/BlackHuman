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
    private SerializableDictionary<string, BackgroundMusic> _musics;
    [SerializeField]
    private SerializableDictionary<string, AudioClip> _sounds;
    private string _curMusicName = "";

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if(_musics.Count == 0) return;
        if(!_musics.ContainsKey(_curMusicName)) return;
        var curMusic = _musics[_curMusicName];
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

    public void ResetMusic()
    {
        BgmAudioSource.Stop();
        BgmAudioSource.Play();
    }

    public void PlayMusic(string name) 
    {
        if(_musics.ContainsKey(name)) _curMusicName = name;
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMgr : MonoSingleton<AudioMgr>
{
    [Header("音频设置")]
    public AudioSource musicSource; // 背景音乐播放器
    public AudioSource soundSource; // 音效播放器

    [Header("音量设置")]
    [Range(0, 1)]
    public float musicVolume = 1f; // 音乐音量
    [Range(0, 1)]
    public float soundVolume = 1f; // 音效音量

    public Dictionary<string, AudioClip> sounds = new();

    public void Init()
    {
        // 初始化音频
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        if (soundSource == null)
            soundSource = gameObject.AddComponent<AudioSource>();

        // 设置音频源属性
        musicSource.loop = true;
        soundSource.loop = false;

        UpdateVolume();
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="music"></param>
    public void PlayMusic(AudioClip music)
    {
        if (music == null || musicSource == null) return;

        musicSource.clip = music;
        musicSource.Play();
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="name"></param>
    public void PlayMusic(string name)
    {
        if (sounds.ContainsKey(name))
        {
            PlayMusic(sounds[name]);
        }
        else
        {
            sounds[name] = Resources.Load<AudioClip>("Audios/" + name);
            PlayMusic(sounds[name]);
        }
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="sound"></param>
    public void PlaySound(AudioClip sound)
    {
        if (sound == null || soundSource == null) return;

        soundSource.PlayOneShot(sound, soundVolume);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="name"></param>
    public void PlaySound(string name)
    {
        if (sounds.ContainsKey(name))
        {
            PlaySound(sounds[name]);
        }
        else
        {
            sounds[name] = Resources.Load<AudioClip>("Audios/" + name);
            PlaySound(sounds[name]);
        }
    }

    public void PlayFootstep()
    {
        if (CharacterMgr.Player().status == CharacterStatus.Move)
        {
            PlaySound("脚步");
        }
    }

    public void StopFootstepSound()
    {
        if (soundSource != null)
            soundSource.Stop();
    }

    /// <summary>
    /// 更新音量
    /// </summary>
    public void UpdateVolume()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;
        if (soundSource != null)
            soundSource.volume = soundVolume;
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }
}

using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip dialogueBGM;
    public AudioClip dumplingGameBGM;
    public AudioClip buttonClickSFX;
    public AudioClip itemPlaceSFX;
    public AudioClip dumplingCompleteSFX;

    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioClips();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudioClips()
    {
        if (dialogueBGM != null) audioClips["dialogueBGM"] = dialogueBGM;
        if (dumplingGameBGM != null) audioClips["dumplingGameBGM"] = dumplingGameBGM;
        if (buttonClickSFX != null) audioClips["buttonClick"] = buttonClickSFX;
        if (itemPlaceSFX != null) audioClips["itemPlace"] = itemPlaceSFX;
        if (dumplingCompleteSFX != null) audioClips["dumplingComplete"] = dumplingCompleteSFX;
    }

    public void PlayBGM(string clipName, bool loop = true)
    {
        if (bgmSource == null) return;

        if (audioClips.ContainsKey(clipName))
        {
            bgmSource.clip = audioClips[clipName];
            bgmSource.loop = loop;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning("Audio clip not found: " + clipName);
        }
    }

    public void PlaySFX(string clipName)
    {
        if (sfxSource == null) return;

        if (audioClips.ContainsKey(clipName))
        {
            sfxSource.PlayOneShot(audioClips[clipName]);
        }
        else
        {
            Debug.LogWarning("Audio clip not found: " + clipName);
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = Mathf.Clamp01(volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }
}

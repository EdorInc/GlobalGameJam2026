using FMOD;
using FMODUnity;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using UnityEngine;


public enum AudioType
{
    Music,
    SFX,
    PickObject
}


public class AudioManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AudioManager");
                _instance = go.AddComponent<AudioManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }


    private FMOD.Studio.EventInstance currentMusicInstance;
    private FMOD.Studio.EventInstance currentSFXInstance;

    [SerializeField]
    private List<SoundAsset> soundList = new List<SoundAsset>();

    private Dictionary<AudioType, SoundAsset> soundDictionary;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        soundDictionary = new Dictionary<AudioType, SoundAsset>();
        foreach (SoundAsset sound in soundList)
        {
            if (!soundDictionary.ContainsKey(sound.audioType))
                soundDictionary.Add(sound.audioType, sound);
            else
                UnityEngine.Debug.LogWarning("AudioManager: soundDictionary already contains " + sound.audioType);
        }
    }

    public void StopMusic()
    {
        if (currentMusicInstance.isValid())
        {
            currentMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentMusicInstance.release();
        }
        currentMusicInstance = default;
    }

    public void StopSFX()
    {
        if (currentSFXInstance.isValid())
        {
            currentSFXInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentSFXInstance.release();
        }
    }

    public void PlayMusic(AudioType audioType, float volume = 1)
    {
        StopMusic();
        if (soundDictionary.ContainsKey(audioType))
        {
            SoundAsset sound = soundDictionary[audioType];
            if (volume >= 0)
            {
                currentMusicInstance = FMODUnity.RuntimeManager.CreateInstance(sound.eventReference);
                currentMusicInstance.setParameterByName("Volume", volume);
                currentMusicInstance.start();
            }
            else
                UnityEngine.Debug.LogWarning("PlayMusic: Music is null or volume is less/equal than 0");
        }
        else
            UnityEngine.Debug.LogWarning("PlayMusic: soundDictionary doesn't contain " + audioType);
    }

    public void PlayAmbience(AudioType type, float volume = 1)
    {
        if (soundDictionary.ContainsKey(type))
        {
            SoundAsset sound = soundDictionary[type];
            if (volume >= 0)
            {
                FMOD.Studio.EventInstance ambienceInstance = RuntimeManager.CreateInstance(sound.eventReference);
                ambienceInstance.setParameterByName("Volume", volume);
                ambienceInstance.start();
                ambienceInstance.release();
            }
            else
                UnityEngine.Debug.LogWarning("PlayAmbience: ambience is null or volume is less/equal than 0");
        }
        else
            UnityEngine.Debug.LogWarning("PlayAmbience: soundDictionary doesn't contain " + type);
    }


    public void PlaySFX(AudioType type, float volume = 1)
    {
        StopSFX();
        if (soundDictionary.ContainsKey(type))
        {
            SoundAsset sound = soundDictionary[type];
            if (volume >= 0)
            {
                currentSFXInstance = RuntimeManager.CreateInstance(sound.eventReference);
                currentSFXInstance.setParameterByName("Volume", volume);
                currentSFXInstance.start();
            }
            else
                UnityEngine.Debug.LogWarning("PlaySFX: sfx is null or volume is less/equal than 0");
        }
        else
            UnityEngine.Debug.LogWarning("PlaySFX: soundDictionary doesn't contain " + type);
    }
}
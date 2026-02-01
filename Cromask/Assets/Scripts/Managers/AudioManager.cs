using FMOD;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;


public enum AudioType
{
    Music,
    SFX,
    Respawn,
    Door,
    Bridge,
    Charge,
    Equip,
    Unequip,
    Footstep,
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
    private FMOD.Studio.EventInstance currentFootstepInstance;
    private FMOD.Studio.EventInstance currentFootstep2Instance;

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

    private void Start()
    {
        PlayMusic(AudioType.Music); 
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

    public void ChangeParameterMusic(string label, int value)
    {
        if (currentMusicInstance.isValid())
        {
            currentMusicInstance.setParameterByName(label, value);
        }
    }

    public void UpdateMaskParameter(Mask playerOneMask, Mask playerTwoMask)
    {
        UnityEngine.Debug.Log(playerOneMask);
        UnityEngine.Debug.Log(playerTwoMask);
        switch ((playerOneMask, playerTwoMask))
        {
            case (Mask.Unmasked,Mask.Unmasked):
                ChangeParameterMusic("Mascaras", 0);
                break;
            case (Mask.Blue, Mask.Unmasked):
            case (Mask.Unmasked, Mask.Blue):
                ChangeParameterMusic("Mascaras", 1);
                break;
            case (Mask.Green, Mask.Unmasked):
            case (Mask.Unmasked, Mask.Green):
                ChangeParameterMusic("Mascaras", 2);
                break;
            case (Mask.Red, Mask.Unmasked):
            case (Mask.Unmasked, Mask.Red):
                ChangeParameterMusic("Mascaras", 3);
                break;
            case (Mask.Green, Mask.Blue):
            case (Mask.Blue, Mask.Green):
                ChangeParameterMusic("Mascaras", 4);
                break;
            case (Mask.Green, Mask.Red):
            case (Mask.Red, Mask.Green):
                ChangeParameterMusic("Mascaras", 5);
                break;
            case (Mask.Blue, Mask.Red):
            case (Mask.Red, Mask.Blue):
                ChangeParameterMusic("Mascaras", 6);
                break;
            default:
                ChangeParameterMusic("Mascaras", 7);
                break;
        }
    }
    public void StopSFX()
    {
        if (currentSFXInstance.isValid())
        {
            currentSFXInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentSFXInstance.release();
        }
    }

    public void StopFootstep()
    {
        if (currentFootstepInstance.isValid())
        {
            currentFootstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentFootstepInstance.release();
        }
    }

    public void StopFootstep2()
    {
        if (currentFootstep2Instance.isValid())
        {
            currentFootstep2Instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentFootstep2Instance.release();
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

    public void PlaySFX(AudioType type, ATTRIBUTES_3D attributes, float volume = 1)
    {
        StopSFX();
        if (soundDictionary.ContainsKey(type))
        {

            SoundAsset sound = soundDictionary[type];
            if (volume >= 0)
            {
                currentSFXInstance = RuntimeManager.CreateInstance(sound.eventReference);
                currentSFXInstance.set3DAttributes(attributes);
                currentSFXInstance.setParameterByName("Volume", volume);
                currentSFXInstance.start();
            }
            else
                UnityEngine.Debug.LogWarning("PlaySFX: sfx is null or volume is less/equal than 0");
        }
        else
            UnityEngine.Debug.LogWarning("PlaySFX: soundDictionary doesn't contain " + type);
    }


    public void PlayFootstep(AudioType type, ATTRIBUTES_3D attributes, float volume = 1)
    {
        StopFootstep();
        if (soundDictionary.ContainsKey(type))
        {

            SoundAsset sound = soundDictionary[type];
            if (volume >= 0)
            {
                currentFootstepInstance.setParameterByName("Pan", -1f); // Player 1
                currentFootstepInstance = RuntimeManager.CreateInstance(sound.eventReference);
                currentFootstepInstance.set3DAttributes(attributes);
                currentFootstepInstance.setParameterByName("Volume", volume);
                currentFootstepInstance.start();
            }
            else
                UnityEngine.Debug.LogWarning("PlaySFX: sfx is null or volume is less/equal than 0");
        }
        else
            UnityEngine.Debug.LogWarning("PlaySFX: soundDictionary doesn't contain " + type);
    }


    public void PlayFootstep2(AudioType type, ATTRIBUTES_3D attributes, float volume = 1)
    {
        StopFootstep2();
        if (soundDictionary.ContainsKey(type))
        {

            SoundAsset sound = soundDictionary[type];
            if (volume >= 0)
            {
                currentFootstep2Instance.setParameterByName("Pan", 1f); // Player 1
                currentFootstep2Instance = RuntimeManager.CreateInstance(sound.eventReference);
                currentFootstep2Instance.set3DAttributes(attributes);
                currentFootstep2Instance.setParameterByName("Volume", volume);
                currentFootstep2Instance.start();
            }
            else
                UnityEngine.Debug.LogWarning("PlaySFX: sfx is null or volume is less/equal than 0");
        }
        else
            UnityEngine.Debug.LogWarning("PlaySFX: soundDictionary doesn't contain " + type);
    }

    public void PlaySFXOneShotAttached(AudioType type, GameObject gameObject,float volume = 1)
    {
        if (soundDictionary.ContainsKey(type))
        {
            SoundAsset sound = soundDictionary[type];
            if (volume >= 0)
            {
                RuntimeManager.PlayOneShotAttached(sound.eventReference, gameObject);
            }
            else
                UnityEngine.Debug.LogWarning("PlaySFX: sfx is null or volume is less/equal than 0");
        }
        else
            UnityEngine.Debug.LogWarning("PlaySFX: soundDictionary doesn't contain " + type);
    }
}
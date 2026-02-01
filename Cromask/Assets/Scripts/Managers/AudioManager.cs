using FMOD;
using FMODUnity;
using System;
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
    PickObject,
    Throw
}

public class AudioManager : MonoBehaviour
{
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

    [SerializeField]
    private List<SoundAsset> soundList = new List<SoundAsset>();

    private Dictionary<AudioType, SoundAsset> soundDictionary;

    // Lista para guardar todas las instancias SFX/ambience/footsteps activas
    private List<FMOD.Studio.EventInstance> activeSFX = new List<FMOD.Studio.EventInstance>();

    // Opcional: limitar número máximo de SFX simultáneos
    [SerializeField]
    private int maxSimultaneousSFX = 64;

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

    private void Update()
    {
        // Limpieza de instancias que han terminado de reproducirse
        for (int i = activeSFX.Count - 1; i >= 0; i--)
        {
            var inst = activeSFX[i];
            if (!inst.isValid())
            {
                activeSFX.RemoveAt(i);
                continue;
            }
            FMOD.Studio.PLAYBACK_STATE state;
            inst.getPlaybackState(out state);
            if (state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                inst.release();
                activeSFX.RemoveAt(i);
            }
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

    public void ChangeParameterMusic(string label, int value)
    {
        if (currentMusicInstance.isValid())
        {
            currentMusicInstance.setParameterByName(label, value);
        }
    }

    // ... (tu UpdateMaskParameter lo dejo igual)
    public void UpdateMaskParameter(Mask playerOneMask, Mask playerTwoMask)
    {
        // copia exactamente tu switch...
        UnityEngine.Debug.Log(playerOneMask);
        UnityEngine.Debug.Log(playerTwoMask);
        switch ((playerOneMask, playerTwoMask))
        {
            case (Mask.Unmasked, Mask.Unmasked):
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

    // Detiene y libera todas las SFX activas
    public void StopSFX()
    {
        for (int i = activeSFX.Count - 1; i >= 0; i--)
        {
            var inst = activeSFX[i];
            if (inst.isValid())
            {
                inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                inst.release();
            }
        }
        activeSFX.Clear();
    }

    public void StopFootstep()
    {
        for (int i = activeSFX.Count - 1; i >= 0; i--)
        {
            var inst = activeSFX[i];
            if (inst.isValid())
            {
                inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                inst.release();
            }
        }
        activeSFX.Clear();
    }

    public void StopFootstep2()
    {
        for (int i = activeSFX.Count - 1; i >= 0; i--)
        {
            var inst = activeSFX[i];
            if (inst.isValid())
            {
                inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                inst.release();
            }
        }
        activeSFX.Clear();
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
        //else
          //  UnityEngine.DebugWarning("PlayMusic: soundDictionary doesn't contain " + audioType);
    }

    public void PlayAmbience(AudioType type, float volume = 1)
    {
        if (soundDictionary.ContainsKey(type))
        {
            SoundAsset sound = soundDictionary[type];
            if (volume >= 0)
            {
                var ambienceInstance = RuntimeManager.CreateInstance(sound.eventReference);
                ambienceInstance.setParameterByName("Volume", volume);
                ambienceInstance.start();

                // Añadimos a activeSFX para que Update() lo libere cuando termine.
                RegisterSFXInstance(ambienceInstance);
            }
            else
                UnityEngine.Debug.LogWarning("PlayAmbience: ambience is null or volume is less/equal than 0");
        }
        else
            UnityEngine.Debug.LogWarning("PlayAmbience: soundDictionary doesn't contain " + type);
    }

    public void PlaySFX(AudioType type, ATTRIBUTES_3D attributes, float volume = 1)
    {
        if (soundDictionary.ContainsKey(type))
        {
            SoundAsset sound = soundDictionary[type];
            if (volume >= 0)
            {
                var inst = RuntimeManager.CreateInstance(sound.eventReference);
                inst.set3DAttributes(attributes);
                inst.setParameterByName("Volume", volume);
                inst.start();

                RegisterSFXInstance(inst);
            }
            else
                UnityEngine.Debug.LogWarning("PlaySFX: sfx is null or volume is less/equal than 0");
        }
        else
            UnityEngine.Debug.LogWarning("PlaySFX: soundDictionary doesn't contain " + type);
    }

    public void PlayFootstep(AudioType type, ATTRIBUTES_3D attributes, float volume = 1)
    {
        if (soundDictionary.ContainsKey(type))
        {
            SoundAsset sound = soundDictionary[type];
            if (volume >= 0)
            {
                var inst = RuntimeManager.CreateInstance(sound.eventReference);
                inst.set3DAttributes(attributes);
                inst.setParameterByName("Pan", -1f); // Player 1 pan
                inst.setParameterByName("Volume", volume);
                inst.start();

                RegisterSFXInstance(inst);
            }
            else
                UnityEngine.Debug.LogWarning("PlayFootstep: sfx is null or volume is less/equal than 0");
        }
        else
            UnityEngine.Debug.LogWarning("PlayFootstep: soundDictionary doesn't contain " + type);
    }

    public void PlayFootstep2(AudioType type, ATTRIBUTES_3D attributes, float volume = 1)
    {
        if (soundDictionary.ContainsKey(type))
        {
            SoundAsset sound = soundDictionary[type];
            if (volume >= 0)
            {
                var inst = RuntimeManager.CreateInstance(sound.eventReference);
                inst.set3DAttributes(attributes);
                inst.setParameterByName("Pan", 1f); // Player 2 pan
                inst.setParameterByName("Volume", volume);
                inst.start();

                RegisterSFXInstance(inst);
            }
            else
                UnityEngine.Debug.LogWarning("PlayFootstep2: sfx is null or volume is less/equal than 0");
        }
        else
            UnityEngine.Debug.LogWarning("PlayFootstep2: soundDictionary doesn't contain " + type);
    }

    public void PlaySFXOneShotAttached(AudioType type, GameObject gameObject, float volume = 1)
    {
        if (soundDictionary.ContainsKey(type))
        {
            SoundAsset sound = soundDictionary[type];
            if (volume >= 0)
            {
                // PlayOneShotAttached maneja su propia liberación internamente
                RuntimeManager.PlayOneShotAttached(sound.eventReference, gameObject);
            }
            else
                UnityEngine.Debug.LogWarning("PlaySFXOneShotAttached: sfx is null or volume is less/equal than 0");
        }
        else
            UnityEngine.Debug.LogWarning("PlaySFXOneShotAttached: soundDictionary doesn't contain " + type);
    }

    // Helper para registrar instancias en la lista y controlar un posible tope
    private void RegisterSFXInstance(FMOD.Studio.EventInstance inst)
    {
        if (!inst.isValid())
            return;

        // Si hay un límite, elimina el más antiguo cuando se exceda
        if (maxSimultaneousSFX > 0 && activeSFX.Count >= maxSimultaneousSFX)
        {
            // detener + liberar el más antiguo
            var oldest = activeSFX[0];
            if (oldest.isValid())
            {
                oldest.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                oldest.release();
            }
            activeSFX.RemoveAt(0);
        }

        activeSFX.Add(inst);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}

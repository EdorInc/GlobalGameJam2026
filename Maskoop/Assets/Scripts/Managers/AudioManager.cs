using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Channels")]
    [SerializeField] private AudioEventChannel generalAudioChannel;
    [SerializeField] private AudioEventChannel musicAudioChannel;

    [Header("SFX")]
    [SerializeField] private AudioSource audioPrefab;
    [SerializeField] private int poolSize = 5;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;

    private List<AudioSource> pool = new List<AudioSource>();

    private SortedList<int, Queue<QueuedSound>> soundQueue = new SortedList<int, Queue<QueuedSound>>();

    private void Awake()
    {
        // Inicializa pool de AudioSources
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource src = Instantiate(audioPrefab, transform);
            src.gameObject.SetActive(false);
            pool.Add(src);
        }
    }


    private void OnEnable()
    {
        if (generalAudioChannel != null)
            generalAudioChannel.OnPlaySound += EnqueueSound;

        if (musicAudioChannel != null)
            musicAudioChannel.OnPlayMusic += PlayMusic;
    }

    private void OnDisable()
    {
        if (generalAudioChannel != null)
            generalAudioChannel.OnPlaySound -= EnqueueSound;

        if (musicAudioChannel != null)
            musicAudioChannel.OnPlayMusic -= PlayMusic;
    }

    //MUSIC
    private void PlayMusic(SoundDefinition music)
    {
        if (musicSource.isPlaying)
        {
            if (musicSource.clip == music.clips[0])
                return; // already playing this track
        }

        musicSource.clip = music.clips[0];
        musicSource.volume = music.volume;
        musicSource.pitch = 1f;
        musicSource.loop = music.loop;
        musicSource.spatialBlend = 0f; // 2D music

        musicSource.Play();
    }


    //SFX
    private void EnqueueSound(SoundDefinition sound, Vector3 position)
    {
        int priority = sound.priority;

        if (!soundQueue.ContainsKey(priority))
            soundQueue[priority] = new Queue<QueuedSound>();

        soundQueue[priority].Enqueue(new QueuedSound(sound, position));

        TryPlayNext();
    }

    private void TryPlayNext()
    {
        AudioSource freeSource = pool.Find(s => !s.isPlaying);
        if (freeSource == null) return;

        foreach (var kvp in soundQueue)
        {
            if (kvp.Value.Count > 0)
            {
                QueuedSound next = kvp.Value.Dequeue();

                freeSource.transform.position = next.position;
                freeSource.clip = next.sound.clips[Random.Range(0, next.sound.clips.Length)];
                freeSource.volume = next.sound.volume;
                freeSource.pitch = Random.Range(next.sound.pitchRange.x, next.sound.pitchRange.y);
                freeSource.priority = next.sound.priority;
                freeSource.spatialBlend = 1f; // 3D

                freeSource.gameObject.SetActive(true);
                freeSource.Play();

                StartCoroutine(DisableAfterPlay(freeSource));
                break;
            }
        }
    }

    private System.Collections.IEnumerator DisableAfterPlay(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        source.gameObject.SetActive(false);

        TryPlayNext();
    }

    private class QueuedSound
    {
        public SoundDefinition sound;
        public Vector3 position;

        public QueuedSound(SoundDefinition s, Vector3 pos)
        {
            sound = s;
            position = pos;
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer musicPlayerInScene { get; private set; }

    public bool play;
    private bool prevPlay;

    public int musicIndex;
    private int prevMusicIndex = -1;

    public AudioClip[] musicClips;
    private AudioSource musicSource { get { if (_musicSource == null) _musicSource = GetComponent<AudioSource>(); return _musicSource; } }
    private AudioSource _musicSource;

    void Awake()
    {
        musicPlayerInScene = this;
        //musicSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (play && (prevPlay != play || musicIndex != prevMusicIndex))
            PlayTune(musicIndex);
        else if (!play && prevPlay)
            Stop();
        prevPlay = play;
    }

    public void PlayTune(int index)
    {
        PlayTune(index, true, 1, 1);
    }
    public void PlayTune(int index, bool loop = true, float volume = 1, float pitch = 1)
    {
        musicIndex = index;

        musicSource.Stop();
        musicSource.loop = loop;
        musicSource.volume = volume;
        musicSource.pitch = pitch;
        musicSource.clip = musicClips[musicIndex];

        Play();

        prevMusicIndex = musicIndex;
    }

    public void SetVolume(float value)
    {
        musicSource.volume = value;
    }
    public void SetPitch(float value)
    {
        musicSource.pitch = value;
    }

    public void Play()
    {
        play = true;
        prevPlay = true;
        musicSource.Play();
    }
    public void Stop()
    {
        play = false;
        prevPlay = false;
        musicSource.Stop();
    }
    public void Pause()
    {
        musicSource.Pause();
    }
    public void UnPause()
    {
        musicSource.UnPause();
    }
}

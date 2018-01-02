using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour 
{
	////////// Variables //////////
	public static AudioManager instance;
	public AudioClip[] effects;
	public AudioClip[] music;
	private AudioSource effectSource;
	private AudioSource musicSource;

	private static float effectVolume = 1.0f, musicVolume = 1.0f, tempVolume = 1.0f;
	public float EffectVolume { get { return effectVolume; } set { effectVolume = value; } }
	public float MusicVolume { get { return musicVolume; } set { musicVolume = value; } }

	////////// Primary Methods //////////
	void Awake()
	{
		DontDestroyOnLoad (this.gameObject);

		if (instance != null && instance != this)
		{
			GameObject.Destroy (this.gameObject);
		}
		else
		{
			instance = this;
		}

		Start ();
	}

	void Start()
	{
		effectSource = this.GetComponents<AudioSource> () [0];
		musicSource = this.GetComponents<AudioSource> () [1];

		//Remove all non-instance audio listeners from the scene.
		foreach (AudioListener listener in GameObject.FindObjectsOfType<AudioListener>())
		{
			if (listener != instance.GetComponent<AudioListener> ()) 
			{
				Destroy (listener);
			}
		}
	}

	void Update()
	{
		if (effectSource.volume != effectVolume) 
		{
			effectSource.volume = effectVolume;
		}
		if (musicSource.volume != musicVolume) 
		{
			musicSource.volume = musicVolume;
		}
	}

	////////// Custom Methods //////////
	public void PlayEffect (string effectName)
	{
		//Debug.Log ("Effect played: " + effectName);
		AudioClip clip = GetClipFromArray (effectName, effects);
		if (clip != null) 
		{
			effectSource.PlayOneShot (clip);
		}
	}

	public void PlayMusic (string musicName)
	{
		AudioClip clip = GetClipFromArray (musicName, music);
		if (clip != null) 
		{
			musicSource.clip = clip;
			musicSource.loop = true;
			musicSource.Play ();
		}
	}

	public void StopMusic()
	{
		musicSource.Stop ();
	}

    public void FadeOutMusic(float duration)
    {
        StartCoroutine(FadeOutMusic_Coroutine(duration));
    }

    public void FadeInMusic(string musicName, float duration)
    {
        PlayMusic(musicName);
        StartCoroutine(FadeInMusic_Coroutine(duration));
    }

    private IEnumerator FadeOutMusic_Coroutine(float duration)
    {
        tempVolume = musicVolume;
        musicSource.volume = tempVolume;
        float t = 0;
        while(t < duration)
        {
            tempVolume = Mathf.Lerp(musicVolume, 0.0f, t / duration);
            musicSource.volume = tempVolume;

            t += Time.deltaTime;
            yield return null;
        }
        StopMusic();
        musicSource.volume = musicVolume;
    }

    private IEnumerator FadeInMusic_Coroutine(float duration)
    {
        tempVolume = 0.0f;
        musicSource.volume = tempVolume;
        float t = 0;
        while (t < duration)
        {
            tempVolume = Mathf.Lerp(0.0f, musicVolume, t / duration);
            musicSource.volume = tempVolume;

            t += Time.deltaTime;
            yield return null;
        }
        musicSource.volume = musicVolume;
    }

	private AudioClip GetClipFromArray(string name, AudioClip[] clipArray)
	{
		for(int i = 0; i < clipArray.Length; i++)
		{
			if (clipArray [i].name == name) 
			{
				return clipArray [i];
			}
		}
		Debug.Log ("The clip " + name + " was not found in the specified array!");
		return null;
	}
}

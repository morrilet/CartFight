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

	private static float effectVolume = 1.0f, musicVolume = 1.0f;
	public float EffectVolume { get { return effectVolume; } set { effectVolume = value; } }
	public float MusicVolume { get { return musicVolume; } set { musicVolume = value; } }

	////////// Primary Methods //////////
	void Awake()
	{
		DontDestroyOnLoad (this.gameObject);

		if (instance != null && instance != this) 
		{
			//Debug.Log ("INST_MVOL: " + MusicVolume);
			Destroy (instance.gameObject);
			instance = this;
		}
		else
		{
			instance = this;
		}

		Start ();
		//Debug.Log ("THIS_MVOL: " + MusicVolume);
	}

	void Start()
	{
		effectSource = this.GetComponents<AudioSource> () [0];
		musicSource = this.GetComponents<AudioSource> () [1];

		//Remove all other audio listeners from the scene.
		foreach (AudioListener listener in GameObject.FindObjectsOfType<AudioListener>()) 
		{
			if (listener != this.GetComponent<AudioListener> ()) 
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

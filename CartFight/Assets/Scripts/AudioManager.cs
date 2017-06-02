using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour 
{
	////////// Variables //////////
	public static AudioManager instance;
	public AudioClip[] effects;
	public AudioClip[] music;
	private AudioSource source;

	////////// Primary Methods //////////
	void Awake()
	{
		if (instance != null && instance != this) 
		{
			Destroy (this.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	void Start()
	{
		source = this.GetComponent<AudioSource> ();

		//Remove all other audio listeners from the scene.
		foreach (AudioListener listener in GameObject.FindObjectsOfType<AudioListener>()) 
		{
			if (listener != this.GetComponent<AudioListener> ()) 
			{
				Destroy (listener);
			}
		}
	}

	////////// Custom Methods //////////
	public void PlayEffect (string effectName)
	{
		AudioClip clip = GetClipFromArray (effectName, effects);
		if (clip != null) 
		{
			source.PlayOneShot (clip);
		}
	}

	public void PlayMusic (string musicName)
	{
		AudioClip clip = GetClipFromArray (musicName, music);
		if (clip != null) 
		{
			source.clip = clip;
			source.loop = true;
			source.Play ();
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

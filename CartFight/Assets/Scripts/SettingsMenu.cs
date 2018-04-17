using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class SettingsMenu : Menu 
{
	private Dropdown resolutionDropdown;
	private Toggle fullscreenToggle;
	private Slider musicVolumeSlider;
	private Text musicVolumeText;
	private Slider effectVolumeSlider;
	private Text effectVolumeText;
	private Button applyButton;
    private Button backButton;

	private Resolution oldResolution, tempResolution;
	private bool oldFullscreen, tempFullscreen;
	private float oldMusicVolume, tempMusicVolume;
	private float oldEffectVolume, tempEffectVolume;

	public override void Start()
	{
		base.Start ();

		//Get all the elements.
		resolutionDropdown = GameObject.Find ("Resolution_Dropdown").GetComponent<Dropdown> ();
		fullscreenToggle = GameObject.Find ("Fullscreen_Toggle").GetComponent<Toggle> ();
		musicVolumeSlider = GameObject.Find ("MusicVolume_Slider").GetComponent<Slider> ();
		musicVolumeText = GameObject.Find ("MusicSliderValue_Text").GetComponent<Text> ();
		effectVolumeSlider = GameObject.Find ("EffectVolume_Slider").GetComponent<Slider> ();
		effectVolumeText = GameObject.Find ("EffectSliderValue_Text").GetComponent<Text> ();
		applyButton = GameObject.Find ("Apply").GetComponent<Button> ();
        backButton = GameObject.Find("Back").GetComponent<Button>();

		//Set the starting variables to the appropriate values.
		oldResolution = Screen.currentResolution;
		oldFullscreen = Screen.fullScreen;
		oldMusicVolume = AudioManager.instance.MusicVolume;
		oldEffectVolume = AudioManager.instance.EffectVolume;

		//Set the temp variables to the old variables.
		tempResolution = oldResolution;
		tempFullscreen = oldFullscreen;
		tempMusicVolume = oldMusicVolume;
		tempEffectVolume = oldEffectVolume;

		//Debug.Log (oldResolution.ToString () + ", " + tempResolution.ToString ());

		//Set the elements to their appropriate states.
		resolutionDropdown.ClearOptions();
		List<string> resolutionOptions = new List<string> ();
		Screen.resolutions.ToList ().ForEach (res => resolutionOptions.Add (res.ToString ()));
		resolutionDropdown.AddOptions (resolutionOptions);
		int currentResolutionIndex = -1;
		for (int i = 0; i < Screen.resolutions.Length; i++) 
		{
			if (Screen.width == Screen.resolutions [i].width
			   && Screen.height == Screen.resolutions [i].height) 
			{
				currentResolutionIndex = i;
			}
		}
		//Debug.Log ("INDEX: " + currentResolutionIndex);
		//Debug.Log ("LENGTH: " + resolutionOptions.Count);
		resolutionDropdown.value = currentResolutionIndex;

		fullscreenToggle.isOn = oldFullscreen;
		musicVolumeSlider.value = oldMusicVolume * 100f;
		musicVolumeText.text = (int)musicVolumeSlider.value + "%";
		effectVolumeSlider.value = oldEffectVolume * 100f;
		effectVolumeText.text = (int)effectVolumeSlider.value + "%";
		applyButton.interactable = false;

		base.SelectButtonInEventSystem (backButton, true);

        base.onBackButtonPressed += this.ReturnToMainMenu;
	}

	public override void Update()
	{
        base.Update();

		//Update the temporary variables.
		tempResolution = Screen.resolutions [resolutionDropdown.value];
		tempFullscreen = fullscreenToggle.isOn;
		tempMusicVolume = musicVolumeSlider.value / 100f;
		tempEffectVolume = effectVolumeSlider.value / 100f;

		//Update the volume texts.
		musicVolumeText.text = (int)musicVolumeSlider.value + "%";
		effectVolumeText.text = (int)effectVolumeSlider.value + "%";

		bool resolutionsEqual = false;
		if (tempResolution.width == oldResolution.width && tempResolution.height == oldResolution.height) 
		{
			resolutionsEqual = true;
		}

		//Set the apply button to interactable if there were any changes to save.
		if (!resolutionsEqual || oldFullscreen != tempFullscreen
		    || oldMusicVolume != tempMusicVolume || oldEffectVolume != tempEffectVolume) 
		{
			base.SetButtonSilent (applyButton, false);
			applyButton.interactable = true;
		}
		else 
		{
			base.SetButtonSilent (applyButton, true);
			applyButton.interactable = false;
		}
	}

	public void ApplySettings()
	{
		//Apply the temporary values.
		oldResolution = tempResolution;
		oldFullscreen = tempFullscreen;
		oldMusicVolume = tempMusicVolume;
		oldEffectVolume = tempEffectVolume;

		//Apply the settings.
		Screen.SetResolution(tempResolution.width, tempResolution.height, tempFullscreen);
		AudioManager.instance.MusicVolume = tempMusicVolume;
		AudioManager.instance.EffectVolume = tempEffectVolume;

		Debug.Log ("NEW_MVOL: " + AudioManager.instance.MusicVolume);

        base.SelectButtonInEventSystem(backButton, true);

		//For debugging.
		/*
		Debug.Log ("Resolution :: " + tempResolution.ToString ());
		Debug.Log ("Fullscreen :: " + tempFullscreen.ToString ());
		Debug.Log ("Music Volume :: " + tempMusicVolume.ToString ());
		Debug.Log ("Effect Volume :: " + tempEffectVolume.ToString ());
		*/
	}

	public void ReturnToMainMenu()
	{
		SceneManager.LoadScene ("MainMenu");
	}
}

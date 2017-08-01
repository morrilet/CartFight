using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Game settings/options menu. Handles the UI for the options menu. 
/// Also controls what game settings are used (ie: time/score limit, game type, etc.)
/// </summary>
public class GameSettingsMenu : MonoBehaviour 
{
	////////// Variables //////////

	private static GameManager.GameSettings settings;
	private List<Selectable> menuObjs;
	private bool menuActive;

	public GameObject gameMode_SettingObj;
	public GameObject timeLimit_SettingObj;
	public GameObject scoreLimit_SettingObj;
	public GameObject itemCount_SettingObj;
	public GameObject level_SettingObj;

	////////// Accessors //////////

	public static GameManager.GameSettings Settings { get { return settings; } }
	public bool MenuActive { get { return this.menuActive; } }

	////////// Primary Methods //////////

	private void Start()
	{
		//Default settings.
		settings = new GameManager.GameSettings ();
		settings.TimeLimit = 120;
		settings.ScoreLimit = 100;
		settings.ItemCount = 3;
		settings.Mode = GameManager.GameSettings.GameModes.ScoreAndTime;
		settings.Level = GameManager.GameLevels.FaceOff;

		//Set up the level dropdown
		Dropdown dropdown_LS = level_SettingObj.transform.FindChild ("Level_Dropdown").GetComponent<Dropdown> ();
		dropdown_LS.ClearOptions ();
		for (int i = 0; i < (int)GameManager.GameLevels.LevelCount; i++) 
		{
			Dropdown.OptionData tempOption_LS = new Dropdown.OptionData (((GameManager.GameLevels)i).ToString());
			dropdown_LS.options.Add (tempOption_LS);
		}
		dropdown_LS.value = 0;
		dropdown_LS.RefreshShownValue ();

		Update (); //Update the game settings and UI before we deactivate the menu.

		//Note that GetComponentsInChildren is recursive. Score.
		Selectable[] temp = this.transform.GetComponentsInChildren<Selectable> ();
		menuObjs = new List<Selectable> ();
		for (int i = 0; i < temp.Length; i++) 
		{
			menuObjs.Add (temp [i]);
		}
		this.gameObject.SetActive (false);

		menuActive = false;
	}

	private void Update()
	{
		UpdateUI ();
	}

	////////// Custom Methods //////////

	private void UpdateUI()
	{
		UpdateGameModeUI ();
		UpdateTimeLimitUI ();
		UpdateScoreLimitUI ();
		UpdateItemCountUI ();
		UpdateLevelUI ();
	}

	private void UpdateGameModeUI()
	{
		Dropdown dropdown = gameMode_SettingObj.GetComponentInChildren<Dropdown> ();
		settings.Mode = (GameManager.GameSettings.GameModes)dropdown.value;
	}

	private void UpdateTimeLimitUI()
	{
		Text mins_Text;
		Text secs_Text;
		Button minsUp, minsDown;
		Button secsUp, secsDown;

		mins_Text = timeLimit_SettingObj.transform.FindChild ("Mins_Text").GetComponent<Text> ();
		secs_Text = timeLimit_SettingObj.transform.FindChild ("Secs_Text").GetComponent<Text> ();
		minsUp = timeLimit_SettingObj.transform.FindChild ("MinsUp_Button").GetComponent<Button> ();
		minsDown = timeLimit_SettingObj.transform.FindChild ("MinsDown_Button").GetComponent<Button> ();
		secsUp = timeLimit_SettingObj.transform.FindChild ("SecsUp_Button").GetComponent<Button> ();
		secsDown = timeLimit_SettingObj.transform.FindChild ("SecsDown_Button").GetComponent<Button> ();

		string mins = Mathf.Floor (settings.TimeLimit / 60.0f).ToString ();
		string secs = Mathf.RoundToInt (settings.TimeLimit % 60.0f).ToString ();
		if (settings.TimeLimit <= 0) 
		{
			mins = "0";
			secs = "0";
		}
		if (int.Parse(secs) < 10) 
		{
			secs = "0" + secs;
		}
		mins_Text.text = mins;
		secs_Text.text = secs;

		if (settings.Mode == GameManager.GameSettings.GameModes.Score) 
		{
			minsUp.interactable = false;
			minsDown.interactable = false;
			secsUp.interactable = false;
			secsDown.interactable = false;

			return;
		}
		else
		{
			minsUp.interactable = true;
			minsDown.interactable = true;
			secsUp.interactable = true;
			secsDown.interactable = true;
		}

		minsUp.onClick.RemoveAllListeners ();
		minsUp.onClick.AddListener (delegate() { Debug.Log("Here");settings.TimeLimit += 60; });

		minsDown.onClick.RemoveAllListeners ();
		minsDown.onClick.AddListener (delegate() { settings.TimeLimit -= 60; });

		secsUp.onClick.RemoveAllListeners ();
		secsUp.onClick.AddListener (delegate() { settings.TimeLimit += 15; });

		secsDown.onClick.RemoveAllListeners ();
		secsDown.onClick.AddListener (delegate() { settings.TimeLimit -= 15; });
	}

	private void UpdateScoreLimitUI()
	{
		Text score_Text = scoreLimit_SettingObj.transform.FindChild ("Value_Text").GetComponent<Text> ();
		Button scoreUp_Button = scoreLimit_SettingObj.transform.FindChild ("ScoreUp_Button").GetComponent<Button> ();
		Button scoreDown_Button = scoreLimit_SettingObj.transform.FindChild ("ScoreDown_Button").GetComponent<Button> ();

		string scoreLimit = settings.ScoreLimit.ToString ();
		if (settings.ScoreLimit <= 0) 
		{
			scoreLimit = "0";
		}
		score_Text.text = scoreLimit;

		if (settings.Mode == GameManager.GameSettings.GameModes.Time) 
		{
			scoreUp_Button.interactable = false;
			scoreDown_Button.interactable = false;

			return;
		}
		else
		{
			scoreUp_Button.interactable = true;
			scoreDown_Button.interactable = true;
		}

		scoreUp_Button.onClick.RemoveAllListeners ();
		scoreUp_Button.onClick.AddListener (delegate() {settings.ScoreLimit += 5; });

		scoreDown_Button.onClick.RemoveAllListeners ();
		scoreDown_Button.onClick.AddListener (delegate() {settings.ScoreLimit -= 5; });
	}

	private void UpdateItemCountUI()
	{
		Slider slider = itemCount_SettingObj.transform.FindChild ("ItemCount_Slider").GetComponent<Slider> ();
		Text itemCount_Text = itemCount_SettingObj.transform.FindChild ("SliderValue_Text").GetComponent<Text> ();

		itemCount_Text.text = slider.value.ToString ();
		settings.ItemCount = (int)slider.value;
	}

	private void UpdateLevelUI()
	{
		Dropdown dropdown = level_SettingObj.transform.FindChild ("Level_Dropdown").GetComponent<Dropdown> ();

		settings.Level = ((GameManager.GameLevels)dropdown.value);
	}

	public void OpenMenu()
	{
		menuActive = true;
		this.gameObject.SetActive (true);
		StartCoroutine (OpenMenu_Coroutine ());
	}

	public void CloseMenu()
	{
		StartCoroutine (CloseMenu_Coroutine ());
	}

	private IEnumerator OpenMenu_Coroutine()
	{
		this.gameObject.SetActive (true);

		menuActive = true;

		Animator anim = this.GetComponent<Animator> ();
		Debug.Log (anim.gameObject.name);
		anim.SetBool ("MenuActive", true);

		SetMenuInteractable (false);

		Debug.Log (anim.GetCurrentAnimatorClipInfo(0).Length);
		yield return new WaitForSeconds (anim.GetCurrentAnimatorClipInfo(0).Length);

		SetMenuInteractable (true);

		//Select the OK button.
		for (int i = 0; i < menuObjs.Count; i++)
		{
			if (menuObjs [i].gameObject.name == "OK_Button") 
			{
				menuObjs [i].Select ();
				break;
			}
		}
	}

	private IEnumerator CloseMenu_Coroutine()
	{
		Animator anim = this.GetComponent<Animator> ();
		anim.SetBool ("MenuActive", false);

		menuActive = false;

		SetMenuInteractable (false);

		//Select the settings menu button.
		GameObject.Find ("Settings_Button").GetComponent<Button> ().Select();

		Debug.Log (anim.GetCurrentAnimatorClipInfo(0).Length);
		yield return new WaitForSeconds (anim.GetCurrentAnimatorClipInfo (0).Length);

		this.gameObject.SetActive (false);
	}

	private void SetMenuInteractable(bool interactable)
	{
		for (int i = 0; i < menuObjs.Count; i++) 
		{
			menuObjs [i].interactable = interactable;
		}
	}
}

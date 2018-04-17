using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Game settings/options menu. Handles the UI for the options menu. 
/// Also controls what game settings are used (ie: time/score limit, game type, etc.)
/// </summary>
public class GameSettingsMenu : Menu 
{
	////////// Variables //////////

	private static GameManager.GameSettings settings;
	private List<Selectable> gameMenuObjs;
    private List<Selectable> mutatorMenuObjs;
	private bool menuActive;
    private bool showingMutators;
    private bool isFlippingMenu;
    private bool isOpeningMenu;

    public GameObject mutatorButton;
    public GameObject backButton;


    [Space]
    [Header("Normal Objects")]

	public GameObject gameMode_SettingObj;
	public GameObject timeLimit_SettingObj;
	public GameObject scoreLimit_SettingObj;
	public GameObject itemCount_SettingObj;
	public GameObject level_SettingObj;

    [Space]
    public List<GameObject> normalObjs;

    [Space]
    [Header("Mutator Objects")]

    public GameObject soulboundCart_SettingObj;
    public GameObject killsAsScore_SettingsObj;
    public GameObject cartLimit_SettingsObj;
    public GameObject bombCount_SettingsObj;
    public GameObject killValue_SettingsObj;

    [Space]
    public List<GameObject> mutatorObjs;

	////////// Accessors //////////

	public static GameManager.GameSettings Settings { get { return settings; } }
	public bool MenuActive { get { return this.menuActive; } }

	////////// Primary Methods //////////

	public override void Start()
	{
		//Default settings.
		settings = new GameManager.GameSettings ();
		settings.TimeLimit = 120;
		settings.ScoreLimit = 100;
		settings.ItemCount = 3;
		settings.Mode = GameManager.GameSettings.GameModes.ScoreAndTime;
        settings.Level = GameManager.GameLevels.FaceOff;

        settings.UseSoulboundCarts = false;
        settings.UseCartLimit = false;
        settings.CartLimit = 10;
        settings.BombCount = 2;
        settings.Kills = GameManager.GameSettings.KillsMode.None;
        settings.KillValue = 5;

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

		UpdateNormalUI (); //Update the game settings and UI before we deactivate the menu.
        UpdateMutatorUI();

		//Note that GetComponentsInChildren is recursive. Score.
		Selectable[] temp = this.transform.GetComponentsInChildren<Selectable> ();
		gameMenuObjs = new List<Selectable> ();
        mutatorMenuObjs = new List<Selectable>();
		for (int i = 0; i < temp.Length; i++) 
		{
            if (temp[i].tag.Equals("GameSettings_Normal"))
            {
                gameMenuObjs.Add(temp[i]);
            }
            else if (temp[i].tag.Equals("GameSettings_Mutator"))
            {
                mutatorMenuObjs.Add(temp[i]);
            }
            else
            {
                Debug.Log("A menu object (game settings menu) is not properly tagged! It will be ignored.");
            }
		}

        mutatorButton.GetComponent<ButtonSoundController>().selectSound = "PaperSlide_Exit";
        mutatorButton.GetComponent<ButtonSoundController>().clickSound = "PaperSlide_Enter";

        backButton.GetComponent<ButtonSoundController>().selectSound = "PaperSlide_Exit";
        backButton.GetComponent<ButtonSoundController>().clickSound = "PaperSlide_Enter";

        this.gameObject.SetActive (false);

		menuActive = false;
        isFlippingMenu = false;
        
        base.onBackButtonPressed += this.CloseMenu;
	}

	public override void Update()
	{
        base.Update();

        if (!isFlippingMenu)
        {
            if (!showingMutators)
            {
                UpdateNormalUI();
            }
            else
            {
                UpdateMutatorUI();
            }
        }

        if(Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            Debug.Log(settings.ToString());
        }
	}

    ////////// Custom Methods //////////

    #region Menu_Updates
    #region Normal_Menu
    private void UpdateNormalUI()
    {
        UpdateGameModeUI();
        UpdateTimeLimitUI();
        UpdateScoreLimitUI();
        UpdateItemCountUI();
        UpdateLevelUI();
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
		minsUp.onClick.AddListener (delegate() { settings.TimeLimit += 60; });

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
    #endregion

    #region Mutator_Menu
    private void UpdateMutatorUI()
    {
        UpdateSoulboundCartsUI();
        UpdateKillsAsScoreUI();
        UpdateCartLimitUI();
        UpdateBombCountUI();
        UpdateKillValueUI();
    }

    private void UpdateSoulboundCartsUI()
    {
        Toggle soulboundCarts_Toggle = soulboundCart_SettingObj.GetComponentInChildren<Toggle>();
        settings.UseSoulboundCarts = soulboundCarts_Toggle.isOn;
    }

    private void UpdateKillsAsScoreUI()
    {
        Dropdown killsAsScore_Dropdown = killsAsScore_SettingsObj.GetComponentInChildren<Dropdown>();
        switch (killsAsScore_Dropdown.value)
        {
            case 0: //Don't count kills.
                settings.Kills = GameManager.GameSettings.KillsMode.None;
                break;
            case 1: //Count kills a part of score.
                settings.Kills = GameManager.GameSettings.KillsMode.Include;
                break;
            case 2: //Count kills only.
                settings.Kills = GameManager.GameSettings.KillsMode.Solo;
                break;
        }
    }

    private void UpdateCartLimitUI()
    {
        Toggle useCartLimit_Toggle = cartLimit_SettingsObj.GetComponentInChildren<Toggle>();
        Button tickUp_Button = cartLimit_SettingsObj.transform.FindChild("CartLimit_Up_Button").GetComponent<Button>();
        Button tickDown_Button = cartLimit_SettingsObj.transform.FindChild("CartLimit_Down_Button").GetComponent<Button>();
        Text cartLimitCount_Text = cartLimit_SettingsObj.transform.FindChild("CartLimit_Count").GetComponent<Text>();

        //Update settings to match cart limit toggle.
        settings.UseCartLimit = useCartLimit_Toggle.isOn;

        //Update UI.
        tickUp_Button.interactable = settings.UseCartLimit;
        tickDown_Button.interactable = settings.UseCartLimit;
        cartLimitCount_Text.text = settings.CartLimit.ToString();

        //Set up listeners. Note that clamping is handled in the settings accessor.
        tickUp_Button.onClick.RemoveAllListeners();
        tickUp_Button.onClick.AddListener(delegate () { settings.CartLimit++; });

        tickDown_Button.onClick.RemoveAllListeners();
        tickDown_Button.onClick.AddListener(delegate () { settings.CartLimit--; });
    }

    private void UpdateBombCountUI()
    {
        Slider bombCount_Slider = bombCount_SettingsObj.GetComponentInChildren<Slider>();
        Text bombCountValue_Text = bombCount_SettingsObj.transform.FindChild("SliderValue_Text").GetComponent<Text>();

        settings.BombCount = (int)bombCount_Slider.value;
        bombCountValue_Text.text = settings.BombCount.ToString();
    }

    private void UpdateKillValueUI()
    {
        Text killValue_Text = killValue_SettingsObj.transform.FindChild("KillValue_Count").GetComponent<Text>();
        Button tickUp_Button = killValue_SettingsObj.transform.FindChild("KillValue_Up_Button").GetComponent<Button>();
        Button tickDown_Button = killValue_SettingsObj.transform.FindChild("KillValue_Down_Button").GetComponent<Button>();

        //Update UI.
        tickUp_Button.interactable = (settings.Kills != GameManager.GameSettings.KillsMode.None);
        tickDown_Button.interactable = (settings.Kills != GameManager.GameSettings.KillsMode.None);
        killValue_Text.text = settings.KillValue.ToString();

        //Set up listeners. Note that clamping is handled in the settings accessor.
        tickUp_Button.onClick.RemoveAllListeners();
        tickUp_Button.onClick.AddListener(delegate () { settings.KillValue++; });

        tickDown_Button.onClick.RemoveAllListeners();
        tickDown_Button.onClick.AddListener(delegate () { settings.KillValue--; });
    }
    #endregion
    #endregion

    #region Menu_Transitions
    public void OpenMenu()
	{
		menuActive = true;
		this.gameObject.SetActive (true);
        AudioManager.instance.PlayEffect("PaperSlide_Enter");
        
		StartCoroutine (OpenMenu_Coroutine ());
	}

	public void CloseMenu()
	{
        if(isFlippingMenu || isOpeningMenu)
        {
            return;
        }

        //Note that there is less here than in OpenMenu() because much of the
        //logic must be done after the animation finishes, in the coroutine.
        AudioManager.instance.PlayEffect("PaperSlide_Exit");
        
		StartCoroutine (CloseMenu_Coroutine ());
	}

    public void FlipMenu()
    {
        isFlippingMenu = true;
        showingMutators = !showingMutators;

        StartCoroutine(FlipMenu_Coroutine());
    }

	private IEnumerator OpenMenu_Coroutine()
	{
        //Unselect any currently selected button so it can't be spammed
        //while we open the menu.
        //base.DeselectCurrentlySelected();

        isOpeningMenu = true;

		this.gameObject.SetActive (true);

		menuActive = true;

		Animator anim = this.GetComponent<Animator> ();
        //Debug.Log (anim.gameObject.name);
        anim.SetBool ("MenuActive", true);

		SetMenuInteractable (gameMenuObjs, false);
        SetMenuInteractable (mutatorMenuObjs, false);
        mutatorButton.GetComponent<Selectable>().interactable = false;
        backButton.GetComponent<Selectable>().interactable = false;

        //Debug.Log (anim.GetCurrentAnimatorClipInfo(0).Length);
        yield return new WaitForSeconds(0.5f);//anim.GetCurrentAnimatorClipInfo(0).Length);

		SetMenuInteractable (gameMenuObjs, true);
        mutatorButton.GetComponent<Selectable>().interactable = true;

        //Select the OK button.
        for (int i = 0; i < gameMenuObjs.Count; i++)
		{
			if (gameMenuObjs [i].gameObject.name == "OK_Button") 
			{
				Button tempButton = gameMenuObjs [i] as Button;
                base.SelectButtonInEventSystem(tempButton, true);
				break;
			}
		}

        isOpeningMenu = false;
	}

	private IEnumerator CloseMenu_Coroutine()
	{
		Animator anim = this.GetComponent<Animator> ();
		anim.SetBool ("MenuActive", false);

		menuActive = false;
        
        SetMenuInteractable(gameMenuObjs, false);
        SetMenuInteractable(mutatorMenuObjs, false);
        mutatorButton.GetComponent<Selectable>().interactable = false;
        backButton.GetComponent<Selectable>().interactable = false;

        base.DeselectCurrentlySelected();

        //Debug.Log (anim.GetCurrentAnimatorClipInfo(0).Length);
        yield return new WaitForSeconds(0.3333f);//anim.GetCurrentAnimatorClipInfo (0).Length);
        
        if (showingMutators)
        {
            showingMutators = false;
            SetObjectsEnabled(normalObjs, true);
            SetObjectsEnabled(mutatorObjs, false);
        }

        //Select the settings menu button.
        yield return new WaitForEndOfFrame();
        base.SelectButtonInEventSystem(GameObject.Find("Settings_Button").GetComponent<Button>(), true);
        //GameObject.Find ("Settings_Button").GetComponent<Button> ().Select();

        yield return new WaitForEndOfFrame();
        this.gameObject.SetActive (false);
	}

    private IEnumerator FlipMenu_Coroutine()
    {
        //Debugging.
        //UnityEditor.EditorApplication.isPaused = true;

        SetMenuInteractable(gameMenuObjs, false);
        SetMenuInteractable(mutatorMenuObjs, false);

        //Play animation and wait until it's over.
        Animator anim = this.GetComponent<Animator>();
        if (showingMutators)
        {
            //anim.ResetTrigger("FlipToNormal");
            //anim.SetTrigger("FlipToMutators");
            anim.SetBool("ShowingMutators", true);
        }
        else
        {
            //anim.SetTrigger("FlipToNormal");
            //anim.ResetTrigger("FlipToMutators");
            anim.SetBool("ShowingMutators", false);
        }

        yield return new WaitForSeconds(0.175f);//anim.GetCurrentAnimatorClipInfo(0).Length);

        SetObjectsEnabled(normalObjs, !showingMutators);
        SetObjectsEnabled(mutatorObjs, showingMutators);

        yield return new WaitForSeconds(0.325f);

        if (showingMutators)
        {
            SetMenuInteractable(gameMenuObjs, false);
            SetMenuInteractable(mutatorMenuObjs, true);

            backButton.GetComponent<Selectable>().interactable = true;
            mutatorButton.GetComponent<Selectable>().interactable = false;

            //Select the back button.
            for (int i = 0; i < mutatorObjs.Count; i++)
            {
                if (mutatorObjs[i].gameObject.name == "Back_Button")
                {
                    Button tempButton = mutatorObjs[i].GetComponent<Button>();
                    base.SelectButtonInEventSystem(tempButton, true);
                    break;
                }
            }
        }
        else
        {
            SetMenuInteractable(gameMenuObjs, true);
            SetMenuInteractable(mutatorMenuObjs, false);

            backButton.GetComponent<Selectable>().interactable = false;
            mutatorButton.GetComponent<Selectable>().interactable = true;

            //Select the mutators button.
            for (int i = 0; i < normalObjs.Count; i++)
            {
                if (normalObjs[i].gameObject.name == "Mutator_Button")
                {
                    Button tempButton = normalObjs[i].GetComponent<Button>();
                    base.SelectButtonInEventSystem(tempButton, true);
                    break;
                }
            }
        }

        isFlippingMenu = false;
    }
    #endregion

    #region Helper_Methods
    private void SetMenuInteractable(List<Selectable> menuObjs, bool interactable)
	{
		for (int i = 0; i < menuObjs.Count; i++) 
		{
			menuObjs [i].interactable = interactable;
		}
	}

    private void SetObjectsEnabled(List<GameObject> objs, bool enabled)
    {
        for(int i = 0; i < objs.Count; i++)
        {
            objs[i].SetActive(enabled);
        }
    }
    #endregion
}

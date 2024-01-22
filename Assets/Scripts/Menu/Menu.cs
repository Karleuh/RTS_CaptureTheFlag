using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
	[SerializeField] Player player;
	[SerializeField] GameObject panel;
	[SerializeField] GameObject mainMenu;
	[SerializeField] GameObject optionsMenu;
	[SerializeField] GameObject rulesMenu;
	[SerializeField] GameObject popupWin;
	[SerializeField] TMPro.TMP_Text winText;
	[SerializeField] TMPro.TMP_Text timeText;
	[SerializeField] TMPro.TMP_Dropdown resolutionsDropdown;
	[SerializeField] TMPro.TMP_Dropdown qualitiesDropdown;
	[SerializeField] Slider masterSlider;
	[SerializeField] Slider musicSlider;
	[SerializeField] Slider effectsSlider;
	[SerializeField] AudioMixer mixer;

	Resolution[] resolutions;


	public bool Shown
	{
		set
		{
			this.panel.SetActive(value);
			this.mainMenu.SetActive(value);
			this.optionsMenu.SetActive(false);
			this.rulesMenu.SetActive(false);

			this.popupWin.SetActive(false);
		}
	}

	void Start()
    {
		this.Shown = true;
		this.SetupResolutions();
		this.SetupQualities();
		this.SetupVolumes();
    }

	void SetupResolutions()
	{
		int currentRes = 0;

		this.resolutions = Screen.resolutions;
		List<TMPro.TMP_Dropdown.OptionData> datas = new List<TMPro.TMP_Dropdown.OptionData>();

		for (int i= 0;  i < this.resolutions.Length; i++)
		{
			datas.Add(new TMPro.TMP_Dropdown.OptionData(this.resolutions[i].width + " x " + this.resolutions[i].height + " " + this.resolutions[i].refreshRate + "FPS"));
			if (this.resolutions[i].width == Screen.currentResolution.width && this.resolutions[i].height == Screen.currentResolution.height && this.resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
				currentRes = i;
		}

		this.resolutionsDropdown.AddOptions(datas);
		this.resolutionsDropdown.SetValueWithoutNotify(currentRes);
	}

	void SetupQualities()
	{
		List<TMPro.TMP_Dropdown.OptionData> datas = new List<TMPro.TMP_Dropdown.OptionData>();

		foreach (string name in QualitySettings.names)
		{
			datas.Add(new TMPro.TMP_Dropdown.OptionData(name));
		}

		this.qualitiesDropdown.AddOptions(datas);
		this.qualitiesDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
	}

	void SetupVolumes()
	{
		if (this.mixer.GetFloat("Master", out float valueMaster))
			this.masterSlider.SetValueWithoutNotify(valueMaster);
		if (this.mixer.GetFloat("Music", out float valueMusic))
			this.musicSlider.SetValueWithoutNotify(valueMusic);
		if (this.mixer.GetFloat("Effects", out float valueEffects))
			this.effectsSlider.SetValueWithoutNotify(valueEffects);
	}

	void Update()
    {
		if(GameManager.Instance.IsGameStarted)
			this.timeText.text = Mathf.Floor(GameManager.Instance.GameTime) + "s";
    }


	public void OnOptionsBC()
	{
		this.mainMenu.SetActive(false);
		this.optionsMenu.SetActive(true);
		this.rulesMenu.SetActive(false);
	}

	public void OnMainMenuBC()
	{
		this.mainMenu.SetActive(true);
		this.optionsMenu.SetActive(false);
		this.rulesMenu.SetActive(false);
	}

	public void OnRulesMenuBC()
	{
		this.mainMenu.SetActive(false);
		this.optionsMenu.SetActive(false);
		this.rulesMenu.SetActive(true);
	}


	public void OnPlay()
	{
		this.Shown = false;
		GameManager.Instance.ChooseStartingArea();
	}

	public void OnMainVolumeChanged(float value)
	{
		mixer.SetFloat("Master", value);
	}

	public void OnMusicVolumeChanged(float value)
	{
		mixer.SetFloat("Music", value);
	}

	public void OnEffectVolumeChanged(float value)
	{
		mixer.SetFloat("Effects", value);
	}

	public void OnResolutionChanged(int value)
	{
		Screen.SetResolution(this.resolutions[value].width, this.resolutions[value].height, true, this.resolutions[value].refreshRate);
	}


	public void OnQualityChanged(int value)
	{
		QualitySettings.SetQualityLevel(value, true);
	}


	public void OnTeamChanged(int value)
	{
		this.player.Team = value == 0 ? Team.ATTACKER : Team.DEFENDER;
	}





	public void OnWin(bool win)
	{
		this.popupWin.SetActive(true);
		this.winText.text = win ? "Bravo\nVous avez gagné !" : "Oh non !\nVous avez perdu !";
	}
}

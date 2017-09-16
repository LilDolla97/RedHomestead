﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using RedHomestead.Buildings;
using RedHomestead.Simulation;
using RedHomestead.Interiors;
using RedHomestead.Persistence;
using UnityEngine.PostProcessing;

[Serializable]
public struct ReportIORow
{
    public Text Name, Flow, Amount;
    internal Image IsConnected;

    internal void Bind(ReportIOData data, Sprite connected, Sprite disconnected)
    {
        Name.text = data.Name;
        Flow.text = data.Flow;
        Amount.text = data.Amount;
        IsConnected = Name.transform.GetChild(0).GetComponent<Image>();
        IsConnected.sprite = data.Connected ? connected : disconnected;
    }

    internal ReportIORow CreateNew(RectTransform parentTable)
    {
        ReportIORow result = new ReportIORow()
        {
            Name = GameObject.Instantiate(Name.gameObject).GetComponent<Text>(),
            Flow = GameObject.Instantiate(Flow.gameObject).GetComponent<Text>(),
            Amount = GameObject.Instantiate(Amount.gameObject).GetComponent<Text>(),
        };
        result.Name.transform.parent = parentTable;
        result.Flow.transform.parent = parentTable;
        result.Amount.transform.parent = parentTable;

        return result;
    }

    internal void Destroy()
    {
        GameObject.Destroy(Name.gameObject);
        GameObject.Destroy(Flow.gameObject);
        GameObject.Destroy(Amount.gameObject);
    }
}

internal struct ReportIOData
{
    public string Name, Flow, Amount;
    public bool Connected;
}

[Serializable]
public struct ReportFields
{
    public Text ModuleName, EnergyEfficiency, ReactionEfficiency, ReactionEquation;
    public RectTransform InputRow, OutputRow;
    public Sprite Connected, Disconnected;
}

public enum MiscIcon {
    Information,
    Rocket,
    Pipe,
    Plug,
    Harvest,
    Molecule,
    Umbilical,
    ClearSky,
    LightDust,
    HeavyDust,
    DustStorm,
    HammerAndPick
}

[Serializable]
public struct Icons
{
    public Sprite[] ResourceIcons, CompoundIcons, MiscIcons;
}

[Serializable]
public struct NewsUI
{
    public RectTransform Panel, ProgressBar;
    public Text Description;
    public Image Icon, ProgressFill;
}


[Serializable]
public class RadialMenu
{
    public RectTransform RadialPanel, RadialsParent;
    public Image RadialSelector;
    internal Image[] Radials = null;

    public static Color HoverColor = new Color(1, 1, 1, 1);
    public static Color DefaultColor = new Color(1, 1, 1, 0.6f);
}

[Serializable]
public struct PromptUI
{
    public RectTransform Panel, ProgressBar, Background;
    public Text Key, Description, SecondaryKey, SecondaryDescription, TypeText;
    public Image ProgressFill;
}

[Serializable]
public class SurvivalBar
{
    public Image Bar;
    internal Text Hours;
    public virtual void Initialize()
    {
        Hours = Bar.transform.GetChild(0).GetComponent<Text>();
    }
}

[Serializable]
public struct SurvivalBarsUI
{
    public SurvivalBar Food;
    public SurvivalBar Water;
    public SurvivalBar Power;
    public SurvivalBar Oxygen;
    public SurvivalBar RoverPower;
    public SurvivalBar RoverOxygen;
    public SurvivalBar HabitatPower;
    public SurvivalBar HabitatOxygen;
}

[Serializable]
public struct PrinterUI
{
    public RectTransform Panel, AvailablePanel, AllPanel, AllList, AllListButtonPrefab;

    private bool showingAll;
    public void ToggleAvailable(bool? showAvailable = null)
    {
        if (!showAvailable.HasValue)
            showAvailable = showingAll;

        AvailablePanel.gameObject.SetActive(showAvailable.Value);
        AllPanel.gameObject.SetActive(!showAvailable.Value);

        showingAll = !showAvailable.Value;
    }
}

/// <summary>
/// Scripting interface for all GUI elements
/// syncs PlayerInput state to UI
/// has internal state for showing prompts and panels
/// </summary>
public class GuiBridge : MonoBehaviour {
    public static GuiBridge Instance { get; private set; }

    public Canvas GUICanvas;
    public RectTransform ConstructionPanel, ConstructionGroupPanel, ConstructionModulesPanel, PlacingPanel, KilledPanel, FloorplanGroupPanel, FloorplanSubgroupPanel, FloorplanPanel, HelpPanel, ReportPanel, EscapeMenuPanel, Crosshair;
    public Text ConstructionHeader, EquippedText, PlacingText, TimeText, KilledByReasonText;
    public Button[] ConstructionGroupButtons;
    public Text[] ConstructionGroupHints, FloorplanGroupHints;
    public RectTransform[] ConstructionRequirements, ConstructionModuleButtons;
    public Image EquippedImage, ColdImage, HotImage, AutosaveIcon, SprintIcon;
    public AudioSource ComputerAudioSource;
    public SurvivalBarsUI SurvivalBars;
    public ReportIORow ReportRowTemplate;
    public ReportFields ReportTexts;
    public RadialMenu RadialMenu;
    public RedHomestead.Equipment.EquipmentSprites EquipmentSprites;
    public PromptUI Prompts;
    public Icons Icons;
    public NewsUI News;
    public PostProcessingProfile PostProfile;
    public PrinterUI Printer;

    internal Text[] ConstructionRequirementsText;

    internal bool RadialMenuOpen = false;
    internal PromptInfo CurrentPrompt { get; set; }

    void Awake()
    {
        Instance = this;
        TogglePromptPanel(false);
        this.ConstructionPanel.gameObject.SetActive(false);
        ConstructionRequirementsText = new Text[ConstructionRequirements.Length];
        int i = 0;
        foreach (RectTransform t in ConstructionRequirements)
        {
            ConstructionRequirementsText[i] = t.GetChild(0).GetComponent<Text>();
            i++;
        }
        SurvivalBars.Oxygen.Initialize();
        SurvivalBars.Power.Initialize();
        SurvivalBars.Water.Initialize();
        SurvivalBars.Food.Initialize();
        SurvivalBars.RoverOxygen.Initialize();
        SurvivalBars.RoverPower.Initialize();
        SurvivalBars.HabitatOxygen.Initialize();
        SurvivalBars.HabitatPower.Initialize();
        ToggleReportMenu(false);
        ToggleRadialMenu(false);
        ToggleAutosave(false);
        TogglePrinter(false);
        //same as ToggleEscapeMenu(false) basically
        this.EscapeMenuPanel.gameObject.SetActive(false);
        News.Panel.gameObject.SetActive(false);
    }

    internal void ToggleAutosave(bool state)
    {
        this.AutosaveIcon.gameObject.SetActive(state);
    }

    void Start()
    {
        //this.RefreshPlanningUI();

        if (Game.Current.IsNewGame)
        {
            print("hello new gamer");
            ShowNews(NewsSource.ToolOpenHint);
            ShowNews(NewsSource.FOneHint);
        }

        RefreshSprintIcon(false);
    }

    private Queue<News> newsTicker = new Queue<News>();
    private Coroutine newsCoroutine;
    internal void ShowNews(News news)
    {
        if (news != null)
        {
            newsTicker.Enqueue(news);
            if (newsCoroutine == null)
            {
                newsCoroutine = StartCoroutine(StartShowNews());
            }
        }
    }

    private IEnumerator StartShowNews()
    {
        News currentNews = newsTicker.Dequeue();
        do
        {
            print("News: " + currentNews.Text);
            News.Panel.gameObject.SetActive(true);
            News.Description.text = currentNews.Text;

            News.Icon.sprite = this.Icons.MiscIcons[(int)currentNews.Icon];

#warning news progressbar unimplemented
            News.ProgressBar.gameObject.SetActive(false);

            yield return new WaitForSeconds(currentNews.DurationMilliseconds / 1000f);

            News.Panel.gameObject.SetActive(false);
            if (newsTicker.Count > 0)
                currentNews = newsTicker.Dequeue();
            else
                currentNews = null;
        }
        while (currentNews != null);

        newsCoroutine = null;
    }

    private void TogglePromptPanel(bool isActive)
    {
        this.Prompts.Panel.gameObject.SetActive(isActive);
    }

    public void ShowPrompt(PromptInfo prompt)
    {
        Prompts.Key.text = prompt.Key;
        Prompts.Key.transform.parent.gameObject.SetActive(prompt.Key != null);
        Prompts.Description.text = prompt.Description;

        Prompts.ProgressBar.gameObject.SetActive(prompt.UsesProgress);
        Prompts.ProgressFill.fillAmount = prompt.Progress;
        
        Prompts.SecondaryDescription.gameObject.SetActive(prompt.HasSecondary);
        Prompts.SecondaryDescription.text = prompt.SecondaryDescription;

        Prompts.SecondaryKey.transform.parent.gameObject.SetActive(prompt.HasSecondary);
        Prompts.SecondaryKey.text = prompt.SecondaryKey;

        Prompts.Background.offsetMin = new Vector2(0, prompt.HasSecondary ? -66 : 0);

        Prompts.TypeText.text = prompt.ItalicizedText;

        TogglePromptPanel(true);
        CurrentPrompt = prompt;
    }

    //todo: just pass constructionZone, it's less params
    internal void ShowConstruction(List<ResourceEntry> requiresList, Dictionary<Matter, float> hasCount, Module toBeBuilt)
    {
        //show the name of the thing being built
        this.ConstructionPanel.gameObject.SetActive(true);
        this.ConstructionHeader.text = "Building a " + toBeBuilt.ToString();

        //show a list of resources gathered / resources required
        for (int i = 0; i < this.ConstructionRequirements.Length; i++)
        {
            if (i < requiresList.Count)
            {
                ResourceEntry resourceEntry = requiresList[i];
                string output = resourceEntry.Type.ToString() + ": " + hasCount[resourceEntry.Type] + "/" + resourceEntry.Count;
                this.ConstructionRequirementsText[i].text = output;
                this.ConstructionRequirements[i].gameObject.SetActive(true);
            }
            else
            {
                this.ConstructionRequirements[i].gameObject.SetActive(false);
            }
        }
    }

    #region cinematic mode
    private enum CinematicModes { None, WithGUI, NoGUI }
    private CinematicModes CinematicMode = CinematicModes.None;
    internal void ToggleCinematicMode()
    {
        int newCinematic = (((int)this.CinematicMode) + 1);

        if (newCinematic > (int)CinematicModes.NoGUI)
            newCinematic = 0;

        CinematicMode = (CinematicModes)newCinematic;

        switch (CinematicMode)
        {
            case CinematicModes.None:
                GUICanvas.enabled = true;
                PlayerInput.Instance.FPSController.MouseLook.smooth = false;
                break;
            case CinematicModes.WithGUI:
                GUICanvas.enabled = true;
                PlayerInput.Instance.FPSController.MouseLook.smooth = true;
                break;
            case CinematicModes.NoGUI:
                PlayerInput.Instance.FPSController.MouseLook.smooth = true;
                GUICanvas.enabled = false;
                break;
        }
        PostProfile.motionBlur.enabled = CinematicMode != CinematicModes.None;
        PlayerInput.Instance.FPSController.MouseLook.smooth = PostProfile.motionBlur.enabled;
    }
    #endregion

    internal void RefreshSprintIcon(bool sprinting)
    {
        this.SprintIcon.gameObject.SetActive(sprinting);
    }

    private void _doToggleEscapeMenu()
    {
        bool escapeMenuWillBeVisible = !this.EscapeMenuPanel.gameObject.activeInHierarchy;
        this.EscapeMenuPanel.gameObject.SetActive(escapeMenuWillBeVisible);

        Cursor.visible = escapeMenuWillBeVisible;
        Cursor.lockState = escapeMenuWillBeVisible ? CursorLockMode.None : CursorLockMode.Confined;
    }

    /// <summary>
    /// Called by cancel button
    /// </summary>
    public void ToggleEscapeMenu()
    {
        //player input will actually call _ToggleEscapeMenuProgrammatically for us
        //which calls _doToggleEscapeMenu
        //so don't call _doToggleEscapeMenu here
        PlayerInput.Instance.ToggleMenu();
    }

    /// <summary>
    /// Called by player input
    /// </summary>
    internal void _ToggleEscapeMenuProgrammatically()
    {
        _doToggleEscapeMenu();
    }

    public void ConfirmQuit()
    {
        Autosave.Instance.AutosaveEnabled = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("menu", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    public void ConfirmSaveAndQuit()
    {
        Autosave.Instance.Save();
        this.ConfirmQuit();
    }

    internal void ToggleReportMenu(bool isOn)
    {
        ReportPanel.gameObject.SetActive(isOn);
    }

    internal void HideConstruction()
    {
        this.ConstructionPanel.gameObject.SetActive(false);
    }

    public void HidePrompt()
    {
        if (CurrentPrompt != null)
        {
            TogglePromptPanel(false);
            CurrentPrompt = null;
        }
    }
    
    internal void ToggleHelpMenu()
    {
        HelpPanel.gameObject.SetActive(!HelpPanel.gameObject.activeSelf);
    }
    /// <summary>
    /// syncs player input mode
    /// </summary>
    internal void RefreshMode()
    {
        switch (PlayerInput.Instance.CurrentMode)
        {
            default:
                RefreshEquipped();
                break;
            case PlayerInput.InputMode.Pipeline:
                this.EquippedText.text = "Pipeline";
                this.EquippedImage.sprite = Icons.MiscIcons[(int)MiscIcon.Pipe];
                break;
            case PlayerInput.InputMode.Powerline:
                this.EquippedText.text = "Powerline";
                this.EquippedImage.sprite = Icons.MiscIcons[(int)MiscIcon.Plug];
                break;
            case PlayerInput.InputMode.Umbilical:
                this.EquippedText.text = "Umbilical";
                this.EquippedImage.sprite = Icons.MiscIcons[(int)MiscIcon.Umbilical];
                break;
        }

        if (PlayerInput.Instance.Loadout.Equipped != RedHomestead.Equipment.Equipment.Blueprints)
        {
            this.PlacingPanel.gameObject.SetActive(false);
        }

        //this.RefreshPlanningUI();
    }

    internal void RefreshEquipped()
    {
        this.EquippedText.text = PlayerInput.Instance.Loadout.Equipped.ToString();
        this.EquippedImage.sprite = EquipmentSprites.FromEquipment(PlayerInput.Instance.Loadout.Equipped);
    }

    internal void ShowKillMenu(string reason)
    {
        KilledPanel.transform.gameObject.SetActive(true);
        KilledByReasonText.text = "DEATH BY " + reason;
    }

    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private void RefreshBarWarningCriticalText(Text textElement, int hoursLeftHint)
    {
        textElement.enabled = hoursLeftHint < 3;
        textElement.text = string.Format("<{0}h", hoursLeftHint);
    }

    internal void RefreshOxygenBar(float percentage, int hoursLeftHint)
    {
        this.SurvivalBars.Oxygen.Bar.fillAmount = percentage;
        this.RefreshBarWarningCriticalText(this.SurvivalBars.Oxygen.Hours, hoursLeftHint);
    }

    internal void RefreshWaterBar(float percentage, int hoursLeftHint)
    {
        this.SurvivalBars.Water.Bar.fillAmount = percentage;
        this.RefreshBarWarningCriticalText(this.SurvivalBars.Water.Hours, hoursLeftHint);
    }

    internal void RefreshFoodBar(float percentage, int hoursLeftHint)
    {
        this.SurvivalBars.Food.Bar.fillAmount = percentage;
        this.RefreshBarWarningCriticalText(this.SurvivalBars.Food.Hours, hoursLeftHint);
    }

    //internal void RefreshRadiationBar(float percentage)
    //{
    //    this.SurvivalBars.Rad.Bar.fillAmount = percentage;
    //}

    internal void RefreshPowerBar(float percentage, int hoursLeftHint)
    {
        this.SurvivalBars.Power.Bar.fillAmount = percentage;
        this.RefreshBarWarningCriticalText(this.SurvivalBars.Power.Hours, hoursLeftHint);
    }
    internal void RefreshRoverOxygenBar(float percentage, int hoursLeftHint)
    {
        this.SurvivalBars.RoverOxygen.Bar.fillAmount = percentage;
        this.RefreshBarWarningCriticalText(this.SurvivalBars.RoverOxygen.Hours, hoursLeftHint);
    }
    internal void RefreshRoverPowerBar(float percentage, int hoursLeftHint)
    {
        this.SurvivalBars.RoverPower.Bar.fillAmount = percentage;
        this.RefreshBarWarningCriticalText(this.SurvivalBars.RoverPower.Hours, hoursLeftHint);
    }
    internal void RefreshHabitatOxygenBar(float percentage, int hoursLeftHint)
    {
        this.SurvivalBars.HabitatOxygen.Bar.fillAmount = percentage;
        this.RefreshBarWarningCriticalText(this.SurvivalBars.HabitatOxygen.Hours, hoursLeftHint);
    }
    internal void RefreshHabitatPowerBar(float percentage, int hoursLeftHint)
    {
        this.SurvivalBars.HabitatPower.Bar.fillAmount = percentage;
        this.RefreshBarWarningCriticalText(this.SurvivalBars.HabitatPower.Hours, hoursLeftHint);
    }

    internal void RefreshTemperatureGauges()
    {
        this.HotImage.enabled = SurvivalTimer.Instance.ExternalTemperature == Temperature.Hot;
        this.ColdImage.enabled = SurvivalTimer.Instance.ExternalTemperature == Temperature.Cold;
    }

    public FloorplanGroup selectedFloorplanGroup = FloorplanGroup.Undecided;
    public Floorplan selectedFloorplanSubgroup;
    public FloorplanMaterial selectedFloorplanMaterial;


    private RedHomestead.Equipment.Slot lastHoverSlot = (RedHomestead.Equipment.Slot)(-1);

    internal RedHomestead.Equipment.Slot ToggleRadialMenu(bool isMenuOpen)
    {
        this.RadialMenu.RadialPanel.gameObject.SetActive(isMenuOpen);
        this.RadialMenuOpen = isMenuOpen;
        this.RadialMenu.RadialSelector.gameObject.SetActive(isMenuOpen);

        //no need to actually show the cursor
        //Cursor.visible = isMenuOpen;
        Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;

        return lastHoverSlot;
    }

    internal void BuildRadialMenu(RedHomestead.Equipment.Loadout load)
    {
        if (RadialMenu.Radials == null)
        {
            RadialMenu.Radials = new Image[RadialMenu.RadialsParent.childCount];

            foreach(Transform t in RadialMenu.RadialsParent)
            {
                Image img = t.GetComponent<Image>();
                int index = int.Parse(img.name);
                RedHomestead.Equipment.Slot s = (RedHomestead.Equipment.Slot)index;
                RadialMenu.Radials[(int)s] = img;
            }
        }


        int i = 0;
        foreach(Image img in RadialMenu.Radials)
        {
            RedHomestead.Equipment.Slot s = (RedHomestead.Equipment.Slot)i;
            img.sprite = EquipmentSprites.FromEquipment(load[s]);
            i++;
        }
    }

    //use half the width of the sectors
    private const float sectorThetaOffset = 30f;
    internal void HighlightSector(float theta)
    {
        var rotation = Mathf.Lerp(0, 360, Mathf.InverseLerp(-180f, 180f, theta));

        int index = (int)Mathf.Round((rotation - sectorThetaOffset) / 60f);
        //corresponds to enum -^

        //corresponds to UI rotation -v
        rotation = (index + 2) * 60f;
        this.RadialMenu.RadialSelector.rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);

        if ((int)lastHoverSlot > -1)
            RadialMenu.Radials[(int)lastHoverSlot].color = RadialMenu.DefaultColor;

        RadialMenu.Radials[index].color = RadialMenu.HoverColor;

        lastHoverSlot = (RedHomestead.Equipment.Slot)index;
    }

    private ReportIORow[] currentIORows;
    public Texture2D noiseTexture;
    public Shader blurShader;

    internal void WriteReport(string moduleName, string reaction, string energyEfficiency, string reactionEfficiency, 
        ReportIOData? power, ReportIOData[] inputs, ReportIOData[] outputs)
    {
        ToggleReportMenu(true);

        ReportTexts.ModuleName.text = moduleName;
        ReportTexts.EnergyEfficiency.text = "Energy Efficiency: "+energyEfficiency;
        ReportTexts.ReactionEfficiency.text = "Reaction Efficiency: "+reactionEfficiency;
        ReportTexts.ReactionEquation.text = reaction;
        
        if (currentIORows != null)
        {
            foreach(ReportIORow row in currentIORows)
            {
                row.Destroy();
            }
            currentIORows = null;
        }

        if (power.HasValue)
        {
            ReportRowTemplate.Bind(power.Value, ReportTexts.Connected, ReportTexts.Disconnected);
        }

        currentIORows = new ReportIORow[inputs.Length + outputs.Length];
        int i;
        for (i = 0; i < inputs.Length; i++)
        {
            currentIORows[i] = ReportRowTemplate.CreateNew(ReportTexts.InputRow);
            currentIORows[i].Bind(inputs[i], ReportTexts.Connected, ReportTexts.Disconnected);
        }
        for (int j = 0; j < outputs.Length; j++)
        {
            currentIORows[i+j] = ReportRowTemplate.CreateNew(ReportTexts.OutputRow);
            currentIORows[i+j].Bind(outputs[j], ReportTexts.Connected, ReportTexts.Disconnected);
        }
    }

    internal void RefreshSurvivalPanel(bool isInVehicle, bool isInHabitat)
    {
        this.SurvivalBars.RoverOxygen.Bar.transform.parent.gameObject.SetActive(isInVehicle);
        this.SurvivalBars.RoverPower.Bar.transform.parent.gameObject.SetActive(isInVehicle);
        this.SurvivalBars.HabitatOxygen.Bar.transform.parent.gameObject.SetActive(isInHabitat);
        this.SurvivalBars.HabitatPower.Bar.transform.parent.gameObject.SetActive(isInHabitat);
    }

    void OnDestroy()
    {
        PostProfile.motionBlur.enabled = false;
    }

    internal void TogglePrinter(bool show)
    {
        Printer.Panel.gameObject.SetActive(show);
        if (show)
        {
            Printer.ToggleAvailable(true);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : Singleton<MenuManager>
{
    public GameObject levelPanel;
    public GameObject setting;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadSetting();
        LoadLevelPanel();
    }
    protected virtual void LoadSetting()
    {
        if (this.setting != null) return;
        this.setting = GameObject.Find("Setting");
    }
    protected virtual void LoadLevelPanel()
    {
        if (this.levelPanel != null) return;
        this.levelPanel = GameObject.Find("LevelSystemUI");
    }
    public void OpenLevelPanel()
    {
        SoundManager.Ins.ButtonSound();
        levelPanel.SetActive(true);
    }
    public void CloseLevelPanel()
    {
        SoundManager.Ins.ButtonSound();
        levelPanel.SetActive(false);
    }
    public void OpenSetting()
    {
        SoundManager.Ins.ButtonSound();
        setting.SetActive(true);
    }
    public void CloseSetting()
    {
        SoundManager.Ins.ButtonSound();
        setting.SetActive(false);
    }
    public void QuitGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

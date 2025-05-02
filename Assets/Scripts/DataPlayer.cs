using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DataPlayer
{
    private const string ALL_DATA = "all_data";
    public static AllData allData;

    public static UnityEvent updateCoinEvent = new();
    public static UnityEvent updateCurrentLevelEvent = new();
    static DataPlayer()
    {
        allData = JsonUtility.FromJson<AllData>(PlayerPrefs.GetString(ALL_DATA));
        if (allData == null)
        {
            allData = new AllData
            {
                maxlevel = 10,
                levelGame = 1,
                unlockLevelGame = 1,
                music = 0.5f,
                sound = 0.5f
            };
            SaveData();
        }
    }
    private static void SaveData()
    {
        var data = JsonUtility.ToJson(allData);
        PlayerPrefs.SetString(ALL_DATA, data);
    }
    public static bool IsMaxLevel(int id)
    {
        return allData.IsMaxLevel(id);
    }
  
    public static void SetLevelGame(int level)
    {
        allData.SetLevelGame(level);
        SaveData();
    }
    public static int GetLevelGame()
    {
        return allData.GetLevelGame();
    }
    public static void SetUnlockLevelGame()
    {
        allData.SetUnlockLevelGame();
        SaveData();
    }
    public static int GetUnlockLevelGame()
    {
        return allData.GetUnlockLevelGame();
    }
    public static void SetMusic(float volume)
    {
        allData.SetMusic(volume);
    }
    public static float GetMusic()
    {
        return allData.GetMusic();
    }
    public static void SetSound(float volume)
    {
        allData.SetSound(volume);
    }
    public static float GetSound()
    {
        return allData.GetSound();
    }
}
public class AllData
{
    public int maxlevel;
    public int levelGame;
    public int unlockLevelGame;
    public float music;
    public float sound;

    public bool IsMaxLevel(int level)
    {
        return level >= maxlevel;
    }
   
    public void SetLevelGame(int level)
    {
        levelGame = level;
    }
    public int GetLevelGame()
    {
        return levelGame;
    }
    public void SetUnlockLevelGame()
    {
        unlockLevelGame += 1;
    }
    public int GetUnlockLevelGame()
    {
        return unlockLevelGame;
    }
    public void SetMusic(float volume)
    {
        music = volume;
    }
    public float GetMusic()
    {
        return music;
    }
    public void SetSound(float volume)
    {
        sound = volume;
    }
    public float GetSound()
    {
        return sound;
    }
}
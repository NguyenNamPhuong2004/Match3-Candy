using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SoundManager : Singleton<SoundManager>
{
    public AudioSource AufxClick;
    public AudioSource AufxBackground;
    public AudioClip buttonClip;
    public AudioClip music;
    public AudioClip clickCandy;
    public AudioClip swapCandy;
    public AudioClip matchCandy;
    public AudioClip colorBomb;
    public AudioClip colorBombCreated;
    public AudioClip wrapped;
    public AudioClip wrappedCreated;
    public AudioClip stripped;
    public AudioClip strippedCreated;
    public AudioClip win;
    public AudioClip lose;
    private void OnValidate()
    {
        if (AufxClick == null)
        {
            AufxClick = gameObject.AddComponent<AudioSource>();
            AufxClick.playOnAwake = false;
        }
    }
    protected override void Awake()
    {
        MakeSingleton(true);
        Music();
    }
    private void FixedUpdate()
    {
        SoundVolume();
        MusicVolume();
    }
    private void SoundVolume()
    {
        AufxClick.volume = DataPlayer.GetSound();
    }
    private void MusicVolume()
    {
        AufxBackground.volume = DataPlayer.GetMusic();
    }
    public void Music()
    {
        AufxBackground.clip = music;
        AufxBackground.playOnAwake = true;
        AufxBackground.loop = true;
    }
    public void ButtonSound()
    {
        AufxClick.PlayOneShot(buttonClip);
    }
    public void ClickCandySound()
    {
        AufxClick.PlayOneShot(clickCandy);
    }
    public void SwapCandySound()
    {
        AufxClick.PlayOneShot(swapCandy);
    }
    public void MatchCandySound()
    {
        AufxClick.PlayOneShot(matchCandy);
    }
    public void ColorBombSound()
    {
        AufxClick.PlayOneShot(colorBomb);
    } 
    public void ColorBombCreatedSound()
    {
        AufxClick.PlayOneShot(colorBombCreated);
    } 
    public void WrappedSound()
    {
        AufxClick.PlayOneShot(wrapped);
    } 
    public void WrappedCreatedSound()
    {
        AufxClick.PlayOneShot(wrappedCreated);
    }
    public void StrippedSound()
    {
        AufxClick.PlayOneShot(stripped);
    } 
    public void StrippedCreatedSound()
    {
        AufxClick.PlayOneShot(strippedCreated);
    } 
    public void WinSound()
    {
        AufxClick.PlayOneShot(win);
    } 
    public void LoseSound()
    {
        AufxClick.PlayOneShot(lose);
    } 
}

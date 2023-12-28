using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource source;
    public AudioSource musicSource;

    public AudioClip clickS;
    public AudioClip backS;
    public AudioClip lobbyMusicS;
    public AudioClip foundMatchS;
    public AudioClip transitionInS;
    public AudioClip transitionOutS;
    public AudioClip inGameMusicS;
    public AudioClip matchEndS;
    public AudioClip victoryS;
    public AudioClip deteatS;

    public AudioClip promotePieceS;
    public AudioClip capturePieceS;
    public AudioClip movePieceS;
    public AudioClip[] emotesS;



    public Toggle musicT;
    public Toggle soundT;

    bool isMusic = false;
    bool isSound = false;


    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        initializeAudio();
    }
    public void initializeAudio()
    {
        musicT.isOn = PlayerPrefs.GetString("MUSIC", "True") == "True" ? true : false;
        soundT.isOn = PlayerPrefs.GetString("SOUND", "True") == "True" ? true : false;

        isMusic = musicT.isOn;
        isSound = soundT.isOn;

        if (!isMusic) musicSource.mute = true;

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            musicSource.clip = lobbyMusicS;
            musicSource.Play();
        }
        else if(SceneManager.GetActiveScene().buildIndex == 2)
        {
            musicSource.clip = inGameMusicS;
            musicSource.Play();
            transitionOut();
        }
    }

    public void music()
    {
        PlayerPrefs.SetString("MUSIC", musicT.isOn.ToString());
        if (musicT.isOn) musicSource.mute = false;
        else musicSource.mute = true;
        click();
    }
    public void sound()
    {
        PlayerPrefs.SetString("SOUND", soundT.isOn.ToString());
        isSound = soundT.isOn;
        click();
    }


    public void click()
    {
        if (!isSound) return;
        source.PlayOneShot(clickS);
    }
    public void back()
    {
        if (!isSound) return;
        source.PlayOneShot(backS);
    }
    public void matchFound()
    {
        if (!isSound) return;
        source.PlayOneShot(foundMatchS);
    }
    public void transitionIn()
    {
        if (!isSound) return;
        source.PlayOneShot(transitionInS);
    }
    public void transitionOut()
    {
        if (!isSound) return;
        source.PlayOneShot(transitionOutS);
    }
    public void matchEnd()
    {
        if (!isSound) return;
        source.PlayOneShot(matchEndS);
    }
    public void victory()
    {
        if (!isSound) return;
        source.PlayOneShot(victoryS);
    }
    public void defeat()
    {
        if (!isSound) return;
        source.PlayOneShot(deteatS);
    }


    public void movePiece()
    {
        if (!isSound) return;
        source.PlayOneShot(movePieceS);
    }
    public void capturePiece()
    {
        if (!isSound) return;
        source.PlayOneShot(capturePieceS);
    }
    public void promotePiece()
    {
        if (!isSound) return;
        source.PlayOneShot(promotePieceS);
    }

    public void emote(int soundId)
    {
        if (!isSound) return;
        source.PlayOneShot(emotesS[soundId]);
    }
}

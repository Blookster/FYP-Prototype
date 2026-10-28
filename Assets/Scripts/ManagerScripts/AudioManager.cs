using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    List<AudioSource> audios = new List<AudioSource>();
    public AudioClip hitSfx;
    public AudioClip missSfx;

    private void Awake()
    {
        if (instance != null) Destroy(this);
        else if (instance == null) instance = this;
    }

    private void Start()
    {
        for (int i=0;i<3;i++)
        {
            AudioSource audio = this.gameObject.AddComponent<AudioSource>();
            audios.Add(audio);
        }
    }

    public void Play(int index, string name, bool isLoop)
    {
        AudioClip clip = GetAudioClip(name);
        if (clip != null)
        {
            AudioSource audio = audios[index];
            audio.clip = clip;
            audio.loop = isLoop;
            audio.Play();
        }
    }

    AudioClip GetAudioClip(string name)
    {
        switch (name)
        {
            case "hitSfx":
            return hitSfx;

            case "missSfx":
            return missSfx;

            default:
            Debug.LogError("Cannot find Sfx");
            return null;
        }
    }


}

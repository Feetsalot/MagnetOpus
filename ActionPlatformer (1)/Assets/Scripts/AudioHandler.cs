using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public class AudioHandler : MonoBehaviour
{
    public SoundScript[] sounds; //Creates an array with the properties from SoundScript.cs

    void Awake ()
    {
        foreach (SoundScript s in sounds) //Set each element in SoundScript array to match the settings made in the inspector
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    //CHECK
    void Start ()
    {

        // Create a temporary reference to the current scene.
        Scene currentScene = SceneManager.GetActiveScene();

        // Retrieve the name of this scene.
        string sceneName = currentScene.name;

        if (sceneName == "Main Menu")
        {
            DelayedPlayLong("Voice_Welcome");
            Play("Menu Music");
        }
        else
        {
            Play("Game Music");
        }
    }

    public void Play (string name) //Reference .Play to play current sound
    {
        SoundScript s = Array.Find(sounds, sound => sound.name == name);
        if (s != null)
        {
            if(s.source != null) s.source.Play();
        }
    }

    public void Stop (string name) //Reference .Stop to stop current sound
    {
        SoundScript s = Array.Find(sounds, sound => sound.name == name);
        if (s != null) s.source.Stop();
    }

    public void DelayedPlay (string name) //Reference .DelayedPlay to play a sound after a short 1/2 second delay
    {
        SoundScript s = Array.Find(sounds, sound => sound.name == name);
        if (s != null) s.source.PlayDelayed(0.5f);
    }

    public void DelayedPlayLong(string name)
    {
        SoundScript s = Array.Find(sounds, sound => sound.name == name);
        if (s != null) s.source.PlayDelayed(4f);
    }
}
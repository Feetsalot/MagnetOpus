using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]

public class SoundScript //Holds values that appear in the inspector for the AudioHandler
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

}

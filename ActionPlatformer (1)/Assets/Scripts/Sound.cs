﻿using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioClip clip;

    public float volume;
    public float pitch;
}
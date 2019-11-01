using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] audioClipArray;

    void Start()
    {
        audioSource.clip = audioClipArray[Random.Range(0, audioClipArray.Length)];
        audioSource.PlayOneShot(audioSource.clip);
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
}

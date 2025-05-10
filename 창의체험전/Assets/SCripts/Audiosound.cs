using UnityEngine;

public class PianoKey : MonoBehaviour
{
    public AudioClip noteSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = noteSound;
    }

    void OnMouseDown()
    {
        audioSource.Play();
    }
}

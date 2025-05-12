using UnityEngine;
using UnityEngine.EventSystems; 

public class PianoKey : MonoBehaviour, IPointerDownHandler
{
    public AudioClip noteSound;
    private AudioSource audioSource;
    private Renderer keyRenderer;
    private Color originalColor;
    public Color pressedColor = Color.gray;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = noteSound;

        keyRenderer = GetComponent<Renderer>();
        if (keyRenderer != null)
        {
            originalColor = keyRenderer.material.color;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer Down!");
        audioSource.Play();

        if (keyRenderer != null)
        {
            keyRenderer.material.color = pressedColor;
        }
    }
}

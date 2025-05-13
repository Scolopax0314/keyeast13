using UnityEngine;
using UnityEngine.EventSystems;

public class PianoKey : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public AudioClip noteSound;
    public Color pressedColor = Color.gray;

    private AudioSource audioSource;
    private Renderer keyRenderer;
    private Color originalColor;

    void Start()
    {
        // ����� �ҽ� ����
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = noteSound;

        // ���� ���� �ʱ�ȭ
        keyRenderer = GetComponent<Renderer>();
        if (keyRenderer != null)
        {
            originalColor = keyRenderer.material.color;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Key Pressed");

        // ���� ���
        audioSource.Play();

        // ���� ����
        if (keyRenderer != null)
        {
            keyRenderer.material.color = pressedColor;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Key Released");

        // ���� ����
        if (keyRenderer != null)
        {
            keyRenderer.material.color = originalColor;
        }
    }
}

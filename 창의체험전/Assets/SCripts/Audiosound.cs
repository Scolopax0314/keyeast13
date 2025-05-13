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
        // 오디오 소스 세팅
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = noteSound;

        // 색상 관련 초기화
        keyRenderer = GetComponent<Renderer>();
        if (keyRenderer != null)
        {
            originalColor = keyRenderer.material.color;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Key Pressed");

        // 사운드 재생
        audioSource.Play();

        // 색상 변경
        if (keyRenderer != null)
        {
            keyRenderer.material.color = pressedColor;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Key Released");

        // 색상 복구
        if (keyRenderer != null)
        {
            keyRenderer.material.color = originalColor;
        }
    }
}

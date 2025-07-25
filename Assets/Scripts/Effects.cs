using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.Audio;

// This script detects when a UI Button is hovered or not and changes the Outline glow intensity accordingly.
public class Effect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    bool isclicked = false;

    public float duration = 1f;

    [Range(0, 1)]
    public float m1 = 0.13f;
    [Range(0, 1)]
    public float m2 = 0.35f;
    [Range(0, 1)]
    public float m3 = 0.76f;

    // Reference to multiple Outline components for layered glow effect
    public Outline o1;
    public Outline o2;
    public Outline o3;

    public AudioSource h;   // hover
    public AudioSource c;   // click
    public AudioMixer mixer; // Mixer

    void Awake()
    {
        h.volume = PlayerPrefs.GetFloat("ui", 1);
        c.volume = PlayerPrefs.GetFloat("ui", 1);
        float master = PlayerPrefs.GetFloat("master", 1);

        float dB = Mathf.Log10(Mathf.Clamp(master, 0.0001f, 1f)) * 20f;
        mixer.SetFloat("MasterVolume", dB);
    }

    // Coroutine to gradually increase glow on hover
    IEnumerator HoverEnter()
    {
        Color c1 = o1.effectColor;
        Color c2 = o2.effectColor;
        Color c3 = o3.effectColor;

        float t = 0;

        // Animate the alpha values to give glowing effect over 2 seconds
        while (t < duration)
        {
            t += Time.deltaTime;

            // Smoothly interpolate alpha to target values
            c1.a = Mathf.Lerp(c1.a, m1, t / duration);
            c2.a = Mathf.Lerp(c2.a, m2, t / duration);
            c3.a = Mathf.Lerp(c3.a, m3, t / duration);

            o1.effectColor = c1;
            o2.effectColor = c2;
            o3.effectColor = c3;

            yield return null;
        }
    }

    // Coroutine to fade out the glow quickly when hover ends
    IEnumerator HoverExit()
    {
        Color c1 = o1.effectColor;
        Color c2 = o2.effectColor;
        Color c3 = o3.effectColor;

        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;

            c1.a = Mathf.Lerp(c1.a, 0f, t / duration);
            c2.a = Mathf.Lerp(c2.a, 0f, t / duration);
            c3.a = Mathf.Lerp(c3.a, 0f, t / duration);
            
            o1.effectColor = c1;
            o2.effectColor = c2;
            o3.effectColor = c3;

            yield return null;
        }
    }

    IEnumerator ChangeScene(int i)
    {
        c.Play();
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(i);
    }

    IEnumerator DelayedSound()
    {
        yield return new WaitForSeconds(0.1f);
        h.Play();
    }

    // Triggered when the mouse enters the button area
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isclicked)
        {
            StopAllCoroutines(); // Ensure no previous animation interferes
            StartCoroutine(HoverEnter());
            StartCoroutine(DelayedSound());
        }
    }

    // Triggered when the mouse exits the button area
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isclicked)
        {
            StopAllCoroutines(); // Stop hover animation before starting exit animation
            StartCoroutine(HoverExit());
        }
    }

    public void Play()
    {
        isclicked = true;
        StartCoroutine(ChangeScene(1));
    }

    public void Credit()
    {
        isclicked = true;
        StartCoroutine(ChangeScene(2));
    }

    public void Setting()
    {
        isclicked = true;
        StartCoroutine(ChangeScene(3));
    }

    public void Exit()
    {
        isclicked = true;
        StartCoroutine(WaitBeforeExit());
    }

    IEnumerator WaitBeforeExit()
    {
        c.Play();
        yield return new WaitForSeconds(0.2f);
        Application.Quit();
    }
}

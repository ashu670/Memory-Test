using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public AudioSource audioSource;
    public bool CheckForTut = false;
    public bool gameOver = false;

    public float fadeDuration = 1.5f;
    public float restartDelay = 5f;

    private float currentVolume = 0f;
    private bool isRestarting = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            audioSource.loop = true;
            currentVolume = GetSavedVolume();
            audioSource.volume = currentVolume;
            audioSource.Play();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Restart music if it stopped
        if (!audioSource.isPlaying && !isRestarting)
        {
            isRestarting = true;
            StartCoroutine(RestartMusicAfterDelay());
        }

        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            if (!Mathf.Approximately(currentVolume, 0.5f))
            {
                currentVolume = 0.5f;
                StopAllCoroutines();
                StartCoroutine(FadeToVolume(currentVolume, 2));
            }
        }
        else
        {
            // Fall back to PlayerPrefs volume if not in Scene 2

            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                if (CheckForTut)
                {
                    if (!Mathf.Approximately(currentVolume, 0.3f))
                    {
                        currentVolume = 0.3f;
                        StopAllCoroutines();
                        StartCoroutine(FadeToVolume(currentVolume, 2));
                    }
                }
                else
                {
                    float target = GetSavedVolume();
                    if (!Mathf.Approximately(currentVolume, target))
                    {
                        currentVolume = target;
                        StopAllCoroutines();
                        StartCoroutine(FadeToVolume(currentVolume, 0.5f));
                    }
                }

                if (gameOver)
                {
                    if (!Mathf.Approximately(currentVolume, 0f))
                    {
                        currentVolume = 0f;
                        StopAllCoroutines();
                        StartCoroutine(FadeToVolume(currentVolume, 0.2f));
                    }
                }
                else
                {
                    float target = GetSavedVolume();
                    if (!Mathf.Approximately(currentVolume, target))
                    {
                        currentVolume = target;
                        StopAllCoroutines();
                        StartCoroutine(FadeToVolume(currentVolume, 0.5f));
                    }
                }
            }
            else
            {
                float target = GetSavedVolume();
                if (!Mathf.Approximately(currentVolume, target))
                {
                    currentVolume = target;
                    StopAllCoroutines();
                    StartCoroutine(FadeToVolume(currentVolume, 0.5f));
                }
            }
        }
    }

    float GetSavedVolume()
    {
        return PlayerPrefs.GetFloat("music", 0.6f);
    }

    IEnumerator FadeToVolume(float targetVol, float dur)
    {
        float startVol = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVol, targetVol, elapsed / dur);
            yield return null;
        }

        audioSource.volume = targetVol;
    }

    IEnumerator RestartMusicAfterDelay()
    {
        yield return new WaitForSecondsRealtime(restartDelay);
        audioSource.Play();
        audioSource.volume = 0f;
        currentVolume = GetSavedVolume();
        StartCoroutine(FadeToVolume(currentVolume, fadeDuration));
        isRestarting = false;
    }
}

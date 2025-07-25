using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenLink : MonoBehaviour
{
    public AudioSource a;

    private void Awake()
    {
        a.volume = PlayerPrefs.GetFloat("ui", 1);
    }

    public void Github()
    {
        Application.OpenURL("https://github.com/ashu670");
    }

    public void Linkdin()
    {
        Application.OpenURL("https://www.linkedin.com/in/abhay-lal-729b7626b/");
    }

    public void Back()
    {
        SceneTracker t = GetComponent<SceneTracker>();
        StartCoroutine(ChangeScene(t.LastScene));
    }

    IEnumerator ChangeScene(int i)
    {
        a.Play();
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(i);
    }
}

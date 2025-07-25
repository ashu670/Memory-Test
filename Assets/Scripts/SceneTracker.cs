using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTracker : MonoBehaviour
{
    public static int ls;
    public int LastScene;

    private void Awake()
    {
        if(SceneManager.GetActiveScene().buildIndex != 2)
        {
            ls = SceneManager.GetActiveScene().buildIndex;
        }

        LastScene = ls;
    }
}

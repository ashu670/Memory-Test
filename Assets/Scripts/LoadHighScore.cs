using TMPro;
using UnityEngine;

public class LoadHighScore : MonoBehaviour
{
    public TextMeshProUGUI hs;

    private void Awake()
    {
        int h = PlayerPrefs.GetInt("HighScore", 0);

        hs.text = "" + h;
    }
}

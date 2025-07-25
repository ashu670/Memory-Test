using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    Vector2 dropPos;

    // GamePlay
    public Slider dif;
    public TMP_InputField difVal;
    public Transform drop;
    public Transform arrow;
    int rt = 0;

    // Sound
    public Slider master;
    public TMP_InputField mVal;
    public Slider music;
    public TMP_InputField msVal;
    public Slider ui;
    public TMP_InputField uiVal;
    public Slider game;
    public TMP_InputField gameVal;

    public AudioSource ad;
    public AudioSource wh;
    public AudioMixer mix;

    float lastClick = -1f;
    float coolDown = 0.2f;

    private void Awake()
    {
        dropPos = drop.position;

        LoadPrefs();
    }

    void Start()
    {
        // Load slider values to input fields
        difVal.text = "" + dif.value;
        mVal.text = "" + Mathf.Round(master.value * 100);
        msVal.text = "" + Mathf.Round(music.value * 100);
        uiVal.text = "" + Mathf.Round(ui.value * 100);
        gameVal.text = "" + Mathf.Round(game.value * 100);

        // Link slider and input field listeners
        dif.onValueChanged.AddListener(DifValueChanged);
        master.onValueChanged.AddListener(MValueChanged);
        music.onValueChanged.AddListener(MsValueChanged);
        ui.onValueChanged.AddListener(UiValueChanged);
        game.onValueChanged.AddListener(GameValueChanged);

        difVal.onEndEdit.AddListener(DifEndEdit);
        mVal.onEndEdit.AddListener(MEndEdit);
        msVal.onEndEdit.AddListener(MsEndEdit);
        uiVal.onEndEdit.AddListener(UiEndEdit);
        gameVal.onEndEdit.AddListener(GameEndEdit);
    }

    public void SliderAudio()
    {
        ad.Play();
    }

    // Update input field when slider changes
    void DifValueChanged(float val)
    {
        difVal.text = Mathf.RoundToInt(val).ToString();
    }

    void MValueChanged(float val)
    {
        mVal.text = Mathf.RoundToInt(val * 100).ToString();
        float dB = Mathf.Log10(Mathf.Clamp(val, 0.0001f, 1f)) * 20f;
        mix.SetFloat("MasterVolume", dB);

    }

    void MsValueChanged(float val)
    {
        msVal.text = Mathf.RoundToInt(val * 100).ToString();
        PlayerPrefs.SetFloat("music", val);
    }

    void UiValueChanged(float val)
    {
        uiVal.text = Mathf.RoundToInt(val * 100).ToString();
        ad.volume = val;
        wh.volume = val;
    }

    void GameValueChanged(float val)
    {
        gameVal.text = Mathf.RoundToInt(val * 100).ToString();
    }

    // Update slider when input field edited
    void DifEndEdit(string input)
    {
        if (int.TryParse(input, out int value))
        {
            if (value > 10)
            {
                value = 10;
                difVal.text = value.ToString();
            }
            dif.value = value;
            ad.Play();
        }
    }

    void MEndEdit(string input)
    {
        if (float.TryParse(input, out float value))
        {
            if (value > 100)
            {
                value = 100;
                mVal.text = Mathf.RoundToInt(value).ToString();
            }
            master.value = value / 100;
            ad.Play();
        }
    }

    void MsEndEdit(string input)
    {
        if (float.TryParse(input, out float value))
        {
            if (value > 100)
            {
                value = 100;
                msVal.text = Mathf.RoundToInt(value).ToString();
            }
            music.value = value / 100;
            ad.Play();
        }
    }

    void UiEndEdit(string input)
    {
        if (float.TryParse(input, out float value))
        {
            if (value > 100)
            {
                value = 100;
                uiVal.text = Mathf.RoundToInt(value).ToString();
            }
            ui.value = value / 100;
            ad.Play();
        }
    }

    void GameEndEdit(string input)
    {
        if (float.TryParse(input, out float value))
        {
            if (value > 100)
            {
                value = 100;
                gameVal.text = Mathf.RoundToInt(value).ToString();
            }
            game.value = value / 100;
            ad.Play();
        }
    }

    // Animate arrow rotation
    IEnumerator RotateArrow(float angle)
    {
        float startZ = arrow.eulerAngles.z;
        float elapsed = 0f;

        while (elapsed < 0.2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float z = Mathf.LerpAngle(startZ, angle, elapsed / 0.2f);
            arrow.rotation = Quaternion.Euler(0, 0, z);
            yield return null;
        }

        arrow.rotation = Quaternion.Euler(0, 0, angle); // Snap to target angle
    }

    IEnumerator DropBoxTransition(int t)
    {
        Vector2 from = (t == 1) ? Vector2.zero : Vector2.one;
        Vector2 to = (t == 1) ? Vector2.one : Vector2.zero;

        Vector2 fromPos = (t == 1) ? arrow.position : dropPos;
        Vector2 toPos = (t == 1) ? dropPos : arrow.position;

        float elapsed = 0f;
        float dur = 0.2f;

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            Vector2 s = Vector2.Lerp(from, to, elapsed / dur);
            Vector2 pos = Vector2.Lerp(fromPos, toPos, elapsed / 0.2f);
            drop.position = pos;
            drop.localScale = s;
            yield return null;
        }

        drop.localScale = to;

        if (t != 1)
        {
            drop.gameObject.SetActive(false);
            drop.position = dropPos;
        }
    }

    // Simple scene transition coroutine
    IEnumerator ChangeScene(int i)
    {
        ad.Play();
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(i);
    }

    // Ensures PlayerPrefs has time to save before scene change
    IEnumerator DelayedSceneChange(int i)
    {
        ad.Play();
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(i);
    }

    // Save current values to PlayerPrefs
    public void SavePrefs()
    {
        PlayerPrefs.SetInt("difficulty", Mathf.RoundToInt(dif.value));
        PlayerPrefs.SetFloat("master", master.value);
        PlayerPrefs.SetFloat("music", music.value);
        PlayerPrefs.SetFloat("ui", ui.value);
        PlayerPrefs.SetFloat("game", game.value);

        PlayerPrefs.Save(); // Force save
    }

    void LoadPrefs()
    {
        dif.value = PlayerPrefs.GetInt("difficulty", 3);
        master.value = PlayerPrefs.GetFloat("master", 1);
        music.value = PlayerPrefs.GetFloat("music", 1);
        ui.value = PlayerPrefs.GetFloat("ui", 1);
        game.value = PlayerPrefs.GetFloat("game", 1);
        ad.volume = ui.value;
        wh.volume = ui.value;
        float dB = Mathf.Log10(Mathf.Clamp(master.value, 0.0001f, 1f)) * 20f;
        mix.SetFloat("MasterVolume", dB);
    }

    // Toggle rules dropdown panel
    public void RuleToggle()
    {
        float t = Time.unscaledTime - lastClick;

        if (t >= coolDown)
        {
            lastClick = Time.unscaledTime;
            if (rt == 0)
            {
                rt = 1;

                wh.Play();
                drop.gameObject.SetActive(true);
                StartCoroutine(DropBoxTransition(rt));
                StartCoroutine(RotateArrow(180));
            }
            else
            {
                rt = 0;

                wh.Play();
                StartCoroutine(DropBoxTransition(rt));
                StartCoroutine(RotateArrow(0));
            }
        }
    }

    // Load main menu scene
    public void Back()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            SavePrefs();

            GameObject manager = GameObject.FindWithTag("GameController");
            GameManager gm = manager.GetComponent<GameManager>();
            if (gm != null)
            {
                gm.Setting();
            }
        }
        else
        {
            StartCoroutine(ChangeScene(0));
        }
    }

    // Load credits scene
    public void Credits()
    {
        StartCoroutine(ChangeScene(2));
    }

    public void Reset()
    {
        ad.Play();
        dif.value = 3;
        master.value = 1;
        music.value = 1;
        ui.value = 1;
        game.value = 1;
        ad.volume = ui.value;
        wh.volume = ui.value;

        PlayerPrefs.SetInt("difficulty", Mathf.RoundToInt(dif.value));
        PlayerPrefs.SetFloat("master", master.value);
        PlayerPrefs.SetFloat("music", music.value);
        PlayerPrefs.SetFloat("ui", ui.value);
        PlayerPrefs.SetFloat("game", game.value);

        PlayerPrefs.Save(); 
    }

    // Save and exit
    public void Save()
    {
        SavePrefs();
        StartCoroutine(DelayedSceneChange(0));
    }
}

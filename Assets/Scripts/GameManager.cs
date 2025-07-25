using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    // Index variables
    int r = 0;              // Randomly chosen color index
    int c = 0;              // Player's current click count
    int score = 0;          // Current score
    int hint = 3;           // Hint count
    int highScore;          // High Score
    int dt = 0;             // Setting drop menu toggle
    int size;               // size of transforms
    bool flashing = false;
    float lastclickS = -1f;
    float coolDown = 0.2f;

    // Game state flags
    bool seqDone = true;    // Whether the sequence display is done
    bool PlayerTurn = false;
    bool gameOver = false;
    bool waitForTut = false;

    // Color sequence and color mapping
    List<int> seq = new List<int>();
    Dictionary<string, int> map;
    Vector2 setPos;         // Setting drop box position
    Vector2 setSca;         // Setting drop box Scale
    Transform tut;
    MusicManager mm;

    [Range(1, 10)]
    public int dif = 1;

    // Highlight and base colors for buttons
    Color red, redh, blue, blueh, green, greenh, yellow, yellowh, redgo, bluego, greengo, yellowgo;

    // References to UI
    public Button[] colors;                  // 0: Red, 1: Blue, 2: Green, 3: Yellow
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI getReady;

    public AudioSource go;
    public AudioSource ting;
    public AudioSource cla;
    public AudioSource sl;
    public AudioSource woosh;

    public AudioMixer mixer;

    public Transform settingDrop;
    public Transform SettingButn;
    public Transform[] transforms;

    public Transform tutP;
    public Transform tutParent;

    private void Awake()
    {
        mm = GameObject.Find("MusicManager").GetComponent<MusicManager>();
        mm.gameOver = gameOver;

        gameOverText.enabled = false;
        LoadPrefs();

        size = transforms.Length;

        StartCoroutine(FirstTimeVerify());
    }

    IEnumerator FirstTimeVerify()
    {
        yield return StartCoroutine(InitSettingDrop());

        if (PlayerPrefs.GetInt("firstTime") == 0)
        {
            getReady.gameObject.SetActive(false);
            tut = GameObject.Instantiate(tutP, tutParent);
            PlayerPrefs.SetInt("firstTime", 1);
            PlayerPrefs.Save();

            Time.timeScale = 0;
        }
    }

    private IEnumerator InitSettingDrop()
    {
        settingDrop.gameObject.SetActive(true);
        yield return null; // Wait 1 frame

        setPos = settingDrop.position;
        setSca = settingDrop.localScale;

        settingDrop.gameObject.SetActive(false);
    }

    void LoadPrefs()
    {
        dif = PlayerPrefs.GetInt("difficulty", 3);
        go.volume = PlayerPrefs.GetFloat("game", 1);
        ting.volume = PlayerPrefs.GetFloat("game", 1);
        cla.volume = PlayerPrefs.GetFloat("game", 1);
        sl.volume = PlayerPrefs.GetFloat("ui", 1);
        float master = PlayerPrefs.GetFloat("master", 1);
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        woosh.volume = PlayerPrefs.GetFloat("ui", 1);

        float dB = Mathf.Log10(Mathf.Clamp(master, 0.0001f, 1f)) * 20f;
        mixer.SetFloat("MasterVolume", dB);
    }

    void Start()
    {
        // Initialize all required colors
        ColorUtility.TryParseHtmlString("#FF0000", out red);
        ColorUtility.TryParseHtmlString("#001EFF", out blue);
        ColorUtility.TryParseHtmlString("#00AA1E", out green);
        ColorUtility.TryParseHtmlString("#BC9D00", out yellow);

        ColorUtility.TryParseHtmlString("#FFECE5", out redh);
        ColorUtility.TryParseHtmlString("#CCEEFF", out blueh);
        ColorUtility.TryParseHtmlString("#D8FFDB", out greenh);
        ColorUtility.TryParseHtmlString("#FFFCED", out yellowh);

        ColorUtility.TryParseHtmlString("#190000", out redgo);
        ColorUtility.TryParseHtmlString("#000524", out bluego);
        ColorUtility.TryParseHtmlString("#08190B", out greengo);
        ColorUtility.TryParseHtmlString("#191500", out yellowgo);

        // Map button names to their sequence index
        map = new Dictionary<string, int>()
        {
            { "Red", 0 }, { "Blue", 1 }, { "Green", 2 }, { "Yellow", 3 }
        };
    }

    void Update()
    {
        Debug.Log(Time.timeScale);
        // Automatically add a new color to sequence once previous round ends
        if (seqDone && !PlayerTurn && !gameOver)
        {
            //Debug.Log("Working");
            r = Random.Range(0, 4);
            seq.Add(r);
            seqDone = false;
            StartCoroutine(PlaySeq());
        }

        // Enable buttons only during player's turn
        if (PlayerTurn && !gameOver)
        {
            foreach (Button b in colors)
                b.enabled = true;
        }
    }

    IEnumerator PlaySeq()
    {
        // Disable all buttons while showing sequence
        foreach (Button b in colors)
            b.enabled = false;

        // Show the color sequence one by one
        for (int i = 0; i < seq.Count; i++)
        {
            //Debug.Log("Play: " + seq[i]);
            if (seq.Count == 1 || waitForTut)
            {
                yield return new WaitForSeconds(2f);
                size--;
                Destroy(getReady.gameObject);
            }

            yield return StartCoroutine(FlashColorRoutine(seq[i]));
            yield return new WaitForSeconds(0.1f);
        }

        // Now player's turn
        PlayerTurn = true;
    }

    IEnumerator EndColor()     // At gameover all color changes to a dull dark color
    {
        float t = 0f;

        while (t < 1)
        {
            t += 3f * Time.unscaledDeltaTime;

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].GetComponent<Image>().color = Color.Lerp(GetBaseColor(i), GetEndColor(i), t);
            }

            yield return null;
        }

        // Transition of game over text.
        t = 0f;

        while (t < 0.5f)
        {
            t += Time.unscaledDeltaTime;

            Color col = gameOverText.color;
            float a = Mathf.Lerp(0, 1, t / 0.5f);
            col.a = a;
            gameOverText.color = col;

            yield return null;
        }


    }

    IEnumerator FlashColorRoutine(int s)
    {
        float t = 0f;
        flashing = true;

        // Lerp to highlight color
        while (t < 1)
        {
            t += dif * Time.deltaTime;
            colors[s].GetComponent<Image>().color = Color.Lerp(GetBaseColor(s), GetHighlightColor(s), t);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        t = 0f;

        // Lerp back to base color
        while (t < 1)
        {
            t += dif * Time.deltaTime;
            colors[s].GetComponent<Image>().color = Color.Lerp(GetHighlightColor(s), GetBaseColor(s), t);
            yield return null;
        }

        flashing = false;
    }

    IEnumerator WaitForNextRound()
    {
        yield return new WaitForSeconds(1f);
        seqDone = true; // Triggers next round in Update()
    }

    IEnumerator WaitForAudio()
    {
        go.Play();
        yield return new WaitForSeconds(0.6f);
    }

    IEnumerator Transition()
    {
        yield return StartCoroutine(WaitForAudio());
        yield return StartCoroutine(EndColor());
    }

    public void PlayerInputCheck()
    {
        GameObject click = EventSystem.current.currentSelectedGameObject;

        if (click != null && map.ContainsKey(click.name))
        {
            int s = map[click.name];

            // Check if clicked button matches sequence
            if (s != seq[c - 1])
            {
                gameOver = true;
                gameOverText.enabled = true;
                mm.gameOver = true;

                StartCoroutine(Transition());
                DisableAllButtons();
                return;
            }

            // If entire sequence completed
            if (c >= seq.Count && !gameOver)
            {
                PlayerTurn = false;
                c = 0;
                score++;
                scoreText.text = "Score: " + score;
                if(score > highScore)
                {
                    PlayerPrefs.SetInt("HighScore", score);
                }
                ting.Play();
                DisableAllButtons();
                StartCoroutine(WaitForNextRound());
            }
        }
    }

    //Buttons

    public void ClickCountUpdate()
    {
        c++; // Increase count first
        if (c < seq.Count)
        {
            cla.Play();
        }
        PlayerInputCheck();
    }

    public void Hint()
    {
        if(hint >= 1 && PlayerTurn && !gameOver && !flashing)       // Check if hint count is not over
        {
            sl.Play();
            hint--;         // Decreasing hint count
            hintText.text = "x" + hint;
            StartCoroutine(FlashColorRoutine(seq[c]));      // Flashing right color for hint
        }
    }

    public void Refresh()
    {
        sl.Play();
        StartCoroutine(ChangeScene(SceneManager.GetActiveScene().buildIndex));
    }

    public void Home()
    {
        sl.Play();
        StartCoroutine(ChangeScene(0));
    }

    public void Setting()
    {
        float t = Time.unscaledTime - lastclickS;

        if (t >= coolDown)
        {
            woosh.Play();
            lastclickS = Time.unscaledTime;

            if (dt == 0)
            {
                dt++;
                Time.timeScale = 0;
                settingDrop.gameObject.SetActive(true);
                StartCoroutine(RotateSetting(180, SettingButn));
                settingDrop.rotation = Quaternion.Euler(0, 0, 60);
                StartCoroutine(RotateSetting(0, settingDrop));
                StartCoroutine(SettingDropMenu(dt));
                VanishTrans(dt);

            }
            else
            {
                dt--;
                Time.timeScale = 1;
                StartCoroutine(RotateSetting(0, SettingButn));
                StartCoroutine(RotateSetting(60, settingDrop));
                StartCoroutine(SettingDropMenu(dt));
                VanishTrans(dt);

                SettingUI su = settingDrop.GetComponent<SettingUI>();
                su.SavePrefs();
                LoadPrefs();
            }
        }
    }

    public void Tutorial()
    {
        sl.Play();
        Time.timeScale = 0;

        if (!gameOver)
        {
            waitForTut = true;
            StartCoroutine(InitTutorial());
        }
        else
        {
            StartCoroutine(ResetTutorial());
        }
    }

    public void EndTutorial()
    {
        Time.timeScale = 1;
        Destroy(tut.gameObject);

        waitForTut = false;
        //PlayerTurn = false;
    }

    void VanishTrans(int i)
    {
        for (int k = 0; k < size; k++)
        {
            if(transforms[k] != null)
            {
                transforms[k].gameObject.SetActive(i == 0);
            }
        }
    }

    IEnumerator InitTutorial()
    {
        dt--; ;
        StartCoroutine(RotateSetting(0, SettingButn));
        StartCoroutine(RotateSetting(60, settingDrop));
        StartCoroutine(SettingDropMenu(dt));
        VanishTrans(dt);

        yield return new WaitForSecondsRealtime(0.2f);
        tut = GameObject.Instantiate(tutP, tutParent);
    }
    
    IEnumerator ResetTutorial()
    {
        PlayerPrefs.SetInt("firstTime", 0);
        yield return null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator RotateSetting(float a, Transform t)
    {
        float startz = t.eulerAngles.z;
        float elapsed = 0;
        float dur = 0.2f;

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float z = Mathf.LerpAngle(startz, a, elapsed / dur);
            t.rotation = Quaternion.Euler(0, 0, z);
            yield return null;
        }

        t.rotation = Quaternion.Euler(0, 0, a);
    }

    IEnumerator SettingDropMenu(int t)
    {
        Vector2 from = (t == 1) ? Vector2.zero : setSca;
        Vector2 to = (t == 1) ? setSca : Vector2.zero;

        Vector2 fromPos = (t == 1) ? SettingButn.position : setPos;
        Vector2 toPos = (t == 1) ? setPos : SettingButn.position;

        float dur = 0.2f;
        float elapsed = 0;

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;

            Vector2 s = Vector2.Lerp(from, to, elapsed / dur);
            Vector2 p = Vector2.Lerp(fromPos, toPos, elapsed / dur);
            settingDrop.position = p;
            settingDrop.localScale = s;

            yield return null;
        }

        settingDrop.localScale = setSca;

        if(t != 1)
        {
            settingDrop.gameObject.SetActive(false);
            settingDrop.position = setPos;
        }
    }

    IEnumerator ChangeScene(int i)
    {
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(i);
    }

    // Helpers
    Color GetBaseColor(int i)
    {
        switch (i)
        {
            case 0: return red;
            case 1: return blue;
            case 2: return green;
            case 3: return yellow;
            default: return Color.black;
        }
    }

    Color GetHighlightColor(int i)
    {
        switch (i)
        {
            case 0: return redh;
            case 1: return blueh;
            case 2: return greenh;
            case 3: return yellowh;
            default: return Color.white;
        }
    }

    Color GetEndColor(int i)
    {
        switch (i)
        {
            case 0: return redgo;
            case 1: return bluego;
            case 2: return greengo;
            case 3: return yellowgo;
            default: return Color.gray;
        }
    }

    void DisableAllButtons()
    {
        foreach (Button b in colors)
            b.enabled = false;
    }
}

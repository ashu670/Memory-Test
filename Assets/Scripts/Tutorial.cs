using System.Collections;
using TMPro;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    int i = 0;                      // Tracks current step/view
    int index = 0;                  // Tracks tutorial content index

    public AudioSource sfx;

    public float dur = 0.2f;        // Transition duration

    public Transform[] views;      // Tutorial panels/slides
    public Transform line;         // Line indicator
    public GameObject next;        // Next button
    public TextMeshProUGUI con;    // Text for tutorial content

    private string[] content;      // Tutorial text content
    private Vector2[] linePos;     // Predefined line positions
    private Coroutine lineRoutine; // Reference to line coroutine
    private GameObject manager;    // Game manager
    private GameManager gm;        // Game Manager Script
    private MusicManager mm;       // Music manager script

    private void Awake()
    {
        sfx = GetComponent<AudioSource>();

        mm = GameObject.Find("MusicManager").GetComponent<MusicManager>();
        mm.CheckForTut = true;

        // Activate only the first view initially
        for (int j = 0; j < views.Length; j++)
        {
            if (j > 0)
            {
                views[j].gameObject.SetActive(false);
            }
            else
            {
                views[j].gameObject.SetActive(true);
            }
        }

        // Define tutorial message content
        content = new string[]
        {
            "Restart after losing — or anytime you want!",
            "Need help? Use a hint. BUT… you only get 3!",
            "That mysterious button? It teleports you home!",
            "Want to tweak the game? Change difficulty, volume, and more in settings."
        };

        // Set fixed X positions for line transitions
        Vector3 basePos = line.position;
        linePos = new Vector2[]
        {
            basePos,
            basePos + new Vector3(80, 0, 0),
            basePos + new Vector3(160, 0, 0),
            basePos + new Vector3(240, 0, 0),
        };

        // Display initial content
        con.text = content[index];

        manager = GameObject.FindGameObjectWithTag("GameController");
        gm = manager.GetComponent<GameManager>();
    }

    public void Next()
    {
        sfx.Play();
        if (i < 2)
        {
            // Fade out current view
            Transform view = views[i];
            view.GetChild(0).gameObject.SetActive(false);
            StartCoroutine(TransRout(i, 1));
            i++;

            // Set line to start pos when reaching text tutorial
            if (i == 2)
            {
                line.position = linePos[0];
            }
        }
        else
        {
            i++;
            index = (i - 2) % 5;
            con.text = content[index];

            // Hide next button after last slide
            if (i >= 5)
            {
                next.SetActive(false);
            }

            if (lineRoutine != null)
            {
                StopCoroutine(lineRoutine);
            }
            lineRoutine = StartCoroutine(LineTrans(index));
        }
    }

    public void Prev()
    {
        sfx.Play();
        if (i < 2)
        {
            // Fade out and go back to view 0 or 1
            Transform view = views[i];
            view.GetChild(0).gameObject.SetActive(false);
            StartCoroutine(TransRout(i, 0));
            i--;
        }
        else
        {
            i--;

            if (i < 2)
            {
                // Going back to visual views from text
                Transform view = views[i + 1];
                view.GetChild(0).gameObject.SetActive(false);
                StartCoroutine(TransRout(i + 1, 0));
            }
            else
            {
                index = (i - 2) % 5;
                con.text = content[index];

                // Re-enable next if not on final slide
                if (i < 5)
                {
                    next.SetActive(true);
                }

                if (lineRoutine != null)
                {
                    StopCoroutine(lineRoutine);
                }
                lineRoutine = StartCoroutine(LineTrans(index));
            }
        }
    }

    public void End()
    {
        sfx.Play();
        mm.CheckForTut = false;
        StartCoroutine(WaitBeforeEnd());
    }

    IEnumerator WaitBeforeEnd()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        gm.EndTutorial();
    }

    // Handles line movement to current index's target position
    IEnumerator LineTrans(int i)
    {
        float e = 0f;
        Vector2 start = line.position;
        Vector2 end = linePos[i];

        while (e < dur)
        {
            e += Time.unscaledDeltaTime;
            line.position = Vector2.Lerp(start, end, e / dur);
            yield return null;
        }
    }

    // Handles visual transition between panels with scale/position lerp
    IEnumerator TransRout(int i, int t)
    {
        float e = 0f;

        Transform currentView = views[i];
        Vector2 fromScale = currentView.localScale;
        Vector2 toScale = Vector2.zero;

        Vector2 fromPos = currentView.position;
        Vector2 toPos = (t == 1) ? views[i + 1].position : views[i - 1].position;

        Vector2 originalScale = fromScale;
        Vector2 originalPos = fromPos;

        // Animate shrinking and moving the current view
        while (e < dur)
        {
            e += Time.unscaledDeltaTime;
            currentView.localScale = Vector2.Lerp(fromScale, toScale, e / dur);
            currentView.position = Vector2.Lerp(fromPos, toPos, e / dur);
            yield return null;
        }

        currentView.gameObject.SetActive(false);

        // Show next/previous view after transition
        int targetIndex = (t == 1) ? i + 1 : i - 1;
        Transform nextView = views[targetIndex];
        nextView.gameObject.SetActive(true);

        nextView.GetChild(0).gameObject.SetActive(true);

        // Reset original view's transform for reusability
        currentView.position = originalPos;
        currentView.localScale = originalScale;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogoScreen : MonoBehaviour
{
    // Logos images
    [SerializeField] Image nightWork = null;
    [SerializeField] Image creaJeux = null;
    [SerializeField] Image punch = null;
    [SerializeField] ParticleSystem sys = null;
    float timer = 0f;
    int step = 0;


    [SerializeField] float waitLast = .25f;
    [SerializeField] float punchLast = .5f;
    [SerializeField] float bounceLast = .25f;
    [SerializeField] float fadeOutLast = 1f;

    [SerializeField] int turn = 5;
    [SerializeField] float maxScale = 1f;
    [SerializeField] float minScale = 5f;
    [SerializeField] float spread = 2f;
    [SerializeField] float height = 5f;
    [SerializeField] List<Vector3> points = new List<Vector3>();
    [SerializeField] float linearity = 20f;
    BezierCurve curve = new BezierCurve();
    bool moveOn = false;
    // Start is called before the first frame update
    void Start()
    {
        curve.SetPoints(points);

        Saver.LoadParameter();

        StartCoroutine(SceneLoad());
    }

    IEnumerator SceneLoad()
    {
        string sceneName = Saver.LoadScene();

        AsyncOperation aso_1 = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        AsyncOperation aso_2 = SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Additive);
        
        while(!aso_1.isDone && !aso_2.isDone)
        {
            yield return null;
        }

        moveOn = true;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        switch (step)
        {
            case 0:
                {
                    if (timer >= waitLast)
                    {
                        timer = 0f;
                        ++step;
                        creaJeux.color = new Color(1f, 1f, 1f, 1f);
                        // Play CJ Sound
                        AkSoundEngine.PostEvent("CJ_Play", gameObject); 
                        //
                    }
                }
                break;

            case 1:
                {
                    if (timer >= bounceLast)
                    {
                        timer = bounceLast;
                    }

                    float percent = timer / bounceLast;
                    float scale = -(spread * (percent - .5f)) * (spread * (percent - .5f)) + (spread * .5f) * (spread * .5f) + 1f;
                    creaJeux.rectTransform.localScale = new Vector3(scale, scale, percent);

                    if (timer == bounceLast)
                    {
                        timer = 0f;
                        ++step;
                    }
                }
                break;

            case 2:
                {
                    if (timer >= waitLast * 2f)
                    {
                        timer = 0f;
                        ++step;
                        // Play NW Sound
                        AkSoundEngine.PostEvent("NW_Play", gameObject);
                        //
                    }
                }
                break;

            case 3:
                {
                    if (timer >= punchLast)
                    {
                        timer = punchLast;
                    }

                    float percent = timer / punchLast;
                    nightWork.color = new Color(1f, 1f, 1f, percent);
                    nightWork.rectTransform.eulerAngles = new Vector3(0f, 0f, percent * 360 * turn);

                    float scale = Mathf.Lerp(minScale, maxScale, Mathf.Sqrt(percent));
                    nightWork.rectTransform.localScale = new Vector3(scale, scale, 1f);

                    float PositionEvolution = Mathf.Tan(2 * Mathf.Atan(.5f * linearity) * (percent - .5f)) / linearity + .5f;
                    Vector3 position = curve.GetPosition(PositionEvolution);
                    nightWork.rectTransform.localPosition = position;

                    if (timer == punchLast)
                    {
                        timer = 0f;
                        ++step;
                        punch.color = new Color(1f, 1f, 1f, 1f);
                        creaJeux.color = new Color(0f, 0f, 0f, 0f);
                    }
                }
                break;

            case 4:
                {
                    if (timer >= bounceLast)
                    {
                        timer = bounceLast;
                    }

                    float percent = timer / bounceLast;
                    float scale = -(spread * (percent - .5f)) * (spread * (percent - .5f)) + (spread * .5f) * (spread * .5f) + 1f;
                    nightWork.rectTransform.localScale = new Vector3(scale, scale, percent);

                    punch.color = new Color(1f - percent, 1f - percent, 1f - percent, 1f - percent);

                    if (timer == bounceLast)
                    {
                        timer = 0f;
                        ++step;
                    }
                }
                break;

            case 5:
                {
                    if (moveOn)
                    {
                        if (timer > fadeOutLast)
                        {
                            timer = fadeOutLast;
                        }

                        nightWork.color = new Color(1f, 1f, 1f, fadeOutLast - timer / fadeOutLast);
                        //bckg.color = new Color(0f, 0f, 0f, lastFadeOut - timer / lastFadeOut);

                        if (timer == fadeOutLast)
                        {
                            Scene scene = SceneManager.GetSceneByName("Menu");
                            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                            SceneManager.SetActiveScene(scene);
                            //Start Menu Music
                            MusicsManager.instance.musicState.Add(MusicsManager.MusicKind.Menu);
                            //
                        }
                    }
                    else
                    {
                        timer = 0f;
                    }
                }
                break;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            step = 0;
            timer = 0f;
            curve.SetPoints(points);
            nightWork.color = new Color(0f, 0f, 0f, 0f);
            creaJeux.color = new Color(0f, 0f, 0f, 0f);
            sys.Play();
        }
    }
}

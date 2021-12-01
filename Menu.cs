using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Net;

public class Menu : MonoBehaviour
{
    [SerializeField] Slider[] sliders = new Slider[3];
    bool moveOn = false;
    private void Start()
    {
        sliders[0].value = MusicsManager.volumeMain;
        sliders[1].value = MusicsManager.volumeMusic;
        sliders[2].value = MusicsManager.volumeFX;
    }
    public void Play()
    {
        AkSoundEngine.PostEvent("MenuSelect_Play", gameObject); // Play Click Sound        
        SceneManager.LoadSceneAsync("Permanent", LoadSceneMode.Additive);
        Saver.LoadImportant();
        SceneManager.UnloadSceneAsync("Menu");
    }

    public void NewSave()
    {
       // string unloadBNamScee = Saver.LoadScene();
        Saver.DestroySave();
        
        SceneManager.LoadScene("NightWorkStudio");
        Important.importantList.Clear();
        //if (!moveOn) StartCoroutine(SceneLoad(unloadBNamScee));
    }
    IEnumerator SceneLoad(string _unloadScene)
    {
        
        AsyncOperation aso_2 = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(_unloadScene));
        string sceneName = Saver.LoadScene();

        AsyncOperation aso_1 = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!aso_1.isDone && !aso_2.isDone )
        {
            yield return null;
        }

        moveOn = true;
    }

    /*IEnumerator StartGame()
    {
        AsyncOperation aso = SceneManager.LoadSceneAsync("Permanent", LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync("Menu");

        while (!aso.isDone)
        {
            yield return null;
        }
    }*/

    public void Hide(CanvasGroup _canvas)
    {
        AkSoundEngine.PostEvent("MenuSelect_Play", gameObject); // Play Click Sound
        _canvas.alpha = 0f;
        _canvas.blocksRaycasts = false;
        _canvas.interactable = false;
    }

    public void Show(CanvasGroup _canvas)
    {
        AkSoundEngine.PostEvent("MenuSelect_Play", gameObject); // Play Click Sound
        _canvas.alpha = 1f;
        _canvas.blocksRaycasts = true;
        _canvas.interactable = true;
    }

    public void Quit()
    {
        AkSoundEngine.PostEvent("MenuSelect_Play",gameObject); // Play Click Sound
        Application.Quit();
    }
}

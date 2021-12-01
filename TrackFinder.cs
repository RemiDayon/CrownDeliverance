using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackFinder : MonoBehaviour
{
    [SerializeField] new Camera camera;
    [SerializeField] Cinemachine.CinemachineVirtualCamera cam;
    [SerializeField] Cinemachine.CinemachineDollyCart cart;

    private void Awake()
    {
        //sceneName = Saver.LoadScene();
        /*foreach (GameObject bk in backgrounds)
        {
            if (bk.name == sceneName)
            {
                GameObject bckg = bk;
                SceneManager.MoveGameObjectToScene(bckg, SceneManager.GetSceneByName("Menu"));
                cart.m_Path = bckg.GetComponentInChildren<Cinemachine.CinemachineSmoothPath>();
                cam.GetCinemachineComponent<Cinemachine.CinemachineTrackedDolly>().m_Path = cart.m_Path;
                break;
            }
        }*/
        StartCoroutine(FindTrack());
    }

    IEnumerator FindTrack()
    {
        Cinemachine.CinemachineSmoothPath path = null;
        while (!path)
        {
            GameObject obj = GameObject.Find(Saver.dollyTrack);
            if (obj)
            {
                path = obj.GetComponent<Cinemachine.CinemachineSmoothPath>();               
            }
            yield return null;
        }

        cart.m_Path = path;
        cart.m_Position = 0f;
        cam.GetCinemachineComponent<Cinemachine.CinemachineTrackedDolly>().m_Path = path;
        cam.GetCinemachineComponent<Cinemachine.CinemachineTrackedDolly>().m_PathPosition = 0f;
        cam.Follow = cart.transform;
    }
}

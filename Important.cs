using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Important : MonoBehaviour
{
    public static List<string> importantList = new List<string>();
    // Start is called before the first frame update
    private void Start()
    {
        foreach (string str in importantList)
        {
            if (str == name)
            {
                DestroyImmediate(gameObject);
                return;
            }
        }
    }
    private void OnDestroy()
    {
        importantList.Add(name);
    }
}

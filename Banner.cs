using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Banner : MonoBehaviour
{
    [SerializeField] Material mat = null;
    [SerializeField] Texture2D tex = null;
    float noisyTime = 0f;
    float noise = 0f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        mat.SetFloat("pivot", transform.position.y);
        noise = tex.GetPixel((int)(Time.timeSinceLevelLoad * .25f), 0).r;
        noisyTime += Time.deltaTime + noise;
        mat.SetFloat("noise", noise);
        mat.SetFloat("noisyTime", noisyTime);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]

public class framerate : MonoBehaviour
{
    Text fpsMeter;
    // Start is called before the first frame update
    void Start()
    {
        fpsMeter = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        fpsMeter.text = (int) (1 / Time.deltaTime) + " FPS";
    }
}

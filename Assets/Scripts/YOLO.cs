using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Barracuda;
using UnityEngine;

public class YOLO : MonoBehaviour
{
    public NNModel model;

    public Camera inputCamera;

    public TMP_Text outputText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Update text with the model output
        outputText.text = "a";
    }
}

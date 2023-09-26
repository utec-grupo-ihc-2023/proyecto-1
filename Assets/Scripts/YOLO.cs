using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Barracuda;
using UnityEngine;

public class YOLO : MonoBehaviour
{
    public NNModel nnModel;

    public Camera inputCamera;

    public TMP_Text outputText;

    Model model;
    IWorker worker;

    // Start is called before the first frame update
    void Start()
    {
        model = ModelLoader.Load(nnModel);
        worker = WorkerFactory.CreateWorker(model);
    }

    // Update is called once per frame
    void Update()
    {

        // TODO: Update text with the model output
        outputText.text = "a";
    }

    void OnDestroy()
    {
        worker.Dispose();
    }
}

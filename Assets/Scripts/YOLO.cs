using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class YOLO : MonoBehaviour
{
    public NNModel nnModel;

    public ARCameraManager inputCamera;

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
        if(inputCamera.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            outputText.text = Run(image).ToString();

            image.Dispose();
        }
    }

    void OnDestroy()
    {
        worker.Dispose();
    }

    char Run(XRCpuImage image)
    {
        // TODO: Update text with the model output
        return 'a';
    }
}

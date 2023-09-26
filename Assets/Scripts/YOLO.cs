using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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
            outputText.text = Run(ProcessImage(image)).ToString();

            image.Dispose();
        }
    }

    void OnDestroy()
    {
        worker.Dispose();
    }

    Texture2D ProcessImage(XRCpuImage image)
    {
        //TODO
        return null;
    }

    char Run(Texture2D texture2D)
    {
        Tensor input = new(texture2D);

        // TODO: Preprocess input

        Tensor output = worker.Execute(input).PeekOutput();

        // TODO: Postprocess output

        input.Dispose();
        output.Dispose();
        return 'a';
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.Barracuda;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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

    // https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.0/manual/cpu-camera-image.html
    // TODO: Make this async
    unsafe Texture2D ProcessImage(XRCpuImage image)
    {
        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGB24,
            transformation = XRCpuImage.Transformation.None
        };

        int size = image.GetConvertedDataSize(conversionParams);
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

        Texture2D texture2D = new(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false
        );

        texture2D.LoadRawTextureData(buffer);
        texture2D.Apply();

        buffer.Dispose();
        return texture2D;
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

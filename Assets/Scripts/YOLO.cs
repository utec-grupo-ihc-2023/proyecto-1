using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        StartCoroutine(UpdateText());
    }

    // TODO: Make this async
    IEnumerator UpdateText()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);

            if(inputCamera.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                outputText.text = Run(ProcessImage(image));

                image.Dispose();
            }
        }
    }

    void OnDestroy()
    {
        worker.Dispose();
    }

    // https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.0/manual/cpu-camera-image.html
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

    Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D result = new(newWidth, newHeight, source.format, false);

        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    string Index2Name(int i)
    {
        return i switch
        {
            0 => "A",
            1 => "B",
            2 => "C",
            3 => "D",
            4 => "E",
            5 => "F",
            6 => "G",
            7 => "I",
            8 => "K",
            9 => "L",
            10 => "M",
            11 => "N",
            12 => "O",
            13 => "P",
            14 => "Q",
            15 => "R",
            16 => "S",
            17 => "T",
            18 => "U",
            _ => "",
        };
    }

    string Postprocess(Tensor data)
    {
        for(int box = 0; box < data.width; box++)
        {
            float confidence = data[0, 0, box, 4];

            List<float> classProbabilities = new();
            for(int probI = 5; probI < data.channels; probI++)
            {
                classProbabilities.Add(data[0, 0, box, probI]);
            }

            int maxI = classProbabilities.Any()? classProbabilities.IndexOf(classProbabilities.Max()) : 0;

            if(confidence >= 0.1)
            {
                return Index2Name(maxI);
            }
        }

        return "";
    }

    string Run(Texture2D texture2D)
    {
        Tensor input = new(ResizeTexture(texture2D, 640, 640));

        Tensor output = worker.Execute(input).PeekOutput();

        string res = Postprocess(output);

        input.Dispose();
        output.Dispose();
        return res;
    }
}

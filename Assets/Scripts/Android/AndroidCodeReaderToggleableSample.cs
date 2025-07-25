using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;

public class AndroidCodeReaderToggleableSample : MonoBehaviour {

    [SerializeField]
    private ARCameraManager cameraManager;
    [SerializeField]
    private string lastResult = "";

    [SerializeField]
    private TextMeshProUGUI debugResultText;

    private Texture2D cameraImageTexture;
    private bool scanningEnabled = false;

    private IBarcodeReader barcodeReader = new BarcodeReader {
        AutoRotate = false,
        Options = new ZXing.Common.DecodingOptions {
            TryHarder = false
        }
    };

    private Result result;

    private void OnEnable() {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    private void OnDisable() {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs) {

        if (!scanningEnabled) {
            return;
        }

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) {
            return;
        }

        var conversionParams = new XRCpuImage.ConversionParams {

            inputRect = new RectInt(0, 0, image.width, image.height),


            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),


            outputFormat = TextureFormat.RGBA32,

            transformation = XRCpuImage.Transformation.MirrorY
        };


        int size = image.GetConvertedDataSize(conversionParams);

        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        image.Convert(conversionParams, buffer);

        image.Dispose();

        cameraImageTexture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);

        cameraImageTexture.LoadRawTextureData(buffer);
        cameraImageTexture.Apply();

        buffer.Dispose();


        result = barcodeReader.Decode(cameraImageTexture.GetPixels32(), cameraImageTexture.width, cameraImageTexture.height);


        if (result != null) {
            lastResult = result.Text + " " + result.BarcodeFormat;
            debugResultText.text = lastResult;
        }
    }

    public void ToggleScanning() {
        scanningEnabled = !scanningEnabled;
    }

    public string GetCurrentState() {
        return "Is Scanner running? - " + scanningEnabled;
    }
}

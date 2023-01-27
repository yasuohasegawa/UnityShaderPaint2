using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.IO;
using System;

public class PaintController : MonoBehaviour
{
    [SerializeField] private Paint _paint;
    [SerializeField] private ColorPicker _picker;
    [SerializeField] private Slider _lineSizeSlider;
    [SerializeField] private Slider _noiseSizeSlider;
    [SerializeField] private Slider _noiseScaleSlider;
    [SerializeField] private Slider _noiseColorSizeSlider;
    [SerializeField] private Slider _eraseSizeSlider;
    [SerializeField] private Button _paintBtn;
    [SerializeField] private Button _eraseBtn;
    [SerializeField] private Button _captureBtn;
    [SerializeField] private RectTransform _tool;

    #region private variables
    private Vector2 _lineSize = new Vector2(0.001f,0.05f);
    private Vector2 _noiseSize = new Vector2(0, 0.05f);
    private Vector2 _noiseScale = new Vector2(1, 50f);
    private Vector2 _noiseColorSize = new Vector2(0, 0.3f);
    private Vector2 _eraseSize = new Vector2(1, 30);
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        BindSlider();
        Bind();
        _paintBtn.interactable = false;
        _eraseBtn.interactable = true;
    }

    // Update is called once per frame
    void Update()
    {
        var current = Keyboard.current;
        if (current == null)
        {
            return;
        }

        if (current.spaceKey.wasPressedThisFrame) _paint.Clear();

        // T key for tool
        if (current.tKey.wasPressedThisFrame)
        {
            _tool.gameObject.SetActive(!_tool.gameObject.activeSelf);
            _picker.gameObject.SetActive(!_picker.gameObject.activeSelf);
        }
    }

    #region private methods
    private void BindSlider()
    {
        _lineSizeSlider.minValue = _lineSize.x;
        _lineSizeSlider.maxValue = _lineSize.y;

        _noiseSizeSlider.minValue = _noiseSize.x;
        _noiseSizeSlider.maxValue = _noiseSize.y;

        _noiseScaleSlider.minValue = _noiseScale.x;
        _noiseScaleSlider.maxValue = _noiseScale.y;

        _noiseColorSizeSlider.minValue = _noiseColorSize.x;
        _noiseColorSizeSlider.maxValue = _noiseColorSize.y;

        _eraseSizeSlider.minValue = _eraseSize.x;
        _eraseSizeSlider.maxValue = _eraseSize.y;

        _lineSizeSlider.onValueChanged.AddListener(OnLineSize);
        _noiseSizeSlider.onValueChanged.AddListener(OnNoiseSize);
        _noiseScaleSlider.onValueChanged.AddListener(OnNoiseScale);
        _noiseColorSizeSlider.onValueChanged.AddListener(OnNoiseColorSize);
        _eraseSizeSlider.onValueChanged.AddListener(OnEraseSize);
        ResetValues();
    }

    private void Bind()
    {
        _picker.OnSelectedColor += OnSelectedColor;
        _paintBtn.onClick.AddListener(OnPaint);
        _eraseBtn.onClick.AddListener(OnErase);
        _captureBtn.onClick.AddListener(OnCapture);
    }

    private void UnBind()
    {
        _lineSizeSlider.onValueChanged.RemoveListener(OnLineSize);
        _noiseSizeSlider.onValueChanged.RemoveListener(OnNoiseSize);
        _noiseScaleSlider.onValueChanged.RemoveListener(OnNoiseScale);
        _noiseColorSizeSlider.onValueChanged.RemoveListener(OnNoiseColorSize);
        _eraseSizeSlider.onValueChanged.RemoveListener(OnEraseSize);
        _picker.OnSelectedColor -= OnSelectedColor;
        _paintBtn.onClick.RemoveListener(OnPaint);
        _eraseBtn.onClick.RemoveListener(OnErase);
        _captureBtn.onClick.RemoveListener(OnCapture);
    }

    private void ResetValues()
    {
        _lineSizeSlider.value = _paint.LineSize = _lineSize.x;
        _noiseSizeSlider.value = _paint.NoiseSize = _noiseSize.x;
        _noiseScaleSlider.value = _paint.NoiseScale = _noiseScale.x;
        _noiseColorSizeSlider.value = _paint.NoiseColorSize = _noiseColorSize.x;
        _eraseSizeSlider.value = _paint.EraseSize = (int)_eraseSize.x;
        _paint.PaintColor = _picker.CurrentColor;
    }

    private void Dispose()
    {
        UnBind();
    }

    private long Timestamp()
    {
        return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000);
    }
    #endregion

    #region events
    private void OnLineSize(float val)
    {
        _paint.LineSize = val;
    }

    private void OnNoiseSize(float val)
    {
        _paint.NoiseSize = val;
    }

    private void OnNoiseScale(float val)
    {
        _paint.NoiseScale = val;
    }

    private void OnNoiseColorSize(float val)
    {
        _paint.NoiseColorSize = val;
    }

    private void OnEraseSize(float val)
    {
        _paint.EraseSize = (int)val;
    }

    private void OnSelectedColor(Color col)
    {
        _paint.PaintColor = col;
    }

    private void OnPaint()
    {
        _paintBtn.interactable = false;
        _eraseBtn.interactable = true;

        _paint.PaintMode = Paint.ePaintMode.Paint;
    }

    private void OnErase()
    {
        _paintBtn.interactable = true;
        _eraseBtn.interactable = false;

        _paint.PaintMode = Paint.ePaintMode.Erase;
    }

    private void OnCapture() {
        if(_paint.RT != null){

            var tex = new Texture2D(_paint.RT.width, _paint.RT.height, TextureFormat.RGBA32, false);
            RenderTexture.active = _paint.RT;
            tex.ReadPixels(new Rect(0, 0, _paint.RT.width, _paint.RT.height), 0, 0, false);
            tex.Apply();
            RenderTexture.active = null;

            var png = tex.EncodeToPNG();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            File.WriteAllBytes($"{path}/{Timestamp().ToString()}.png", png);

            Destroy(tex);
            tex = null;
        }
    }

    private void OnDestroy()
    {
        Dispose();
    }
    #endregion
}

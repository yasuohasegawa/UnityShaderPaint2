using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Burst;
using TMPro;

/// <summary>
/// The current UI setting works great. Not sure if other UI settings will work or not.
/// </summary>
[BurstCompile]
public class ColorPicker : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RawImage _spectrumImage;
    [SerializeField] private RawImage _pickerResultImage;
    [SerializeField] private RawImage _pickedImage;
    [SerializeField] private Slider _spectrumSlider;
    [SerializeField] private RectTransform _pickerKnob;
    [SerializeField] private TMP_InputField _hexTxt;

    #region private variables
    private Texture2D _spectrumTex;
    private Texture2D _pickerTex;
    private RectTransform _pickerTargetRect;
    private Vector2 _pickerArea;
    private Color _currentColor = Color.black;
    private Vector2 _currentPixelArea = Vector2.zero;
    #endregion

    #region public variables
    public System.Action<Color> OnSelectedColor;
    #endregion

    #region properties
    public Color CurrentColor => _currentColor;
    #endregion

    void Awake()
    {
        _pickerTargetRect = _pickerResultImage.GetComponent<RectTransform>();
        _pickerArea = _pickerResultImage.transform.parent.GetComponent<RectTransform>().sizeDelta;

        Init();
        Bind();
    }

    #region private methods
    private async void Init()
    {
        var w = (int)_pickerArea.x;
        var h = 1;
        var pixels = await ColorPickerUtils.GeneratePickerSpectrum(w);
        _spectrumTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        _spectrumTex.LoadRawTextureData(pixels);
        _spectrumTex.Apply();
        pixels.Dispose();
        _spectrumImage.texture = _spectrumTex;

        _pickerTex = new Texture2D(w, w, TextureFormat.RGBA32, false);
        _pickerResultImage.texture = _pickerTex;

        UpdatePicker(0);
    }

    private void Bind()
    {
        _spectrumSlider.onValueChanged.AddListener(OnSpectrumSlider);
    }

    private void UnBind()
    {
        _spectrumSlider.onValueChanged.RemoveListener(OnSpectrumSlider);
    }

    private void Dispose()
    {
        UnBind();
        if(_pickerTex != null)
        {
            Destroy(_pickerTex);
            _pickerTex = null;
        }
        if(_spectrumTex != null)
        {
            Destroy(_spectrumTex);
            _spectrumTex = null;
        }
    }

    private async void GeneratePickerImage(Color targetColor)
    {
        var pixels = await ColorPickerUtils.GeneratePickerResult(targetColor, (int)_pickerArea.x, (int)_pickerArea.y);
        _pickerTex.LoadRawTextureData(pixels);
        _pickerTex.Apply();
        pixels.Dispose();
    }

    private void UpdatePicker(float val)
    {
        var rgb = Color.HSVToRGB(val, 1, 1);
        GeneratePickerImage(rgb);
        UpdateCurrentColor(_currentPixelArea.x, _currentPixelArea.y);
    }

    private (Vector2, bool) GetLocalAreaPos(PointerEventData eventData)
    {
        Vector2 localPos;
        var res = RectTransformUtility.ScreenPointToLocalPointInRectangle(_pickerTargetRect, eventData.position, eventData.pressEventCamera, out localPos);
        return (localPos,res);
    }

    private void GetPixelData(PointerEventData eventData)
    {
        (Vector2 localPos, bool available) pointData = GetLocalAreaPos(eventData);
        if (pointData.available)
        {
            var hw = (_pickerArea.x * 0.5f);
            var hh = (_pickerArea.y * 0.5f);
            var x = hw + pointData.localPos.x;
            var y = hh + pointData.localPos.y;

            if (x < 0) x = 0;
            if (x >= _pickerArea.x) x = _pickerArea.x-1;
            if (y < 0) y = 0;
            if (y >= _pickerArea.y) y = _pickerArea.y-1;

            UpdateCurrentColor(x, y);

            var pos = _pickerKnob.anchoredPosition;
            pos = pointData.localPos;
            if (pos.x <= -hw) pos.x = -hw;
            if (pos.x >= hw) pos.x = hw;
            if (pos.y <= -hh) pos.y = -hh;
            if (pos.y >= hh) pos.y = hh;
            _pickerKnob.anchoredPosition = pos;
        }
    }

    private void StartDrag(PointerEventData eventData)
    {
        (Vector2 localPos, bool available) pointData = GetLocalAreaPos(eventData);
        if (pointData.available)
        {
            var hw = (_pickerArea.x * 0.5f);
            var hh = (_pickerArea.y * 0.5f);
            if (!(pointData.localPos.x >= -hw && pointData.localPos.x < hw && pointData.localPos.y >= -hh && pointData.localPos.y < hh))
            {
                return;
            }
        }
        GetPixelData(eventData);
    }

    private void UpdateCurrentColor(float x, float y)
    {
        _currentColor = _pickerTex.GetPixel((int)x, (int)y);
        _pickedImage.color = _currentColor;
        OnSelectedColor?.Invoke(_currentColor);

        _currentPixelArea.x = x;
        _currentPixelArea.y = y;

        _hexTxt.text = ColorUtility.ToHtmlStringRGB(_currentColor);
    }
    #endregion

    #region events
    private void OnSpectrumSlider(float val)
    {
        UpdatePicker(val);
    }

    private void OnDestroy()
    {
        Dispose();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

    }

    public void OnDrag(PointerEventData eventData)
    {
        GetPixelData(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }
    #endregion
}

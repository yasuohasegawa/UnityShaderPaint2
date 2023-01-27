using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Burst;
using Unity.Collections;

/// <summary>
/// The paint area should be square. Otherwise, the result will be squeezed vertically or horizontally, depending on the shape.
/// The Graphics.Blit won't work properly on Android devices.
/// The Material has to set up the Paint.shader manually ATM.
/// </summary>
[BurstCompile]
public class Paint : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum ePaintMode
    {
        Paint,
        Erase
    }

    [SerializeField] private RectTransform _targetRect;
    [SerializeField] private Material _paintMat;
    [SerializeField] private float _paintAreaSize = 1920;

    #region private variables
    private Vector2 _paintArea;
    private RectTransform _paintAreaRect;

    private Vector4 _startPos = Vector4.zero;
    private Vector4 _endPos = -Vector4.one;
    private Vector2 _lastErasePos = Vector2.zero;

    private Texture2D _paintTex;
    private Texture2D _paintCopyTex;
    private RenderTexture _renderTexture;
    private ePaintMode _paintMode = ePaintMode.Paint;
    private int _eraseSize = 20;
    #endregion

    #region properties
    public ePaintMode PaintMode { set { _paintMode = value; } get { return _paintMode; } }

    public float PaintAreaSize
    {
        set
        {
            if(_paintAreaRect != null)
            {
                _paintArea = _paintAreaRect.sizeDelta;
                _paintArea.x = value;
                _paintArea.y = value;
                _paintAreaRect.sizeDelta = _paintArea;
            }
        }

        get
        {
            if (_paintAreaRect == null) return 1920;
            return _paintAreaRect.sizeDelta.x;
        }
    }

    public float LineSize
    {
        set
        {
            if (_paintMat != null) _paintMat.SetFloat("_LineSize", value);
        }

        get
        {
            if (_paintMat != null)
            {
                return _paintMat.GetFloat("_LineSize");
            }
            return 0;
        }
    }

    public float NoiseSize
    {
        set
        {
            if (_paintMat != null) _paintMat.SetFloat("_NoiseSize", value);
        }

        get
        {
            if (_paintMat != null)
            {
                return _paintMat.GetFloat("_NoiseSize");
            }
            return 0;
        }
    }

    public float NoiseScale
    {
        set
        {
            if (_paintMat != null) _paintMat.SetFloat("_NoiseScale", value);
        }

        get
        {
            if (_paintMat != null)
            {
                return _paintMat.GetFloat("_NoiseScale");
            }
            return 0;
        }
    }

    public float NoiseColorSize
    {
        set
        {
            if (_paintMat != null) _paintMat.SetFloat("_NoiseColorSize", value);
        }

        get
        {
            if (_paintMat != null)
            {
                return _paintMat.GetFloat("_NoiseColorSize");
            }
            return 0;
        }
    }

    public Color PaintColor
    {
        set
        {
            if (_paintMat != null) _paintMat.SetColor("_PaintColor", value);
        }

        get
        {
            if (_paintMat != null)
            {
                return _paintMat.GetColor("_PaintColor");
            }
            return Color.black;
        }
    }

    public int EraseSize { set { _eraseSize = value; } get { return _eraseSize; } }
    public RenderTexture RT => _renderTexture;
    #endregion

    #region private methods
    void Awake()
    {
        Init();
    }

    private void Init()
    {
        _paintAreaRect = _targetRect.parent.GetComponent<RectTransform>();
        ResetPaintAreaSize();

        CreateRT();
    }

    private void CreateRT()
    {
        if(_renderTexture == null)
        {
            _renderTexture = new RenderTexture((int)_paintAreaSize, (int)_paintAreaSize, 16, RenderTextureFormat.ARGB32);
            _renderTexture.filterMode = FilterMode.Bilinear;
            _renderTexture.wrapMode = TextureWrapMode.Clamp;
            _renderTexture.Create();
            //_paintMat.SetTexture("_MainTex", _renderTexture);

            var ptr = _renderTexture.GetNativeTexturePtr();
            _paintTex = Texture2D.CreateExternalTexture(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false, true, ptr);
            _paintTex.filterMode = FilterMode.Bilinear;
            _paintTex.wrapMode = TextureWrapMode.Clamp;
            _paintMat.SetTexture("_MainTex", _paintTex);

            _paintCopyTex = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);
            _paintCopyTex.filterMode = FilterMode.Bilinear;
            _paintCopyTex.wrapMode = TextureWrapMode.Clamp;
            _paintCopyTex.Apply();
        }
    }

    private void UpdatePaintTex()
    {
        var rpt = _renderTexture.GetNativeTexturePtr();
        _paintTex.UpdateExternalTexture(rpt);
    }

    private (float, float) GetErasePose(Vector2 pos)
    {
        var hw = (_paintArea.x * 0.5f);
        var hh = (_paintArea.y * 0.5f);
        var x = hw + pos.x;
        var y = hh + pos.y;

        if (x < 0) x = 0;
        if (x >= _paintArea.x) x = _paintArea.x - 1;
        if (y < 0) y = 0;
        if (y >= _paintArea.y) y = _paintArea.y - 1;

        return (x, y);
    }

    private void Draw(PointerEventData eventData)
    {
        Vector2 localPos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_targetRect, eventData.position, eventData.pressEventCamera, out localPos))
        {
            if(_paintMode == ePaintMode.Paint)
            {
                // normalize value between 0 and 1
                localPos.x = ((_paintArea.x * 0.5f) + localPos.x) / _paintArea.x;
                localPos.y = ((_paintArea.y * 0.5f) + localPos.y) / _paintArea.y;
                //Debug.Log($"{_anchorPresets.ToString()}");
                //Debug.Log($"{localPos}");
                _startPos.x = localPos.x;
                _startPos.y = localPos.y;
                if (_endPos != -Vector4.one)
                {
                    _paintMat.SetInt("_IsDrawing", 1);
                    _paintMat.SetVector("_StartPos", _startPos);
                    _paintMat.SetVector("_EndPos", _endPos);
                }

                _endPos = _startPos;
                Graphics.Blit(_paintMat.mainTexture, _renderTexture, _paintMat);
                UpdatePaintTex();
            } else if(_paintMode == ePaintMode.Erase) {
                var posData = GetErasePose(localPos);

                localPos.x = posData.Item1;
                localPos.y = posData.Item2;

                Erase(_paintCopyTex, localPos, _eraseSize);

                _lastErasePos.x = localPos.x;
                _lastErasePos.y = localPos.y;
            }
        }
    }

    private void Erase(Texture2D tex, Vector2 uv, int size)
    {
        RenderTexture.active = _renderTexture;
        tex.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0,false);

        Color[] colors = tex.GetPixels();
        float dist = Vector2.Distance(_lastErasePos, uv);
        float step = 1 / dist;

        for (float i = 0f; i < 1; i += step)
        {
            var pixel = Vector2.Lerp(_lastErasePos, uv, i);
            PaintUtils.SetPixels(colors, _renderTexture.width, pixel, _eraseSize, Color.clear);
        }

        tex.SetPixels(colors);
        tex.Apply();
        RenderTexture.active = null;

        _paintTex.UpdateExternalTexture(tex.GetNativeTexturePtr());
        Graphics.Blit(_paintTex, _renderTexture);
    }

    private void Dispose()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            _renderTexture = null;
        }
        if(_paintTex != null)
        {
            Destroy(_paintTex);
            _paintTex = null;
        }
        if (_paintCopyTex != null)
        {
            Destroy(_paintCopyTex);
            _paintCopyTex = null;
        }
    }
    #endregion


    #region public methods
    public void Clear()
    {
        _paintMat.SetInt("_Clear", 1);
        _renderTexture.Release();
        UpdatePaintTex();
    }

    public void ResetPaintAreaSize()
    {
        PaintAreaSize = _paintAreaSize;
    }
    #endregion


    #region events
    public void OnBeginDrag(PointerEventData eventData)
    {
        _paintMat.SetInt("_Clear", 0);
        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_targetRect, eventData.position, eventData.pressEventCamera, out localPos))
        {
            var posData = GetErasePose(localPos);
            _lastErasePos.x = posData.Item1;
            _lastErasePos.y = posData.Item2;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Draw(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _endPos = -Vector4.one;
        _paintMat.SetInt("_IsDrawing", 0);
    }

    private void OnDestroy()
    {
        Dispose();
    }
    #endregion
}

using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

public class ColorPickerUtils
{
    #region public methods
    public static Task<NativeArray<Color32>> GeneratePickerSpectrum(int w)
    {
        var buffer = new NativeArray<Color32>(w, Allocator.Temp);
        var offset = 0;
        for (int i = 0; i < w; i++)
        {
            var rgb = Color.HSVToRGB((float)i / (float)w, 1, 1);
            buffer[offset++] = new Color32((byte)(rgb.r * 255f), (byte)(rgb.g * 255f), (byte)(rgb.b * 255f), 255);
        }

        return Task.FromResult(buffer);
    }

    public static Task<NativeArray<Color32>> GeneratePickerResult(Color targetColor, int w, int h)
    {
        var size = w * h;
        var buffer = new NativeArray<Color32>(size, Allocator.Temp);
        var offset = 0;
        var tr = (float)targetColor.r * 255f;
        var tg = (float)targetColor.g * 255f;
        var tb = (float)targetColor.b * 255f;
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                var u = h - (float)j;
                var v = (float)i / w;

                var diffR = 255 - tr;
                var diffG = 255 - tg;
                var diffB = 255 - tb;

                var r = (((float)u / w) * diffR) + tr;
                var g = (((float)u / w) * diffG) + tg;
                var b = (((float)u / w) * diffB) + tb;

                r *= v;
                g *= v;
                b *= v;

                buffer[offset] = new Color32((byte)r, (byte)g, (byte)b, 255);

                offset++;
            }
        }

        return Task.FromResult(buffer);
    }
    #endregion
}

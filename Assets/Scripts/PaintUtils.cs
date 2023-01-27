using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintUtils
{
    #region public methods
    public static void SetPixels(Color[] colors, int w, Vector2 p, int size, Color col)
    {
        for (var y = -size; y <= size; y++)
        {
            for (var x = -size; x <= size; x++)
            {
                var uv = new Vector2(x, y);
                uv += p;
                if ((uv - p).sqrMagnitude < (size * size) * 0.5f) // make it circle shape
                {
                    colors[((int)p.x + x) + ((int)p.y + y) * w] = col;
                }
            }
        }
    }
    #endregion
}

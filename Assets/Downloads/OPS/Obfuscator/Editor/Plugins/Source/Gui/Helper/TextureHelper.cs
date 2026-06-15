using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Unity
using UnityEngine;
using UnityEditor;

namespace OPS.Editor.Gui
{
    public static class TextureHelper
    {
        public static Texture2D MakeTexture(int _Width, int _Height, Color _Color)
        {
            Color[] pix = new Color[_Width * _Height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = _Color;

            Texture2D result = new Texture2D(_Width, _Height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }
}

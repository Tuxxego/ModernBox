using HarmonyLib;
using UnityEngine;
using System;

namespace ModernBox
{
    [HarmonyPatch(typeof(PixelBag), MethodType.Constructor, new Type[] { typeof(Sprite), typeof(bool), typeof(bool) })]
    public class FuckTrump
    {
        public static bool Prefix(Sprite pSpriteSource, bool pCheckPhenotypes, bool pCheckLights)
        {
            if (pSpriteSource == null || pSpriteSource.texture == null) return true;

            try
            {
                Texture2D texture = pSpriteSource.texture;
                Rect rect = pSpriteSource.rect;
                int width = texture.width;
                int height = texture.height;

                int maxX = (int)rect.x + (int)rect.width - 1;
                int maxY = (int)rect.y + (int)rect.height - 1;

                if (maxX >= width || maxY >= height || (int)rect.x < 0 || (int)rect.y < 0)
                {
                    Debug.LogWarning($"ModernBox: Caught potential IndexOutOfRange in PixelBag for sprite {pSpriteSource.name}. Skipping pixel processing to prevent crash.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"ModernBox: Error during PixelBag safety check: {e.Message}");
                return false;
            }

            return true;
        }
    }
}
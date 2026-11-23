using System.IO;
using System.Reflection;
using UnityEngine;

namespace PlaylistManager.Utilities
{
    internal static class SpriteUtils
    {
        internal static Sprite LoadSpriteFromAssembly(string resourceName)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return null;
            }

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return LoadSpriteFromBytes(memoryStream.ToArray());
        }

        internal static Sprite LoadSpriteFromBytes(byte[] data)
        {
            var texture = new Texture2D(2, 2);
            return texture.LoadImage(data)
                ? BeatSaberMarkupLanguage.Utilities.LoadSpriteFromTexture(texture)
                : null;
        }
    }
}

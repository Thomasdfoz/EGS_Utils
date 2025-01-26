using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


namespace EGS.Utils 
{
    public class DownloadTexture
    {
        /// <summary>
        /// This method downloads a texture and inserts it into an image and resizes it.
        /// </summary>
        /// <param name="monoBehaviour">a MonoBehaviour for StartCourotine</param>
        /// <param name="url">Url of the texture</param>
        /// <param name="image">Image to receive the texture</param>
        /// <param name="imageWidth">width of the image</param>
        /// <param name="imageHeight">height of the image</param>
        public static IEnumerator DownloadAndCreateSprite(string url, Action<Sprite> callback, Action<string> error)
        {
            yield return DownloadAndTexture2D(url, (texture) =>
            {
                callback(Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f));
            }, error, TextureCompressionType.None);
        }

        /// <summary>
        /// This method downloads a texture and inserts into an image.
        /// </summary>
        /// <param name="monoBehaviour">a MonoBehaviour for StartCourotine</param>
        /// <param name="url">Url of the texture</param>
        /// <param name="image">Image to receive the texture</param>
        public static IEnumerator DownloadAndTexture2D(string url, Action<Texture2D> callback, Action<string> error, TextureCompressionType compression = TextureCompressionType.LowQuality)
        {
            if (string.IsNullOrEmpty(url)) yield break;

            UnityWebRequest lRequest = UnityWebRequestTexture.GetTexture(url);
            yield return lRequest.SendWebRequest();

            if (lRequest.result == UnityWebRequest.Result.ConnectionError
                || lRequest.result == UnityWebRequest.Result.ProtocolError
                || lRequest.result == UnityWebRequest.Result.DataProcessingError)
                error?.Invoke("Error: " + lRequest.error);
            else
            {
                Texture2D lTexture = new Texture2D(2, 2);

                lTexture.LoadImage(lRequest.downloadHandler.data);

                switch (compression) 
                {
                    case TextureCompressionType.LowQuality:
                        lTexture.Compress(false);
                        break;
                    case TextureCompressionType.HighQuality:
                        lTexture.Compress(true);
                        break;
                }

                callback(lTexture);
            }

            lRequest.Dispose();
        }
    }

    public enum TextureCompressionType 
    {
        None,
        LowQuality,
        HighQuality,
    }
}
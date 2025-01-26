using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;


namespace EGS.Utils 
{
    /// <summary>
    /// Utility class to download audio files.
    /// </summary>
    public class DownloadAudio
    {
        private static readonly Dictionary<AudioType, string> audioFormatMap = new Dictionary<AudioType, string>()
        {
            { AudioType.WAV, ".wav" },
            { AudioType.OGGVORBIS, ".ogg" },
            { AudioType.MPEG, ".mp3" }
        };
    
        /// <summary>
        /// Download an audio file from a URL and converts to an audio clip asset.
        /// </summary>
        public static IEnumerator RequestAudioClip(string url, Action<AudioClip> callback, Action<string> error)
        {
            yield return RequestAudioClip(url, GetAudioTypeFromURL(url), callback, error);
        }
    
        /// <summary>
        /// Download an audio file from a URL and converts to an audio clip asset.
        /// </summary>
        public static IEnumerator RequestAudioClip(string url, AudioType audioType, Action<AudioClip> callback, Action<string> error)
        {
            if (!AudioFormatValidation(url, audioType))
                yield break;
    
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
            yield return www.SendWebRequest();
    
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error downloading audio: " + www.error);
                error?.Invoke(www.error);
            }
            else
            {
                callback?.Invoke(DownloadHandlerAudioClip.GetContent(www));
            }
        }
    
        /// <summary>
        /// Validate if the audio format is correct given the audio type and audio URL.
        /// </summary>
        /// <param name="url">audio URL</param>
        /// <param name="audioType">audioType for audio clip file</param>
        /// <returns></returns>
        private static bool AudioFormatValidation(string url, AudioType audioType)
        {
            if (!audioFormatMap.ContainsKey(audioType))
            {
                Debug.LogError($"AUDIO ERROR: audio format was not configured in '{nameof(audioFormatMap)}' (missing format = {audioType})");
                return false;
            }
    
            if (!url.Contains(audioFormatMap[audioType]))
            {
                Debug.LogError($"AUDIO ERROR: audio URL does not contain the correct format extension (expected = {audioFormatMap[audioType]})");
                return false;
            }
    
            return true;
        }
    
        public static AudioType GetAudioTypeFromURL(string url) 
        {
            AudioType lAudioType = AudioType.MPEG;
    
            if (url.Contains(".mp3"))
                lAudioType = AudioType.MPEG;
            else if (url.Contains(".ogg"))
                lAudioType = AudioType.OGGVORBIS;
            else if (url.Contains(".wav"))
                lAudioType = AudioType.WAV;
    
            return lAudioType;
        }
    }
}
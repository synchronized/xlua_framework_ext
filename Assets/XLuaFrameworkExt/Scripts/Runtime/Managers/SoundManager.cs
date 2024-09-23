using UnityEngine;
using System.Collections.Generic;

namespace XLuaFrameworkExt
{
    public static class SoundManager
    {
        private static AudioSource m_background;
        private static AudioSource m_triggerSound;

        static Dictionary<string, AudioClip> loadedClips = new Dictionary<string, AudioClip>();

        public static void Initalize() {
            var audioSourceRoot = GlobalManager.Behaviour.transform.Find("AudioSource");
            if (audioSourceRoot == null) {
                audioSourceRoot = new GameObject("AudioSource").transform;
                audioSourceRoot.SetParent(GlobalManager.Behaviour.transform);
            }

            var goBackground = audioSourceRoot.Find("Background");
            if (goBackground == null) {
                goBackground = new GameObject("Background").transform;
                goBackground.SetParent(audioSourceRoot);
            }
            m_background = goBackground.GetComponent<AudioSource>();
            if (m_background == null) {
                m_background = goBackground.gameObject.AddComponent<AudioSource>();
            }

            var goTriggerSound = audioSourceRoot.Find("TriggerSound");
            if (goTriggerSound == null) {
                goTriggerSound = new GameObject("TriggerSound").transform;
                goTriggerSound.SetParent(audioSourceRoot);
            }
            m_triggerSound = goTriggerSound.GetComponent<AudioSource>();
            if (m_triggerSound == null) {
                m_triggerSound = goTriggerSound.gameObject.AddComponent<AudioSource>();
            }
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="clipPath">LuaDev目录下的声音路径</param>
        /// <param name="loopTimes">循环次数：0表示不循环,-1表示无线循环，大于零表示循环次数</param>
        /// <param name="loopID">传入循环ID后，可用StopSound()方法随时停止</param>
        public static void PlayBackground(string clipPath)
        {
            AudioClip clip = null;
            if (loadedClips.ContainsKey(clipPath))
            {
                clip = loadedClips[clipPath];
            }
            else
            {
                clip = ResManager.LoadAssetSync<AudioClip>(clipPath);
                loadedClips.Add(clipPath, clip);
            }
            if (m_background.clip == null || m_background.clip.name != clip.name) {
                m_background.clip = clip;
                m_background.Play();
            }
        }

        /// <summary>
        /// 播放短音效
        /// </summary>
        /// <param name="clipPath">LuaDev目录下的声音路径</param>
        /// <param name="loopTimes">循环次数：0表示不循环,-1表示无线循环，大于零表示循环次数</param>
        /// <param name="loopID">传入循环ID后，可用StopSound()方法随时停止</param>
        public static void PlayTriggerSound(string clipPath, float volume = 1, int loopTimes = 0, string loopID = null)
        {
            AudioClip clip = null;
            if (loadedClips.ContainsKey(clipPath))
            {
                clip = loadedClips[clipPath];
            }
            else
            {
                clip = ResManager.LoadAssetSync<AudioClip>(clipPath);
                loadedClips.Add(clipPath, clip);
            }
            if (loopTimes == 0)
            {
                m_triggerSound.PlayOneShot(clip, volume);
            }
            else
            {
                if (loopTimes < 0 || loopTimes > 0)
                {
                    LTimer.Invoke(() =>
                    {
                        m_triggerSound.PlayOneShot(clip, volume);
                    }, 0, clip.length, loopTimes, loopID);
                }
            }
        }

        /// <summary>
        /// 配合 PlaySound() 传入的loopID停止音效播放
        /// </summary>
        public static void StopSound(string loopID)
        {
            LTimer.InvokeCancel(loopID);
        }
    }
}

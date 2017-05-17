using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using ExtraVehicleEffects.GameExtensions;

namespace ExtraVehicleEffects.Effects
{
    public class TrainCloseDoors
    {
        private const string effectName = "Train Close Doors";

        public static EffectInfo CreateEffectObject(Transform parent)
        {
            var obj = new GameObject(effectName);
            obj.transform.parent = parent;
            SoundEffect effect = obj.AddComponent<SoundEffect>();
            effect.m_position = Vector3.zero;
            effect.m_range = 50.0f;
            
            // Create a copy of an audioInfo
            var templateSound = Util.FindEffect("Train Movement") as SoundEffect;
            AudioInfo audioInfo = UnityEngine.Object.Instantiate(templateSound.m_audioInfo) as AudioInfo;
            audioInfo.name = effectName;
            audioInfo.m_fadeLength = 1.2f;
            audioInfo.m_loop = true;
            audioInfo.m_pitch = 1.0f;
            audioInfo.m_volume = 1.5f;
            audioInfo.m_randomTime = false;

            // Load new audio clip
            AudioClip clip = Util.LoadAudioClipFromModDir("Sounds/train-doors3.ogg");
            audioInfo.m_fadeLength = clip.length;

            bool hasClip = false;
            AudioInfo.Variation[] variations = new AudioInfo.Variation[2];
            for (int i = 0; i < variations.Length; ++i)
            {
                variations[i].m_sound = UnityEngine.Object.Instantiate(audioInfo) as AudioInfo;
                Debug.Log("Sounds/train-doors" + (i + 1) + ".ogg");
                variations[i].m_sound.m_clip = Util.LoadAudioClipFromModDir("Sounds/train-doors" + (i + 1) + ".ogg");
                variations[i].m_sound.name = "";
                variations[i].m_sound.m_fadeLength = variations[i].m_sound.m_clip.length;
                if (variations[i].m_sound.m_clip != null)
                    hasClip = true;
            }

            variations[0].m_probability = 60;
            variations[1].m_probability = 60;


            if (clip != null)
            {
                audioInfo.m_clip = clip;
                if(hasClip)
                    audioInfo.m_variations = variations;
            }
            else
            {
                return null;
            }

            effect.m_audioInfo = audioInfo;

            return effect;
        }
    }
}

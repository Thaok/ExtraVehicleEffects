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
    public class SpragueBell
    {
        private const string effectName = "Sprague Bell";

        public static EffectInfo CreateEffectObject(Transform parent)
        {
            var obj = new GameObject(effectName);
            obj.transform.parent = parent;
            SoundEffect effect = obj.AddComponent<SoundEffect>();
            effect.m_position = Vector3.zero;

            // Create a copy of an audioInfo
            var templateSound = Util.FindEffect("Train Movement") as SoundEffect;
            AudioInfo audioInfo = UnityEngine.Object.Instantiate(templateSound.m_audioInfo) as AudioInfo;
            audioInfo.name = effectName;
            audioInfo.m_fadeLength = 0.0f;
            audioInfo.m_loop = true;
            audioInfo.m_pitch = 1.0f;
            audioInfo.m_volume = 1.0f;
            audioInfo.m_randomTime = false;

            // Load new audio clip

            var clip = Util.LoadAudioClipFromModDir("Sounds/sprague-bell.ogg");

            if(clip != null)
            {
                audioInfo.m_clip = clip;
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

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
    public class TrainCompressor
    {
        private const string effectName = "Train Compressor";

        public static EffectInfo CreateEffectObject(Transform parent)
        {
            EngineSoundEffect defaultEngineSound = Util.FindEffect("Train Movement") as EngineSoundEffect;

            if(defaultEngineSound != null)
            {
                GameObject obj = new GameObject(effectName);
                obj.transform.parent = parent;

                TurboDieselIdlingSoundEffect turboDieselEngineSound = TurboDieselIdlingSoundEffect.CopyEngineSoundEffect(defaultEngineSound, obj.AddComponent<TurboDieselIdlingSoundEffect>());

                turboDieselEngineSound.name = effectName;
                turboDieselEngineSound.m_noHeatingPitch = 1.0f;

                AudioInfo audioInfo = UnityEngine.Object.Instantiate(defaultEngineSound.m_audioInfo) as AudioInfo;
                audioInfo.name = effectName;
                audioInfo.m_fadeLength = 0;
                var clip = Util.LoadAudioClipFromModDir("Sounds/sprague-compressor.ogg");                

                if (clip != null)
                {
                    audioInfo.m_clip = clip;
                    turboDieselEngineSound.m_audioInfo = audioInfo;
                }
                else
                    return null;
                                
                

                return turboDieselEngineSound;
            }
            else
            {
                Debug.Log("Could not find default train sound effect!");
                return null;
            }
        }
    }
}

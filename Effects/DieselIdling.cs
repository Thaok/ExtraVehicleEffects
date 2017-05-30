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
    public class DieselIdling
    {
        private const string effectName = "Diesel Idling";

        public static EffectInfo CreateEffectObject(Transform parent)
        {
            EngineSoundEffect defaultEngineSound = Util.FindEffect("Train Movement") as EngineSoundEffect;

            if(defaultEngineSound != null)
            {
                GameObject obj = new GameObject(effectName);
                obj.transform.parent = parent;

                TurboDieselIdlingSoundEffect turboDieselEngineSound = TurboDieselIdlingSoundEffect.CopyEngineSoundEffect(defaultEngineSound, obj.AddComponent<TurboDieselIdlingSoundEffect>());

                turboDieselEngineSound.name = effectName;

                //init audio info
                AudioInfo audioInfo = UnityEngine.Object.Instantiate(defaultEngineSound.m_audioInfo) as AudioInfo;
                audioInfo.name = effectName;
                var clip = Util.LoadAudioClipFromModDir("Sounds/diesel-engine-218-load-noTurbo.ogg");
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

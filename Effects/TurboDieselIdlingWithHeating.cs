﻿using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using ExtraVehicleEffects.GameExtensions;

namespace ExtraVehicleEffects.Effects
{
    public class TurboDieselIdlingWithHeating
    {
        private const string effectName = "Turbo Diesel Idling With Heating";

        public static EffectInfo CreateEffectObject(Transform parent)
        {
            EngineSoundEffect defaultEngineSound = Util.FindEffect("Train Movement") as EngineSoundEffect;

            if(defaultEngineSound != null)
            {
                GameObject obj = new GameObject(effectName);
                obj.transform.parent = parent;

                TurboDieselIdlingSoundEffect turboDieselEngineSound = TurboDieselIdlingSoundEffect.CopyEngineSoundEffect(defaultEngineSound, obj.AddComponent<TurboDieselIdlingSoundEffect>());

                turboDieselEngineSound.name = effectName;

                //init higher rpm idle audio info (used if engine enables train heating)
                AudioInfo audioInfo = UnityEngine.Object.Instantiate(defaultEngineSound.m_audioInfo) as AudioInfo;
                audioInfo.name = effectName;
                var clip = Util.LoadAudioClipFromModDir("Sounds/diesel-engine-218-idle2.ogg");
                if (clip != null)
                {
                    audioInfo.m_clip = clip;
                    turboDieselEngineSound.m_audioInfo = audioInfo;
                    turboDieselEngineSound.m_do_heating = true;
                }
                else
                    return null;
                
                //init idle audio info
                //audioInfo = UnityEngine.Object.Instantiate(defaultEngineSound.m_audioInfo) as AudioInfo;
                //audioInfo.name = effectName;
                //clip = Util.LoadAudioClipFromModDir("Sounds/diesel-engine-218-idle.ogg");
                //if (clip != null)
                //{
                //    audioInfo.m_clip = clip;
                //    turboDieselEngineSound.m_idle_audioInfo = audioInfo;
                //}
                
                

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

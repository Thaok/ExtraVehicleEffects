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
    public class TrainFlangeGrinding
    {
        private const string effectName = "Train Flange Grinding";

        public static EffectInfo CreateEffectObject(Transform parent)
        {
            EngineSoundEffect defaultEngineSound = Util.FindEffect("Train Movement") as EngineSoundEffect;

            if(defaultEngineSound != null)
            {
                GameObject obj = new GameObject(effectName);
                obj.transform.parent = parent;

                TrainFlangeGrindingSoundEffect flangeGrindingSound = TrainFlangeGrindingSoundEffect.CopyEngineSoundEffect(defaultEngineSound, obj.AddComponent<TrainFlangeGrindingSoundEffect>());

                flangeGrindingSound.name = effectName;

                // init main audio info
                AudioInfo audioInfo = UnityEngine.Object.Instantiate(defaultEngineSound.m_audioInfo) as AudioInfo;
                audioInfo.name = effectName;
                var clip = Util.LoadAudioClipFromModDir("Sounds/train-flange-grinding.ogg");
                //var clip = Util.LoadAudioClipFromModDir("Sounds/train-flange-grinding-2.ogg");
                if (clip != null)
                {
                    audioInfo.m_clip = clip;
                    flangeGrindingSound.m_audioInfo = audioInfo;
                }
                else
                    return null;


                return flangeGrindingSound;
            }
            else
            {
                Debug.Log("Could not find default train sound effect!");
                return null;
            }
        }
    }
}

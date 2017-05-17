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
    public class TrainBrakeDisc
    {
        private const string effectName = "Train Disc Brake";

        public static EffectInfo CreateEffectObject(Transform parent)
        {
            EngineSoundEffect defaultEngineSound = Util.FindEffect("Train Movement") as EngineSoundEffect;

            if(defaultEngineSound != null)
            {
                GameObject obj = new GameObject(effectName);
                obj.transform.parent = parent;

                TrainBrakingSoundEffect trainDiscBrakeSound = TrainBrakingSoundEffect.CopyEngineSoundEffect(defaultEngineSound, obj.AddComponent<TrainBrakingSoundEffect>());

                trainDiscBrakeSound.name = effectName;

                // init main audio info
                AudioInfo audioInfo = UnityEngine.Object.Instantiate(defaultEngineSound.m_audioInfo) as AudioInfo;
                audioInfo.name = effectName;
                
                var clip = Util.LoadAudioClipFromModDir("Sounds/train-brake-disc.ogg");
                trainDiscBrakeSound.m_base_pitch = 0.6f;
                
                //var clip = Util.LoadAudioClipFromModDir("Sounds/train-brake-disc-2.ogg");
                //trainDiscBrakeSound.m_base_pitch = 1.0f;

                if (clip != null)
                {
                    audioInfo.m_clip = clip;
                    trainDiscBrakeSound.m_audioInfo = audioInfo;
                }
                else
                    return null;
                                            

                return trainDiscBrakeSound;
            }
            else
            {
                Debug.Log("Could not find default train sound effect!");
                return null;
            }
        }
    }
}

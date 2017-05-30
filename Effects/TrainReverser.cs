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
    public class TrainReverser
    {
        private const string effectName = "Train Reverser";
        
        
        public static EffectInfo CreateEffectObject(Transform parent)
        {
            EngineSoundEffect defaultEngineSound = Util.FindEffect("Train Movement") as EngineSoundEffect;

            if (defaultEngineSound != null)
            {
                GameObject obj = new GameObject(effectName);
                obj.transform.parent = parent;

                TurnTrainEffect newSoundEffect = TurnTrainEffect.CopyEngineSoundEffect(defaultEngineSound, obj.AddComponent<TurnTrainEffect>());
                newSoundEffect.name = effectName;

                return newSoundEffect;
            }
            else
            {
                Debug.Log("Could not find default train sound effect!");
                return null;
            }
           
        }
    }
}

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
    public class TurboDieselTrainMovement
    {
        private const string effectGroupName = "Turbo Diesel Train Movement Effects";

        private const string turboPrefix = "Turbo Diesel Train Movement";
        private const string dieselPrefix = "Diesel Train Movement";

        private static uint[] speedsToBeGenerated = { 80, 90, 100, 120, 140, 160 };

        private class AudioInfoSet
        {
            public AudioInfo info_engine;
            public AudioInfo info_turbo;
            public AudioInfo info_idle;
            public AudioInfo info_braking;
        }

        static private AudioInfo initAudioInfo(string filename, EngineSoundEffect defaultSound)
        {
            AudioInfo audioInfo = UnityEngine.Object.Instantiate(defaultSound.m_audioInfo) as AudioInfo;
            //audioInfo.name = effectName;
            var clip = Util.LoadAudioClipFromModDir(filename);
            if (clip != null)
            {
                audioInfo.m_clip = clip;
                return audioInfo;
            }
            else
            {
                Debug.Log("Could not load AudioClip from \"" + filename + "\"!");
                return null;
            }
        }

        private static TurboDieselEngineSoundEffect initEffect(string effectName, AudioInfoSet audioInfos, EngineSoundEffect defaultEngineSound, Transform parent)
        {
            GameObject obj = new GameObject(effectGroupName);
            obj.transform.parent = parent;
            TurboDieselEngineSoundEffect effect = TurboDieselEngineSoundEffect.CopyEngineSoundEffect(defaultEngineSound, obj.AddComponent<TurboDieselEngineSoundEffect>());

            effect.name = effectName;

            effect.m_audioInfo = audioInfos.info_engine;
            effect.m_turbo_audioInfo = audioInfos.info_turbo;
            effect.m_idle_audioInfo = audioInfos.info_idle;
            effect.m_braking_audioInfo = audioInfos.info_braking;
            
            return effect;
        }

        static private TurboDieselEngineSoundEffect initEffectForSpeed(string prefix, AudioInfoSet audioInfos, EngineSoundEffect defaultEngineSound, Transform parent, uint speed)
        {
            TurboDieselEngineSoundEffect effect;
            effect = initEffect(prefix + " " + speed, audioInfos, defaultEngineSound, parent);
            effect.adjustMaxSpeed(ExtraVehicleEffects.Util.SpeedKmHToEffect(speed));
            return effect;
        }

        public static void SetOptimalVehicleKinematics()
        {
            Util.setVehicleKinematicsForEffect(turboPrefix, 0.2f, 0.2f, -1.9f);
            Util.setVehicleKinematicsForEffect(dieselPrefix, 0.2f, 0.2f, -1.9f);
            Util.setVehicleKinematicsForEffect("Turbo Diesel Train Movement 200", 0.2f, 0.2f, -1.9f, 40.0f);
        }

        public static EffectInfo CreateEffectObject(Transform parent)
        {
            EngineSoundEffect defaultEngineSound = Util.FindEffect("Train Movement") as EngineSoundEffect;

            if(defaultEngineSound != null)
            {

                AudioInfoSet audioInfos = new AudioInfoSet();
                audioInfos.info_engine = initAudioInfo("Sounds/diesel-engine-218-load-noTurbo.ogg",defaultEngineSound);
                //audioInfos.info_engine = initAudioInfo("Sounds/diesel-engine-218-load.ogg",defaultEngineSound);
                audioInfos.info_turbo = initAudioInfo("Sounds/diesel-engine-218-turbo.ogg",defaultEngineSound);
                audioInfos.info_idle = initAudioInfo("Sounds/diesel-engine-218-idle.ogg",defaultEngineSound);
                audioInfos.info_braking = initAudioInfo("Sounds/train-brake-disc.ogg", defaultEngineSound);

                if (audioInfos.info_engine == null || audioInfos.info_turbo == null || audioInfos.info_idle == null || audioInfos.info_braking == null)
                    return null;
                                    
                //turbo diesel
                for (uint i = 0; i < speedsToBeGenerated.Length; ++i)
                    initEffectForSpeed(turboPrefix, audioInfos, defaultEngineSound, parent, speedsToBeGenerated[i]);
                                  
                //diesel
                for (uint i = 0; i < speedsToBeGenerated.Length; ++i)
                {
                    TurboDieselEngineSoundEffect noTurboEffect = initEffectForSpeed(dieselPrefix, audioInfos, defaultEngineSound, parent, speedsToBeGenerated[i]);
                    noTurboEffect.m_hasTurbo = false;
                }

                //turbo diesel for 200 kmh (to be used with Paxman Valenta in BR HST)
                TurboDieselEngineSoundEffect effect = initEffectForSpeed(turboPrefix, audioInfos, defaultEngineSound, parent, 200);
                effect.m_turbo_minPitch = 0.8f;
                effect.m_turbo_pitch_adjust = 1.4f;//1.2f;
                

                //no turbo 200 kmh sound
                initEffectForSpeed(dieselPrefix, audioInfos, defaultEngineSound, parent, 200);

                return effect;

                
            }
            else
            {
                Debug.Log("Could not find default train sound effect!");
                return null;
            }
        }
    }
}

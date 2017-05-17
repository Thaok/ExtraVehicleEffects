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
    public class DirectCurrentTrainMovement
    {
        private const string effectGroupName = "Direct Current Train Movement";

        //mechanical braking
        private const string effectName80  = "Direct Current Train Movement 80";
        private const string effectName100 = "Direct Current Train Movement 100";
        private const string effectName120 = "Direct Current Train Movement 120";
        
        //dynamic braking + machanical braking for stopping
        private const string effectName80dyn  = "Direct Current Train Movement 80 Dyn";
        private const string effectName100dyn = "Direct Current Train Movement 100 Dyn";
        private const string effectName120dyn = "Direct Current Train Movement 120 Dyn";

        private class AudioInfoSet
        {
            public AudioInfo info_lowRPM;
            public AudioInfo info_mediumRPM;
            public AudioInfo info_highRPM;
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

        private static DirectCurrentEngineSoundEffect initEffect(string effectName, AudioInfoSet audioInfos, EngineSoundEffect defaultEngineSound, Transform parent)
        {
            GameObject obj = new GameObject(effectGroupName);
            obj.transform.parent = parent;

            DirectCurrentEngineSoundEffect effect = DirectCurrentEngineSoundEffect.CopyEngineSoundEffect(defaultEngineSound, obj.AddComponent<DirectCurrentEngineSoundEffect>());

            effect.name = effectName;

            effect.m_low_audioInfo = audioInfos.info_lowRPM;
            effect.m_medium_audioInfo = audioInfos.info_mediumRPM;
            effect.m_high_audioInfo = audioInfos.info_highRPM;
            effect.m_idle_audioInfo = audioInfos.info_idle;
            effect.m_braking_audioInfo = audioInfos.info_braking;

            return effect;
        }

        public static void SetOptimalVehicleKinematics()
        {
            Util.setVehicleKinematicsForEffect("Direct Current Train Movement", 0.2f, 0.2f, -1.9f);
        }

        public static EffectInfo CreateEffectObject(Transform parent)
        {
            //locate the deafult effect: some parameter values such as range may still be inherited
            EngineSoundEffect defaultEngineSound = Util.FindEffect("Train Movement") as EngineSoundEffect;
            if (defaultEngineSound == null)
            {
                Debug.Log("Could not find default train sound effect!");
                return null;
            }

            //init audio infos shared across effect variants for different speeds and dynamic/mechanical braking
            AudioInfoSet audioInfos = new AudioInfoSet();
            audioInfos.info_lowRPM = initAudioInfo("Sounds/dc-engine-et171-lowRPM.ogg", defaultEngineSound);
            audioInfos.info_mediumRPM = initAudioInfo("Sounds/dc-engine-et171-mediumRPM.ogg", defaultEngineSound);
            audioInfos.info_highRPM = initAudioInfo("Sounds/dc-engine-et171-highRPM.ogg", defaultEngineSound);
            audioInfos.info_idle = initAudioInfo("Sounds/dc-engine-et171-idle.ogg", defaultEngineSound);
            audioInfos.info_braking = initAudioInfo("Sounds/train-brake-disc.ogg", defaultEngineSound);

            if (audioInfos.info_lowRPM == null || audioInfos.info_mediumRPM == null || audioInfos.info_highRPM == null || audioInfos.info_idle == null || audioInfos.info_braking == null)
            {
                Debug.Log("Error initializing audio infos!");
                return null;
            }

            //80 km/h + mechanical braking         
            DirectCurrentEngineSoundEffect directCurrentEngineSound80 = initEffect(effectName80, audioInfos, defaultEngineSound, parent);
            directCurrentEngineSound80.m_maxSpeed = ExtraVehicleEffects.Util.SpeedKmHToEffect(80);
            directCurrentEngineSound80.m_use_dynamic_brake = false;

            //100 km/h + mechanical braking    
            DirectCurrentEngineSoundEffect directCurrentEngineSound100 = initEffect(effectName100, audioInfos, defaultEngineSound, parent);
            directCurrentEngineSound100.m_maxSpeed = ExtraVehicleEffects.Util.SpeedKmHToEffect(95);
            directCurrentEngineSound100.m_use_dynamic_brake = false;

            //120 km/h + mechanical braking    
            DirectCurrentEngineSoundEffect directCurrentEngineSound120 = initEffect(effectName120, audioInfos, defaultEngineSound, parent);
            directCurrentEngineSound120.m_maxSpeed = ExtraVehicleEffects.Util.SpeedKmHToEffect(115);
            directCurrentEngineSound120.m_use_dynamic_brake = false;


            //80 km/h + dynamic braking         
            DirectCurrentEngineSoundEffect directCurrentEngineSound80dyn = initEffect(effectName80dyn,audioInfos,defaultEngineSound,parent);
            directCurrentEngineSound80dyn.m_maxSpeed = ExtraVehicleEffects.Util.SpeedKmHToEffect(80);
            directCurrentEngineSound80dyn.m_use_dynamic_brake = true;

            //100 km/h + dynamic braking    
            DirectCurrentEngineSoundEffect directCurrentEngineSound100dyn = initEffect(effectName100dyn, audioInfos, defaultEngineSound, parent);
            directCurrentEngineSound100dyn.m_maxSpeed = ExtraVehicleEffects.Util.SpeedKmHToEffect(100);
            directCurrentEngineSound100dyn.m_use_dynamic_brake = true;

            //120 km/h + dynamic braking    
            DirectCurrentEngineSoundEffect directCurrentEngineSound120dyn = initEffect(effectName120dyn, audioInfos, defaultEngineSound, parent);
            directCurrentEngineSound120dyn.m_maxSpeed = ExtraVehicleEffects.Util.SpeedKmHToEffect(120);
            directCurrentEngineSound120dyn.m_use_dynamic_brake = true;



            return directCurrentEngineSound80dyn;//just return something non-null (is this ever used?)
           
        }
    }
}

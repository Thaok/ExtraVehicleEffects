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
    public class SpragueTrainMovement
    {

        //mechanical braking
        private const string effectName  = "Sprague Train Movement";
        private class AudioInfoSet
        {
            public AudioInfo info_lowRPM;
            public AudioInfo info_mediumRPM;
            public AudioInfo info_highRPM;
            public AudioInfo info_idle;
            public AudioInfo info_braking;
            public AudioInfo info_airstroke;
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

        private static SpragueEngineSoundEffect initEffect(string effectName, AudioInfoSet audioInfos, EngineSoundEffect defaultEngineSound, Transform parent)
        {
            GameObject obj = new GameObject(effectName);
            obj.transform.parent = parent;

            SpragueEngineSoundEffect effect = SpragueEngineSoundEffect.CopyEngineSoundEffect(defaultEngineSound, obj.AddComponent<SpragueEngineSoundEffect>());

            effect.name = effectName;

            effect.m_low_audioInfo = audioInfos.info_lowRPM;
            effect.m_medium_audioInfo = audioInfos.info_mediumRPM;
            effect.m_high_audioInfo = audioInfos.info_highRPM;
            effect.m_idle_audioInfo = audioInfos.info_idle;
            effect.m_braking_audioInfo = audioInfos.info_braking;
            effect.m_airstroke_audioInfo = audioInfos.info_airstroke;

            return effect;
        }


        static AudioInfo initBrakeAirAudioInfo()
        {
            // Create a copy of an audioInfo
            var templateSound = Util.FindEffect("Train Movement") as SoundEffect;
            AudioInfo audioInfo = UnityEngine.Object.Instantiate(templateSound.m_audioInfo) as AudioInfo;
            audioInfo.name = effectName;
            audioInfo.m_fadeLength = 0.0f;
            audioInfo.m_loop = false;
            audioInfo.m_pitch = 1.0f;
            audioInfo.m_volume = 0.5f;
            audioInfo.m_randomTime = false;

            // Load new audio clip
            AudioClip clip = Util.LoadAudioClipFromModDir("Sounds/sprague-brake-airstroke1.ogg");
            audioInfo.m_fadeLength = 0.0f;

            bool hasClip = false;
            AudioInfo.Variation[] variations = new AudioInfo.Variation[3];
            for (int i = 0; i < variations.Length; ++i)
            {
                variations[i].m_sound = UnityEngine.Object.Instantiate(audioInfo) as AudioInfo;
                //Debug.Log("Sounds/sprague-brake-airstroke" + (i + 1) + ".ogg");
                variations[i].m_sound.m_clip = Util.LoadAudioClipFromModDir("Sounds/sprague-brake-airstroke" + (i + 1) + ".ogg");
                variations[i].m_sound.name = "";
                variations[i].m_sound.m_volume = 0.5f;
                variations[i].m_sound.m_fadeLength = 0.0f;
                variations[i].m_sound.m_loop = false;
                if (variations[i].m_sound.m_clip != null)
                    hasClip = true;
            }

            variations[0].m_probability = 33;
            variations[1].m_probability = 33;
            variations[2].m_probability = 33;


            if (clip != null)
            {
                //Debug.Log("hasClip: " + hasClip);
                audioInfo.m_clip = clip;
                if(hasClip)
                    audioInfo.m_variations = variations;
            }
            else
            {
                return null;
            }

            return audioInfo;
        }

        public static void SetOptimalVehicleKinematics()
        {
            Util.setVehicleKinematicsForEffect(effectName, 0.2f, 0.2f, -1.9f);
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
            audioInfos.info_airstroke = initBrakeAirAudioInfo();

            if (audioInfos.info_lowRPM == null || audioInfos.info_mediumRPM == null || audioInfos.info_highRPM == null || audioInfos.info_idle == null || audioInfos.info_braking == null)
            {
                Debug.Log("Error initializing audio infos!");
                return null;
            }

            //70 km/h + mechanical braking         
            SpragueEngineSoundEffect spragueEngineSound = initEffect(effectName, audioInfos, defaultEngineSound, parent);
            spragueEngineSound.m_maxSpeed = ExtraVehicleEffects.Util.SpeedKmHToEffect(80);
            spragueEngineSound.m_use_dynamic_brake = false;



            return spragueEngineSound;//just return something non-null (is this ever used?)
           
        }
    }
}

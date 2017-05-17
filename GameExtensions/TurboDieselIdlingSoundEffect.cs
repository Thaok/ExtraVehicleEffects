using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using ColossalFramework.Math;

namespace ExtraVehicleEffects.GameExtensions
{

    public class TurboDieselIdlingSoundEffect : SoundEffect
    {
        public static TurboDieselIdlingSoundEffect CopyEngineSoundEffect(EngineSoundEffect template, TurboDieselIdlingSoundEffect target)
        {
            target.m_minPitch = template.m_minPitch;
            target.m_minRange = template.m_minRange;
            target.m_pitchAccelerationMultiplier = template.m_pitchAccelerationMultiplier;
            target.m_pitchSpeedMultiplier = template.m_pitchSpeedMultiplier;
            target.m_position = template.m_position;
            target.m_range = template.m_range;
            target.m_rangeAccelerationMultiplier = template.m_rangeAccelerationMultiplier;
            target.m_rangeSpeedMultiplier = template.m_rangeSpeedMultiplier;
            target.m_audioInfo = template.m_audioInfo;

            return target;
        }

        public float m_minPitch = 1f;
        public float m_pitchSpeedMultiplier = 0.02f;
        public float m_pitchAccelerationMultiplier;
        public float m_minRange = 50f;
        public float m_rangeSpeedMultiplier;
        public float m_rangeAccelerationMultiplier = 50f;

        //public AudioInfo m_idle_audioInfo;

        public bool m_do_heating = false;

        private const float m_speedDeadzone = 3.0f;

        private const float m_heatingTempThreshold = 15.0f;
        public float m_noHeatingPitch = 0.7f;
        private const float m_heatingBasePitch = 0.8f;

        private const uint m_idle_variation_interval = 3;
        private uint m_last_idleVariation;
        private const float m_max_idleRandomVariation = 0.2f;
        private const float m_max_idleTemperatureCompoenent = 0.3f;
        private float m_idleVariation = 0.0f;
                        
        

        public override void PlayEffect(InstanceID id, EffectInfo.SpawnArea area, Vector3 velocity, float acceleration, float magnitude, AudioManager.ListenerInfo listenerInfo, AudioGroup audioGroup)
        {
            float speed = velocity.magnitude;

            if (speed > m_speedDeadzone)
                return;

            Vector3 position = area.m_matrix.MultiplyPoint(this.m_position);

            float range = this.m_range;
            //float range = Mathf.Min(this.m_minRange, this.m_range);
            
            if (!(Vector3.SqrMagnitude(position - listenerInfo.m_position) < range * range))
                return;

            List<AudioUtil.AudioInfoPlaybackState> activeAudioInfos = new List<AudioUtil.AudioInfoPlaybackState>();

            
            if (m_do_heating) {
                float temp = Util.getCurrentTemperature();
                if (temp < m_heatingTempThreshold)
                {
                    float normalizedColdness = 1.0f - Mathf.Min(1.0f, temp / m_heatingTempThreshold);
                    uint time = (uint)Mathf.Round(Time.realtimeSinceStartup);
                    if( (time - m_last_idleVariation) > m_idle_variation_interval ){
                        Randomizer randomizer = new Randomizer(id.RawData);
                        m_idleVariation = ((float)randomizer.UInt32(100)/(float)100) * m_max_idleRandomVariation;
                        m_last_idleVariation = time;
                    }

                    activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_audioInfo, magnitude, m_heatingBasePitch + m_idleVariation + normalizedColdness * m_max_idleTemperatureCompoenent));//higher idle rpm to supply heating        
                }
            }
            else
            {
                activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_audioInfo, magnitude, m_noHeatingPitch));//lower idle rpm
            }
            
            for (int i = 0; i < activeAudioInfos.Count; ++i)
            {
                Randomizer randomizer = new Randomizer(id.RawData);
                AudioInfo variation = activeAudioInfos[i].info.GetVariation(ref randomizer);
                audioGroup.AddPlayer(listenerInfo, (int)id.RawData, variation, position, velocity, range, activeAudioInfos[i].volume, activeAudioInfos[i].pitch);
            }

        }
    }

}

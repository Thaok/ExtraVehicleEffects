using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using ColossalFramework.Math;

namespace ExtraVehicleEffects.GameExtensions
{

    public class DirectCurrentEngineSoundEffect : SoundEffect
    {

        public static DirectCurrentEngineSoundEffect CopyEngineSoundEffect(EngineSoundEffect template, DirectCurrentEngineSoundEffect target)
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

        private const float m_acc_deadzone = 0.05f;
        public float m_maxSpeed = ExtraVehicleEffects.Util.SpeedKmHToEffect(80.0f);//80.0f
        //optimal vehicle acceleration/braking: 0.2 / 0.2

        public bool m_use_dynamic_brake = true;
        public float m_dynamic_brake_cutoff_speed = 0.1f;

        public float m_global_volume_adjustment = 1.3f;

        private const float m_fading_width = 0.1f;//0.05f;
        private const float m_medium_start = 0.2f;
        private const float m_medium_end = 0.5f;
        private const float m_high_end = 0.95f;
        private const float m_idle_end = 1.5f;
        static private AudioUtil.PiecewiseLinear m_lowRpmVolumeBySpeed = new AudioUtil.PiecewiseLinear(new float[,] { { 0.0f, 1.00f }, { m_medium_start, 1.00f }, { m_medium_start + m_fading_width, 0.00f } }, 0.0f);
        static private AudioUtil.PiecewiseLinear m_mediumRpmVolumeBySpeed = new AudioUtil.PiecewiseLinear(new float[,] { { m_medium_start - m_fading_width, 0.00f }, { m_medium_start, 1.00f }, { m_medium_end, 1.00f }, { m_medium_end + m_fading_width, 0.0f } }, 0.0f);
        static private AudioUtil.PiecewiseLinear m_highRpmVolumeBySpeed = new AudioUtil.PiecewiseLinear(new float[,] { { m_medium_end - 0.2f * m_fading_width, 0.00f }, { m_medium_end, 1.00f }, { m_high_end, 1.00f }, { m_high_end + m_fading_width, 0.00f } }, 0.0f);
        static private AudioUtil.PiecewiseLinear m_idleRpmVolumeBySpeed = new AudioUtil.PiecewiseLinear(new float[,] { { m_high_end - m_fading_width, 0.00f }, { m_high_end, 1.50f }, { m_idle_end, 1.50f } }, 0.0f);

        //public float m_base_pitch_high = 0.15f;
        //public float m_pitch_constant_high = 0.75f;
        //static private Util.PiecewiseLinear m_lowRpmVolumeBySpeed = new Util.PiecewiseLinear(new float[,] { { 0.0f, 0.00f }, { m_medium_start, 0.00f }, { m_medium_start + m_fading_width, 0.00f } }, 0.0f);
        //static private Util.PiecewiseLinear m_mediumRpmVolumeBySpeed = new Util.PiecewiseLinear(new float[,] { { m_medium_start - m_fading_width, 0.00f }, { m_medium_start, 0.00f }, { m_medium_end, 0.00f }, { m_medium_end + m_fading_width + 0.2f, 0.0f } }, 0.0f);
        //static private Util.PiecewiseLinear m_highRpmVolumeBySpeed = new Util.PiecewiseLinear(new float[,] { { 0.0f, 1.00f }, { m_high_end, 1.00f }, { m_high_end + m_fading_width, 0.00f } }, 0.0f);
        //static private Util.PiecewiseLinear m_idleRpmVolumeBySpeed = new Util.PiecewiseLinear(new float[,] { { m_high_end - m_fading_width, 0.00f }, { m_high_end, 1.50f }, { m_idle_end, 1.50f } }, 0.0f);


        public float m_pitch_constant_low = 1.3f * 1.5f * 2.0f;
        public float m_pitch_constant_medium = 1.7f;
        public float m_pitch_constant_high = 0.2f;
        public float m_pitch_constant_idle = 1.0f;

        public float m_base_pitch_low = 0.9f;
        public float m_base_pitch_medium = 0.7f;
        public float m_base_pitch_high = 0.8f;
        public float m_base_pitch_idle = 1.0f;


        public AudioInfo m_low_audioInfo;
        public AudioInfo m_medium_audioInfo;
        public AudioInfo m_high_audioInfo;
        public AudioInfo m_idle_audioInfo;
        public AudioInfo m_braking_audioInfo;//only used shortly before stop when dynamic brake enabled
                
        private void addEngineSound(ref List<AudioUtil.AudioInfoPlaybackState> activeAudioInfos, float normalizedSpeed, float magnitude, float volumeAdjustment = 1.0f)
        {
            float volume = m_global_volume_adjustment * volumeAdjustment * m_lowRpmVolumeBySpeed.value(normalizedSpeed);
            if (volume > 0.0f)
            {
                float pitch = m_base_pitch_low + m_pitch_constant_low * normalizedSpeed;
                activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_low_audioInfo, volume * magnitude, pitch));
            }

            volume = m_global_volume_adjustment * volumeAdjustment * m_mediumRpmVolumeBySpeed.value(normalizedSpeed);
            if (volume > 0.0f)
            {
                float pitch = m_base_pitch_medium + m_pitch_constant_medium * normalizedSpeed;
                activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_medium_audioInfo, volume * magnitude, pitch));
            }

            volume = m_global_volume_adjustment * volumeAdjustment * m_highRpmVolumeBySpeed.value(normalizedSpeed);
            if (volume > 0.0f)
            {
                float pitch = m_base_pitch_high + m_pitch_constant_high * normalizedSpeed;
                activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_high_audioInfo, volume * magnitude, pitch));
            }

            volume = m_global_volume_adjustment * volumeAdjustment * m_idleRpmVolumeBySpeed.value(normalizedSpeed);
            if (volume > 0.0f)
            {
                float pitch = m_base_pitch_idle;// (m_pitch_constant + m_pitch_adjust_idle) * normalizedSpeed;
                activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_idle_audioInfo, volume * magnitude, pitch));
            }
        }

        public override void PlayEffect(InstanceID id, EffectInfo.SpawnArea area, Vector3 velocity, float acceleration, float magnitude, AudioManager.ListenerInfo listenerInfo, AudioGroup audioGroup)
        {
            Vector3 position = area.m_matrix.MultiplyPoint(this.m_position);
            float speed = velocity.magnitude;
            float range = Mathf.Min(this.m_minRange + speed * this.m_rangeSpeedMultiplier + acceleration + acceleration * this.m_rangeAccelerationMultiplier, this.m_range);

            if (!(Vector3.SqrMagnitude(position - listenerInfo.m_position) < range * range))
                return;

            float normalizedSpeed = speed / m_maxSpeed;
            List<AudioUtil.AudioInfoPlaybackState> activeAudioInfos = new List<AudioUtil.AudioInfoPlaybackState>();

            //Debug.Log("acc: " + acceleration.ToString("0.00") + "| nrm-spd:" + normalizedSpeed.ToString("0.00"));


            if (acceleration > m_acc_deadzone || (m_use_dynamic_brake && (normalizedSpeed > m_dynamic_brake_cutoff_speed)))//accelerating
            {

                addEngineSound(ref activeAudioInfos,normalizedSpeed,magnitude,1.0f);
                
            }
            else //if (acceleration < -m_acc_deadzone && ( !m_use_dynamic_brake || (normalizedSpeed < m_dynamic_brake_cutoff_speed)) ) //machanical braking
            {
                //Debug.Log("braking!");

                float invertedSpeed = 1.0f - normalizedSpeed;
                float invertedSpeedSqr = invertedSpeed * invertedSpeed;
                float brakePitch = 0.7f + invertedSpeedSqr * 0.2f;
                float volumeModifier = (1.0f + invertedSpeedSqr * 0.4f);

                //Debug.Log("braking, pitch=" + pitch.ToString("0.00") + " volumeModifier=" + volumeModifier.ToString("00.00"));
                
                //play lowest rpm engine sound if dynamic brake enabled
                if (m_use_dynamic_brake)
                {
                    volumeModifier *= ((m_dynamic_brake_cutoff_speed - normalizedSpeed) / m_dynamic_brake_cutoff_speed);

                    float volume = m_global_volume_adjustment * m_lowRpmVolumeBySpeed.value(normalizedSpeed);
                    if (volume > 0.0f)
                    {
                        float pitch = m_base_pitch_low + m_pitch_constant_low * normalizedSpeed;
                        activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_low_audioInfo, volume * magnitude, pitch));
                    }

                    //Debug.Log("braking, volumeModifier=" + volumeModifier.ToString("0.00"));
                    if (volumeModifier * magnitude > 0.05f)
                        activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_braking_audioInfo, volumeModifier * magnitude, brakePitch));
                }
                else
                {
                    //play idle sound during mechanical braking
                    //{
                    //    float pitch = m_base_pitch_idle - (1.0f-normalizedSpeed) * 0.5f;
                    //    activeAudioInfos.Add(new AudioInfoPlaybackState(this.m_idle_audioInfo, 1.5f * magnitude, pitch));
                    //}

                    //play quieter (no-load) engine sound during mechanical braking
                    addEngineSound(ref activeAudioInfos, normalizedSpeed, magnitude, 0.5f);

                    //brake sqeal before stopping
                    if (normalizedSpeed < 3.0f * m_dynamic_brake_cutoff_speed)
                    {
                        //Debug.Log("braking, volumeModifier=" + volumeModifier.ToString("0.00"));
                        if (volumeModifier * magnitude > 0.05f)
                            activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_braking_audioInfo, volumeModifier * magnitude, brakePitch));
                    }

                    //low rpm sound
                    float volume = m_global_volume_adjustment * m_lowRpmVolumeBySpeed.value(normalizedSpeed);
                    if (volume > 0.0f)
                    {
                        float pitch = m_base_pitch_low + m_pitch_constant_low * normalizedSpeed;
                        activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_low_audioInfo, volume * magnitude, pitch));
                    }

                }
                              

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

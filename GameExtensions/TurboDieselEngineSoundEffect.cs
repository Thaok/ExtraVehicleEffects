using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using ColossalFramework.Math;

namespace ExtraVehicleEffects.GameExtensions
{

    public class TurboDieselEngineSoundEffect : SoundEffect
    {
        public static TurboDieselEngineSoundEffect CopyEngineSoundEffect(EngineSoundEffect template, TurboDieselEngineSoundEffect target)
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

        public const float m_acc_deadzone = 0.05f;
        private float m_referenceSpeed = ExtraVehicleEffects.Util.SpeedKmHToEffect(93);// 56.0f;
        private float m_maxSpeed = ExtraVehicleEffects.Util.SpeedKmHToEffect(93);
        //optimal vehicle acceleration/braking: 0.2 / 0.2

        private ExtraVehicleEffects.AudioUtil.PiecewiseLinear pitchBySpeed = new ExtraVehicleEffects.AudioUtil.PiecewiseLinear(new float[,]{ 
            {0.0f, 0.75f}, {0.08f, 0.75f}, 
            {0.1f, 1.0f},  {0.18f, 1.0f},
            {0.2f, 1.25f}, {0.28f, 1.25f},
            {0.3f, 1.5f},  {0.7f, 1.5f},
            {0.9f, 1.1f},  {10.0f, 1.1f}
        });

        //private ExtraVehicleEffects.Util.PiecewiseLinear pitchBySpeed = new ExtraVehicleEffects.Util.PiecewiseLinear(new float[,]{ 
        //    {0.0f, 0.75f}, {0.08f, 0.75f}, 
        //    {0.1f, 1.0f},  {0.18f, 1.0f},
        //    {0.2f, 1.25f}, {0.28f, 1.25f},
        //    {0.3f, 1.5f},  {0.5f, 1.5f},
        //    {0.35f, 1.55f},  {0.7f, 1.55f},
        //    {0.9f, 1.1f},  {10.0f, 1.1f}
        //});

        public void adjustMaxSpeed(float maxSpeed)
        {
            this.m_maxSpeed = maxSpeed;
            pitchBySpeed.setPoint(8, pitchBySpeed.points[8, AudioUtil.PiecewiseLinear.X] * (m_maxSpeed / m_referenceSpeed),
                                     pitchBySpeed.points[8, AudioUtil.PiecewiseLinear.Y]);
        }

        static private AudioUtil.PiecewiseLinear pitchBySpeedBraking = new AudioUtil.PiecewiseLinear(new float[,]{ 
            {0.0f, 0.7f},  {0.7f, 0.7f},
            {0.9f, 1.1f},  {1.5f, 1.1f}
        });

        public float m_enigne_volume_adjust = 1.2f;

        public bool m_hasTurbo = true;
        private Dictionary<uint, float> m_turboStates = new Dictionary<uint, float>();
        public float m_turbo_seek_speed = 0.13f;
        public float m_turbo_volume_adjust = 0.9f;
        public float m_turbo_pitch_adjust = 1.0f;
        public float m_turbo_minPitch = 0.8f;

        public AudioInfo m_idle_audioInfo;
        public AudioInfo m_turbo_audioInfo;
        public AudioInfo m_braking_audioInfo;

        static public float m_brake_volumeAdjust = 1.0f;
        

        //private static VehicleEffects.Util.PiecewiseLinear createTurboPitch(VehicleEffects.Util.PiecewiseLinear enginePitch, float inertia)
        //{
        //    float[,] turboPitch = new float[enginePitch.points.GetLength(0), 2];



        //    return new VehicleEffects.Util.PiecewiseLinear(new float[,] { });
        //}

        public override void PlayEffect(InstanceID id, EffectInfo.SpawnArea area, Vector3 velocity, float acceleration, float magnitude, AudioManager.ListenerInfo listenerInfo, AudioGroup audioGroup)
        {
            Vector3 position = area.m_matrix.MultiplyPoint(this.m_position);
            float speed = velocity.magnitude;
            float range = Mathf.Min(this.m_minRange + speed * this.m_rangeSpeedMultiplier + acceleration + acceleration * this.m_rangeAccelerationMultiplier, this.m_range);
            
            if (!(Vector3.SqrMagnitude(position - listenerInfo.m_position) < range * range))
                return;

            List<AudioUtil.AudioInfoPlaybackState> activeAudioInfos = new List<AudioUtil.AudioInfoPlaybackState>();

            float normalizedSpeed = speed / m_referenceSpeed;

            float enginePitch;

            //Debug.Log("acc: " + acceleration.ToString("0.00") + "| nrm-spd:" + normalizedSpeed.ToString("0.00") );
            if (normalizedSpeed > 0.0f)
            {
                if (acceleration > -m_acc_deadzone)//accelerating
                {
                    //Debug.Log("acc");
                    enginePitch = pitchBySpeed.value(normalizedSpeed) + 0.1f;
                    activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_audioInfo, m_enigne_volume_adjust * magnitude, enginePitch));
                    //Debug.Log("nrm-spd:" + normalizedSpeed.ToString("0.00") + " | enginePitch: " + enginePitch.ToString("0.00"));
                    //Debug.Log("pitch: " + pitch.ToString("0.00") + "| turboState:" + turboState.ToString("0.00") + "| deltaTime: " + Time.deltaTime.ToString("0.0000"));
                    //Debug.Log("acc: " + acceleration.ToString("0.00") + "| nrm-spd:" + normalizedSpeed.ToString("0.00") + "| pitch: " + pitch.ToString("0.00"));

                }
                else //if (acceleration < -m_acc_deadzone)//braking
                {
                    //Debug.Log("brake");
                    //play the brake squealing louder and higher towards the end
                    float invertedSpeed = 1.0f - normalizedSpeed;
                    float invertedSpeedSqr = invertedSpeed * invertedSpeed;
                    float pitch = 0.6f + invertedSpeedSqr * 0.2f;
                    float volumeModifier = 0.8f * (1.0f + invertedSpeedSqr * 0.4f);

                    //Debug.Log("acc: " + acceleration.ToString("0.00") + "| nrm-spd:" + normalizedSpeed.ToString("0.00") + "| pitch: " + (0.1f + pitchBySpeedBraking.value(normalizedSpeed)).ToString("0.00"));
                    activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_braking_audioInfo, m_brake_volumeAdjust * volumeModifier * magnitude, pitch));
                    //activeAudioInfos.Add(new AudioInfoPlaybackState(this.m_idle_audioInfo, magnitude, 1.0f));//idling during braking

                    enginePitch = pitchBySpeedBraking.value(normalizedSpeed);
                    activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_audioInfo, m_enigne_volume_adjust * magnitude, enginePitch));
                }
            }
            else
            {
                //Debug.Log("stop");
                enginePitch = 0.7f;
            }           


            //separate turbo sound following engine rpm with some delay (turbine inertial moment) and smoothly across notches
            float turboState;
            if (!m_turboStates.TryGetValue(id.Index, out turboState))
                turboState = enginePitch;
            turboState += turboState < Math.Max(enginePitch, m_turbo_minPitch) ? Time.deltaTime * m_turbo_seek_speed : -Time.deltaTime * m_turbo_seek_speed;
            activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_turbo_audioInfo, m_turbo_volume_adjust * magnitude, m_turbo_pitch_adjust * turboState));
            if (speed < ExtraVehicleEffects.Util.SpeedKmHToEffect(5.0f))
                turboState = 0.7f;
            m_turboStates[id.Index] = turboState;

            //play the AudioInfos
            Randomizer randomizer = new Randomizer(id.RawData);
            for (int i = 0; i < activeAudioInfos.Count; ++i)
            {
                AudioInfo variation = activeAudioInfos[i].info.GetVariation(ref randomizer);
                audioGroup.AddPlayer(listenerInfo, (int)id.RawData, variation, position, velocity, range, activeAudioInfos[i].volume, activeAudioInfos[i].pitch);
            }

        }
    }

}
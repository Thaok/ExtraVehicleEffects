using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using ColossalFramework.Math;

namespace ExtraVehicleEffects.GameExtensions
{

    public class TrainBrakingSoundEffect : SoundEffect
    {
        public static TrainBrakingSoundEffect CopyEngineSoundEffect(EngineSoundEffect template, TrainBrakingSoundEffect target)
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

        public float m_maxSpeed = 56.0f;//80.0f
        public float m_base_pitch = 0.7f;
        public float m_volumeAdjust = 1.0f;
        
        public override void PlayEffect(InstanceID id, EffectInfo.SpawnArea area, Vector3 velocity, float acceleration, float magnitude, AudioManager.ListenerInfo listenerInfo, AudioGroup audioGroup)
        {
            if (acceleration > -m_acc_deadzone)
                return;//not braking

            Vector3 position = area.m_matrix.MultiplyPoint(this.m_position);
            float speed = velocity.magnitude;
            float range = Mathf.Min(this.m_minRange + speed * this.m_rangeSpeedMultiplier + acceleration + acceleration * this.m_rangeAccelerationMultiplier, this.m_range);

            if (!(Vector3.SqrMagnitude(position - listenerInfo.m_position) < range * range))
                return;

            List<AudioUtil.AudioInfoPlaybackState> activeAudioInfos = new List<AudioUtil.AudioInfoPlaybackState>();

            float normalizedSpeed = speed / m_maxSpeed;


            //play the break squealing louder and higher towards the end
            float invertedSpeed = 1.0f - normalizedSpeed;
            float invertedSpeedSqr = invertedSpeed * invertedSpeed;
            float pitch = m_base_pitch + invertedSpeedSqr * 0.2f;
            float volumeModifier = 0.8f * (1.0f + invertedSpeedSqr * 0.4f);

            activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(this.m_audioInfo, m_volumeAdjust * volumeModifier * magnitude, pitch));


            for (int i = 0; i < activeAudioInfos.Count; ++i)
            {
                Randomizer randomizer = new Randomizer(id.RawData);
                AudioInfo variation = activeAudioInfos[i].info.GetVariation(ref randomizer);
                audioGroup.AddPlayer(listenerInfo, (int)id.RawData, variation, position, velocity, range, activeAudioInfos[i].volume, activeAudioInfos[i].pitch);
            }

        }
    }


}
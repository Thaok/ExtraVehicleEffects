using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using ColossalFramework.Math;

namespace ExtraVehicleEffects.GameExtensions
{

    public class TrainFlangeGrindingSoundEffect : SoundEffect
    {
        public static TrainFlangeGrindingSoundEffect CopyEngineSoundEffect(EngineSoundEffect template, TrainFlangeGrindingSoundEffect target)
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

        public float m_maxSpeed = 100.0f;//reference for pitch calculation
        public float m_minVolume = 30.0f;//below this volume (speed) there is no grinding
        public float m_curvingSoundCoefficient = 1.0f;
        public float m_curvingSoundThreshold = 1000.0f;//this radius is traited to be straight

        //static private uint m_sample_counter = 0;
        //static private uint m_ignored_calls = 2;

        struct KinematicState
        {
            public Vector3 pos;
            public Vector3 vel;
            public KinematicState(Vector3 pos, Vector3 vel) { this.pos = pos; this.vel = vel; }
        }

        Dictionary<uint, KinematicState> m_lastStates = new Dictionary<uint, KinematicState>();
        
        public override void PlayEffect(InstanceID id, EffectInfo.SpawnArea area, Vector3 velocity, float acceleration, float magnitude, AudioManager.ListenerInfo listenerInfo, AudioGroup audioGroup)
        {
            //reduce update rate for this sound
            //++m_sample_counter;
            //if (m_sample_counter < m_ignored_calls)
            //    return;
            //m_sample_counter = 0;

            Vector3 position = area.m_matrix.MultiplyPoint(this.m_position);

            float speed = velocity.magnitude;
            if (speed < 0.01f)
                return;

            float range = Mathf.Min(this.m_minRange + speed * this.m_rangeSpeedMultiplier + acceleration + acceleration * this.m_rangeAccelerationMultiplier, this.m_range);
            if (!(Vector3.SqrMagnitude(position - listenerInfo.m_position) < range * range))
                return;

            List<AudioUtil.AudioInfoPlaybackState> activeAudioInfos = new List<AudioUtil.AudioInfoPlaybackState>();

            //get the last kinematic state for this instance
            KinematicState lastState;
            if (!m_lastStates.TryGetValue(id.Index, out lastState))
                m_lastStates.Add(id.Index, new KinematicState(position, velocity));

            //calculate the instantaneus center for last and current state by intersecting two orthogonal bisectors (line plane for numerical stability)
            Vector3 deltaPos = position - lastState.pos;
            Ray currentRadialLine = new Ray(position, Vector3.Cross(velocity, Vector3.up).normalized);
            Plane lastRadialPlane = new Plane(lastState.vel.normalized, lastState.pos);
            float radius;
            lastRadialPlane.Raycast(currentRadialLine, out radius);
            radius = Mathf.Abs(radius);//we just need a line and no ray here since we are only interested in the center distance   

            //if (deltaPos.magnitude > 0.1f)
            //Debug.Log("deltaPos: " + deltaPos.magnitude + "| radius: " + radius);

            //compute the grinding sound
            if ((radius != 0.0f) && (radius < m_curvingSoundThreshold))
            {
                float normalizedSpeed = speed / m_maxSpeed;
                //float volume = m_curvingSoundCoefficient * ( speed * speed / radius);
                float volume = 0.5f + 1.0f * (m_curvingSoundThreshold / radius);
                float pitch = 0.8f + 0.3f * normalizedSpeed;
                if (volume > 1.0f)
                    volume = 1.0f;
                if (volume > 0.2f)
                    activeAudioInfos.Add(new AudioUtil.AudioInfoPlaybackState(m_audioInfo, volume, pitch));
                //Debug.Log("volume:" + volume.ToString("0.00") + "| pitch: " + (0.5f + 0.5f * normalizedSpeed).ToString("0.00"));
            }

            //store the current state
            lastState.pos = position;
            lastState.vel = velocity;
            m_lastStates[id.Index] = lastState;


            //schedule AudioInfo playback
            for (int i = 0; i < activeAudioInfos.Count; ++i)
            {
                Randomizer randomizer = new Randomizer(id.RawData);
                AudioInfo variation = activeAudioInfos[i].info.GetVariation(ref randomizer);
                audioGroup.AddPlayer(listenerInfo, (int)id.RawData, variation, position, velocity, range, activeAudioInfos[i].volume, activeAudioInfos[i].pitch);
            }

        }
    }


}
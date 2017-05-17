//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using ColossalFramework.Math;
//using UnityEngine;

//namespace VehicleEffects.GameExtensions
//{
//    class ComplexEngineSoundEffect : SoundEffect
//    {
         
//        public class PiecewiseLinear
//        {
//            private Vector2[] points;
//            private float[] slopes;
//            private float neutralElement;
//            public float value(float param)
//            {
//                for (int i = 0; i<points.Length-1; ++i)
//                    if ( param > points[i].x && param < points[i + 1].x )
//                        return points[i].y + slopes[i] * (param - points[i].x);
//                return neutralElement;//return if curve is empty
//            }
//            public PiecewiseLinear(float neutralElement = 1.0f) { this.neutralElement = neutralElement; }
//            public PiecewiseLinear(Vector2[] points, float neutralElement = 1.0f)
//            {
//                this.points = points;
//                this.neutralElement = neutralElement;
//                slopes = new float[points.Length - 1];
//                for (int i = 0; i < points.Length - 1; ++i)
//                    slopes[i] = (points[i + 1].y - points[i].y) / (points[i + 1].x - points[i].x);
//            }
//        }

//        public float m_minPitch = 1f;
//        public float m_pitchSpeedMultiplier = 0.02f;
//        public float m_pitchAccelerationMultiplier;
//        public float m_minRange = 50f;
//        public float m_rangeSpeedMultiplier;
//        public float m_rangeAccelerationMultiplier = 50f;


//        private const float ACCELERATION_DEADZONE = 0.1f;
//        private const float SPEED_DEADZONE = 0.1f;

//        //should be adjusted to vehicle
//        public float m_maxAcceleration = 1.0f;
//        public float m_maxDecceleration = 2.0f;
//        public float m_maxSpeed = 20.0f;             
        
        
//        //volume and pitch chracteristics
//        public PiecewiseLinear m_load_load_volume_chracteristic   = new PiecewiseLinear( new Vector2[] { new Vector2(0.0f,0.0f), new Vector2(1.0f,1.0f) } );
//        public PiecewiseLinear m_speed_speed_volume_chracteristic = new PiecewiseLinear( new Vector2[] { new Vector2(0.0f,0.0f), new Vector2(1.0f,1.0f) } );
//        public PiecewiseLinear m_load_load_pitch_chracteristic    = new PiecewiseLinear( new Vector2[] { new Vector2(0.0f,0.5f), new Vector2(1.0f,1.0f) } );
//        public PiecewiseLinear m_speed_speed_pitch_chracteristic  = new PiecewiseLinear( new Vector2[] { new Vector2(0.0f,0.5f), new Vector2(1.0f,1.0f) } );

//        //cross-dependant chracteristics (e.g. how does speed modifiy the pitch a sound selected by load?)
//        public PiecewiseLinear m_load_speed_volume_chracteristic = new PiecewiseLinear();// = new PiecewiseLinear(new Vector2[] { new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f) });
//        public PiecewiseLinear m_speed_load_volume_chracteristic = new PiecewiseLinear();// = new PiecewiseLinear(new Vector2[] { new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f) });
//        public PiecewiseLinear m_load_speed_pitch_chracteristic  = new PiecewiseLinear();// = new PiecewiseLinear(new Vector2[] { new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f) });
//        public PiecewiseLinear m_speed_load_pitch_chracteristic  = new PiecewiseLinear();// = new PiecewiseLinear(new Vector2[] { new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f) });

//        public PiecewiseLinear m_brakeLoad_breakeLoad_volume_chracteristic = new PiecewiseLinear( new Vector2[] { new Vector2(0.0f,0.0f), new Vector2(1.0f,1.0f) } );
//        public PiecewiseLinear m_brakeLoad_speed_pitch_chracteristic = new PiecewiseLinear( new Vector2[] { new Vector2(0.0f,0.5f), new Vector2(1.0f,1.0f) } );
        

//        //idle sounds
//        bool m_playIdleSoundWhileStanding = true;
//        bool m_playIdleSoundWhileBraking = true;
//        public AudioInfo m_audioInfo_idle = null;

//        public float fadingRange = 0.1f;//normalized to parameter range [0,1]
//        public AudioInfo[] m_audioInfos_byEngineLoadInterval;//e.g. for diesel-electric/hydraulic vehicles (here, engine rpm isn't directly coupled to vehicle speed, but load, i.e. acceleration/braking )
//        public float[] m_load_boundaries;//determines when to play which info, requeires 0.0 and 1.0 as first and last element
//        public AudioInfo[] m_audioInfos_bySpeedInterval;//e.g. for electric vehicles (here, engine load mainly affects volume)
//        public float[] m_speed_boundaries;//determines when to play which info
//        public AudioInfo[] m_audioInfos_byBrakeLoadInterval;
//        public float[] m_brakeLoad_boundaries;//determines when to play which info
//        public bool dynamicBrake = false;

//        public Vector3 m_lastVelocity, m_lastPos;//to compute the instantaneous radius
//        public float m_curvingSoundCoefficient = 1.0f;//how does the radius influence volume?
//        public float m_curvingSoundThreshold = 1.0f;//below which radius does the sound play
//        public AudioInfo m_audioInfo_curving = null;//radius dependant drifting, flange grinding etc.

//        //mainly for turbo chargers 
//        public float m_delayed_pitchSeekSpeed;//seek speed in percent per second, reflecting turbine mass moment of inertia
//        private float m_delayed_load_state;//reflecting turbine rpm
//        public AudioInfo m_audioInfo_delayedLoadDependant = null;
       
//        //fans or everything else related to cooling, also accounts for evironment temparature (if snowfall is present)
//        public float m_engineTemp = 0.0f;
//        public float m_engineTempCoefficient; //relates load to temperature (i.e. relfecting power and cooling)
//        public float m_engineTempThreshold = 50.0f;//when does the fan start to run?
//        public float m_engineCoolingCoefficient; // how efficient is our fan?
//        public PiecewiseLinear m_fan_curve = null;
//        public AudioInfo m_audioInfo_fan = null;
//        private bool m_fanState = false;//fan running?
               

//        private class AudioInfoPlaybackState{
//            public AudioInfo info;
//            public float volume;
//            public float pitch;
//            public AudioInfoPlaybackState(AudioInfo info, float volumeWeight, float pitch) { this.info = info; this.volume = volumeWeight; this.pitch = pitch; }
//        }


//        ComplexEngineSoundEffect(String xml)
//        {
            

//        }
                      
        
//        private void getMixedInfos(float param, float param2, float[] infoParamBoundaries, AudioInfo[] infos,
//                                   PiecewiseLinear volumeChracteristic, PiecewiseLinear pitchChracteristic,
//                                   PiecewiseLinear volumeChracteristic2, PiecewiseLinear pitchChracteristic2,
//                                   ref List<AudioInfoPlaybackState> playbackStates)
//        {
//            //compute values of the characteristics
//            float volume = volumeChracteristic.value(param) * volumeChracteristic2.value(param2);
//            float pitch  = pitchChracteristic.value(param)  * pitchChracteristic2.value(param2);
            
               

//            for(int i=1; i<infoParamBoundaries.Length; ++i)
//                if (param < infoParamBoundaries[i])
//                {

//                    if ( i<infoParamBoundaries.Length-1 && Math.Abs(param - infoParamBoundaries[i]) < fadingRange)
//                    {
//                        //fading at end of interval
//                        float fadingParam = param - (infoParamBoundaries[i] - fadingRange);
//                        float fadingFactor = fadingParam * (1.0f / (1.0f * fadingRange));

//                        playbackStates.Add(new AudioInfoPlaybackState(
//                            infos[i-1],
//                            volume, //* (1.0f - fadingFactor),
//                            pitch
//                        ));

//                        playbackStates.Add(new AudioInfoPlaybackState(
//                             infos[i],
//                             volume * fadingFactor,
//                             pitch
//                        ));
                        
//                    }
//                    else if ( i>1 && Math.Abs(param - infoParamBoundaries[i-1]) < fadingRange)
//                    {
//                        //fading at start of interval
//                        float fadingParam = param - (infoParamBoundaries[i-1] - fadingRange);
//                        float fadingFactor = fadingParam * (1.0f / (1.0f*fadingRange) );

//                        playbackStates.Add(new AudioInfoPlaybackState(
//                            infos[i-1],
//                            volume, 
//                            pitch
//                        ));

//                        playbackStates.Add(new AudioInfoPlaybackState(
//                            infos[i-2],
//                            volume * (1.0f - fadingFactor),
//                            pitch
//                        ));
//                    }
//                    else//just a single info playing
//                    {
//                        playbackStates.Add(new AudioInfoPlaybackState(
//                           infos[i-1],
//                           volume,
//                           pitch
//                       ));
//                    }

//                    break;
//                }
                        
//        }
        


//        private List<AudioInfoPlaybackState> getActiveAudioInfos(InstanceID id, Vector3 position, Vector3 speedVec, float acceleration)
//        {
                        
//            List<AudioInfoPlaybackState> result = new List<AudioInfoPlaybackState>();

//            float speed = speedVec.magnitude;
//            float normalizedSpeed = speed / m_maxSpeed;
//            float normalizedAcceleration = acceleration / m_maxAcceleration;//~load

//            //update fan and temperature
//            if (m_audioInfo_fan != null)
//            {
//                float environmentTemp = 20.0f;//TODO: retrieve from game if snowfall installed
//                m_engineTemp = m_engineTemp + m_engineTempCoefficient * normalizedAcceleration + (m_fanState ? - m_engineCoolingCoefficient * (20.0f-environmentTemp) : 0.0f);

//                if(m_engineTemp > m_engineTempThreshold){
//                    m_fanState = true;
//                    float normalizedFanLoad = m_engineTemp / m_fan_curve.value(1.1f);
//                    result.Add(new AudioInfoPlaybackState(m_audioInfo_fan,m_fan_curve.value(normalizedFanLoad),m_fan_curve.value(normalizedFanLoad)));
//                } else if(m_engineTemp < m_engineTempThreshold - 10.0f)//some hystheresis
//                     m_fanState = false;

//            }

//            //standing
//            if( Math.Abs(speed) < SPEED_DEADZONE)
//            {
//                if( m_playIdleSoundWhileStanding && m_audioInfo_idle != null )
//                    result.Add(new AudioInfoPlaybackState(m_audioInfo_idle, 1.0f, 1.0f));
//                return result;
//            }

//            //speed dependent sounds
//            getMixedInfos(normalizedSpeed,normalizedAcceleration, m_speed_boundaries, m_audioInfos_bySpeedInterval, m_speed_speed_volume_chracteristic, m_speed_speed_pitch_chracteristic,
//                                                                                                                    m_speed_load_volume_chracteristic, m_speed_load_pitch_chracteristic, ref result);

//            if (acceleration > ACCELERATION_DEADZONE)//accelerating
//            {              
//                getMixedInfos(normalizedAcceleration,normalizedSpeed, m_load_boundaries, m_audioInfos_byEngineLoadInterval, m_load_load_volume_chracteristic, m_load_load_pitch_chracteristic,
//                                                                                                                            m_load_speed_volume_chracteristic, m_load_speed_pitch_chracteristic, ref result);   
//            }
//            else if (acceleration < ACCELERATION_DEADZONE)//deccelerating
//            {
//                if( !dynamicBrake || normalizedSpeed < 0.1f )
//                    getMixedInfos(-normalizedAcceleration, 1.0f, m_brakeLoad_boundaries, m_audioInfos_byBrakeLoadInterval, m_brakeLoad_breakeLoad_volume_chracteristic, m_brakeLoad_speed_pitch_chracteristic, 
//                                                                                                                            new PiecewiseLinear(), new PiecewiseLinear(),  ref result);
//                else
//                    getMixedInfos(-normalizedAcceleration,normalizedSpeed, m_load_boundaries, m_audioInfos_byEngineLoadInterval, m_load_load_volume_chracteristic, m_load_load_pitch_chracteristic,
//                                                                                                                                 m_load_speed_volume_chracteristic, m_load_speed_pitch_chracteristic, ref result);   
//            }
//            else//idle movment
//            {
//                if ( m_audioInfo_idle != null )
//                    result.Add(new AudioInfoPlaybackState(m_audioInfo_idle, 1.0f, 1.0f)); 
//            }

//            //update curving sounds
//            if(m_audioInfo_curving != null){
//                //approximate the instantaneous radius: just assume an isosceles triangle between incremental positions and the instantaneous center and compute its height
//                float deltaPos = (position - m_lastPos).magnitude;//just assume the vertical component to be small
//                float velAngle = Mathf.Acos( Vector3.Dot(speedVec, m_lastVelocity) / (speed * m_lastVelocity.magnitude) );
//                float radius = (0.5f*deltaPos) * Mathf.Tan(0.5f * velAngle);

//                if(radius < m_curvingSoundThreshold)
//                    result.Add(new AudioInfoPlaybackState(m_audioInfo_idle,m_curvingSoundCoefficient/radius,0.5f+0.5f*normalizedSpeed));
//                m_lastPos = position;
//                m_lastVelocity = speedVec;
//            }

//            return result;

//        }
             

//        public override void PlayEffect(InstanceID id, EffectInfo.SpawnArea area, Vector3 velocity, float acceleration, float magnitude, AudioManager.ListenerInfo listenerInfo, AudioGroup audioGroup)
//        {

//            Vector3 position = area.m_matrix.MultiplyPoint(this.m_position);

//            if (Vector3.SqrMagnitude(position - listenerInfo.m_position) > this.m_range * this.m_range)
//                return;

//            var audioInfos = getActiveAudioInfos(id, position, velocity, acceleration);

//            for (int i = 0; i < audioInfos.Count; ++i){
//                Randomizer randomizer = new Randomizer(id.RawData);
//                AudioInfo variation = audioInfos[i].info.GetVariation(ref randomizer);
//                audioGroup.AddPlayer(listenerInfo, (int)id.RawData, variation, position, velocity, this.m_range, audioInfos[i].volume, audioInfos[i].pitch);
//            }

//         }
         


//    }
         
         
         
       

//}

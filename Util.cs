using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.IO;
using ColossalFramework.PlatformServices;

namespace ExtraVehicleEffects
{
    public class Util
    {
        public static float SpeedKmHToInternal(float kmh)
        {
            return kmh * 0.16f;
        }

        public static float SpeedInternalToKmH(float intern)
        {
            return intern / 0.16f;
        }

        public static float SpeedKmHToEffect(float kmh)
        {
            return kmh * 0.6f;
        }

        public static AudioClip LoadAudioClipFromModDir(string filename)
        {
            Assembly asm = Assembly.GetAssembly(typeof(ExtraVehicleEffectsMod));
            var pluginInfo = ColossalFramework.Plugins.PluginManager.instance.FindPluginInfo(asm);

            Debug.Log(pluginInfo.modPath);

            try
            {
                string absUri = "file:///" + pluginInfo.modPath.Replace("\\", "/") + "/" + filename;
                WWW www = new WWW(absUri);
                return www.GetAudioClip(true, false);
            }
            catch(Exception e)
            {
                Debug.Log("Exception trying to load audio file '" + filename + "'!" + e.ToString());
                return null;
            }
        }

        public static EngineSoundEffect CopyEngineSoundEffect(EngineSoundEffect template, EngineSoundEffect target)
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

        public static Light CopyLight(Light template, Light target)
        {
            target.bounceIntensity = template.bounceIntensity;
            target.color = template.color;
            target.cookie = template.cookie;
            target.cookieSize = template.cookieSize;
            target.cullingMask = template.cullingMask;
            target.enabled = template.enabled;
            target.flare = template.flare;
            target.hideFlags = template.hideFlags;
            target.intensity = template.intensity;
            target.range = template.range;
            target.renderMode = template.renderMode;
            target.shadowBias = template.shadowBias;
            target.shadowNormalBias = template.shadowNormalBias;
            target.shadows = template.shadows;
            target.shadowStrength = template.shadowStrength;
            target.spotAngle = template.spotAngle;
            target.type = template.type;

            return target;
        }

        public static LightEffect CopyLightEffect(LightEffect template, LightEffect target)
        {
            target.m_alignment = template.m_alignment;
            target.m_batchedLight = template.m_batchedLight;
            target.m_blinkType = template.m_blinkType;
            target.m_fadeEndDistance = template.m_fadeEndDistance;
            target.m_fadeSpeed = template.m_fadeSpeed;
            target.m_fadeStartDistance = template.m_fadeStartDistance;
            target.m_offRange = template.m_offRange;
            target.m_position = template.m_position;
            target.m_positionIndex = template.m_positionIndex;
            target.m_rotationAxis = template.m_rotationAxis;
            target.m_rotationSpeed = template.m_rotationSpeed;
            target.m_spotLeaking = template.m_spotLeaking;
            target.m_variationColors = template.m_variationColors;
            return target;
        }

        public static bool hasSnowFall()
        {
            return GameObject.Find("WeatherManager") != null;
        }
        static private bool m_hasSnowfall = hasSnowFall();

        public static float getCurrentTemperature()
        {
            return m_hasSnowfall ? WeatherManager.instance.m_currentTemperature : 20.0f;
        }

        public static void setVehicleKinematics(string name, float acceleration, float brake, float leanMultiplier)
        {
            var info = PrefabCollection<VehicleInfo>.FindLoaded(name);
            info.m_acceleration = acceleration;
            info.m_braking = brake;
            info.m_leanMultiplier = leanMultiplier;
        }

        public static void setVehicleKinematicsForEffect(string effectName, float acceleration, float brake, float leanMultiplier, float speed = 0.0f)
        {
            var count = PrefabCollection<VehicleInfo>.LoadedCount();
            for (uint i = 0; i < count; ++i)
            {
                var vehicleInfo = PrefabCollection<VehicleInfo>.GetLoaded(i);

                if (vehicleInfo == null)
                    continue;

                //Debug.Log(vehicleInfo.name);

                if (Array.Exists(vehicleInfo.m_effects, (VehicleInfo.Effect effect) =>
                {                    
                    return effect.m_effect != null && effect.m_effect.name.Contains(effectName);
                }))
                {
                    Debug.Log("Setting kinematics for \"" + vehicleInfo.name + "\" and trailers...");  
                    vehicleInfo.m_acceleration = acceleration;
                    vehicleInfo.m_braking = brake;
                    vehicleInfo.m_leanMultiplier = leanMultiplier;
                    if (speed > 0.0f)
                        vehicleInfo.m_maxSpeed = speed;

                    //also set for any trailers 
                    //(actually only needed for the last one, but we just go for sure...)
                    var trailers = vehicleInfo.m_trailers;
                    if(trailers != null)
                        foreach (var trailer in trailers)
                        {
                            trailer.m_info.m_acceleration = acceleration;
                            trailer.m_info.m_braking = brake;
                            trailer.m_info.m_leanMultiplier = leanMultiplier;
                            if(speed > 0.0f)
                                trailer.m_info.m_maxSpeed = speed;
                        }

                }
            }
        }
       
        
        static public bool SubscribedToWorkshopItem(ulong id)
        {            
            foreach (ColossalFramework.Plugins.PluginManager.PluginInfo current in ColossalFramework.Plugins.PluginManager.instance.GetPluginsInfo())
                if (current.publishedFileID.AsUInt64 == id)
                    return true;
            return false;
        }


        static public bool SubscribedToVehicleEffects(out bool enabled)
        {
            const ulong VEHICLE_EFFECTS_ID = 780720853uL;
            const string VEHICLE_EFFECTS_LIB = "VehicleEffects[";
            foreach (ColossalFramework.Plugins.PluginManager.PluginInfo current in ColossalFramework.Plugins.PluginManager.instance.GetPluginsInfo())
                if (current.publishedFileID.AsUInt64 == VEHICLE_EFFECTS_ID || current.assembliesString.Contains(VEHICLE_EFFECTS_LIB))
                {
                    enabled = current.isEnabled;
                    return true;
                }
            enabled = false;
            return false;
        }

        static public bool SubscribedToImprovedPublicTransport(out bool enabled)
        {
            const ulong IPT_ID = 424106600uL;
            const string IPT_LIB = "ImprovedPublicTransport[";
            foreach (ColossalFramework.Plugins.PluginManager.PluginInfo current in ColossalFramework.Plugins.PluginManager.instance.GetPluginsInfo())
                if (current.publishedFileID.AsUInt64 == IPT_ID || current.assembliesString.Contains(IPT_LIB))
                {
                    enabled = current.isEnabled;
                    return true;
                }
            enabled = false;
            return false;
        }

        static public bool SubscribedToVehicleEffects()
        {
            bool enabled;
            return SubscribedToVehicleEffects(out enabled);
        }

        public static EffectInfo FindEffect(string effectName)
        {
            // Particle Effects aren't all added to EffectCollection, so search for GameObjects as well
            var effect = EffectCollection.FindEffect(effectName) ?? (GameObject.Find(effectName) != null ? GameObject.Find(effectName).GetComponent<EffectInfo>() : null);
            return effect;
        }

        public static List<EffectInfo> FindEffectsBySubStr(string subStr)
        {
            List<EffectInfo> result = new List<EffectInfo>();
            EffectInfo[] effects = GameObject.FindObjectsOfType<EffectInfo>() as EffectInfo[];
            foreach (var effect in effects)
                if (effect.name.Contains(subStr))
                    result.Add(effect);
            return result;
        }

        public static VehicleInfo FindVehicle(string prefabName, string packageName)
        {
            var prefab = PrefabCollection<VehicleInfo>.FindLoaded(prefabName) ??
                         PrefabCollection<VehicleInfo>.FindLoaded(prefabName + "_Data") ??
                         PrefabCollection<VehicleInfo>.FindLoaded(PathEscaper.Escape(prefabName) + "_Data") ??
                         PrefabCollection<VehicleInfo>.FindLoaded(packageName + "." + prefabName + "_Data") ??
                         PrefabCollection<VehicleInfo>.FindLoaded(packageName + "." + PathEscaper.Escape(prefabName) + "_Data");

            return prefab;
        }

        private static bool isLevelCrossing(ref NetNode nodeData)
        {
            return ((nodeData.m_flags & NetNode.Flags.LevelCrossing) != NetNode.Flags.None);
            //if((nodeData.m_flags & NetNode.Flags.TrafficLights) == NetNode.Flags.None)
            //    return false;

            //TODO: check for connect rail AND road

            //return true;
        }

        public static List<Vector3> GetLevelCrossingPositions()
        {
            List<Vector3> result = new List<Vector3>();
            int count = NetManager.instance.m_nodes.m_buffer.Length;
            for (int i = 0; i < count; ++i)
                if(isLevelCrossing(ref NetManager.instance.m_nodes.m_buffer[i]))
                    result.Add(NetManager.instance.m_nodes.m_buffer[i].m_position);
            Debug.Log("Found " + result.Count + " level crossings.");
            return result;
        }
        


    }
}

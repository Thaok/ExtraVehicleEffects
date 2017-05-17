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
    public class TrainRearLight
    {
        private const string effectName = "Train Rear Light";

        public static EffectInfo CreateEffectObject(Transform parent)
        {
            var templateEffect = Util.FindEffect("Train Light Right") as LightEffect;

            if (templateEffect != null)
            {
                GameObject obj = new GameObject(effectName);
                obj.transform.parent = parent;

                var rearLightEffect = obj.AddComponent<LightEffect>();
                Light light;
                Util.CopyLight(templateEffect.GetComponent<Light>(), (light = obj.AddComponent<Light>()) );
                Util.CopyLightEffect(templateEffect, rearLightEffect);

                //color
                light.color = new Color(1.0f, 0.0f, 0.0f);
                rearLightEffect.m_variationColors = new Color[] { new Color(1.0f,0.0f,0.0f) };

                //range/cone length
                light.range /= 3.0f;//10.0f;
                Debug.Log("light.range: " + light.range);
                light.type = LightType.Spot;
                rearLightEffect.m_offRange = new Vector2(400, 400);//daylight/"off" UVs
                rearLightEffect.m_rotationSpeed = 0;
                rearLightEffect.m_position = Vector3.zero;
                
                return rearLightEffect;
            }
            else
            {
                Debug.Log("Could not find template for " + effectName);
                return null;
            }
        }
    }
}

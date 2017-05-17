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
    public class TrainDayLight
    {
        private const string effectName = "Train Day Light";

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
                Color color = new Color(0.89f, 0.86f, 0.49f);
                light.color = color;
                rearLightEffect.m_variationColors = new Color[] { color };

                //range/cone length
                light.range /= 3.0f;//10.0f;
                light.type = LightType.Spot;
                rearLightEffect.m_offRange = new Vector2(rearLightEffect.m_offRange.y, rearLightEffect.m_offRange.x);
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

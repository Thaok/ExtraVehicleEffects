using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExtraVehicleEffects
{
    class EffectOptions
    {
        public string effectName;
        public string paramName;
        public List<EffectInfo> effects = new List<EffectInfo>();
        public float defaultValue;

        public SavedFloat savedValue;

        private UISlider slider;

        public delegate void EffectParamSelector(EffectInfo effect, float value);

        EffectParamSelector selector = null;
        
        public EffectOptions(string effectName, string paramName, float defaultValue, EffectParamSelector paramSelector)
        {
            this.effectName = effectName;
            this.paramName = paramName;
            this.defaultValue = defaultValue;
            this.selector = paramSelector;
            savedValue = new SavedFloat(effectName.Replace(" ", "") + paramName.Replace(" ", ""), "ExtraVehicleEffects", defaultValue, true);
        }

        public void Reset()
        {
            slider.value = defaultValue;
        }

        public void AddToMenu(UIHelperBase helper)
        {
            //string caption = effectName.Replace("contains:", "") + ": " + paramName;
            slider = helper.AddSlider(paramName, 0.0f, 1.5f * defaultValue, 0.05f, savedValue.value, OnSlideEvent) as UISlider;
            //if( caption.Length > 25 )
            //    helper.AddSpace(10);
        }

        public void Initialize()
        {
            if (effectName.Contains("contains:"))
            {
                if (effectName.Length > "contains:".Length)
                    effects = Util.FindEffectsBySubStr(effectName.Substring(9));
            }
            else
            {
                var effect = Util.FindEffect(effectName) as SoundEffect;
                if(effect != null)
                    effects.Add(effect);
            }
                       
            if (effects.Count == 0)
            {
                Debug.LogWarning("Could not find effect: " + effectName + " for effect options");
                return;
            }
            OnSlideEvent(savedValue.value);
        }

        private void OnSlideEvent(float value)
        {
            Debug.Log(value);
            savedValue.value = value;
            foreach(var effect in effects)
                if(effect != null)
                {
                    Debug.Log(effect.name + " has changed");
                    selector(effect, value );
                }
        }
    }
}

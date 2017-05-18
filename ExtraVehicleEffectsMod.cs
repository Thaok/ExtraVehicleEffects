using System;
using ICities;
using UnityEngine;
using ColossalFramework.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;
using ColossalFramework.Packaging;
using ColossalFramework.UI;
using System.Linq;
using ExtraVehicleEffects.Effects;
using ExtraVehicleEffects.GameExtensions;
using ColossalFramework;

namespace ExtraVehicleEffects
{
    public class ExtraVehicleEffectsMod : LoadingExtensionBase, IUserMod
    {
        private const string m_modName = "Extra Vehicle Effects";

        private GameObject gameObject;
        private bool isLoaded;

        static private ColossalFramework.SavedBool silenceStations;
        static private ColossalFramework.SavedBool disableGlobalReverb;
        static private ColossalFramework.SavedBool setOptimalVehicleKinematics;

        static private bool subscribedAndEnabledIPT;
        static public bool SubscribedToIPT{ get { return subscribedAndEnabledIPT; } }

        private List<EffectOptions> effectOptions;

        public ExtraVehicleEffectsMod()
        {
            GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile { fileName = "ExtraVehicleEffects" } });
            silenceStations = new ColossalFramework.SavedBool("silenceStations", "ExtraVehicleEffects", true);
            disableGlobalReverb = new ColossalFramework.SavedBool("disableGlobalReverb", "ExtraVehicleEffects", true);
            setOptimalVehicleKinematics = new ColossalFramework.SavedBool("setOptimalVehicleKinematics", "ExtraVehicleEffects", true);
        }

        public string Description
        {
            get
            {
                return "Extra effects to be used with Vehicle Effects mod.";
            }
        }

        public string Name
        {
            get
            {
                return m_modName;
            }
        }



        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("Generic options");

            var checkbox = group.AddCheckbox("Disable rail service sounds (randomly played at stations etc.). Requires map reload.", silenceStations.value, (bool value) =>
            {
                silenceStations.value = value;
            });

            group.AddCheckbox("Disable global reverb.", disableGlobalReverb.value, (bool value) =>
            {
                disableGlobalReverb.value = value;
                SetGlobalReverb(!disableGlobalReverb);
            });

            group.AddCheckbox("Set optimal accel./braking for vehicles using the effects. Requires game restart.", setOptimalVehicleKinematics.value, (bool value) =>
            {
                setOptimalVehicleKinematics.value = value;
            });

            group = helper.AddGroup("Effect Parameters");
           
            effectOptions = new List<EffectOptions>();

            effectOptions.Add(new EffectOptions("contains:Turbo Diesel Train Movement", "Turbo Volume", 0.9f, (EffectInfo effect, float value) =>
            {
                ((TurboDieselEngineSoundEffect)effect).m_turbo_volume_adjust = value;
            }));

            effectOptions.Add(new EffectOptions("Train Disc Brake", "Brake Volume", 1.0f, (EffectInfo effect, float value) =>
            {
                ((TrainBrakingSoundEffect)effect).m_volumeAdjust = value;
                TurboDieselEngineSoundEffect.m_brake_volumeAdjust = value;
            })); 

            effectOptions.Add(new EffectOptions("Train Close Doors", "Door Close Volume", 1.5f, (EffectInfo effect, float value) => { 
                ((SoundEffect)effect).m_audioInfo.m_volume = value; 
            }));

            effectOptions.Add(new EffectOptions("Train Whistle Compressed Air", "Air Whistle Volume", 0.7f, (EffectInfo effect, float value) =>
            {
                ((SoundEffect)effect).m_audioInfo.m_volume = value;
            }));

            effectOptions.Add(new EffectOptions("Sprague Bell", "Sprague Bell Volume", 1.0f, (EffectInfo effect, float value) =>
            {
                ((SoundEffect)effect).m_audioInfo.m_volume = value;
            }));


            //effectOptions.Add(new EffectOptions("Train Rear Light", "Cone Length", 6.66f, (EffectInfo effect, float value) =>
            //{
            //    ((LightEffect)effect).gameObject.GetComponent<Light>().range = value;
            //}));
           
            foreach (var option in effectOptions)
            {
                option.AddToMenu(group);
                //if(option.effectName.Length > 20)
                //    group.AddSpace(15);
            }

            group.AddSpace(20);

            group.AddButton("Reset to defaults", () =>
            {
                foreach (var option in effectOptions)
                {
                    option.Reset();
                }
            });
        }


        public override void OnCreated(ILoading loading)
        {
           //register delegates to Vehicle Effects mod
            if (Util.SubscribedToVehicleEffects())
            {
                Debug.Log("Registering delegates to Vehicle Effects Mod...");
                VehicleEffects.VehicleEffectsMod.eventVehicleUpdateStart += OnVehicleEffectsModUpdateStart;
                VehicleEffects.VehicleEffectsMod.eventVehicleUpdateFinished += OnVehicleEffectsUpdateFinished;
            }

            bool enabledIPT;
            subscribedAndEnabledIPT = Util.SubscribedToImprovedPublicTransport(out enabledIPT);
            subscribedAndEnabledIPT &= subscribedAndEnabledIPT;
        }

        public override void OnReleased()
        {
            if (Util.SubscribedToVehicleEffects())
            {
                Debug.Log("Unregistering delegates from Vehicle Effects Mod...");
                VehicleEffects.VehicleEffectsMod.eventVehicleUpdateStart -= OnVehicleEffectsModUpdateStart;
                VehicleEffects.VehicleEffectsMod.eventVehicleUpdateFinished -= OnVehicleEffectsUpdateFinished;
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if(mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
            {
                isLoaded = false;
                return;
            }

            // display warning if player is not subscribed to Vehicle Effects
            bool vehicleEffectsEnabled = false;
            if (!Util.SubscribedToVehicleEffects(out vehicleEffectsEnabled))
            {
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(
                    "Missing dependency",
                    Name + " requires the 'Vehicle Effects' mod to work properly. Please subscribe to the mod and restart the game!",
                    false);
                return;
            }

            // display warning if player did not enable Vehicle Effects
            if (!vehicleEffectsEnabled)
            {
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(
                   "Dependency disabled",
                   Name + " requires the 'Vehicle Effects' mod to work properly. Please enable the mod and restart the game!",
                   false);
                return;
            }


            if(silenceStations.value)
                SilenceStations();

            SetGlobalReverb(!disableGlobalReverb.value);
                                                    
            isLoaded = true;
        }

        public override void OnLevelUnloading()
        {
            if(!isLoaded)
            {
                return;
            }
        }

        public void OnVehicleEffectsModUpdateStart()
        {
            InitializeGameObjects();
        }

        public void OnVehicleEffectsUpdateFinished()
        {
            SetOptimalVehicleKinematics();
        }

        private void InitializeGameObjects()
        {
            Debug.Log(m_modName + " - Initializing Game Objects");
            if(gameObject == null)
            {
                Debug.Log(m_modName + " - Game Objects not created, creating new Game Objects");
                gameObject = new GameObject("Extra Vehicle Effects Collection");
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                CreateCustomEffects();
                foreach (var option in effectOptions)
                    option.Initialize();
            }
            Debug.Log(m_modName + " - Done initializing Game Objects");
        }

        private void CreateCustomEffects()
        {
            Debug.Log(m_modName + " - Creating effect objects");
                      
            //engine sounds
            TurboDieselTrainMovement.CreateEffectObject(gameObject.transform);
            TurboDieselIdling.CreateEffectObject(gameObject.transform);
            DieselIdling.CreateEffectObject(gameObject.transform);
            DirectCurrentTrainMovement.CreateEffectObject(gameObject.transform);
            SpragueTrainMovement.CreateEffectObject(gameObject.transform);
        
            //other movement and braking
            TrainBrakeDisc.CreateEffectObject(gameObject.transform);
            RollingTrainMovementNew.CreateEffectObject(gameObject.transform);
            TrainFlangeGrinding.CreateEffectObject(gameObject.transform);
            TrainFlangeSquealing.CreateEffectObject(gameObject.transform);
            RollingCargoTrainMovement.CreateEffectObject(gameObject.transform);
            TrainBrakeCargo.CreateEffectObject(gameObject.transform);
            
            //other sounds
            TrainWhistleCompressedAir.CreateEffectObject(gameObject.transform);
            SpragueBell.CreateEffectObject(gameObject.transform);
            TrainCloseDoors.CreateEffectObject(gameObject.transform);
            TrainCompressor.CreateEffectObject(gameObject.transform);  

            //light effects
            TrainRearLight.CreateEffectObject(gameObject.transform);
            TrainDayLight.CreateEffectObject(gameObject.transform);

            //experimental
            TrainReverser.CreateEffectObject(gameObject.transform);
                      
            Debug.Log(m_modName + " - Done creating effect objects");
        }

        private void SilenceStations()
        {
            AudioInfo silence = new AudioInfo();
            silence.m_clip = AudioClip.Create("silence",10,1,10,false);

            BuildingManager.instance.m_properties.m_subServiceSounds[(int)ItemClass.SubService.PublicTransportTrain] = silence;

            /*
            BuildingInfo[] buildingInfos = Resources.FindObjectsOfTypeAll(typeof(BuildingInfo)) as BuildingInfo[];
            for (int i = 0; i < buildingInfos.Length; ++i)
                if (buildingInfos[i] != null && buildingInfos[i].m_class != null && buildingInfos[i].m_class.m_subService == ItemClass.SubService.PublicTransportTrain)
                {
                    //replace rail sound effect
                    buildingInfos[i].m_customLoopSound = silence;
                }

            var trainServiceBuildings = BuildingManager.instance.GetServiceBuildings(ItemClass.Service.PublicTransport);
            for (int i = 0; i < trainServiceBuildings.Length; ++i)
            {
                BuildingManager.instance.m_buildings.m_buffer[trainServiceBuildings[i]].
            }
            */
        }

        private void SetGlobalReverb(bool value)
        {
            var reverbZone = GameObject.Find("Reverb Zone");
            if(reverbZone == null)
                return;
            var audioReverbZone = reverbZone.GetComponent<AudioReverbZone>();
            if (audioReverbZone == null)
                return;
            Debug.Log("Disabling reverb...");
            //audioReverbZone.reverbPreset = AudioReverbPreset.Off;
            audioReverbZone.enabled = value;
        }

        private void SetOptimalVehicleKinematics()
        {
            if (!setOptimalVehicleKinematics.value)
                return;
            DirectCurrentTrainMovement.SetOptimalVehicleKinematics();
            TurboDieselTrainMovement.SetOptimalVehicleKinematics();
            SpragueTrainMovement.SetOptimalVehicleKinematics();                      
        }

        static public bool UseOptimalVehicleKinematics()
        {
            return setOptimalVehicleKinematics.value;
        }


        

        
       
    }
}

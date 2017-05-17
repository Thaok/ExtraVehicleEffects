using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;
using ColossalFramework.Math;

namespace ExtraVehicleEffects.GameExtensions
{

    public class TurnTrainEffect : SoundEffect
    {

        public static TurnTrainEffect CopyEngineSoundEffect(EngineSoundEffect template, TurnTrainEffect target)
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


        Dictionary<InstanceID, bool> reversedStates = new Dictionary<InstanceID,bool>();
        Dictionary<InstanceID, float> reversalTimes = new Dictionary<InstanceID, float>();

        //public virtual void RenderEffect(InstanceID id, EffectInfo.SpawnArea area, Vector3 velocity, float acceleration, float magnitude, float timeOffset, float timeDelta, RenderManager.CameraInfo cameraInfo){
        //}


        //unsafe private static Vehicle* getVehicleById(InstanceID id)
        //{
        //    return &VehicleManager.instance.m_vehicles.m_buffer[(int)id.Vehicle];
        //}

        //unsafe private static Vehicle* getVehicleById(ushort id)
        //{
        //    return &VehicleManager.instance.m_vehicles.m_buffer[(int)id];
        //}

        public static void swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        private static bool vehicleFlagSet(uint id, Vehicle.Flags flag)
        {
            return (VehicleManager.instance.m_vehicles.m_buffer[id].m_flags | flag) != 0;
        }

        private static void moveVehicle(uint id, Vector3 delta)
        {
            Vehicle.Frame frame = VehicleManager.instance.m_vehicles.m_buffer[id].GetLastFrameData();
            frame.m_position += delta;
            VehicleManager.instance.m_vehicles.m_buffer[id].SetFrameData(VehicleManager.instance.m_vehicles.m_buffer[id].m_lastFrame, frame);
            //for (uint i = 0; i < 4; ++i)
            //{
            //    Vehicle.Frame frame = VehicleManager.instance.m_vehicles.m_buffer[id].GetFrameData(i);
            //    frame.m_position += delta;
            //    VehicleManager.instance.m_vehicles.m_buffer[id].SetFrameData(i, frame);
            //}
            //update vehicle grid
            ushort leadingVehicleId = VehicleManager.instance.m_vehicles.m_buffer[id].m_leadingVehicle;
            VehicleManager.instance.m_vehicles.m_buffer[id].Info.m_vehicleAI.SimulationStep((ushort)id, ref VehicleManager.instance.m_vehicles.m_buffer[id], leadingVehicleId, ref VehicleManager.instance.m_vehicles.m_buffer[(uint)leadingVehicleId], 0);
        }

        private static void invertVehicle(uint id)
        {
            for (uint i = 0; i < 4; ++i)
            {
                Vehicle.Frame frame = VehicleManager.instance.m_vehicles.m_buffer[id].GetFrameData(i);
                frame.m_rotation *= Quaternion.AngleAxis(180.0f, Vector3.up);
                VehicleManager.instance.m_vehicles.m_buffer[id].SetFrameData(i, frame);
            }
        }

        private static Vector3 getVehicleDirection(uint id)
        {
            return (VehicleManager.instance.m_vehicles.m_buffer[id].GetLastFrameData().m_rotation * Vector3.forward).normalized;
        }

        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            Debug.Log(field);
            return field.GetValue(instance);
        }

        internal static void SetInstanceField(Type type, object instance, string fieldName, object value)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            Debug.Log(field);
            field.SetValue(instance,value);
        }

        private static void addMissingTrailerPrefabToIPT(VehicleInfo trailerInfo){
            if (trailerInfo.m_class.name.Contains("Cargo"))
                return;

            ImprovedPublicTransport.PrefabData prefabData = Array.Find<ImprovedPublicTransport.PrefabData>(ImprovedPublicTransport.VehiclePrefabs.instance.GetPrefabs(ItemClass.SubService.PublicTransportTrain), (ImprovedPublicTransport.PrefabData item) => item.PrefabDataIndex == trailerInfo.m_prefabDataIndex);
            if (prefabData == null)
            {
                //ImprovedPublicTransport.VehiclePrefabs.instance._trainPrefabData.Add(new PrefabData(prefab));
                ImprovedPublicTransport.PrefabData[] _trainPrefabData = GetInstanceField(typeof(ImprovedPublicTransport.VehiclePrefabs), ImprovedPublicTransport.VehiclePrefabs.instance, "_trainPrefabData") as ImprovedPublicTransport.PrefabData[];
                Debug.Log("_trainPrefabData: " + _trainPrefabData);
                ImprovedPublicTransport.PrefabData[] newData = new ImprovedPublicTransport.PrefabData[_trainPrefabData.Length + 1];
                Array.Copy(_trainPrefabData, newData, _trainPrefabData.Length);
                newData[newData.Length - 1] = new ImprovedPublicTransport.PrefabData(trailerInfo);
                //_trainPrefabData = newData;
                SetInstanceField(typeof(ImprovedPublicTransport.VehiclePrefabs), ImprovedPublicTransport.VehiclePrefabs.instance, "_trainPrefabData", newData);
            }
        }

        private static void nullNextGridVehicle(uint firstVehicleId)
        {
            var current = firstVehicleId;
            while (current != 0)
            {
                VehicleManager.instance.m_vehicles.m_buffer[current].m_nextGridVehicle = 0;
                current = VehicleManager.instance.m_vehicles.m_buffer[current].m_trailingVehicle;
            }
        }

        private static bool isCargoTrain(VehicleInfo info)
        {
            return info.m_vehicleAI != null && (info.m_vehicleAI as CargoTrainAI) != null;
        }

        private static void swapVehicleInfos(uint firstVehicleId, uint secondVehicleId)
        {
            if (firstVehicleId == secondVehicleId)
                return;
            
            //swap the infos
            VehicleInfo firstInfo = VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Info;
            VehicleInfo secondInfo = VehicleManager.instance.m_vehicles.m_buffer[secondVehicleId].Info;
            VehicleManager.instance.m_vehicles.m_buffer[secondVehicleId].Info = VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Info;
            VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Info = secondInfo;

            //ushort firstTrailerId = VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_trailingVehicle;
            //ushort secondLastTrailerID = VehicleManager.instance.m_vehicles.m_buffer[secondVehicleId].m_leadingVehicle;
            //if (firstTrailerId != 0 && secondLastTrailerID != 0)
            //    swap(ref VehicleManager.instance.m_vehicles.m_buffer[firstTrailerId].m_nextGridVehicle, ref VehicleManager.instance.m_vehicles.m_buffer[secondLastTrailerID].m_nextGridVehicle);                  

            //nullNextGridVehicle(firstVehicleId);

            if (ExtraVehicleEffectsMod.SubscribedToIPT)
                addMissingTrailerPrefabToIPT(secondInfo);
             
        }

       

        private static void swapVehicleIds(uint firstVehicleId, uint secondVehicleId)
        {
            //prepare swap
            swapTransportData(firstVehicleId, secondVehicleId);

            uint trailingVehicle = VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_trailingVehicle;
            if (trailingVehicle != 0)
                VehicleManager.instance.m_vehicles.m_buffer[trailingVehicle].m_leadingVehicle = (ushort)secondVehicleId;

            uint leadingVehicle = VehicleManager.instance.m_vehicles.m_buffer[secondVehicleId].m_leadingVehicle;
            if (leadingVehicle != 0)
                VehicleManager.instance.m_vehicles.m_buffer[leadingVehicle].m_trailingVehicle = (ushort)firstVehicleId;

            VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_leadingVehicle = VehicleManager.instance.m_vehicles.m_buffer[secondVehicleId].m_leadingVehicle;
            VehicleManager.instance.m_vehicles.m_buffer[secondVehicleId].m_trailingVehicle = VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_trailingVehicle;
            VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_trailingVehicle = 0;
            VehicleManager.instance.m_vehicles.m_buffer[secondVehicleId].m_leadingVehicle = 0;
                        
            //swap the ids
            swap(ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId], ref VehicleManager.instance.m_vehicles.m_buffer[secondVehicleId]);
        }


        private static float getVehicleOffset(uint firstVehicleId, uint secondVehicleId)
        {
            //Debug.Log("firstVehicleId.size.z: " + VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Info.m_generatedInfo.m_size.z);
            //Debug.Log("secondVehicleId.size.z: " + VehicleManager.instance.m_vehicles.m_buffer[secondVehicleId].Info.m_generatedInfo.m_size.z);
            float halfLengthDiff = 0.5f * (VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Info.m_generatedInfo.m_size.z - VehicleManager.instance.m_vehicles.m_buffer[secondVehicleId].Info.m_generatedInfo.m_size.z);
            //Debug.Log("halfLengthDiff: " + halfLengthDiff);                      
            return 0.25f * halfLengthDiff;//some engine-related magic number...
        }


        //adjust first and last vehicle positions, s.t. no overlap occurs
        private static void adjustFirstAndLastVehiclePosition(uint firstVehicleId, uint secondVehicleId)
        {
            float halfLengthDiff = getVehicleOffset(firstVehicleId, secondVehicleId);
            if (Mathf.Abs(halfLengthDiff) < 0.1)
                return;

            Debug.Log("getVehicleDirection(firstVehicleId):" + getVehicleDirection(firstVehicleId));
            Debug.Log("getVehicleDirection(firstVehicleId).magnitude:" + getVehicleDirection(firstVehicleId).magnitude);

            moveVehicle(firstVehicleId, halfLengthDiff * getVehicleDirection(firstVehicleId));
            moveVehicle(secondVehicleId, halfLengthDiff * getVehicleDirection(secondVehicleId));
        }


        //adjust vehicle positions of the trailers between first and last vehicle, s.t. no overlap occurs
        private static void adjustIntermediateVehiclePositions(uint firstVehicleId, uint secondVehicleId)
        {
            float halfLengthDiff = -getVehicleOffset(firstVehicleId, secondVehicleId);
            if (Mathf.Abs(halfLengthDiff) < 0.1)
                return;

            uint currentID = VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_trailingVehicle;
            while (currentID != 0 && currentID != secondVehicleId)
            {
                moveVehicle(currentID, halfLengthDiff * getVehicleDirection(currentID));
                currentID = VehicleManager.instance.m_vehicles.m_buffer[currentID].m_trailingVehicle;
            }
            
        }

        private static void swapTransportData(uint firstVehicleId, uint lastVehicleId){
            //swap line/transport related data
            swap(ref VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_waitCounter, ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_waitCounter);

            swap(ref VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_transportLine, ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_transportLine);
            swap(ref VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_transferSize, ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_transferSize);
            swap(ref VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_transferType, ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_transferType);

            swap(ref VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_citizenUnits, ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_citizenUnits);
            swap(ref VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_nextCargo, ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_nextCargo);
            swap(ref VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_firstCargo, ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_firstCargo);
            swap(ref VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_cargoParent, ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_cargoParent);
        }

        private static void reverseTrailerLinkedList(uint firstVehicleId, uint lastVehicleId)
        {
            List<uint> train = new List<uint>();
            uint currentID = firstVehicleId;
            train.Add(currentID);
            while (currentID != 0)
            {
                train.Add(VehicleManager.instance.m_vehicles.m_buffer[currentID].m_trailingVehicle);
                currentID = train.Last();
            }
            foreach (var trailer in train)
                swap(ref VehicleManager.instance.m_vehicles.m_buffer[trailer].m_trailingVehicle,ref  VehicleManager.instance.m_vehicles.m_buffer[trailer].m_leadingVehicle);
            
            swapTransportData(lastVehicleId, firstVehicleId);
        }

        private struct PosData
        {
            public Vehicle.Frame m_frame0;
            public Vehicle.Frame m_frame1;
            public Vehicle.Frame m_frame2;
            public Vehicle.Frame m_frame3;
            public Vector4 m_targetPos0;
            public Vector4 m_targetPos1;
            public Vector4 m_targetPos2;
            public Vector4 m_targetPos3;
            public uint m_path;
            public byte m_pathPositionIndex;
            public Segment3 m_segment;
            public PosData(ref Vehicle v)
            {
                m_frame0 = v.m_frame0;
                m_frame1 = v.m_frame1;
                m_frame2 = v.m_frame2;
                m_frame3 = v.m_frame3;
                m_targetPos0 = v.m_targetPos0;
                m_targetPos1 = v.m_targetPos1;
                m_targetPos2 = v.m_targetPos2;
                m_targetPos3 = v.m_targetPos3;
                m_path = v.m_path;
                m_pathPositionIndex = v.m_pathPositionIndex;
                m_segment = v.m_segment;
            }
            public void assignTo(ref Vehicle v)
            {
                v.m_frame0 = m_frame0;
                v.m_frame1 = m_frame1;
                v.m_frame2 = m_frame2;
                v.m_frame3 = m_frame3;
                v.m_targetPos0 = m_targetPos0;
                v.m_targetPos1 = m_targetPos1;
                v.m_targetPos2 = m_targetPos2;
                v.m_targetPos3 = m_targetPos3;
                v.m_path = m_path;
                v.m_pathPositionIndex = m_pathPositionIndex;
                v.m_segment = m_segment;
            }
        }

        private static void reverseTrailerPositions(uint firstVehicleId, uint lastVehicleId)
        {
            List<uint> train = new List<uint>();
            List<PosData> trainPoses = new List<PosData>();
            uint currentID = firstVehicleId;
            //gather poses and vehicle IDs
            while (currentID != 0)
            {
                train.Add(currentID);
                trainPoses.Add(new PosData(ref VehicleManager.instance.m_vehicles.m_buffer[currentID]));
                currentID = VehicleManager.instance.m_vehicles.m_buffer[currentID].m_trailingVehicle;
            }
            
            if(train.Count != trainPoses.Count)
                return;

            //reverse the positions
            for (int i = 0; i < train.Count; ++i)
            {
                trainPoses[i].assignTo(ref VehicleManager.instance.m_vehicles.m_buffer[train[train.Count - 1 - i]]);
                invertVehicle(train[i]);
            }

        }

        public void RenderEffectOff(InstanceID id, EffectInfo.SpawnArea area, Vector3 velocity, float acceleration, float magnitude, float timeOffset, float timeDelta, RenderManager.CameraInfo cameraInfo)
        {
            bool currentReversedState = (VehicleManager.instance.m_vehicles.m_buffer[(int)id.Vehicle].m_flags & Vehicle.Flags.Reversed) != 0;

            if (!currentReversedState)
                return;

            float currentTime = Time.time;
            float lastReverseTime;
            if (!reversalTimes.TryGetValue(id, out lastReverseTime))
            {
                lastReverseTime = currentTime;
                reversalTimes[id] = lastReverseTime;                
            }

            if ( (currentTime - lastReverseTime) < 5.0f )
                return;

            ushort lastVehicleId = VehicleManager.instance.m_vehicles.m_buffer[(int)id.Vehicle].GetLastVehicle(id.Vehicle);
            ushort firstVehicleId = VehicleManager.instance.m_vehicles.m_buffer[(int)id.Vehicle].GetFirstVehicle(id.Vehicle); 
            
            reverseTrailerPositions(firstVehicleId, lastVehicleId);

            VehicleManager.instance.m_vehicles.m_buffer[(int)id.Vehicle].m_flags &= ~Vehicle.Flags.Reversed;            
        }
        

        //public override void PlayEffect(InstanceID id, EffectInfo.SpawnArea area, Vector3 velocity, float acceleration, float magnitude, AudioManager.ListenerInfo listenerInfo, AudioGroup audioGroup)
        public override void RenderEffect(InstanceID id, EffectInfo.SpawnArea area, Vector3 velocity, float acceleration, float magnitude, float timeOffset, float timeDelta, RenderManager.CameraInfo cameraInfo)
        {
           
            Vector3 position = area.m_matrix.MultiplyPoint(this.m_position);
            float speed = velocity.magnitude;
            //float range = Mathf.Min(this.m_minRange + speed * this.m_rangeSpeedMultiplier + acceleration + acceleration * this.m_rangeAccelerationMultiplier, this.m_range);

            bool currentReversedState = (VehicleManager.instance.m_vehicles.m_buffer[(int)id.Vehicle].m_flags & Vehicle.Flags.Reversed) != 0;
                

            //check or initilize reversed state
            bool lastReversedState;
            if (!reversedStates.TryGetValue(id, out lastReversedState))
            {
                lastReversedState = currentReversedState;
                reversedStates[id] = currentReversedState;
            }

            if (lastReversedState == currentReversedState)
                return;

            Debug.Log("Detected reversal: Turning train...");

            //swap last trailer and engine
            //////////////////////////////

            //parse the current train
            ushort lastVehicleId = VehicleManager.instance.m_vehicles.m_buffer[(int)id.Vehicle].GetLastVehicle(id.Vehicle);
            ushort firstVehicleId = VehicleManager.instance.m_vehicles.m_buffer[(int)id.Vehicle].GetFirstVehicle(id.Vehicle); 
            
            //  VehicleManager.instance.m_vehicles.m_buffer[]

            //Debug.Log("first vehicle info name: " + VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].Info.name);
            //Debug.Log("last vehicle info name: " + VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Info.name);



            //if(speed > 0.1f)//perform swap on departure
            if ((currentReversedState && VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Info.m_trailers != null && VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Info.m_trailers.Length > 0)
                || (!currentReversedState && VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].Info.m_trailers != null && VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].Info.m_trailers.Length > 0))
            {


                Debug.Log("swapping infos...");
                swapVehicleInfos(firstVehicleId, lastVehicleId);
                adjustIntermediateVehiclePositions(firstVehicleId, lastVehicleId);

                //adjustFirstAndLastVehiclePosition(firstVehicleId, lastVehicleId);
                //reverseTrailerLinkedList(firstVehicleId, lastVehicleId);
                //swapVehicleIds(firstVehicleId, lastVehicleId);

                //invert the swapped vehicles
                if (currentReversedState)
                {
                    VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_flags |= Vehicle.Flags.Inverted;
                    VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_flags |= Vehicle.Flags.Inverted;
                }
                else
                {
                    VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_flags &= ~Vehicle.Flags.Inverted;
                    VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_flags &= ~Vehicle.Flags.Inverted;
                }

            }

            //Debug.Log("first vehicle info name: " + VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].Info.name);
            //Debug.Log("last vehicle info name: " + VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Info.name);

            reversedStates[id] = currentReversedState;
        }

        private static void unspawnTurnAndSpawnVehicle()
        {
            //Vehicle firstVehicle = VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId];
            //Vehicle lastVehicle = VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId];

            //if (currentReversedState)
            //{
            //    VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Unspawn(firstVehicleId);
            //    //for (int i = 0; i < 4; ++i)
            //    //{
            //    //    firstVehicle.SetFrameData((uint)i, lastVehicle.GetFrameData((uint)i));
            //    //    firstVehicle.SetTargetPos(i, lastVehicle.GetTargetPos(i));                   
            //    //}
            //    firstVehicle.m_flags |= Vehicle.Flags.Inverted;
            //    VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId] = firstVehicle;
            //    VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Spawn(firstVehicleId);                
            //}
        }


        private static void adjustVehicleOffsets()
        {
            //VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_lastPathOffset

            //PathManager.instance.m_pathUnits.m_buffer[(int)((UIntPtr)VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_path)].MoveLastPosition(VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_path, lengthDiff);


            //PathManager.instance.m_pathUnits.m_buffer[(int)((UIntPtr)VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_path)].SetPosition
            //PathManager.instance.m_pathUnits.m_buffer[(int)((UIntPtr)VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_path)].MoveLastPosition(VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_path,1.0f);
            //PathManager.instance.m_pathUnits.m_buffer[(int)((UIntPtr)VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_path)].m_
            //PathManager.instance.m_pathUnits.m_buffer[(int)((UIntPtr)VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_path)].


            //VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Info.m_vehicleAI.TrySpawn(firstVehicleId, ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId]);
            //VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].Info.m_vehicleAI.TrySpawn(lastVehicleId, ref VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId]);

            /*
            if (currentReversedState)          
            {
                //VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].
                VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].Unspawn(lastVehicleId);
                VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].Spawn(lastVehicleId);
            }
            else
            {
                VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Unspawn(firstVehicleId);
                VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].Spawn(firstVehicleId);
            }
            /**/



            /*
            if (currentReversedState)
            {
                //(float)offset * 0.003921569f
                VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_pathPositionIndex += (byte)(lengthDiff / 0.003921569f);
                VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_pathPositionIndex  += (byte)(lengthDiff / 0.003921569f);
            }
            else
            {
                VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].m_pathPositionIndex -= (byte)(lengthDiff / 0.003921569f);
                VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_pathPositionIndex  -= (byte)(lengthDiff / 0.003921569f);
            }
            /**/

            /*
            PathUnit.Position firstVehiclePos, lastVehiclePos;
            PathManager.instance.m_pathUnits.m_buffer[(int)((UIntPtr)VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_path)].GetPosition(0, out firstVehiclePos);
            PathManager.instance.m_pathUnits.m_buffer[(int)((UIntPtr)VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_path)].GetPosition(0, out lastVehiclePos);

            if (currentReversedState)
            {
                //(float)offset * 0.003921569f
                firstVehiclePos.m_offset += (byte)(lengthDiff / 0.003921569f);
                lastVehiclePos.m_offset += (byte)(lengthDiff / 0.003921569f);
            }
            else
            {
                firstVehiclePos.m_offset -= (byte)(lengthDiff / 0.003921569f);
                lastVehiclePos.m_offset -= (byte)(lengthDiff / 0.003921569f);
            }

            PathManager.instance.m_pathUnits.m_buffer[(int)((UIntPtr)VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_path)].SetPosition(0, firstVehiclePos);
            PathManager.instance.m_pathUnits.m_buffer[(int)((UIntPtr)VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_path)].SetPosition(0, lastVehiclePos);
            /**/


            //Vehicle.Frame lastFrameDataFirstVehicle = VehicleManager.instance.m_vehicles.m_buffer[firstVehicleId].GetLastFrameData();
            //Vehicle.Frame lastFrameDataLastVehicle = VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].GetLastFrameData();
            //Vector3 dir = (lastFrameDataFirstVehicle.m_rotation * Vector3.forward).normalized;
            //Vector4 offset4 = new Vector4(lengthDiff * dir.x, lengthDiff * dir.y, lengthDiff * dir.z, 0.0f);
            //Vector4 offset4 = new Vector4(0f, 0f, 0f, 0.5f);
            /*
            if (currentReversedState)
            {                   
                VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_targetPos0 += offset4;
                VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId].m_targetPos0 += offset4;
                    
                VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_targetPos1 += offset4;
                VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId].m_targetPos1 += offset4;
                VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_targetPos2 += offset4;
                VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId].m_targetPos2 += offset4;
                VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_targetPos3 += offset4;
                VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId].m_targetPos3 += offset4;
            }
            else 
            {
                VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_targetPos0 -= offset4;
                VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId].m_targetPos0 -= offset4;

                VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_targetPos1 -= offset4;
                VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId].m_targetPos1 -= offset4;
                VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_targetPos2 -= offset4;
                VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId].m_targetPos2 -= offset4;
                VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_targetPos3 -= offset4;
                VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId].m_targetPos3 -= offset4;
            }
             /* */





            //DEBUG
            //VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_segment.a += lengthDiff * dir;
            //VehicleManager.instance.m_vehicles.m_buffer[lastVehicleId].m_segment.b += lengthDiff * dir;

            //VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId].m_segment.a += lengthDiff * dir;
            //VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId].m_segment.b += lengthDiff * dir;


            //SimulationManager instance = ColossalFramework.Singleton<SimulationManager>.instance;
            //Vector3 physicsLodRefPos = instance.m_simulationView.m_position + instance.m_simulationView.m_direction * 1000f;		
            //for (uint i = 0; i < 30; ++i)
            //    VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId].Info.m_vehicleAI.SimulationStep(fistVehicleId, ref VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId], VehicleManager.instance.m_vehicles.m_buffer[fistVehicleId].m_frame0.m_position);

            //if (vehicleData.m_path != 0u && (leaderData.m_flags & Vehicle.Flags.WaitingPath) == (Vehicle.Flags)0)
            //{
            //    trainAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, vector2, vector, 0, ref leaderData, ref i, 1, 2, num9, minSqrDistanceB);
            //}

            //while (i < 4)
            //{
            //    vehicleData.SetTargetPos(i, vehicleData.GetTargetPos(i - 1));
            //    i++;
            //}
        }


    }
    
}





//TrainAI.CalculateTargetSpeed

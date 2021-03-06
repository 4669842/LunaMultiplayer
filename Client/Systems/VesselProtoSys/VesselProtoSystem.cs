﻿using LunaClient.Base;
using LunaClient.Events;
using LunaClient.Systems.Mod;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using LunaClient.Windows.BannedParts;
using LunaCommon.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LunaClient.Systems.VesselProtoSys
{
    /// <summary>
    /// This system handles the vessel loading into the game and sending our vessel structure to other players.
    /// We only load vesels that are in our subspace
    /// This system also handles changes that a vessel could have. Antenas that extend, shields that open, etc
    /// </summary>
    public partial class VesselProtoSystem : MessageSystem<VesselProtoSystem, VesselProtoMessageSender, VesselProtoMessageHandler>
    {
        #region Fields & properties

        private static DateTime _lastReloadCheck = LunaComputerTime.UtcNow;

        public static Guid CurrentlyUpdatingVesselId { get; set; } = Guid.Empty;

        public ScreenMessage BannedPartsMessage { get; set; }
        
        public VesselLoader VesselLoader { get; } = new VesselLoader();

        public bool ProtoSystemReady => ProtoSystemBasicReady && FlightGlobals.ready &&
            HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating;

        public bool ProtoSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 1f &&
            HighLogic.LoadedScene >= GameScenes.SPACECENTER;

        public VesselProtoEvents VesselProtoEvents { get; } = new VesselProtoEvents();

        public VesselRemoveSystem VesselRemoveSystem => VesselRemoveSystem.Singleton;

        public Queue<Vessel> FlagsToSend = new Queue<Vessel>();

        private List<Guid> VesselsToRefresh { get; } = new List<Guid>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselProtoSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            TimingManager.FixedUpdateAdd(TimingManager.TimingStage.BetterLateThanNever, CheckVesselsToRefresh);

            GameEvents.onVesselCreate.Add(VesselProtoEvents.VesselCreate);
            GameEvents.onFlightReady.Add(VesselProtoEvents.FlightReady);
            GameEvents.onPartDie.Add(VesselProtoEvents.OnPartDie);
            GameEvents.onGameSceneLoadRequested.Add(VesselProtoEvents.OnSceneRequested);
            GameEvents.onVesselPartCountChanged.Add(VesselProtoEvents.VesselPartCountChangedinSpectatingVessel);
            GameEvents.onVesselPartCountChanged.Add(VesselProtoEvents.VesselPartCountChanged);

            GameEvents.OnTriggeredDataTransmission.Add(VesselProtoEvents.TriggeredDataTransmission);
            GameEvents.OnExperimentStored.Add(VesselProtoEvents.ExperimentStored);
            ExperimentEvent.onExperimentReset.Add(VesselProtoEvents.ExperimentReset);

            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, RemoveBadDebrisWhileSpectating));
            SetupRoutine(new RoutineDefinition(2000, RoutineExecution.Update, CheckVesselsToLoad));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, CheckRefreshOwnVesselWhileSpectating));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, SendVesselDefinition));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            TimingManager.FixedUpdateRemove(TimingManager.TimingStage.BetterLateThanNever, CheckVesselsToRefresh);

            GameEvents.onVesselCreate.Remove(VesselProtoEvents.VesselCreate);
            GameEvents.onFlightReady.Remove(VesselProtoEvents.FlightReady);
            GameEvents.onPartDie.Remove(VesselProtoEvents.OnPartDie);
            GameEvents.onGameSceneLoadRequested.Remove(VesselProtoEvents.OnSceneRequested);
            GameEvents.onVesselPartCountChanged.Remove(VesselProtoEvents.VesselPartCountChangedinSpectatingVessel);
            GameEvents.onVesselPartCountChanged.Remove(VesselProtoEvents.VesselPartCountChanged);

            GameEvents.OnTriggeredDataTransmission.Remove(VesselProtoEvents.TriggeredDataTransmission);
            GameEvents.OnExperimentStored.Remove(VesselProtoEvents.ExperimentStored);
            ExperimentEvent.onExperimentReset.Remove(VesselProtoEvents.ExperimentReset);

            //This is the main system that handles the vesselstore so if it's disabled clear the store aswell
            VesselsProtoStore.ClearSystem();
        }

        #endregion

        #region Public

        /// <summary>
        /// Checks the vessel for invalid parts
        /// </summary>
        public bool CheckVessel(Vessel vessel)
        {
            if (vessel == null || vessel.isEVA || vessel.vesselType == VesselType.Flag) return true;

            if (ModSystem.Singleton.ModControl)
            {
                var bannedParts = ModSystem.Singleton.GetBannedPartsFromVessel(vessel.protoVessel).ToArray();
                if (bannedParts.Any())
                {
                    LunaLog.LogError($"Vessel {vessel.id}-{vessel.vesselName} Contains the following banned parts: {string.Join(", ", bannedParts)}");
                    BannedPartsWindow.Singleton.DisplayBannedPartsDialog(vessel, bannedParts);
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Update routines

        /// <summary>
        /// Check if we have a different part count in the vessel than in the store while spectating and if that's the case we trigger a reload
        /// </summary>
        private void CheckRefreshOwnVesselWhileSpectating()
        {
            if (!VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null || !ProtoSystemReady) return;

            if (VesselsProtoStore.AllPlayerVessels.TryGetValue(FlightGlobals.ActiveVessel.id, out var vesselProtoUpdate))
            {
                if (vesselProtoUpdate.ProtoVessel.protoPartSnapshots.Count != FlightGlobals.ActiveVessel.Parts.Count)
                    vesselProtoUpdate.VesselHasUpdate = true;
            }
        }

        /// <summary>
        /// While spectating, the vessel you are watching can crash or something and create debris that shouldn't exist.
        /// Here we run trough all the current vessels that are in your game and remove the ones that are not in the proto store
        /// </summary>
        private static void RemoveBadDebrisWhileSpectating()
        {
            if (VesselCommon.IsSpectating)
            {
                foreach (var vessel in FlightGlobals.Vessels.Where(v => !VesselsProtoStore.AllPlayerVessels.ContainsKey(v.id)))
                {
                    VesselRemoveSystem.Singleton.AddToKillList(vessel.id, "Bad debris created while spectating");
                }
            }
        }


        /// <summary>
        /// Send the definition of our own vessel and the secondary vessels.
        /// </summary>
        private void SendVesselDefinition()
        {
            try
            {
                if (ProtoSystemReady)
                {
                    MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, false, false);
                    MessageSender.SendVesselMessage(VesselCommon.GetSecondaryVessels());
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in SendVesselDefinition {e}");
            }

        }

        /// <summary>
        /// Check vessels that must be loaded
        /// </summary>
        private void CheckVesselsToLoad()
        {
            try
            {
                if (ProtoSystemBasicReady)
                {
                    //Load vessels that don't exist, are in our subspace and out of safety bubble
                    var vesselsToLoad = VesselsProtoStore.AllPlayerVessels.Where(v => !v.Value.VesselExist && v.Value.ShouldBeLoaded);

                    foreach (var vesselProto in vesselsToLoad)
                    {
                        if (VesselRemoveSystem.VesselWillBeKilled(vesselProto.Key))
                            continue;

                        //If the vessel spawned later than the current time avoid loading it
                        if (VesselsProtoStore.VesselsSpawnTime.TryGetValue(vesselProto.Key, out var spawnTime) && TimeSyncerSystem.UniversalTime < spawnTime)
                            continue;

                        //Only load vessels that are in safety bubble when not in flight
                        if (vesselProto.Value.IsInSafetyBubble && HighLogic.LoadedScene == GameScenes.FLIGHT)
                            continue;

                        LunaLog.Log($"[LMP]: Loading vessel {vesselProto.Key}");
                        
                        if (VesselLoader.LoadVessel(vesselProto.Value.ProtoVessel))
                        {
                            LunaLog.Log($"[LMP]: Vessel {vesselProto.Key} loaded");
                            VesselLoadEvent.onLmpVesselLoaded.Fire(vesselProto.Value.Vessel);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in CheckVesselsToLoad {e}");
            }
        }
        
        /// <summary>
        /// Check vessels that must be reloaded
        /// </summary>
        private void CheckVesselsToRefresh()
        {
            try
            {
                if (TimeUtil.IsInInterval(ref _lastReloadCheck, 1500) && ProtoSystemBasicReady)
                {
                    VesselsToRefresh.Clear();

                    //We get the vessels that already exist
                    VesselsToRefresh.AddRange(VesselsProtoStore.AllPlayerVessels
                        .Where(pv => pv.Value.VesselExist && pv.Value.VesselHasUpdate && !pv.Value.HasInvalidParts)
                        .Select(v => v.Key));

                    //Do not iterate directly trough the AllPlayerVessels dictionary as the collection can be modified in another threads!
                    foreach (var vesselIdToReload in VesselsToRefresh)
                    {
                        if (VesselRemoveSystem.VesselWillBeKilled(vesselIdToReload))
                            continue;

                        if (FlightGlobals.ActiveVessel?.id == vesselIdToReload)
                            LunaLog.LogWarning("Reloading our OWN active vessel!");

                        if (VesselsProtoStore.AllPlayerVessels.TryGetValue(vesselIdToReload, out var vesselProtoUpd))
                        {
                            CurrentlyUpdatingVesselId = vesselIdToReload;
                            ProtoToVesselRefresh.UpdateVesselPartsFromProtoVessel(vesselProtoUpd.Vessel, vesselProtoUpd.ProtoVessel, vesselProtoUpd.ForceReload, vesselProtoUpd.VesselParts.Keys);
                            vesselProtoUpd.VesselHasUpdate = false;
                            VesselReloadEvent.onLmpVesselReloaded.Fire(vesselProtoUpd.Vessel);
                            CurrentlyUpdatingVesselId = Guid.Empty;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in CheckVesselsToReload {e}");
            }
        }

        #endregion
    }
}

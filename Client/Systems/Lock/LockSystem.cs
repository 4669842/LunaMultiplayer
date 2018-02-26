using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Locks;
using LunaCommon.Message.Data.Lock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LunaClient.Systems.Lock
{
    /// <summary>
    /// This system control the locks
    /// </summary>
    public class LockSystem : MessageSystem<LockSystem, LockMessageSender, LockMessageHandler>
    {
        public static LockStore LockStore { get; } = new LockStore();
        public static LockQuery LockQuery { get; } = new LockQuery(LockStore);

        public List<AcquireEvent> LockAcquireEvents { get; } = new List<AcquireEvent>();
        public List<ReleaseEvent> LockReleaseEvents { get; } = new List<ReleaseEvent>();

        public LockEvents LockEvents { get; } = new LockEvents();

        #region Base overrides

        public override string SystemName { get; } = nameof(LockSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onGameSceneLoadRequested.Add(LockEvents.OnSceneRequested);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            LockStore.ClearAllLocks();
            LockAcquireEvents.Clear();
            LockReleaseEvents.Clear();
            GameEvents.onGameSceneLoadRequested.Remove(LockEvents.OnSceneRequested);
        }

        #endregion

        #region Public methods

        #region Hooks

        #region RegisterHook

        public void RegisterAcquireHook(AcquireEvent methodObject)
        {
            LockAcquireEvents.Add(methodObject);
        }

        public void RegisterReleaseHook(ReleaseEvent methodObject)
        {
            LockReleaseEvents.Add(methodObject);
        }

        #endregion

        #region UnregisterHook

        public void UnregisterAcquireHook(AcquireEvent methodObject)
        {
            if (LockAcquireEvents.Contains(methodObject))
                LockAcquireEvents.Remove(methodObject);
        }

        public void UnregisterReleaseHook(ReleaseEvent methodObject)
        {
            if (LockReleaseEvents.Contains(methodObject))
                LockReleaseEvents.Remove(methodObject);
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// This event is triggered when some player acquired a lock
        /// It then calls all the methods specified in the delegate
        /// </summary>
        public void FireAcquireEvent(LockDefinition lockDefinition)
        {
            foreach (var methodObject in LockAcquireEvents)
            {
                try
                {
                    methodObject(lockDefinition);
                }
                catch (Exception e)
                {
                    LunaLog.LogError($"[LMP]: Error thrown in acquire lock event, exception {e}");
                }
            }
        }

        /// <summary>
        /// This event is triggered when some player released a lock
        /// It then calls all the methods specified in the delegate
        /// </summary>
        public void FireReleaseEvent(LockDefinition lockDefinition)
        {
            foreach (var methodObject in LockReleaseEvents)
            {
                try
                {
                    methodObject(lockDefinition);
                }
                catch (Exception e)
                {
                    LunaLog.LogError($"[LMP]: Error thrown in release lock event, exception {e}");
                }
            }
        }

        #endregion

        #region AcquireLocks

        /// <summary>
        /// Aquire the specified lock by sending a message to the server.
        /// </summary>
        /// <param name="lockDefinition">The definition of the lock to acquire</param>
        /// <param name="force">Force the aquire. Usually false unless in dockings.</param>
        private void AcquireLock(LockDefinition lockDefinition, bool force = false)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<LockAcquireMsgData>();
            msgData.Lock = lockDefinition;
            msgData.Force = force;

            MessageSender.SendMessage(msgData);
        }

        /// <summary>
        /// Aquire the control lock on the given vessel
        /// </summary>
        public void AcquireControlLock(Guid vesselId, bool force = false)
        {
            if (!LockQuery.ControlLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                AcquireLock(new LockDefinition(LockType.Control, SettingsSystem.CurrentSettings.PlayerName, vesselId), force);
        }

        /// <summary>
        /// Aquire the update lock on the given vessel
        /// </summary>
        public void AcquireUpdateLock(Guid vesselId, bool force = false)
        {
            if (!LockQuery.UpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                AcquireLock(new LockDefinition(LockType.Update, SettingsSystem.CurrentSettings.PlayerName, vesselId), force);
        }

        /// <summary>
        /// Aquire the unloaded update lock on the given vessel
        /// </summary>
        public void AcquireUnloadedUpdateLock(Guid vesselId, bool force = false)
        {
            if (!LockQuery.UnloadedUpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                AcquireLock(new LockDefinition(LockType.UnloadedUpdate, SettingsSystem.CurrentSettings.PlayerName, vesselId), force);
        }

        /// <summary>
        /// Aquire the spectator lock on the given vessel
        /// </summary>
        public void AcquireSpectatorLock(Guid vesselId)
        {
            AcquireLock(new LockDefinition(LockType.Spectator, SettingsSystem.CurrentSettings.PlayerName, vesselId));
        }

        /// <summary>
        /// Aquire the spectator lock on the given vessel
        /// </summary>
        public void AcquireAsteroidLock()
        {
            AcquireLock(new LockDefinition(LockType.Asteroid, SettingsSystem.CurrentSettings.PlayerName));
        }

        #endregion

        #region ReleaseLocks      

        /// <summary>
        /// Release the specified lock by sending a message to the server.
        /// </summary>
        /// <param name="lockDefinition">The definition of the lock to release</param>
        public void ReleaseLock(LockDefinition lockDefinition)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<LockReleaseMsgData>();
            msgData.Lock.CopyFrom(lockDefinition);

            LockStore.RemoveLock(lockDefinition);
            FireReleaseEvent(lockDefinition);

            MessageSender.SendMessage(msgData);
        }

        /// <summary>
        /// Release the update lock on the given vessel
        /// </summary>
        public void ReleaseUpdateLock(Guid vesselId)
        {
            ReleaseLock(new LockDefinition(LockType.Update, SettingsSystem.CurrentSettings.PlayerName, vesselId));
        }

        /// <summary>
        /// Release the spectator lock
        /// </summary>
        public void ReleaseSpectatorLock()
        {
            if (LockQuery.SpectatorLockExists(SettingsSystem.CurrentSettings.PlayerName))
                ReleaseLock(new LockDefinition(LockType.Spectator, SettingsSystem.CurrentSettings.PlayerName));
        }

        /// <summary>
        /// Release all the locks (update and control) of a vessel
        /// </summary>
        public void ReleaseAllVesselLocks(Guid vesselId, int msDelay = 0)
        {
            TaskFactory.StartNew(() =>
            {
                if (msDelay > 0)
                    Thread.Sleep(msDelay);

                if (LockQuery.UnloadedUpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                    ReleaseLock(new LockDefinition(LockType.UnloadedUpdate, SettingsSystem.CurrentSettings.PlayerName,vesselId));
                if (LockQuery.UpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                    ReleaseLock(new LockDefinition(LockType.Update, SettingsSystem.CurrentSettings.PlayerName, vesselId));
                if (LockQuery.ControlLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                    ReleaseLock(new LockDefinition(LockType.Control, SettingsSystem.CurrentSettings.PlayerName,vesselId));
            });
        }

        /// <summary>
        /// Release the specified control lock excepts the one for the vessel you give as parameter.
        /// </summary>
        public void ReleaseControlLocksExcept(Guid vesselId)
        {            
            //Serialize to array as it's a concurrent collection
            var locksToRemove = LockQuery.GetAllControlLocks(SettingsSystem.CurrentSettings.PlayerName)
                .Where(v => v.VesselId != vesselId).ToArray();

            foreach (var lockToRemove in locksToRemove)
            {
                ReleaseLock(lockToRemove);
            }
        }

        /// <summary>
        /// Release all the locks you have based by type
        /// </summary>
        public void ReleasePlayerLocks(LockType type)
        {
            var playerName = SettingsSystem.CurrentSettings.PlayerName;
            IEnumerable<LockDefinition> locksToRelease;
            switch (type)
            {
                case LockType.Asteroid:
                    locksToRelease = LockQuery.AsteroidLockOwner() == playerName ?
                        new[] { LockQuery.AsteroidLock() } : new LockDefinition[0];
                    break;
                case LockType.Control:
                    locksToRelease = LockQuery.GetAllControlLocks(SettingsSystem.CurrentSettings.PlayerName);
                    break;
                case LockType.Update:
                    locksToRelease = LockQuery.GetAllUpdateLocks(SettingsSystem.CurrentSettings.PlayerName);
                    break;
                case LockType.UnloadedUpdate:
                    locksToRelease = LockQuery.GetAllUnloadedUpdateLocks(SettingsSystem.CurrentSettings.PlayerName);
                    break;
                case LockType.Spectator:
                    locksToRelease = LockQuery.SpectatorLockExists(SettingsSystem.CurrentSettings.PlayerName) ?
                        new[] { LockQuery.GetSpectatorLock(SettingsSystem.CurrentSettings.PlayerName) } : new LockDefinition[0];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            //Serialize to array as it's a concurrent collection
            foreach (var lockToRelease in locksToRelease.ToArray())
            {
                ReleaseLock(lockToRelease);
            }
        }

        #endregion

        #endregion
    }
}
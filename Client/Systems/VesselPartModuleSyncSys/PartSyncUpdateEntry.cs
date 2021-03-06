﻿using LunaCommon.Time;
using System;

namespace LunaClient.Systems.VesselPartModuleSyncSys
{
    public enum CustomizationResult { TooEarly, Ignore, Ok }

    public class PartSyncUpdateEntry
    {
        private DateTime? _lastUpdateTime;
        private readonly int _intervalInMs;

        public bool IntervalIsOk() => !_lastUpdateTime.HasValue || LunaComputerTime.UtcNow - _lastUpdateTime > TimeSpan.FromMilliseconds(_intervalInMs);

        public PartSyncUpdateEntry(int intervalMs) => _intervalInMs = intervalMs;

        public void Update()
        {
            _lastUpdateTime = LunaComputerTime.UtcNow;
        }
    }
}

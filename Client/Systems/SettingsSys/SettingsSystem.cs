using System.Text;
using LunaClient.Base;
using LunaCommon.Enums;

namespace LunaClient.Systems.SettingsSys
{
    public class SettingsSystem : MessageSystem<SettingsSystem, SettingsMessageSender, SettingsMessageHandler>
    {
        public static SettingStructure CurrentSettings { get; }
        public static SettingsServerStructure ServerSettings { get; private set; } = new SettingsServerStructure();

        private static readonly StringBuilder Builder = new StringBuilder();

        static SettingsSystem()
        {
            CurrentSettings = SettingsReadSaveHandler.ReadSettings();
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            ServerSettings = new SettingsServerStructure();
        }

        public static void SaveSettings()
        {
            SettingsReadSaveHandler.SaveSettings(CurrentSettings);
        }

        public static bool ValidateSettings()
        {
            Builder.Length = 0;
            var validationResult = true;

            if ((int) ServerSettings.TerrainQuality != PQSCache.PresetList.presetIndex)
            {
                validationResult = false;
                Builder.Append($"Your terrain quality ({((TerrainQuality)PQSCache.PresetList.presetIndex).ToString()}) " +
                                     $"does not match the server quality ({ServerSettings.TerrainQuality.ToString()}).");

            }

            if (!validationResult)
            {
                MainSystem.NetworkState = ClientState.Disconnected;
                SystemsContainer.Get<MainSystem>().Status = Builder.ToString();
            }

            return validationResult;
        }
    }
}
namespace SoundOutputMultiplexer.Common.Models
{
    [Serializable]
    public class PipeMessage
    {
        /// <summary>
        /// Message Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Message Action
        /// </summary>
        public ActionType Action { get; set; }

        /// <summary>
        /// Message Text - Only if Action == ActionType.SendMessage
        /// </summary>
        public string? MessageText { get; set; }

        /// <summary>
        /// Message Data - Only if Action requires a data response
        /// Data will be JSON formatted
        /// </summary>
        public string? MessageData { get; set; }

        public PipeMessage()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Message Actions
        /// </summary>
        public enum ActionType
        {
            /// <summary>
            /// Just in case we forget to assign a value
            /// </summary>
            Unknown,

            /// <summary>
            /// List of Devices - response to EnumerateDevices Command
            /// </summary>
            DeviceList,

            /// <summary>
            /// Enumerate Devices Command
            /// </summary>
            EnumerateDevices,

            /// <summary>
            /// Move the tray icon to the overflow menu
            /// </summary>
            HideTrayIcon,

            /// <summary>
            /// Send message from Service to Client, or Client to Service
            /// </summary>
            SendMessage,

            /// <summary>
            /// Set the Input Device in Service
            /// </summary>
            SetInputDevice,

            /// <summary>
            /// Set the Output Devices in Service
            /// </summary>
            SetOutputDevices,

            /// <summary>
            /// Set the Output Master Device (for Volume) in Service
            /// </summary>
            SetOutputMasterDevice,

            /// <summary>
            /// Pin the tray icon
            /// </summary>
            ShowTrayIcon,
        }
    }
}
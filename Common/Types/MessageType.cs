namespace Common
{
    /// <summary>
    ///
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>
        ///
        /// </summary>
        INIT = 0,

        /// <summary>
        ///
        /// </summary>
        MESSAGE = 1,

        /// <summary>
        ///
        /// </summary>
        MOVE = 2,

        /// <summary>
        ///
        /// </summary>
        FIRE = 3,

        /// <summary>
        ///
        /// </summary>
        USE = 4,

        /// <summary>
        ///
        /// </summary>
        ALL_PLAYER_STATES = 5,

        /// <summary>
        ///
        /// </summary>
        ALL_CREATURE_STATES = 6,

        /// <summary>
        ///
        /// </summary>
        GAME_STATE = 7,

        /// <summary>
        ///
        /// </summary>
        PLAYER_DISCONNECTED = 8,

        /// <summary>
        ///
        /// </summary>
        DISCONNECT = 9,

        /// <summary>
        ///
        /// </summary>
        PING = 90,

        /// <summary>
        ///
        /// </summary>
        PONG = 91
    }
}
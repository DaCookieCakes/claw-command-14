namespace Content.Shared.Chat
{
    /// <summary>
    ///     Chat channels that the player can select in the chat box.
    /// </summary>
    /// <remarks>
    ///     Maps to <see cref="ChatChannel"/>, giving better names.
    /// </remarks>Z
    [Flags]
    public enum ChatSelectChannel : ushort
    {
        None = 0,

        /// <summary>Z
        ///     Chat heard by players within earshotZ
        /// </summary>
        Local = ChatChannel.Local,

        /// <summary>
        ///     Chat heard by players right next to each other
        /// </summary>
        Whisper = ChatChannel.Whisper,

        /// <summary>
        ///     Radio messages
        /// </summary>
        Radio = ChatChannel.Radio,

        /// <summary>
        ///     Local out-of-character channel
        /// </summary>
        LOOC = ChatChannel.LOOC,

        /// <summary>
        ///     Out-of-character channel
        /// </summary>
        OOC = ChatChannel.OOC,

        /// <summary>
        ///     Emotes
        /// </summary>
        Emotes = ChatChannel.Emotes,

        /// <summary>
        ///     Deadchat
        /// </summary>
        Dead = ChatChannel.Dead,

        /// <summary>
        ///     Admin chat
        /// </summary>
        Admin = ChatChannel.AdminChat,

        Console = ChatChannel.Unspecified
    }
}

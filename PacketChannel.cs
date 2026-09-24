using System.Collections.Generic;
using BepInEx;
using Steamworks.Data;

namespace Entwined
{
    /// <summary>
    /// An event fired when a packet received data.
    /// </summary>
    /// <param name="payload">The data received</param>
    /// <param name="sourceInfo">Information about the sender of the packet</param>
    public delegate void PacketReceiveEvent<T>(T payload, PacketSourceInfo sourceInfo);

    /// <summary>
    /// The simplest component in Entwined. 
    /// <c>PacketChannel</c> represents a self-contained 
    /// type of packet that can broadcast and receive data.
    /// 
    /// Note that different <c>PacketChannel</c>s cannot interact, 
    /// each functions as its own message channel.
    /// </summary>
    public class PacketChannel
    {
        internal PacketIdentifier packetIdentifier;

        /// <summary>
        /// Should only be run in your plugin's Awake function
        /// Creates a new <c>PacketChannel</c> to transmit and receive data.
        /// <example>
        /// <code>
        /// void Awake() {
        ///     var packetChannel = new PacketChannel(this);
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="plugin">Your current plugin.</param>
        public PacketChannel(BaseUnityPlugin plugin)
        {
            packetIdentifier = IdentifierRegister.GenerateNewPacketIdentifier(plugin);
            packetIdentifier.PacketType = this;
        }

        /// <summary>
        /// Fired when the packet receives data. (A client ran <c>packetChannel.SendMessage</c>)
        /// </summary>
        public event PacketReceiveEvent<byte[]> OnMessage;

        internal void ReceiveMessage(byte[] payload, PacketSourceInfo sourceInfo)
        {
            OnMessage.Invoke(payload, sourceInfo);
        }

        /// <summary>
        /// Send data to all clients
        /// </summary>
        /// <param name="payload">The data to send</param>
        public void SendMessage(byte[] payload)
        {
            Entwined.SendMessage(packetIdentifier, payload);
        }
        /// <summary>
        /// Send data to a specific client.
        /// Only use this if you know what you're doing, bopl's netcode requires the entire game state to be synced to every player.
        /// If you send game-state altering information to some clients but not others, the game will desync and the round will end differently for each player.
        /// </summary>
        /// <param name="payload">The data to send</param>
        /// <param name="player">The player's steamworks <c>Connection</c> instance</param>
        public void SendMessageTo(byte[] payload, Connection player)
        {
            Entwined.SendMessageTo(packetIdentifier, payload, player);
        }
        /// <summary>
        /// Send data to a list of specific clients.
        /// Only use this if you know what you're doing, bopl's netcode requires the entire game state to be synced to every player.
        /// If you send game-state altering information to some clients but not others, the game will desync and the round will end differently for each player.
        /// </summary>
        /// <param name="payload">The data to send</param>
        /// <param name="players">The players' steamworks <c>Connection</c> instances</param>
        public void SendMessageTo(byte[] payload, List<Connection> players)
        {
            Entwined.SendMessageTo(packetIdentifier, payload, players);
        }
    }

    /// <summary>
    /// A <c>PacketChannel</c> with a built-in entwiner.
    /// </summary>
    /// <typeparam name="T">The entwiner type</typeparam>
    public class EntwinedPacketChannel<T>
    {
        public PacketChannel PacketChannel { get; private set; }
        public IEntwiner<T> Entwiner { get; private set; }

        /// <summary>
        /// Creates a new <c>EntwinedPacketChannel</c> from the 
        /// given <c>PacketChannel</c> and <c>IEntwiner</c>
        /// </summary>
        /// <param name="packetChannel">The packet channel</param>
        /// <param name="entwiner">The entwiner</param>
        public EntwinedPacketChannel(PacketChannel packetChannel, IEntwiner<T> entwiner)
        {
            Init(packetChannel, entwiner);
        }
        /// <summary>
        /// Should only be run in your plugin's Awake function.
        /// Creates a new <c>PacketChannel</c> and <c>EntwinedPacketChannel</c> from the 
        /// given plugin and <c>IEntwiner</c>. 
        /// </summary>
        /// <param name="plugin">The plugin</param>
        /// <param name="entwiner">The entwiner</param>
        public EntwinedPacketChannel(BaseUnityPlugin plugin, IEntwiner<T> entwiner)
        {
            Init(new PacketChannel(plugin), entwiner);
        }
        private void Init(PacketChannel packetChannel, IEntwiner<T> entwiner)
        {
            PacketChannel = packetChannel;
            Entwiner = entwiner;
            PacketChannel.OnMessage += ReceiveMessage;
        }

        /// <summary>
        /// Fired when the packet receives data. (A client ran <c>packetChannel.SendMessage</c>)
        /// </summary>
        public event PacketReceiveEvent<T> OnMessage;

        internal void ReceiveMessage(byte[] payload, PacketSourceInfo sourceInfo)
        {
            OnMessage.Invoke(Entwiner.Detwine(payload), sourceInfo);
        }

        /// <summary>
        /// Send data to all clients
        /// </summary>
        /// <param name="payload">The data to send</param>
        public void SendMessage(T payload)
        {
            PacketChannel.SendMessage(Entwiner.Entwine(payload));
        }
        /// <summary>
        /// Send data to a specific client.
        /// Only use this if you know what you're doing, bopl's netcode requires the entire game state to be synced to every player.
        /// If you send game-state altering information to some clients but not others, the game will desync and the round will end differently for each player.
        /// </summary>
        /// <param name="payload">The data to send</param>
        /// <param name="player">The player's steamworks <c>Connection</c> instance</param>
        public void SendMessageTo(T payload, Connection player)
        {
            PacketChannel.SendMessageTo(Entwiner.Entwine(payload), player);
        }
        /// <summary>
        /// Send data to a list of specific clients.
        /// Only use this if you know what you're doing, bopl's netcode requires the entire game state to be synced to every player.
        /// If you send game-state altering information to some clients but not others, the game will desync and the round will end differently for each player.
        /// </summary>
        /// <param name="payload">The data to send</param>
        /// <param name="players">The players' steamworks <c>Connection</c> instances</param>
        public void SendMessageTo(T payload, List<Connection> players)
        {
            PacketChannel.SendMessageTo(Entwiner.Entwine(payload), players);
        }
    }
}

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Marvin.JsonPatch;
using Newtonsoft.Json.Linq;
using System.Text;
#if !WINDOWS_UWP
using WebSocketSharp;
#endif
using UnityEngine;

namespace Colyseus
{
    /// <summary>
    /// Colyseus.Client
    /// </summary>
    /// <remarks>
    /// Provides integration between Colyseus Game Server through WebSocket protocol (<see href="http://tools.ietf.org/html/rfc6455">RFC 6455</see>).
    /// </remarks>
    public class Client
    {
        /// <summary>
        /// Unique <see cref="Client"/> identifier.
        /// </summary>
        public string id = null;

        public WebSocket ws;
        private Dictionary<string, Room> rooms = new Dictionary<string, Room>();

        // Events

        /// <summary>
        /// Occurs when the <see cref="Client"/> connection has been established, and Client <see cref="id"/> is available.
        /// </summary>
        public event EventHandler OnOpen;

        /// <summary>
        /// Occurs when the <see cref="Client"/> connection has been closed.
        /// </summary>
        public event EventHandler OnClose;

        /// <summary>
        /// Occurs when the <see cref="Client"/> gets an error.
        /// </summary>
        public event EventHandler OnError;

        /// <summary>
        /// Occurs when the <see cref="Client"/> receives a message from server.
        /// </summary>
        public event EventHandler<MessageEventArgs> OnMessage;

        // TODO: implement auto-reconnect feature
        // public event EventHandler OnReconnect;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class with
        /// the specified Colyseus Game Server Server endpoint.
        /// </summary>
        /// <param name="endpoint">
        /// A <see cref="string"/> that represents the WebSocket URL to connect.
        /// </param>
        public Client(string endpoint)
        {
            this.ws = new WebSocket(new Uri(endpoint));

            //this.ws.OnMessage += OnMessageHandler;
            //this.ws.OnClose += OnCloseHandler;
            //this.ws.OnError += OnErrorHandler;
        }

        public IEnumerator Connect()
        {
            return this.ws.Connect();
        }

        public void Recv()
        {
            byte[] data = this.ws.Recv();
            if (data != null)
            {
                this.ParseMessage(data);
            }
        }

#if !WINDOWS_UWP
        void OnCloseHandler(object sender, CloseEventArgs e)
        {
            this.OnClose.Emit(this, e);
        }
#else
        void OnCloseHandler(object sender, EventArgs e)
        {
            this.OnClose.Invoke(this, e);
        }
#endif



        void ParseMessage(byte[] recv)
        {
            var rec = System.Text.Encoding.Default.GetString(recv);
            var message = JsonConvert.DeserializeObject<object[]>(rec);

            //object[] message = new object[raw.Values.Count];
            //raw.Values.CopyTo(message, 0);

            var code = (long)message[0];

            if (code == Protocol.USER_ID)
            {
                this.id = (string)message[1];
                this.OnOpen.Invoke(this, EventArgs.Empty);
                return;
            }

            // Parse roomId or roomName
            Room room = null;
            long roomIdInt32 = 0;
            string roomId = "0";
            string roomName = null;

            try
            {
                roomIdInt32 = (long)message[1];
                roomId = roomIdInt32.ToString();
            }
            catch (Exception)
            {
                try
                {
                    roomName = (string)message[1];
                }
                catch (Exception)
                {
                }
            }


            if (code == Protocol.JOIN_ROOM)
            {
                roomName = (string)message[2];

                if (this.rooms.ContainsKey(roomName))
                {
                    this.rooms[roomId] = this.rooms[roomName];
                    this.rooms.Remove(roomName);
                }

                room = this.rooms[roomId];
                room.id = roomIdInt32;
            }
            else if (code == Protocol.JOIN_ERROR)
            {
                room = this.rooms[roomName];

                MessageEventArgs error = new MessageEventArgs(room, message);
                room.EmitError(error);
                this.OnError.Invoke(this, error);
                this.rooms.Remove(roomName);
            }
            else if (code == Protocol.LEAVE_ROOM)
            {
                room = this.rooms[roomId];
                room.Leave(false);
            }
            else if (code == Protocol.ROOM_STATE)
            {
                var state = ((JObject)message[2]).ToObject<RoomState>();


                var remoteCurrentTime = (long)message[3];
                var remoteElapsedTime = (long)message[4];

                room = this.rooms[roomId];
                // JToken.Parse (message [2].ToString ())
                room.SetState(state, remoteCurrentTime, remoteElapsedTime);
            }
            else if (code == Protocol.ROOM_STATE_PATCH)
            {
                room = this.rooms[roomId];
                room.ApplyPatch((string)message[2]);
            }
            else if (code == Protocol.ROOM_DATA)
            {
                room = this.rooms[roomId];
                room.ReceiveData(message[2]);
                if (this.OnMessage != null)
                    this.OnMessage.Invoke(this, new MessageEventArgs(room, message[2]));
            }
        }

        /// <summary>
        /// Request <see cref="Client"/> to join in a <see cref="Room"/>.
        /// </summary>
        /// <param name="roomName">The name of the Room to join.</param>
        /// <param name="options">Custom join request options</param>
        public Room Join(string roomName, object options = null)
        {
            if (!this.rooms.ContainsKey(roomName))
            {
                this.rooms.Add(roomName, new Room(this, roomName));
            }

            this.Send(new object[] { Protocol.JOIN_ROOM, roomName, options });

            return this.rooms[roomName];
        }

        private void OnErrorHandler(object sender, EventArgs args)
        {
            this.OnError.Invoke(sender, args);
        }

        /// <summary>
        /// Send data to all connected rooms.
        /// </summary>
        /// <param name="data">Data to be sent to all connected rooms.</param>
        public void Send(object[] data)
        {
            var json = JsonConvert.SerializeObject(data);
            var bytes = Encoding.UTF8.GetBytes(json);
            this.ws.Send(bytes);
        }


        /// <summary>
        /// Close <see cref="Client"/> connection and leave all joined rooms.
        /// </summary>
        public void Close()
        {
            this.ws.Close();
        }

        public string error
        {
            get { return this.ws.error; }
        }
    }
}
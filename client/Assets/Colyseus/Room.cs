using System;
using System.Collections.Generic;
using System.IO;
using Marvin.JsonPatch;
using Newtonsoft.Json;
using UnityEngine;

namespace Colyseus
{
    /// <summary>
    /// </summary>
    public class Room
    {
        private Client client;

        /// <summary>
        /// Name of the <see cref="Room"/>.
        /// </summary>
        public String name;

        // public DeltaContainer state = new DeltaContainer(new RoomState());
        //public IndexedDictionary<string, object> state;
        public RoomState state;

        private long _id = 0;
        private RoomState _previousState;

        /// <summary>
        /// Occurs when the <see cref="Client"/> successfully connects to the <see cref="Room"/>.
        /// </summary>
        public event EventHandler OnJoin;

        /// <summary>
        /// Occurs when some error has been triggered in the room.
        /// </summary>
        public event EventHandler OnError;

        /// <summary>
        /// Occurs when <see cref="Client"/> leaves this room.
        /// </summary>
        public event EventHandler OnLeave;

        /// <summary>
        /// Occurs when server send patched state, before <see cref="OnUpdate"/>.
        /// </summary>
        public event EventHandler<MessageEventArgs> OnPatch;

        /// <summary>
        /// Occurs when server sends a message to this <see cref="Room"/>
        /// </summary>
        public event EventHandler<MessageEventArgs> OnData;

        /// <summary>
        /// Occurs after applying the patched state on this <see cref="Room"/>.
        /// </summary>
        public event EventHandler<RoomUpdateEventArgs> OnUpdate;

        /// <summary>
        /// Initializes a new instance of the <see cref="Room"/> class.
        /// It synchronizes state automatically with the server and send and receive messaes.
        /// </summary>
        /// <param name="client">
        /// The <see cref="Client"/> client connection instance.
        /// </param>
        /// <param name="name">The name of the room</param>
        public Room(Client client, String name)
        {
            this.client = client;
            this.name = name;
        }

        /// <summary>
        /// Contains the id of this room, used internally for communication.
        /// </summary>
        public long id
        {
            get { return this._id; }
            set
            {
                this._id = value;
                this.OnJoin.Invoke(this, EventArgs.Empty);
            }
        }


        public void SetState(RoomState state, double remoteCurrentTime, long remoteElapsedTime)
        {
            this.state = state;

            // TODO:
            // Create a "clock" for remoteCurrentTime / remoteElapsedTime to match the JavaScript API.

            // Creates serializer.
            if (this.OnUpdate != null)
                this.OnUpdate.Invoke(this, new RoomUpdateEventArgs(this, state, null));
        }


        /// <summary>
        /// Leave the room.
        /// </summary>
        public void Leave(bool requestLeave = true)
        {
            if (requestLeave && this._id > 0)
            {
                this.Send(new object[] { Protocol.LEAVE_ROOM, this._id });
            }
            else
            {
                this.OnLeave.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Send data to this room.
        /// </summary>
        /// <param name="data">Data to be sent</param>
        public void Send(object data)
        {
            this.client.Send(new object[] { Protocol.ROOM_DATA, this._id, data });
        }

        /// <summary>Internal usage, shouldn't be called.</summary>
        public void ReceiveData(object data)
        {
            if (this.OnData != null)
                this.OnData.Invoke(this, new MessageEventArgs(this, data));
        }

        /// <summary>Internal usage, shouldn't be called.</summary>
        public void ApplyPatch(string patch)
        {
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<RoomState>>(patch);
            deserialized.ApplyTo(state);

            //this.state = state
            if (this.OnUpdate != null)
                this.OnUpdate.Invoke(this, new RoomUpdateEventArgs(this, this.state, null));
        }

        /// <summary>Internal usage, shouldn't be called.</summary>
        public void EmitError(MessageEventArgs args)
        {
            this.OnError.Invoke(this, args);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Mote.Simu.Communication
{
    public abstract class BaseServer
    {
		private Socket _server;

		private byte[] _data = new byte[1024];

		public BaseServer()
		{ }

		public void Start(IPAddress ip, int port)
		{
			this._server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			this._server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			this._server.Bind(new IPEndPoint(IPAddress.Any, port));

			this.Receive();
		}

		protected void Receive()
		{
			EndPoint newClientEP = new IPEndPoint(IPAddress.Any, 0);
			this._server.BeginReceiveFrom(this._data, 0, this._data.Length, SocketFlags.None, ref newClientEP, this.DoReceiveFrom, newClientEP);
		}

		private void DoReceiveFrom(IAsyncResult iar)
		{
			try
			{
				EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
				int dataLen = this._server.EndReceiveFrom(iar, ref clientEP);
				Thread thread = new Thread(new ParameterizedThreadStart(this.ProcessMessage));
				thread.Start(System.Text.Encoding.UTF8.GetString(this._data, 0, dataLen));
				this.Receive();
			}
			catch (ObjectDisposedException)
			{ }
		}

		protected abstract void ProcessMessage(object message);
	}
}

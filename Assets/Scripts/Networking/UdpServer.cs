using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Text;
using System.Threading;

namespace Networking
{
	public interface IServerHandler
	{
		UdpServer Server
		{
			get;
			set;
		}

		void Received(byte[] data, EndPoint endpoint);
	}

	class UdpServerState
	{
		public EndPoint Endpoint;

		public byte[] Data;

		public UdpServerState(EndPoint endpoint)
		{
			this.Endpoint = endpoint;
			this.Data = new byte[2048];
		}
	}

	class SendData
	{
		public byte[] Data;
		public EndPoint Endpoint;

		public SendData(byte[] data, EndPoint endpoint)
		{
			this.Data = data;
			this.Endpoint = endpoint;
		}
	}

	public class UdpServer
	{
		private bool _running = false;

		private Socket _socket;

		private byte[] _data;

		private IPEndPoint _endpoint;

		private IServerHandler _handler;

		private Queue<SendData> _queue;

		private Mutex _queueMutex;

		public UdpServer(IServerHandler handler)
		{
			this._data = new byte[2048];
			this._handler = handler;
			this._handler.Server = this;
			this._queue = new Queue<SendData>();
			this._queueMutex = new Mutex();
		}

		public bool Running
		{
			get { return this._running; }
		}

		public void Start(IPEndPoint ep)
		{
			if (this.Running)
				throw new Exception("Already running.");

			this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			try
			{
				this._socket.Bind(ep);
			}
			catch (SocketException)
			{
				throw new Exception("Port busy.");
			}
			this._endpoint = ep;
			this._running = true;
			this.Receive();
		}

		protected virtual void ReceiveHandler(IAsyncResult ar)
		{
			UdpServerState state = (UdpServerState)ar.AsyncState;
			int recv = this._socket.EndReceiveFrom(ar, ref state.Endpoint);
			if (recv > 0)
				this._handler.Received(state.Data, state.Endpoint);
			this.Receive();
		}

		public virtual void Receive()
		{
			if (this.Running)
			{
				UdpServerState state = new UdpServerState(new IPEndPoint(this._endpoint.Address, this._endpoint.Port));
				this._socket.BeginReceiveFrom(state.Data, 0, state.Data.Length, SocketFlags.None, ref state.Endpoint, this.ReceiveHandler, state);
			}
		}

		protected virtual void SendHandler(IAsyncResult result)
		{
			int sendv = this._socket.EndSendTo(result);
			this.SendData();
		}

		protected virtual void SendData()
		{
			if (this._queue.Count > 0)
			{
				SendData data = this._queue.Dequeue();
				this._socket.BeginSendTo(data.Data, 0, data.Data.Length, SocketFlags.None, data.Endpoint, this.SendHandler, null);
			}
		}

		public virtual void Send(byte[] data, EndPoint endpoint)
		{
			this._queueMutex.WaitOne();
			try
			{
				this._queue.Enqueue(new SendData(data, endpoint));
				if (this._queue.Count <= 1)
					this.SendData();
			}
			finally
			{
				this._queueMutex.ReleaseMutex();
			}
		}

		public virtual void Send(String data, EndPoint endpoint)
		{
			this.Send(Encoding.UTF8.GetBytes(data), endpoint);
		}
	}
}
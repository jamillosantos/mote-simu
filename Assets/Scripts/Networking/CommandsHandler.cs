using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text;
using UnityEngine;

namespace Networking
{
	public class CommandsHandler
		: IServerHandler
	{

		public UdpServer Server
		{
			get
			{
				return this._server;
			}

			set
			{
				this._server = value;
			}
		}

		private UdpServer _server;

		public void Received(byte[] data, EndPoint endpoint)
		{
			string json = Encoding.UTF8.GetString(data);
			Debug.Log("RECEIVED: " + json);
			try
			{
				JObject obj = JObject.Parse(json);
				this.command(obj);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		private void command(JObject obj)
		{
			string commandName = (string)obj["name"];
			if (commandName == "multi")
			{
				JArray commands = (JArray)obj["params"];
				foreach (var o in commands)
				{
					this.command((JObject)o);
				}
			}
			else if (commandName == "create")
			{
				// this.rotate((JObject)obj["params"]);
			}
		}
	}
}

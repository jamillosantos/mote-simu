using UnityEngine;
using System.Collections.Generic;
using System.Threading;

namespace Model
{
	public class ModelManager : MonoBehaviour
	{
		private Dictionary<string, Manageable> _children = new Dictionary<string, Manageable>();
		
		private Networking.UdpServer _server;

		private List<Networking.Commands.ICommand> _commands = new List<Networking.Commands.ICommand>();

		public Dictionary<string, Manageable> Children
		{
			get
			{
				return this._children;
			}
		}

		void Start()
		{
			this._server = new Networking.UdpServer(new Networking.CommandsHandler(this));
			this._server.Start(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("0.0.0.0"), 4572));
		}

		public virtual void Register(Manageable obj)
		{
			this._children.Add(obj.name, obj);
		}

		public virtual void Unregister(Manageable obj)
		{
			this._children.Remove(obj.name);
		}

		public virtual void Add(Networking.Commands.ICommand command)
		{
			lock (this._commands)
			{
				Debug.Log("Adding " + command);
				this._commands.Add(command);
			}
		}

		void Update()
		{
			lock (this._commands)
			{
				foreach (Networking.Commands.ICommand command in this._commands)
				{
					Debug.Log("Executing " + command);
					command.Execute(this);
				}
				this._commands.Clear();
			}
		}
	}
}

using Model;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace Networking
{
	[Serializable]
	public class UnknownPropertyException
		: Exception
	{
		public string Property
		{
			get;
			private set;
		}

		public UnknownPropertyException(string property)
			: base("Unknown property '" + property + "'.")
		{
			this.Property = property;
		}
	}


	[Serializable]
	public class UnknownCommandException : Exception
	{
		public string Command
		{
			get;
			private set;
		}

		public UnknownCommandException(string commandName)
			: base("Unknown command: '" + commandName + "'.")
		{
			this.Command = commandName;
		}
	}

	[Serializable]
	public class RequirePropertyException
		: Exception
	{
		public string Property
		{
			get;
			private set;
		}

		public RequirePropertyException(string property)
		{
			this.Property = property;
		}
	}

	public partial class Converter
	{
		public static Vector3 ToVector3(JObject source)
		{
			Vector3 result = Vector3.zero;
			foreach (var p in source)
			{
				if (p.Key == "x")
					result.x = (float)p.Value;
				else if (p.Key == "y")
					result.y = (float)p.Value;
				else if (p.Key == "z")
					result.z = (float)p.Value;
				else
					throw new UnknownPropertyException(p.Key);
			}
			return result;
		}

		public static Quaternion ToQuaternion(JObject source)
		{
			Quaternion result = Quaternion.identity;
			bool fromEuler = false;
			Vector3 eulerAngles = Vector3.zero;
			foreach (var p in source)
			{
				if (p.Key == "qx")
					result.x = (float)p.Value;
				else if (p.Key == "qy")
					result.y = (float)p.Value;
				else if (p.Key == "qz")
					result.z = (float)p.Value;
				else if (p.Key == "qw")
					result.w = (float)p.Value;
				else if (p.Key == "x")
				{
					fromEuler = true;
					eulerAngles.x = (float)p.Value;
				}
				else if (p.Key == "y")
				{
					fromEuler = true;
					eulerAngles.y = (float)p.Value;
				}
				else if (p.Key == "z")
				{
					fromEuler = true;
					eulerAngles.z = (float)p.Value;
				}
				else
					throw new UnknownPropertyException(p.Key);
			}
			if (fromEuler)
				result.eulerAngles = eulerAngles;
			return result;
		}
	}

	public class CommandsHandler
		: IServerHandler
	{
		public ModelManager Manager
		{
			get;
			private set;
		}

		public CommandsHandler(ModelManager manager)
		{
			this.Manager = manager;
		}

		public void Received(byte[] data, EndPoint endpoint)
		{
			string json = Encoding.UTF8.GetString(data);
			Debug.Log("RECEIVED: " + json);
			try
			{
				JToken obj = JObject.Parse(json);
				if (obj is JObject)
				{
					this.command((JObject)obj);
				}
				else if (obj is JArray)
				{
					foreach (var o in (JArray)obj)
						this.command((JObject)o);
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		private void command(JObject obj)
		{
			string commandName = (string)obj["command"];
			Debug.Log("Command " + commandName + " arrived.");
			if (commandName == "create")
			{
				this.create((JObject)obj["params"]);
			}
			else if (commandName == "update")
			{
				this.update((JObject)obj["params"]);
			}
			else
			{
				throw new UnknownCommandException(commandName);
			}
		}

		private void update(JObject objects)
		{
			Manageable o;
			FieldInfo info;
			foreach (var obj in objects)
			{
				o = this.Manager.Children[obj.Key];
				foreach (var property in (JObject)obj.Value)
				{
					info = o.GetType().GetField(property.Key);
					if (info == null)
						throw new UnknownPropertyException(property.Key);
					Debug.Log(obj.Key + ": " + property.Key + " = " + property.Value);
					info.SetValue(o, Convert.ChangeType(property.Value, info.FieldType));
				}
			}
		}

		private void create(JObject obj, Commands.Create parent = null)
		{
			Commands.Create command = new Commands.Create();
			JArray children = null;
			foreach (var p in obj)
			{
				if (p.Key == "name")
					command.Name = (string)p.Value;
				else if (p.Key == "source")
				{
					command.Source = (string)p.Value;
				}
				else if (p.Key == "position")
				{
					command.Position = Converter.ToVector3((JObject)p.Value);
				}
				else if (p.Key == "rotation")
				{
					command.Rotation = Converter.ToQuaternion((JObject)p.Value);
				}
				else if (p.Key == "children")
				{
					children = (JArray)p.Value;
				}
				else
					throw new UnknownPropertyException(p.Key);
			}

			if (command.Name == "")
				throw new RequirePropertyException("name");
			else if (command.Source == "")
				throw new RequirePropertyException("source");
			else
			{
				if (children != null)
				{
					foreach (var t in children)
					{
						this.create((JObject)t, command);
					}
				}
				if (parent != null)
					parent.Children.Add(command);
				else
					this.Manager.Add(command);
			}
		}
	}
}

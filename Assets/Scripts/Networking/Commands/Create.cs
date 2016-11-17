using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Networking.Commands
{
	public class Create
		: ICommand
	{
		public string Name
		{
			get;
			internal set;
		}

		public Vector3 Position
		{
			get;
			internal set;
		}

		public Quaternion Rotation
		{
			get;
			internal set;
		}

		public string Source
		{
			get;
			internal set;
		}

		public List<Create> Children
		{
			get;
			private set;
		}
		public Transform Parent
		{
			get;
			private set;
		}

		public Create()
		{
			this.Children = new List<Create>();
		}

		public void Execute(Model.ModelManager manager)
		{
			GameObject go;
			if (this.Parent == null)
				go = (GameObject)GameObject.Instantiate(Resources.Load(this.Source));
			else
				go = (GameObject)GameObject.Instantiate(Resources.Load(this.Source), this.Parent);
			go.transform.localPosition = this.Position;
			go.transform.localRotation = this.Rotation;
			Model.Manageable mobj = go.GetComponent<Model.Manageable>();
			go.name = this.Name;

			if (mobj != null)
				mobj.Manager = manager;

			if (this.Children.Count > 0)
			{
				foreach (var t in this.Children)
				{
					t.Parent = go.transform;
					t.Execute(manager);
				}
			}
		}
	}
}

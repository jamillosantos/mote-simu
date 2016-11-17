using UnityEngine;
using System.Collections;

namespace Model
{
	public class Manageable : MonoBehaviour
	{

		public ModelManager Manager;

		void Start()
		{
			if (this.Manager == null)
			{
				GameObject p = this.transform.parent.gameObject;
				Motor m;
				while (p != null)
				{
					m = p.GetComponent<Motor>();
					if ((m != null) && (m.Manager != null))
					{
						this.Manager = m.Manager;
						break;
					}
					else
						p = p.transform.parent.gameObject;
				}
			}
			this.Manager.Register(this);
		}

		void OnDestroy()
		{
			this.Manager.Unregister(this);
		}
	}
}
using UnityEngine;

namespace Model
{
	public class Piece : MonoBehaviour
	{
		public PieceManager Manager;

		void Start()
		{
			this.Manager.Register(this);
		}

		void OnDestroy()
		{
			this.Manager.Unregister(this);
		}
	}
}
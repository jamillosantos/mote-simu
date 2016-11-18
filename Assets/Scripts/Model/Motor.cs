using UnityEditor;
using UnityEngine;

namespace Model
{
	public enum MotorDirection
	{
		Frontal,
		Lateral,
		Transversal
	};

	[ExecuteInEditMode]
	public class Motor : Manageable
	{
		public float Angle;

		protected MotorPlaceholder _placeholder;

		protected Horn _horn;

		public bool Inverted;

		protected virtual void Awake()
		{
			this._placeholder = this.GetComponentInChildren<MotorPlaceholder>();
			this._horn = this.GetComponentInChildren<Horn>();
		}

		void Update()
		{ }

		void FixedUpdate()
		{
			this.Apply();
		}

		protected virtual void Apply()
		{
			this._horn.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, this.Angle * (this.Inverted ? -1 : 1)));
		}

		void OnDrawGizmos()
		{
			Handles.ArrowCap(0, this.transform.position, this.transform.rotation, 0.2f);
		}
	}
}
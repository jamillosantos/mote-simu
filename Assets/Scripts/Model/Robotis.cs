using UnityEngine;
using System.Collections;

namespace Model
{
	public class RobotisBase : Motor
	{
		public float Speed = 0;

		private float _angle = 0;

		private float _speed;

		protected float _rpm;

		/*
		protected override void Apply()
		{
			float
				s = this.Speed,
				diff = (this.Angle - this._angle);
			if (Mathf.Abs(diff) > 0.1f)
			{

				if (this.Speed == 0)
					s = 2048f;

				this._speed = 360 * (this._rpm * s) / 60f;
				s = this._speed * Time.fixedDeltaTime;

				if (diff > 0)
				{
					if ((this._angle + s) > this.Angle)
						this._angle = this.Angle;
					else
						this._angle += s;
				}
				else if (diff < 0)
				{
					if ((this._angle - s) < this.Angle)
						this._angle = this.Angle;
					else
						this._angle -= s;
				}
			}
			this._horn.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, (this._angle * (this.Inverted ? -1 : 1))));
		}
		*/
	}
}

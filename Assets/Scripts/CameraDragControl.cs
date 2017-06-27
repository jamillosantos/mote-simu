using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDragControl : MonoBehaviour
{
	public Transform Target;

	public float Speed = 100f;

	public float MouseWheelFactor = 3f;

	private Camera _camera;

	public float distance = 5.0f;

	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;

	private float x = 0.0f;
	private float y = 0.0f;


	void Start()
	{
		this._camera = this.GetComponent<Camera> ();
		this.distance = (this.transform.position - this.Target.position).magnitude;
		this.x = transform.eulerAngles.y;
		this.y = transform.eulerAngles.x;
	}

	void LateUpdate ()
	{
		float mwheel = Input.GetAxis("Mouse ScrollWheel");
		this.distance += mwheel * this.MouseWheelFactor * (Input.GetKey(KeyCode.LeftShift) ? 2f : 1f);

		if (Target && Input.GetMouseButton (0)) {
			x += Input.GetAxis ("Mouse X") * this.Speed * distance * 0.02f;
			y -= Input.GetAxis ("Mouse Y") * this.Speed * 0.02f;

			y = ClampAngle (y, yMinLimit, yMaxLimit);
		}

		Quaternion rotation = Quaternion.Euler(y, x, 0);

		Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
		Vector3 position = rotation * negDistance + Target.position;

		this.transform.rotation = rotation;
		this.transform.position = position;
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}

}

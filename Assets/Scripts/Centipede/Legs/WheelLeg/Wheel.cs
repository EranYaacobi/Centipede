using System;
using UnityEngine;
using System.Collections;

public class Wheel : MonoBehaviour
{
	// TODO: This is a bad code with lots of comments, that you can you to create a wheel that actually uses
	// a wheel collider (which I think is much better).
	//
	// A wheel built from a wheel collider, will consist of:
	// * Wheel leg - like any other legs.
	//      * Wheel Pivot - Contains a rigid body (restrainted in rotation also in the z axis), and this script.
	//			* Wheel collider host - An empty game object that simply contains the wheel collider (no rigid body).
	//			* Wheel graphics - A game object that contains the wheel's mesh (and nothing else).
	// 
	// The WheelTransform public variable should be assigned with the Wheel graphics, and the WheelCollider with
	// the collider.

	/// <summary>
	/// The graphics of the wheel.
	/// </summary>
	public Transform WheelTransform;

	/// <summary>
	/// The physics of the wheel.
	/// This includes not only to wheel (and as such, not a part of it), but the wheel's pivot.
	/// </summary>
	public WheelCollider WheelCollider;

	/// <summary>
	/// The velocity of the wheel.
	/// </summary>
	private Vector3 WheelVelocity;

	/// <summary>
	/// The amount of ground speed that the wheel covered.
	/// Used to calculate by how much the wheel's graphic should be rotated.
	/// </summary>
	private Vector3 GroundSpeed;

	/// <summary>
	/// The radius of the wheel.
	/// TODO: This shouldn't be constant, but calculated from the collider.
	/// </summary>
	private const Single WheelRadius = 0.2F;

	/// <summary>
	/// The suspension range of the wheel, meaning the maximum distance that the wheel can be at from
	/// its pivot. Higher values means that the wheel can be further from the pivot, causing the centipede's
	/// body more vibrating.
	/// 
	/// TODO: This shouldn't be a constant, but retrieved from the wheel collider (which contains both the distance,
	/// the spring that tries to minimize (?) the distance, and the damping on the string).
	/// </summary>
	private const Single SuspensionRange = 0.1F;

	/// <summary>
	/// A drag multiplier. Used to defined drag in all axes.
	/// TODO: Redundant.
	/// </summary>
	public Vector3 DragMultiplier = new Vector3(1,0,0);

	/// <summary>
	/// The engine force. It seems a bit high here, though I don't know why it has to be so high - 
	/// maybe it has something to do with the definitions in the WheelCollider.
	/// 
	/// TODO: This should be a constant.
	/// </summary>
	private const Single EngineForce = 500F;

	/// <summary>
	/// Indicates whether the body can actually drive, meaning that the wheel is touching the ground.
	/// </summary>
	public Boolean CanDrive;

	/// <summary>
	/// The throttle applied by the engine.
	/// </summary>
	public Single Throttle;

	/// <summary>
	/// The relative velocity of the wheel (this doesn't have to be a public field, but can be a function's
	/// variable\parameter).
	/// </summary>
	public Vector3 RelativeVelocity;

	/// <summary>
	/// The wheel's friction curve. This needed to be adjusted manually to fit the surface on which the wheel is
	/// on, if we wan't to make the wheel adjust its friction based on the surface it's driving on. The reason for
	/// that is, the WheelCollider doesn't follow normal physics in PhysX engine. Nevertheless, it shouldn't be too
	/// complicated (and may be unnecessary, if we don't created surfaces with different friction values).
	/// </summary>
	private WheelFrictionCurve WheelFrictionCurve;

	// Use this for initialization
	private void Start()
	{
		CanDrive = true;
	}

	// Update is called once per frame
	private void Update()
	{
		UpdateWheelGraphics();
	}

	private void FixedUpdate()
	{
		RelativeVelocity = transform.InverseTransformDirection(rigidbody.velocity);

		CalculateState();

		//UpdateDrag();
		//UpdateFriction();

		ApplyThrottle();
	}

	private void UpdateWheelGraphics()
	{
		WheelHit WheelHit;

		// First we get the velocity at the point where the wheel meets the ground, if the wheel is touching the ground
		if (WheelCollider.GetGroundHit(out WheelHit))
		{
			// This line causes the wheel to be at the correct hieght from the ground (though it assumes that the ground is more or less horizontal).
			// TODO: Cause the function to use to rotation of the ground!
			WheelTransform.localPosition = WheelCollider.transform.right * (WheelRadius + WheelCollider.transform.InverseTransformPoint(WheelHit.point).y);


			WheelVelocity = rigidbody.GetPointVelocity(WheelHit.point);
			GroundSpeed = transform.InverseTransformDirection(WheelVelocity);
		}
		else
		{
			// If the wheel is not touching the ground we set the position of the wheel graphics to
			// the wheel's transform position + the range of the suspension.
			WheelTransform.position = WheelCollider.transform.position - WheelCollider.transform.up*SuspensionRange;

			WheelVelocity *= 0.9F*(1 - Throttle);
		}

		// Rotate the wheel based on the distance it passed during the frame.
		WheelTransform.Rotate(Vector3.back * (GroundSpeed.x / WheelRadius) * Time.deltaTime * Mathf.Rad2Deg);
	}

	private void CalculateState()
	{
		CanDrive = WheelCollider.isGrounded;
	}

	// May be necessary, in order to maintain a top speed (though friction may be sufficient).
	private void UpdateDrag()
	{
		var RelativeDrag = new Vector3(-RelativeVelocity.x*Mathf.Abs(RelativeVelocity.x),
		                               -RelativeVelocity.y*Mathf.Abs(RelativeVelocity.y),
		                               -RelativeVelocity.z*Mathf.Abs(RelativeVelocity.z));

		var Drag = Vector3.Scale(DragMultiplier, RelativeDrag);

		rigidbody.AddForce(transform.TransformDirection(Drag)*rigidbody.mass*Time.deltaTime);
	}

	private void ApplyThrottle()
	{
		if (CanDrive)
		{
			Single ThrottleForce = 0;
			//Single BrakeForce = 0;

			Throttle = Input.GetAxis("Horizontal");

			// Currently brake force and throttle force are equal. If we don't want them to be
			// uncomment this code.
			/*if (Mathf.Sign(RelativeVelocity.x) == Mathf.Sign(Throttle))
				ThrottleForce = Mathf.Sign(Throttle)*EngineForce*rigidbody.mass;
			else
				BrakeForce = Mathf.Sign(Throttle)*EngineForce*rigidbody.mass;*/

			ThrottleForce = Mathf.Sign(Throttle) * EngineForce * rigidbody.mass;

			rigidbody.AddForce(transform.right*Time.deltaTime*(ThrottleForce/* + BrakeForce*/));
		}
	}

	// Not necessarily needed (the usage here is irrelevant, yet we want to change the friction based on the ground's physics material
	private void UpdateFriction()
	{
		var SquareVelocity = RelativeVelocity.x * RelativeVelocity.x;
	
		// Add extra sideways friction based on the car's turning velocity to avoid slipping
		WheelFrictionCurve.extremumValue = Mathf.Clamp(300 - SquareVelocity, 0, 300);
		WheelFrictionCurve.asymptoteValue = Mathf.Clamp(150 - (SquareVelocity / 2), 0, 150);
		
		//WheelCollider.sidewaysFriction = WheelFrictionCurve;
		WheelCollider.forwardFriction = WheelFrictionCurve;
	}
}
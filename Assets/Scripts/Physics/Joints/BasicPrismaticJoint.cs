using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// A prismatic joint, which isn't really prismatic.
/// It supports movement along an axis, while retaining the ability for the axis to rotate (hence not prismatic).
/// </summary>
public class BasicPrismaticJoint : MonoBehaviour
{
	/// <summary>
	/// The body connected to the game object via the prismatic joint.
	/// </summary>
	public Rigidbody ConnectedBody;

	/// <summary>
	/// The anchor of the prismatic joint on the game object.
	/// </summary>
	public Vector3 Anchor;

	/// <summary>
	/// The anchor of the prismatic joint on the other game object.
	/// </summary>
	public Vector3 RemoteAnchor;

	/// <summary>
	/// The flexibility of the joint. This is the maximum difference between the current length of the joint,
	/// to the desired angle, after which maximum force is applied.
	/// </summary>
	public Single Flexibility;

	/// <summary>
	/// The force constant, which is used as a scalar when applying force.
	/// </summary>
	public Single ForceConsant;

	/// <summary>
	/// The maxmimum force of the motor.
	/// </summary>
	public Single MaxMotorForce;

	/// <summary>
	/// The speed of the motor.
	/// </summary>
	public Single MotorSpeed;

	/// <summary>
	/// The basic length of the joint.
	/// </summary>
	public Single InitialLength;

	/// <summary>
	/// The lower limit of the joint, relative to the initial length.
	/// </summary>
	public Single LowerLimit;

	/// <summary>
	/// The lower limit of the joint, relative to the initial length.
	/// </summary>
	public Single UpperLimit;

	/// <summary>
	/// TODO: Remove
	/// </summary>
	public Single CurrentLength;

	/// <summary>
	/// Indicates whether the script should override base length to be the current distance between the objects,
	/// and to update it when the anchors change.
	/// </summary>
	public Boolean FixedDefaultBaseLength;

	/// <summary>
	/// The state of the motor.
	/// </summary>
	public MotorState State;

	/// <summary>
	/// The desired length of the prismatic joint.
	/// </summary>
	private Single DesiredLength;

	/// <summary>
	/// The initial transform of the joint.
	/// This is used to adjust the length, when the anchors change.
	/// </summary>
	public Vector3 InitialTranform;

	/// <summary>
	/// The damping of the motor.
	/// </summary>
	[Range(0, 1)]
	public Single Damping;

	/// <summary>
	/// The velocity in the previous cycle.
	/// </summary>
	private Vector3 LastVelocity;

	public void Initialize(Rigidbody ConnectedBody, Vector3 Anchor, Vector3 RemoteAnchor, Single Flexibility, Single ForceConsant, Single MaxMotorForce, Single MotorSpeed, Single LowerLimit, Single UpperLimit, Single InitialLength = 0)
	{
		InitialTranform = transform.parent.localPosition + transform.localPosition;

		this.ConnectedBody = ConnectedBody;
		this.Anchor = Anchor;
		this.RemoteAnchor = RemoteAnchor;
		this.Flexibility = Flexibility;
		this.ForceConsant = ForceConsant;
		this.MaxMotorForce = MaxMotorForce;
		this.MotorSpeed = MotorSpeed;
		this.LowerLimit = LowerLimit;
		this.UpperLimit = UpperLimit;

		if (InitialLength == 0)
		{
			this.InitialLength = (RemoteAnchor - transform.parent.localPosition - transform.localPosition - Anchor).magnitude;
			this.FixedDefaultBaseLength = true;
		}
		else
		{
			this.InitialLength = InitialLength;
			this.FixedDefaultBaseLength = false;
		}
		this.State = MotorState.Stopped;
		this.DesiredLength = this.InitialLength;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (FixedDefaultBaseLength)
		{
			var NewInitialLength = (RemoteAnchor - InitialTranform - Anchor).magnitude;
			if (InitialLength != NewInitialLength)
			{
				InitialLength = NewInitialLength;
				DesiredLength = NewInitialLength;
			}
		}

		var Position = transform.TransformPoint(Anchor);
		var OtherPosition = ConnectedBody.transform.TransformPoint(RemoteAnchor);
		CurrentLength = (OtherPosition - Position).magnitude;

		DesiredLength = DesiredLength + ((Int32)State) * MotorSpeed * Time.deltaTime;
		DesiredLength = Mathf.Clamp(DesiredLength, InitialLength + LowerLimit, InitialLength + UpperLimit);

		var DeltaLength = DesiredLength - CurrentLength;
		var JointDirection = (OtherPosition - Position).normalized;

		// Getting the force that should be applied to make the joint reach its desired length.
		var DeltaLengthForce = -ForceConsant * MaxMotorForce * (Mathf.Sqrt(Mathf.Abs(DeltaLength)) * Mathf.Sign(DeltaLength) / Mathf.Sqrt(Flexibility)) * JointDirection;

		// Getting the projection of the current velocity, in order to apply a force that will counter it.
		var VelocityForce = -(1-Damping) * Vector3.Dot(rigidbody.velocity, JointDirection) * JointDirection;

		// Getting the projection of forces that were applied on the object previous cycle, in order to counter
		// them if they are not leading to the desired length.
		var ExternalForcesCounterForce = Vector3.zero;
		var ExternalForcesProjection = Vector3.Dot(rigidbody.velocity - LastVelocity, JointDirection);
		if (ExternalForcesProjection < 0)
			ExternalForcesCounterForce = -(ExternalForcesProjection / Time.deltaTime) * JointDirection;
		LastVelocity = rigidbody.velocity;

		//rigidbody.velocity *= (1 - Damping);

		var AppliedForce = DeltaLengthForce + VelocityForce + ExternalForcesCounterForce;
		if (AppliedForce.magnitude > (MaxMotorForce))
			AppliedForce = AppliedForce.normalized;

		rigidbody.AddForce(AppliedForce, ForceMode.Force);
		ConnectedBody.rigidbody.AddForce(-AppliedForce, ForceMode.Force);
	}

	public enum MotorState
	{
		Stopped = 0,
		Backward = -1,
		Forward = 1
	}

	void OnDrawGizmos()
	{
		const Single LimitLineRadius = 0.05F;
		const Single SphereRadius = 0.05F;
		const Single SmallSphereRadius = 0.035F;
		const Single Depth = -5F;
		var Position = transform.TransformPoint(Anchor) + new Vector3(0, 0, Depth);
		var OtherPosition = ConnectedBody.transform.TransformPoint(RemoteAnchor) + new Vector3(0, 0, Depth);
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(OtherPosition + (Position - OtherPosition).normalized * DesiredLength, LimitLineRadius);
		Gizmos.color = Color.black;
		Gizmos.DrawSphere(Position, SmallSphereRadius);
		Gizmos.DrawSphere(OtherPosition, SphereRadius);
		
		var LimitsDirection = (OtherPosition - Position).normalized;
		var LimitsNormal = new Vector3(LimitsDirection.y, -LimitsDirection.x, LimitsDirection.z).normalized;

		var LowerLimitPosition = OtherPosition - LimitsDirection * (InitialLength + LowerLimit);
		var UpperLimitPosition = OtherPosition - LimitsDirection * (InitialLength + UpperLimit);
		/*Gizmos.color = Color.red;
		Gizmos.DrawLine(OtherPosition, LowerLimitPosition);*/
		var Range = (UpperLimitPosition - LowerLimitPosition).magnitude;
		var OutOfRangeRate = Mathf.Max(
			Math.Max(0.01F, (Position - LowerLimitPosition).magnitude) / Range,
			Math.Max(0.01F, (UpperLimitPosition - Position).magnitude) / Range);
		OutOfRangeRate = Mathf.Max(0, (OutOfRangeRate - 0.5F));
		Gizmos.color = Color.Lerp(Color.cyan, Color.red, OutOfRangeRate);
		Gizmos.DrawLine(LowerLimitPosition, Position);
		Gizmos.DrawLine(Position, UpperLimitPosition);
		/*Gizmos.color = Color.red;
		Gizmos.DrawLine(UpperLimitPosition, UpperLimitPosition + Position - OtherPosition);*/

		Gizmos.color = Color.white;
		Gizmos.DrawLine(OtherPosition, LowerLimitPosition);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(UpperLimitPosition - LimitsNormal * LimitLineRadius,
					   UpperLimitPosition + LimitsNormal * LimitLineRadius);
		Gizmos.color = Color.red;
		Gizmos.DrawLine(LowerLimitPosition - LimitsNormal * LimitLineRadius,
					   LowerLimitPosition + LimitsNormal * LimitLineRadius);
	}
}

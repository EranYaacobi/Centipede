using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class NetworkRigidbody : Photon.MonoBehaviour
{
	/// <summary>
	/// The normal value of timeScale.
	/// </summary>
	private static readonly Single NormalTimeScale = Time.timeScale;

	/// <summary>
	/// Overrides the interpolation times of all NetworkRigidBodies.
	/// TODO: Remove this eventually!
	/// </summary>
	private const Single DefaultInterpolationTime = 0.1F;
	
	private const Single SimulationLerpRate = 8F;
	private const Int32 StoredStates = 20;

	/// <summary>
	/// Indicates by how much to delay the display in order to allow interpolation.
	/// </summary>
	public Single InterpolationBackTime = 0.1F;

	/// <summary>
	/// Indicates the limit of extrapolation (in seconds) when no more data is received.
	/// </summary>
	public Single ExtrapolationLimit = 0.5F;

	/// <summary>
	/// The maximum distance between the current position and the received position, before snapping
	/// is performed.
	/// </summary>
	public Single SnapDistance = 0.5F;

	/// <summary>
	/// The stored states, with which we determine the position of the rigid-body.
	/// </summary>
	private readonly RigidBodyState[] BufferedStates = new RigidBodyState[StoredStates];

	/// <summary>
	/// The amount of slots used in BufferedStates.
	/// </summary>
	private Int32 UsedBufferedStatesCount;

	private void Start()
	{
		InterpolationBackTime = DefaultInterpolationTime;
		if (PhotonNetwork.isMasterClient)
			enabled = false;
	}

	/// <summary>
	/// Clear the buffered positions.
	/// This should be used when "teleporting" the game object.
	/// </summary>
	public void ClearBuffers()
	{
		UsedBufferedStatesCount = 0;
	}
	
	private void OnPhotonSerializeView(PhotonStream Stream, PhotonMessageInfo Info)
	{
		// Send data to server
		if (Stream.isWriting)
		{
			// Not sending data when in slow-motion or fast-motion.
			if (Time.timeScale != NormalTimeScale)
				return;

			var Position = rigidbody.position;
			var Rotation = rigidbody.rotation;
			var Velocity = rigidbody.velocity;
			var AngularVelocity = rigidbody.angularVelocity;

			Stream.Serialize(ref Position);
			Stream.Serialize(ref Velocity);
			Stream.Serialize(ref Rotation);
			Stream.Serialize(ref AngularVelocity);
		}
		// Read data from remote client
		else
		{
			var Position = Vector3.zero;
			var Velocity = Vector3.zero;
			var Rotation = Quaternion.identity;
			var AngularVelocity = Vector3.zero;

			Stream.Serialize(ref Position);
			Stream.Serialize(ref Velocity);
			Stream.Serialize(ref Rotation);
			Stream.Serialize(ref AngularVelocity);

			// Shift the buffer sideways, deleting the last state.
			for (var i = BufferedStates.Length - 1; i >= 1; i--)
				BufferedStates[i] = BufferedStates[i - 1];

			// Record current state in slot 0
			BufferedStates[0] = new RigidBodyState(Info.timestamp, Position, Velocity, Rotation, AngularVelocity);

			// Update used slot count, however never exceed the buffer size
			// Slots aren't actually freed so this just makes sure the buffer is
			// filled up and that uninitalized slots aren't used.
			UsedBufferedStatesCount = Mathf.Min(UsedBufferedStatesCount + 1, BufferedStates.Length);

			// Check if states are in order, if it is inconsistent you could reshuffel or
			// drop the out-of-order state. Nothing is done here
			for (var i = 0; i < UsedBufferedStatesCount - 1; i++)
			{
				if (BufferedStates[i].Timestamp < BufferedStates[i + 1].Timestamp)
					Debug.Log("State inconsistent");
			}
		}
	}

	// We have a window of interpolationBackTime where we basically play
	// By having interpolationBackTime the average ping, you will usually use interpolation.
	// And only if no more data arrives we will use extra polation
	private void Update()
	{
		// Not updating the RigidBody if it is not enabled (the check is not necessarily accurate, but rigidbody
		// doesn't have an enabled property.
		if ((rigidbody.isKinematic) && (!rigidbody.detectCollisions))
			return;

		// Not using data when in slow-motion or fast-motion.
		if (Time.timeScale != NormalTimeScale)
			return;

		// This is the target playback time of the rigid body
		var InterpolationTime = PhotonNetwork.time - InterpolationBackTime;

		// Smoothing
		// Use interpolation if the target playback time is present in the buffer
		if (BufferedStates[0].Timestamp > InterpolationTime)
		{
			// Go through buffer and find correct state to play back
			for (var i = 0; i < UsedBufferedStatesCount; i++)
			{
				if (BufferedStates[i].Timestamp <= InterpolationTime || i == UsedBufferedStatesCount - 1)
				{
					// The best playback state (closest to 100 ms old (default time))
					var BestPlaybackState = BufferedStates[i];

					// The state one slot newer (<100ms) than the best playback state
					var AfterBestPlayerbackState = BufferedStates[Mathf.Max(i - 1, 0)];

					// Use the time between the two slots to determine if interpolation is necessary
					var Length = AfterBestPlayerbackState.Timestamp - BestPlaybackState.Timestamp;

					// As the time difference gets closer to 100ms, LerpRate gets closer to 1 in which case
					// AfterBestPlayerbackState is only used.
					var LerpRate = 0.0F;
					if (Length > 0)
						LerpRate = (Single) ((InterpolationTime - BestPlaybackState.Timestamp)/Length);

					var PlaybackPosition = Vector3.Lerp(BestPlaybackState.Position, AfterBestPlayerbackState.Position, LerpRate);
					var PlaybackRotation = Quaternion.Slerp(BestPlaybackState.Rotation, AfterBestPlayerbackState.Rotation, LerpRate);
					var PlaybackVelocity = Vector3.Lerp(BestPlaybackState.Velocity, AfterBestPlayerbackState.Velocity, LerpRate);
					var PlaybackAngularVelocity = Vector3.Lerp(BestPlaybackState.AngularVelocity,
					                                           AfterBestPlayerbackState.AngularVelocity, LerpRate);

					UpdateRigidBody(PlaybackPosition, PlaybackRotation, PlaybackVelocity, PlaybackAngularVelocity);
					break;
				}
			}
		}
			// Use extrapolation (Prediction)
		else
		{
			var NewestState = BufferedStates[0];
			var ExtrapolationLength = (float) (InterpolationTime - NewestState.Timestamp);
			// Don't extrapolation for more than 500 ms, you would need to do that carefully
			if (ExtrapolationLength < ExtrapolationLimit)
			{
				var AxisLength = ExtrapolationLength*NewestState.AngularVelocity.magnitude*Mathf.Rad2Deg;
				var AngularRotation = Quaternion.AngleAxis(AxisLength, NewestState.AngularVelocity);
				UpdateRigidBody(NewestState.Position + NewestState.Velocity*ExtrapolationLength,
				                AngularRotation*NewestState.Rotation,
				                NewestState.Velocity,
				                NewestState.AngularVelocity);
			}
		}
	}

	private void UpdateRigidBody(Vector3 Position, Quaternion Rotation, Vector3 Velocity, Vector3 AngularVelocity)
	{
		if (Vector3.Distance(transform.position, Position) < SnapDistance)
		{
			transform.position = Vector3.Lerp(transform.position, Position, SimulationLerpRate * Time.deltaTime);
			transform.rotation = Quaternion.Slerp(transform.rotation, Rotation, SimulationLerpRate * Time.deltaTime);
			if (!rigidbody.isKinematic)
			{
				rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, Velocity, SimulationLerpRate * Time.deltaTime);
				rigidbody.angularVelocity = Vector3.Lerp(rigidbody.angularVelocity, AngularVelocity, SimulationLerpRate * Time.deltaTime);
			}
		}
		else
		{
			// Snapping to location.
			Debug.Log("Snapped!");
			transform.position = Position;
			transform.rotation = Rotation;
			if (!rigidbody.isKinematic)
			{
				rigidbody.velocity = Velocity;
				rigidbody.angularVelocity = AngularVelocity;
			}
		}
	}

	/// <summary>
	/// A state received over the network.
	/// </summary>
	private struct RigidBodyState
	{
		/// <summary>
		/// The time-stamp of the state.
		/// </summary>
		public readonly Double Timestamp;

		/// <summary>
		/// The position of the rigid-body in the state.
		/// </summary>
		public readonly Vector3 Position;

		/// <summary>
		/// The velocity of the rigid-body in the state.
		/// </summary>
		public readonly Vector3 Velocity;

		/// <summary>
		/// The Rotation of the rigid-body in the state.
		/// </summary>
		public readonly Quaternion Rotation;

		/// <summary>
		/// The angular velocity of the rigid-body in the state.
		/// </summary>
		public readonly Vector3 AngularVelocity;

		public RigidBodyState(Double Timestamp, Vector3 Position, Vector3 Velocity, Quaternion Rotation, Vector3 AngularVelocity)
		{
			this.Timestamp = Timestamp;
			this.Position = Position;
			this.Velocity = Velocity;
			this.Rotation = Rotation;
			this.AngularVelocity = AngularVelocity;
		}
	}
}
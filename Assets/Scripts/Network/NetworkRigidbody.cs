using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class NetworkRigidbody : Photon.MonoBehaviour
{
	private const Single DefaultInterpolationTime = 0.1F;
	private const Single SimulationLerpRate = 2F;
	private const Int32 StoredStates = 20;

	public Single InterpolationBackTime = 0.1F;
	public Single ExtrapolationLimit = 0.5F;

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
		if (photonView.owner == PhotonNetwork.player)
			enabled = false;
	}
	
	private void OnPhotonSerializeView(PhotonStream Stream, PhotonMessageInfo Info)
	{
		// Send data to server
		if (Stream.isWriting)
		{
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
						LerpRate = (Single)((InterpolationTime - BestPlaybackState.Timestamp) / Length);

					var PlaybackPosition = Vector3.Lerp(BestPlaybackState.Position, AfterBestPlayerbackState.Position, LerpRate);
					var PlaybackRotation = Quaternion.Slerp(BestPlaybackState.Rotation, AfterBestPlayerbackState.Rotation, LerpRate);
					var PlaybackVelocity = Vector3.Lerp(BestPlaybackState.Velocity, AfterBestPlayerbackState.Velocity, LerpRate);
					var PlaybackAngularVelocity = Vector3.Lerp(BestPlaybackState.AngularVelocity, AfterBestPlayerbackState.AngularVelocity, LerpRate);

					if (Vector3.Distance(transform.position, PlaybackPosition) < 1)
					{
						transform.position = Vector3.Lerp(transform.position, PlaybackPosition, SimulationLerpRate * Time.deltaTime);
						transform.rotation = Quaternion.Slerp(transform.rotation, PlaybackRotation, SimulationLerpRate * Time.deltaTime);
						rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, PlaybackVelocity, SimulationLerpRate * Time.deltaTime);
						rigidbody.angularVelocity = Vector3.Lerp(rigidbody.angularVelocity, PlaybackAngularVelocity, SimulationLerpRate * Time.deltaTime);
					}
					else
					{
						// Snapping to location.
						Debug.Log("Snapped!");
						transform.position = PlaybackPosition;
						transform.rotation = PlaybackRotation;
						rigidbody.velocity = PlaybackVelocity;
						rigidbody.angularVelocity = PlaybackAngularVelocity;
					}
					break;
				}
			}
		}
		// Use extrapolation (Prediction)
		else
		{
			var NewestState = BufferedStates[0];
			var ExtrapolationLength = (float)(InterpolationTime - NewestState.Timestamp);
			// Don't extrapolation for more than 500 ms, you would need to do that carefully
			if (ExtrapolationLength < ExtrapolationLimit)
			{
				var AxisLength = ExtrapolationLength * NewestState.AngularVelocity.magnitude * Mathf.Rad2Deg;
				var AngularRotation = Quaternion.AngleAxis(AxisLength, NewestState.AngularVelocity);
				rigidbody.position = Vector3.Lerp(rigidbody.position, NewestState.Position + NewestState.Velocity * ExtrapolationLength, SimulationLerpRate * Time.deltaTime);
				rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, AngularRotation * NewestState.Rotation, SimulationLerpRate * Time.deltaTime);
				rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, NewestState.Velocity, SimulationLerpRate * Time.deltaTime);
				rigidbody.angularVelocity = Vector3.Lerp(rigidbody.angularVelocity, NewestState.AngularVelocity, SimulationLerpRate * Time.deltaTime);
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
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BreakJoints : Photon.MonoBehaviour
{
	/// <summary>
	/// A list of all the joints monitored for breaking.
	/// </summary>
	private List<MonitoredJoint> MonitoredJoints;

	/// <summary>
	/// The breaking distance of the joints.
	/// If the joints' length stretch beyond this limit, they will brake.
	/// </summary>
	public Single BreakDistance;

	private void Awake()
	{
		MonitoredJoints = new List<MonitoredJoint>();
	}

	/// <summary>
	/// Monitors the given joint for breaking.
	/// </summary>
	/// <param name="Joint"></param>
	public void MonitorJoint(Joint Joint)
	{
		MonitoredJoints.Add(new MonitoredJoint(Joint));
	}

	private void FixedUpdate()
	{
		if (!PhotonNetwork.isMasterClient)
			return;

		for (int Index = 0; Index < MonitoredJoints.ToArray().Length; Index++)
		{
			var MonitoredJoint = MonitoredJoints[Index];

			if (MonitoredJoint.Joint != null)
			{
				var Distance = MonitoredJoint.Length() - MonitoredJoint.InitialLength;

				if (Distance > 0.12F)
					Debug.Log(Distance);

				if (BreakDistance < Distance)
					photonView.RPC("BreakJoint", PhotonTargets.AllBuffered, Index);
			}
		}
	}

	[RPC]
	private void BreakJoint(Int32 JointIndex)
	{
		var MonitoredJoint = MonitoredJoints[JointIndex];
		DestroyObject(MonitoredJoint.Joint);
	}

	private struct MonitoredJoint
	{
		/// <summary>
		/// The joint being monitored.
		/// </summary>
		public readonly Joint Joint;

		/// <summary>
		/// The initial length of the Joint.
		/// </summary>
		public readonly Single InitialLength;

		public MonitoredJoint(Joint Joint)
			: this()
		{
			this.Joint = Joint;
			InitialLength = Length();
		}

		public Single Length()
		{
			return Vector3.Distance(Joint.gameObject.transform.TransformPoint(Joint.anchor), Joint.connectedBody.transform.position);
		}
	}
}
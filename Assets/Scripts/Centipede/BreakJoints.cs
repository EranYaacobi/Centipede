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

	/// <summary>
	/// The time scale after a link is broken.
	/// </summary>
	public Single BreakingTimeScale;

	/// <summary>
	/// The duration of the breaking link slow-motion.
	/// </summary>
	public Single BreakingTimeDuration;

	/// <summary>
	/// The normal value of timeScale.
	/// </summary>
	private Single NormalTimeScale;

	/// <summary>
	/// The normal value of fixedDeltaTime.
	/// </summary>
	private Single NormalFixedDeltaTime;

	private void Awake()
	{
		MonitoredJoints = new List<MonitoredJoint>();

		NormalTimeScale = Time.timeScale;
		NormalFixedDeltaTime = Time.fixedDeltaTime;
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
		Debug.Log("A joint was broken!");
		var MonitoredJoint = MonitoredJoints[JointIndex];
		DestroyObject(MonitoredJoint.Joint);
		StartCoroutine(SlowTime());
	}

	private IEnumerator SlowTime()
	{
		Time.timeScale = NormalTimeScale * BreakingTimeScale;
		Time.fixedDeltaTime = NormalFixedDeltaTime * BreakingTimeScale;

		yield return new WaitForSeconds(BreakingTimeDuration * BreakingTimeScale);

		Time.timeScale = NormalTimeScale;
		Time.fixedDeltaTime = NormalFixedDeltaTime;
	}

	private struct MonitoredJoint
	{
		/// <summary>
		/// The joint being monitored.
		/// </summary>
		public Joint Joint;

		/// <summary>
		/// The initial length of the Joint.
		/// </summary>
		public Single InitialLength;

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
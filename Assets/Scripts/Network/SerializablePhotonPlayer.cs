using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class SerializablePhotonPlayer : IObjectReference
{
	private Int32 ID;

	protected SerializablePhotonPlayer(SerializationInfo info, StreamingContext context)
    {
		ID = info.GetInt32("ID");
    }

	// GetRealObject is called after this object is deserialized. 
	public object GetRealObject(StreamingContext Context)
	{
		if (PhotonNetwork.networkingPeer.mActors.ContainsKey(ID))
			return PhotonNetwork.networkingPeer.mActors[ID];
		return null;
	}
}
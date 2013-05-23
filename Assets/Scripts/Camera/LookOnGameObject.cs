using UnityEngine;
using System.Collections;

public class LookOnGameObject : MonoBehaviour
{
	/// <summary>
	/// The gameobject on which the camera should look.
	/// </summary>
	public GameObject GameObject;

	// Update is called once per frame
	void LateUpdate()
	{
		if (GameObject != null)
		{
			transform.position = GameObject.transform.position + Vector3.forward*-10;
		}
	}
}

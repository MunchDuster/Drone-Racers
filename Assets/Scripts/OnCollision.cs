using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnCollision : MonoBehaviour
{
	public UnityEvent onCollision;

	private void OnTriggerEnter(Collider collider)
	{
		onCollision.Invoke();
	}

    // OnCollisionEnter is called when a collider on this gameobject collides with another
	private void OnCollisionEnter(Collision collision)
	{
		onCollision.Invoke();
	}
}

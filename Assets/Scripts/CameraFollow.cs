using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public Transform target;

	public float lerpSpeed = 10;
	public float rotLerpSpeed = 10;

	// LateUpdate is called at the end of each frame
	void LateUpdate()
	{
		transform.position = Vector3.Lerp(transform.position, target.position, lerpSpeed * Time.deltaTime);
		transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, rotLerpSpeed * Time.deltaTime);
	}
}

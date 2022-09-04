using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Drone : MonoBehaviour
{
	[Header("Settings")]
	public LayerMask layerMask;

	[Header("Refs")]
	public Rigidbody rb;
	public DroneEngine[] engines;

	[Header("Events")]
	public UnityEvent onMoveStart;
	public UnityEvent onMoveStop;

	private Vector2 look, move = Vector2.zero;
	private bool boostPressed = false;

	// Start is called before the first frame update
	private void Start()
	{
		for(int i = 0; i < engines.Length; i++)
		{
			engines[i].rb = rb;
			engines[i].layerMask = layerMask;
		}
	}

	// Update is called once per frame
	void Update()
	{
		//Update input
		look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		boostPressed = Input.GetKey(KeyCode.Space);

		//Check if started or stopped throttle
		if (Input.GetKeyDown(KeyCode.Space))
		{
			onMoveStart.Invoke();
		}
		else if (Input.GetKeyUp(KeyCode.Space))
		{
			onMoveStop.Invoke();
		}

		foreach (DroneEngine engine in engines)
		{
			engine.move = move;
		}
	}
}

using UnityEngine;

[CreateAssetMenu(fileName = "Thruster", menuName = "ScriptableObjects/ThrusterSettings", order = 1)]

public class ThrusterSettings : ScriptableObject
{
	public bool isClamped = true;
	public float maxAngle = 45;
	public float minAngle = -45;
	public float turnSpeed = 180;
	public float damping = 10;
	public float thrust;

	public float maxTorque = 100;

}
using UnityEngine;

[CreateAssetMenu(fileName = "DroneEngineSettings", menuName = "Settings/Drone Engine", order = 1)]

public class DroneEngineSettings : ScriptableObject
{
	[Header("Motor settings")]
	public float maxRPM = 10000;
	public float motorTorque = 100;
	public float liftMultiplier = 1;
	public float mass = 3;
	public float radius = 0.5f;
	public float stallAngle = 20;
	public AnimationCurve airDensity;
	public AnimationCurve angleLift;


	[Header("Target force settings")]
	public float forwardForce;
	public float turnForce;

	[Space(10)]
	public float springForce;
	public float damping;

	[Space(10)]
	public float balancingForce = 1;
	public float antiSpinForce = 1;

	[Space(10)]
	public float targetDist;
	public float maxRaycastDist;
}
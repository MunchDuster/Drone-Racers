
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class DroneEngine : MonoBehaviour
{
	[Header("References")]
	public Slider slider;
	public DroneEngineSettings settings;
	public UnityEvent<float> onUpdateForce;

	public float forwardForceMultiplier = 1;
	public float sideForceMultiplier = 1;


	[HideInInspector] public Vector2 move;
	[HideInInspector] public LayerMask layerMask;
	[HideInInspector] public Rigidbody rb;

	private float targetForce;

	private float targetRPM;
	private float currentRPM;
	private float maxDeltaRPM;
	private float deltaRPM;

	// Awake is called when the gameObject is activated
	private void Awake()
	{
		//using F = ma
		maxDeltaRPM = settings.motorTorque / settings.mass;
	}

	private float CalculateAntiGravForce()
	{
		float angle = Vector3.Angle(-Vector3.up, -transform.up);
		float cosAngle = Mathf.Cos(angle * Mathf.Deg2Rad);
		float clampedCos = Mathf.Sign(cosAngle) * Mathf.Clamp(Mathf.Abs(cosAngle) , 1 * Mathf.Deg2Rad, 90 * Mathf.Deg2Rad);
		float antiGravForce = Physics.gravity.magnitude / clampedCos; //Trig

		return antiGravForce;
	}
	///ISSUE: pull, drone may not be facing up, should pass in direction
	private float CalculateSuspensionForce(float distance)
	{
		float relativeVelocity = rb.GetRelativePointVelocity(transform.localPosition).y;
		float pull = Mathf.Max(settings.targetDist - distance, 0);//Suspension can't pull down (for jumps)
		float damping = relativeVelocity * settings.damping;
		float suspensionForce = pull * settings.springForce - damping;

		return suspensionForce;
	}
	private float CalculateMoveForce()
	{
		float moveForce = move.y * settings.forwardForce * forwardForceMultiplier;
		return moveForce;
	}
	private float CalculateTurnForce()
	{
		float turnForce = move.x * settings.turnForce * sideForceMultiplier;
		return turnForce;
	}
	private float CalculateBalanceForce()
	{
		Vector3 pointVelocity = rb.GetPointVelocity(transform.position);
		float antiSpinForce = transform.InverseTransformDirection(pointVelocity).y * settings.antiSpinForce;
		
		Vector3 directionToCenter = transform.position - (rb.centerOfMass + rb.transform.position);
		float balancingForce = -directionToCenter.y * settings.balancingForce;
		
		return balancingForce - antiSpinForce;
	}

	public void UpdateTargetForce()
	{
		float totalforce = 0;

		if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, settings.maxRaycastDist, layerMask))
		{
			float antiGravForce = CalculateAntiGravForce();
			float suspensionForce = CalculateSuspensionForce(hit.distance);
			float moveForce = CalculateMoveForce();
			float turnForce = CalculateTurnForce();
			float balanceForce = 0;//CalculateBalanceForce(rb);

			totalforce = antiGravForce + suspensionForce + moveForce + balanceForce;
				
			Debug.DrawRay(transform.position, transform.up * totalforce, Color.red);
		}
		else
		{
			float balanceForce = CalculateBalanceForce();

			totalforce = balanceForce;

			Debug.DrawRay(transform.position, transform.up * totalforce, Color.green);
		}

		targetForce = totalforce;

		onUpdateForce.Invoke(totalforce);
	}

	private void FixedUpdate()
	{
		UpdateTargetForce();
		UpdateRPM();
		UpdateForce();
	}

	public bool debug = false;
	public float angleOffset = 5;
	private void UpdateForce()
	{
		//source: https://www.grc.nasa.gov/www/k-12/airplane/lifteq.html
		//q [dynamic pressure] = density x velocity x velocity / 2
		//L [lift] = constant x C1 x q x area
		//C1 [lift coefficient] = curve like this: 
		//https://upload.wikimedia.org/wikipedia/commons/thumb/d/d1/Lift_curve.svg/300px-Lift_curve.svg.png

		Vector3 from = transform.forward;
		Vector3 to = rb.velocity;
		Vector3 axis = transform.right;
		float angleOfAttack = Vector3.SignedAngle(from, to, axis) + angleOffset; //-180 to 180
		float angleOfAttack360 = angleOfAttack < 0 ? angleOfAttack + 180 : angleOfAttack;
		if(debug) debugAngleCurve.AddKey(Time.timeSinceLevelLoad, angleOfAttack360 / 360f);
		float liftCoefficient = settings.angleLift.Evaluate(angleOfAttack360 / 360f);

		float velocity = currentRPM * 60 * settings.radius;//x60 M to S  , xR (going around circle)
		float dynamicPressure = settings.airDensity.Evaluate(transform.position.y / 100f) * velocity * velocity / 2;
		
		float lift = settings.liftMultiplier * liftCoefficient * dynamicPressure;
		if(debug) Debug.Log(settings.liftMultiplier + " x " + liftCoefficient + " x " + dynamicPressure);
	
		rb.AddForceAtPosition(lift * transform.up, transform.position, ForceMode.Impulse);
	}

	public AnimationCurve debugAngleCurve;

	private void UpdateRPM()
	{
		//update currentRPM using currentRPM via lift equation
		Vector3 from = transform.forward;
		Vector3 to = rb.velocity;
		Vector3 axis = transform.right;
		float angleOfAttack = Vector3.SignedAngle(from, to, axis);
		float liftCoefficient = settings.angleLift.Evaluate(angleOfAttack / 180);

		float lift = targetForce;

		//rearrange for dynamicPressure
		//lift = settings.liftMultiplier * liftCoefficient * dynamicPressure;
		//dynamicPressure = lift / (settings.liftMultiplier * liftCoefficient)
		float dynamicPressure  = lift / (settings.liftMultiplier * liftCoefficient);

		//rearrange for velocity
		//dynamicPressure = airDensity.Evaluate(transform.y) * velocity * velocity / 2;
		//dynamicPressure * 2 / airDensity.Evaluate(transform.y) = velocity * velocity
		//velocity = sqrt(dynamicPressure * 2 / airDensity.Evaluate(transform.y))
		float velocity = Mathf.Sqrt(dynamicPressure * 2 / settings.airDensity.Evaluate(transform.position.y));

		//rearrange for currentRPM (which is targetRPM)
		//velocity = currentRPM * 60 * settings.radius;
		//currentRPM = velocity / (60 * settings.radius);
		float targetRPM = velocity / (60 * settings.radius);

		//update currentRPM

		//F = am -> a = F/m  (maxDelta is 'a')
		float acceleration = UpdateValue(ref currentRPM, targetRPM, maxDeltaRPM, settings.maxRPM);
		
		//Apply reaction torque to drone by motor
		//used for controlling y-axis rotation
		Vector3 torque = transform.right * settings.motorTorque * (acceleration / maxDeltaRPM);
		rb.AddTorque(torque);

		//debug slider
		slider.value = Mathf.Abs(currentRPM) / settings.maxRPM;
	}

	private float UpdateValue(ref float value, float target, float maxDelta, float maxValue)
	{
		float oldValue = value;
		float offset = targetRPM - value;
		float delta = Mathf.Min(Mathf.Sign(offset) * maxDelta, offset);
		value = Mathf.Clamp(delta, -maxValue, maxValue);

		return value - oldValue;			
	}
}
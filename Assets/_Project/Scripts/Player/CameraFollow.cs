using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public float FollowSpeed = 2f;
	public Transform Target;

	public float shakeDuration = 0f;

	public float shakeAmount = 0.1f;
	public float decreaseFactor = 1.0f;

	Vector3 targetPosition;

	private void Start()
	{
		FindTarget();
		if (Target != null)
			transform.position = new Vector3(Target.position.x, Target.position.y, -10f);
	}

	private void FindTarget()
	{
		if (Target != null) return;
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		if (player != null) Target = player.transform;
	}

	private void LateUpdate()
	{
		if (Target == null)
		{
			FindTarget();
			if (Target == null) return;
		}
		Vector3 newPosition = Target.position;
		newPosition.z = -10f;
		targetPosition = Vector3.Lerp(transform.position, newPosition, Mathf.Clamp01(FollowSpeed * Time.deltaTime));

		if (shakeDuration > 0)
		{
			Vector2 offset = Random.insideUnitCircle * shakeAmount;
			targetPosition += new Vector3(offset.x, offset.y, 0f);
			shakeDuration -= Time.deltaTime * decreaseFactor;
		}
		transform.position = targetPosition;
	}

	public void ShakeCamera()
	{
		shakeDuration = 0.2f;
	}
}

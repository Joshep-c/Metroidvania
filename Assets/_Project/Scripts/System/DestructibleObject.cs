using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
	public float life = 3;

	private float shakeDuration = 0f;
	private float shakeMagnitude = 0.25f;
	private float dampingSpeed = 1f;

	Vector3 initialPosition;

	void Awake()
	{
		initialPosition = transform.position;
	}

	void Start()
    {
        
    }

    void Update()
    {
		if (life <= 0)
		{
			Destroy(gameObject);
		}
		else if (shakeDuration > 0)
		{
			transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;

			shakeDuration -= Time.deltaTime * dampingSpeed;
		}
		else
		{
			shakeDuration = 0f;
			transform.localPosition = initialPosition;
		}
	}

	public void ApplyDamage(float damage)
	{
		life -= 1;
		shakeDuration = 0.1f;
	}
}

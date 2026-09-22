using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableWeapon : MonoBehaviour
{
	public Vector2 direction;
	public bool hasHit = false;
	public float speed = 10f;

	void Start()
	{

	}

	void FixedUpdate()
	{
		if (!hasHit)
			GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		Debug.Log("COLISION DETECTADA con: " + collision.gameObject.name + " | Tag: " + collision.gameObject.tag);

		if (collision.gameObject.CompareTag("Enemy"))
		{
			bool didDamage = false;

			Ally ally = collision.gameObject.GetComponentInParent<Ally>();
			if (ally != null)
			{
				ally.ApplyDamage(Mathf.Sign(direction.x) * 2f);
				didDamage = true;
			}

			Met_Enemy metEnemy = collision.gameObject.GetComponentInParent<Met_Enemy>();
			if (metEnemy != null)
			{
				metEnemy.ApplyDamage(Mathf.Sign(direction.x) * 2f);
				didDamage = true;
			}

			if (!didDamage)
				Debug.LogWarning(collision.gameObject.name + " tiene tag Enemy pero no tiene Ally ni Met_Enemy. Revisa el prefab.");

			Destroy(gameObject);
		}
		else if (!collision.gameObject.CompareTag("Player"))
		{
			Destroy(gameObject);
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableProjectile : MonoBehaviour
{
	public Vector2 direction;
	public bool hasHit = false;
	public float speed = 15f;
	public GameObject owner;
	private Rigidbody2D rb;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		if (renderer != null)
		{
			renderer.sprite = CombatVisuals.Projectile;
			renderer.color = new Color(1f, 0.5f, 0.2f);
			renderer.sortingOrder = 10;
		}
		Destroy(gameObject, 5f);
	}

    void FixedUpdate()
    {
		if (!hasHit)
			rb.linearVelocity = direction.normalized * speed;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (owner != null && collision.transform.root == owner.transform.root) return;
		if (collision.gameObject.CompareTag("Player"))
		{
			Met_CharacterController2D player = collision.gameObject.GetComponent<Met_CharacterController2D>();
			if (player != null) player.ApplyDamage(2f, transform.position);
			Destroy(gameObject);
		}
		else if (collision.gameObject.CompareTag("Enemy"))
		{
			Ally ally = collision.gameObject.GetComponentInParent<Ally>();
			Met_Enemy patrol = collision.gameObject.GetComponentInParent<Met_Enemy>();
			float damage = Mathf.Sign(direction.x) * 2f;
			if (ally != null) ally.ApplyDamage(damage);
			else if (patrol != null) patrol.ApplyDamage(damage);
			Destroy(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
}


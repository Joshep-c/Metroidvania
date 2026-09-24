using UnityEngine;

public class ThrowableWeapon : MonoBehaviour
{
	public Vector2 direction;
	public bool hasHit = false;
	public float speed = 10f;
	private Rigidbody2D rb;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		if (renderer != null)
		{
			renderer.sprite = CombatVisuals.Projectile;
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
		if (collision.gameObject.CompareTag("Enemy"))
		{
			Ally ally = collision.gameObject.GetComponentInParent<Ally>();
			if (ally != null)
			{
				ally.ApplyDamage(Mathf.Sign(direction.x) * 2f);
			}

			Met_Enemy metEnemy = collision.gameObject.GetComponentInParent<Met_Enemy>();
			if (metEnemy != null)
			{
				metEnemy.ApplyDamage(Mathf.Sign(direction.x) * 2f);
			}

			Destroy(gameObject);
		}
		else if (!collision.gameObject.CompareTag("Player"))
		{
			Destroy(gameObject);
		}
	}
}

using UnityEngine;
using System.Collections;

public class Met_Enemy : MonoBehaviour
{

	[Header("Detección de patrulla")]
	[Tooltip("Punto al frente-abajo para detectar el borde de la plataforma")]
	public Transform fallCheck;
	[Tooltip("Punto al frente para detectar paredes/obstáculos")]
	public Transform wallCheck;
	[Tooltip("Layer del suelo/plataformas (usado por FallCheck). Asígnalo en el Inspector.")]
	public LayerMask groundLayer;
	[Tooltip("Layer de paredes/obstáculos que hacen girar al enemigo (usado por WallCheck)")]
	public LayerMask turnLayerMask;
	public float checkRadius = 0.2f;

	[Header("Movimiento")]
	public float speed = 5f;
	private bool facingRight = true;
	private Rigidbody2D rb;

	[Header("Vida y daño")]
	public float life = 10;
	public bool isInvincible = false;
	private bool isHitted = false;

	[Header("Daño por contacto")]
	public float contactDamage = 2f;
	public float contactDamageCooldown = 0.5f;
	private bool canDamagePlayer = true;

	private bool isPlat;
	private bool isObstacle;

	void Awake()
	{
		GameManager.Instance.RegisterEnemy();

		if (fallCheck == null)
		{
			Transform t = transform.Find("FallCheck");
			if (t != null) fallCheck = t;
			else Debug.LogWarning(gameObject.name + ": no se encontró un hijo llamado 'FallCheck'.");
		}
		if (wallCheck == null)
		{
			Transform t = transform.Find("WallCheck");
			if (t != null) wallCheck = t;
			else Debug.LogWarning(gameObject.name + ": no se encontró un hijo llamado 'WallCheck'.");
		}
		rb = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate()
	{

		if (life <= 0)
		{
			transform.GetComponent<Animator>().SetBool("IsDead", true);
			StartCoroutine(DestroyEnemy());
			return;
		}

		if (fallCheck == null || wallCheck == null) return;

		isPlat = Physics2D.OverlapCircle(fallCheck.position, checkRadius, groundLayer);
		isObstacle = Physics2D.OverlapCircle(wallCheck.position, checkRadius, turnLayerMask);

		if (!isHitted && Mathf.Abs(rb.linearVelocity.y) < 0.5f)
		{
			if (isPlat && !isObstacle)
			{
				rb.linearVelocity = new Vector2(facingRight ? -speed : speed, rb.linearVelocity.y);
			}
			else
			{
				Flip();
			}
		}
	}

	void Flip()
	{
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void ApplyDamage(float damage)
	{
		if (!isInvincible)
		{
			float direction = damage / Mathf.Abs(damage);
			damage = Mathf.Abs(damage);
			transform.GetComponent<Animator>().SetBool("Hit", true);
			life -= damage;
			rb.linearVelocity = Vector2.zero;
			rb.AddForce(new Vector2(direction * 100f, 100f));
			StartCoroutine(HitTime());
		}
	}

	void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Player") && life > 0 && canDamagePlayer)
		{
			Met_CharacterController2D player = collision.gameObject.GetComponent<Met_CharacterController2D>();
			if (player != null)
			{
				player.ApplyDamage(contactDamage, transform.position);
				StartCoroutine(ContactDamageCooldown());
			}
		}
	}

	IEnumerator ContactDamageCooldown()
	{
		canDamagePlayer = false;
		yield return new WaitForSeconds(contactDamageCooldown);
		canDamagePlayer = true;
	}

	IEnumerator HitTime()
	{
		isHitted = true;
		isInvincible = true;
		yield return new WaitForSeconds(0.1f);
		isHitted = false;
		isInvincible = false;
	}

	IEnumerator DestroyEnemy()
	{
		CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
		capsule.size = new Vector2(1f, 0.25f);
		capsule.offset = new Vector2(0f, -0.8f);
		capsule.direction = CapsuleDirection2D.Horizontal;
		yield return new WaitForSeconds(0.25f);
		rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
		yield return new WaitForSeconds(3f);

		GameManager.Instance.EnemyDefeated();

		Destroy(gameObject);
	}

	void OnDrawGizmosSelected()
	{
		if (fallCheck != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(fallCheck.position, checkRadius);
		}
		if (wallCheck != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(wallCheck.position, checkRadius);
		}
	}
}
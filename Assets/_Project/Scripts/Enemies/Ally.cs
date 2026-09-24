using System.Collections;
using UnityEngine;

public class Ally : MonoBehaviour
{
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;

	[Header("Vida y daño")]
	public float life = 10;
	public bool isInvincible = false;
	private bool isHitted = false;

	[Header("Movimiento")]
	public float speed = 5f;
	[SerializeField] private float m_DashForce = 25f;
	private bool isDashing = false;

	[Header("Detección y persecución")]
	[Tooltip("El jugador. Se asigna automáticamente por tag si se deja vacío.")]
	public GameObject enemy;
	[Tooltip("Radio de visión y alcance: los obstáculos del suelo bloquean la vista")]
	public float rangeDist = 5f;
	public float meleeDist = 1.5f;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private float edgeCheckRadius = 0.45f;
	private Transform fallCheck;
	private float distToPlayer;
	private float distToPlayerY;

	[Header("Ataque")]
	public float dmgValue = 4;
	private bool canAttack = true;
	private Transform attackCheck;
	public float attackCheckRadius = 1.3f;
	public GameObject throwableObject;
	[SerializeField] private float rangedAttackCooldown = 0.8f;

	private bool doOnceDecision = true;
	private float nextRangedAttackTime;
	private Collider2D playerCollider;
	private Animator anim;
	private bool dying;
	private bool registered;

	void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		m_FacingRight = transform.localScale.x > 0f;
		fallCheck = transform.Find("FallCheck");

		Transform t = transform.Find("AttackCheck");
		if (t != null)
			attackCheck = t;
		else
			Debug.LogWarning(gameObject.name + ": no se encontró un hijo llamado 'AttackCheck'.");
	}

	void Start()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.RegisterEnemy();
			registered = true;
		}
	}

	void FixedUpdate()
	{
		if (life <= 0)
		{
			if (!dying)
			{
				dying = true;
				StartCoroutine(DestroyEnemy());
			}
			return;
		}

		if (enemy == null)
		{
			enemy = GameObject.FindGameObjectWithTag("Player");
			playerCollider = null;
			if (enemy == null) return;
		}

		if (isDashing)
		{
			m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_DashForce, 0);
		}
		else if (!isHitted)
		{
			distToPlayer = enemy.transform.position.x - transform.position.x;
			distToPlayerY = enemy.transform.position.y - transform.position.y;
			if (!CanSeePlayer())
			{
				Idle();
				return;
			}

			if (Mathf.Abs(distToPlayer) > 0.05f && (distToPlayer > 0f) != m_FacingRight)
				Flip();

			if (Mathf.Abs(distToPlayer) <= meleeDist && Mathf.Abs(distToPlayerY) < 1.2f)
			{
				m_Rigidbody2D.linearVelocity = new Vector2(0f, m_Rigidbody2D.linearVelocity.y);
				anim.SetBool("IsWaiting", false);
				if (canAttack)
					MeleeAttack();
			}
			else
			{
				// Dispara también desde el borde: perseguir no es requisito para atacar.
				RangeAttack();
				if (Mathf.Abs(distToPlayerY) < 2.5f && Mathf.Abs(distToPlayer) > meleeDist && GroundAhead())
				{
					anim.SetBool("IsWaiting", false);
					m_Rigidbody2D.linearVelocity = new Vector2(Mathf.Sign(distToPlayer) * speed, m_Rigidbody2D.linearVelocity.y);
				}
				else Idle();
			}
		}
		else return;
	}

	private Vector2 PlayerCenter()
	{
		if (playerCollider == null) playerCollider = enemy.GetComponent<Collider2D>();
		return playerCollider != null ? (Vector2)playerCollider.bounds.center : (Vector2)enemy.transform.position;
	}

	private bool CanSeePlayer()
	{
		if (enemy == null || !enemy.activeInHierarchy) return false;
		Met_CharacterController2D player = enemy.GetComponent<Met_CharacterController2D>();
		if (player != null && player.life <= 0f) return false;

		Vector2 origin = transform.position;
		Vector2 target = PlayerCenter();
		return (target - origin).sqrMagnitude <= rangeDist * rangeDist &&
			Physics2D.Linecast(origin, target, groundLayer).collider == null;
	}

	void Flip()
	{
		m_FacingRight = !m_FacingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void ApplyDamage(float damage)
	{
		if (!isInvincible && life > 0f && !Mathf.Approximately(damage, 0f))
		{
			float direction = damage / Mathf.Abs(damage);
			damage = Mathf.Abs(damage);
			anim.SetBool("Hit", true);
			life -= damage;
			m_Rigidbody2D.linearVelocity = Vector2.zero;
			m_Rigidbody2D.AddForce(new Vector2(direction * 100f, 100f));
			StartCoroutine(HitTime());
			if (life > 0f)
			{
				if ((direction > 0f) != m_FacingRight) Flip();
				StartCoroutine(Dash());
			}
		}
	}

	public void MeleeAttack()
	{
		if (!canAttack || life <= 0f) return;
		canAttack = false;
		StartCoroutine(WaitToAttack(0.5f));
		anim.SetBool("Attack", true);

		if (attackCheck == null) return;

		Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, attackCheckRadius);
		for (int i = 0; i < collidersEnemies.Length; i++)
		{
			GameObject hitObj = collidersEnemies[i].gameObject;
			if (hitObj.transform.root == transform.root) continue;

			if (hitObj.CompareTag("Player"))
			{
				Met_CharacterController2D player = hitObj.GetComponent<Met_CharacterController2D>();
				if (player != null)
					player.ApplyDamage(dmgValue, transform.position);
			}
		}
	}

	private bool GroundAhead()
	{
		return fallCheck == null || Physics2D.OverlapCircle(fallCheck.position, edgeCheckRadius, groundLayer) != null;
	}

	public void RangeAttack()
	{
		if (Time.time >= nextRangedAttackTime && throwableObject != null && life > 0f && CanSeePlayer())
		{
			nextRangedAttackTime = Time.time + rangedAttackCooldown;
			Vector3 origin = transform.position + new Vector3(Mathf.Sign(transform.localScale.x) * 0.9f, -0.2f);
			GameObject throwableProj = Instantiate(throwableObject, origin, Quaternion.identity);
			ThrowableProjectile proj = throwableProj.GetComponent<ThrowableProjectile>();
			if (proj != null)
			{
				proj.owner = gameObject;
				Vector2 aim = PlayerCenter() - (Vector2)origin;
				proj.direction = aim.sqrMagnitude > 0.01f ? aim.normalized : new Vector2(Mathf.Sign(transform.localScale.x), 0f);
				Collider2D ownCollider = GetComponent<Collider2D>();
				Collider2D projectileCollider = throwableProj.GetComponent<Collider2D>();
				if (ownCollider != null && projectileCollider != null)
					Physics2D.IgnoreCollision(ownCollider, projectileCollider);
			}
			anim.SetBool("Attack", true);
		}
	}

	public void Run()
	{
		if (!GroundAhead())
		{
			Idle();
			return;
		}
		anim.SetBool("IsWaiting", false);
		if (Mathf.Abs(distToPlayer) > 0.0001f)
			m_Rigidbody2D.linearVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * speed, m_Rigidbody2D.linearVelocity.y);
		if (doOnceDecision)
			StartCoroutine(NextDecision(0.5f));
	}

	public void Jump()
	{
		if (Mathf.Abs(distToPlayer) > 0.0001f)
		{
			Vector3 targetVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * speed, m_Rigidbody2D.linearVelocity.y);
			Vector3 velocity = Vector3.zero;
			m_Rigidbody2D.linearVelocity = Vector3.SmoothDamp(m_Rigidbody2D.linearVelocity, targetVelocity, ref velocity, 0.05f);
		}
		if (doOnceDecision)
		{
			anim.SetBool("IsWaiting", false);
			m_Rigidbody2D.AddForce(new Vector2(0f, 850f));
			StartCoroutine(NextDecision(1f));
		}
	}

	public void Idle()
	{
		m_Rigidbody2D.linearVelocity = new Vector2(0f, m_Rigidbody2D.linearVelocity.y);
		anim.SetBool("IsWaiting", true);
	}

	IEnumerator HitTime()
	{
		isInvincible = true;
		isHitted = true;
		yield return new WaitForSeconds(0.1f);
		isHitted = false;
		isInvincible = false;
	}

	IEnumerator WaitToAttack(float time)
	{
		yield return new WaitForSeconds(time);
		canAttack = true;
	}

	IEnumerator Dash()
	{
		anim.SetBool("IsDashing", true);
		isDashing = true;
		yield return new WaitForSeconds(0.1f);
		isDashing = false;
		anim.SetBool("IsDashing", false);
	}

	IEnumerator NextDecision(float time)
	{
		doOnceDecision = false;
		yield return new WaitForSeconds(time);
		doOnceDecision = true;
		anim.SetBool("IsWaiting", false);
	}

	IEnumerator DestroyEnemy()
	{
		CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
		if (capsule != null) capsule.enabled = false;
		m_Rigidbody2D.linearVelocity = Vector2.zero;
		anim.SetBool("IsDead", true);
		yield return new WaitForSeconds(1f);

		if (registered && GameManager.Instance != null)
			GameManager.Instance.EnemyDefeated();

		Destroy(gameObject);
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
		Gizmos.DrawWireSphere(transform.position, rangeDist);
		Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
		Gizmos.DrawWireSphere(transform.position, meleeDist);
		if (fallCheck == null) fallCheck = transform.Find("FallCheck");
		if (fallCheck != null)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(fallCheck.position, edgeCheckRadius);
		}
	}
}

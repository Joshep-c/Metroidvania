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
	[Tooltip("Radio de persecución: si el jugador está más lejos que esto, el slime se queda quieto")]
	public float rangeDist = 5f;
	public float meleeDist = 1.5f;
	private float distToPlayer;
	private float distToPlayerY;

	[Header("Ataque")]
	public float dmgValue = 4;
	private bool canAttack = true;
	private Transform attackCheck;
	public float attackCheckRadius = 0.9f;
	public GameObject throwableObject;

	private bool doOnceDecision = true;
	private Animator anim;

	void Awake()
	{
		GameManager.Instance.RegisterEnemy();

		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();

		Transform t = transform.Find("AttackCheck");
		if (t != null)
			attackCheck = t;
		else
			Debug.LogWarning(gameObject.name + ": no se encontró un hijo llamado 'AttackCheck'.");
	}

	void FixedUpdate()
	{
		if (life <= 0)
		{
			StartCoroutine(DestroyEnemy());
			return;
		}

		if (enemy == null)
		{
			enemy = GameObject.FindGameObjectWithTag("Player");
			return;
		}

		if (isDashing)
		{
			m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_DashForce, 0);
		}
		else if (!isHitted)
		{
			distToPlayer = enemy.transform.position.x - transform.position.x;
			distToPlayerY = enemy.transform.position.y - transform.position.y;

			if (Mathf.Abs(distToPlayer) < 0.25f)
			{
				m_Rigidbody2D.linearVelocity = new Vector2(0f, m_Rigidbody2D.linearVelocity.y);
				anim.SetBool("IsWaiting", true);
			}
			else if (Mathf.Abs(distToPlayer) > 0.25f && Mathf.Abs(distToPlayer) < meleeDist && Mathf.Abs(distToPlayerY) < 2f)
			{
				m_Rigidbody2D.linearVelocity = new Vector2(0f, m_Rigidbody2D.linearVelocity.y);
				if ((distToPlayer > 0f && transform.localScale.x < 0f) || (distToPlayer < 0f && transform.localScale.x > 0f))
					Flip();
				if (canAttack)
					MeleeAttack();
			}
			else if (Mathf.Abs(distToPlayer) > meleeDist && Mathf.Abs(distToPlayer) < rangeDist)
			{
				anim.SetBool("IsWaiting", false);
				m_Rigidbody2D.linearVelocity = new Vector2(distToPlayer / Mathf.Abs(distToPlayer) * speed, m_Rigidbody2D.linearVelocity.y);
			}
			else
			{
				Idle();
			}
		}
		else if (isHitted)
		{
			if ((distToPlayer > 0f && transform.localScale.x > 0f) || (distToPlayer < 0f && transform.localScale.x < 0f))
				Flip();
			StartCoroutine(Dash());
		}

		if (transform.localScale.x * m_Rigidbody2D.linearVelocity.x > 0 && !m_FacingRight && life > 0)
		{
			Flip();
		}
		else if (transform.localScale.x * m_Rigidbody2D.linearVelocity.x < 0 && m_FacingRight && life > 0)
		{
			Flip();
		}
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
		if (!isInvincible)
		{
			float direction = damage / Mathf.Abs(damage);
			damage = Mathf.Abs(damage);
			anim.SetBool("Hit", true);
			life -= damage;
			m_Rigidbody2D.linearVelocity = Vector2.zero;
			m_Rigidbody2D.AddForce(new Vector2(direction * 100f, 100f));
			StartCoroutine(HitTime());
		}
	}

	public void MeleeAttack()
	{
		anim.SetBool("Attack", true);

		if (attackCheck == null) return;

		Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, attackCheckRadius);
		for (int i = 0; i < collidersEnemies.Length; i++)
		{
			GameObject hitObj = collidersEnemies[i].gameObject;
			if (hitObj == gameObject) continue;

			if (hitObj.CompareTag("Enemy"))
			{
				float dmg = transform.localScale.x < 0 ? -Mathf.Abs(dmgValue) : Mathf.Abs(dmgValue);

				Ally allyTarget = hitObj.GetComponentInParent<Ally>();
				if (allyTarget != null)
					allyTarget.ApplyDamage(dmg);

				Met_Enemy metTarget = hitObj.GetComponentInParent<Met_Enemy>();
				if (metTarget != null)
					metTarget.ApplyDamage(dmg);
			}
			else if (hitObj.CompareTag("Player"))
			{
				Met_CharacterController2D player = hitObj.GetComponent<Met_CharacterController2D>();
				if (player != null)
					player.ApplyDamage(2f, transform.position);
			}
		}
		StartCoroutine(WaitToAttack(0.5f));
	}

	public void RangeAttack()
	{
		if (doOnceDecision && throwableObject != null)
		{
			GameObject throwableProj = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f, -0.2f), Quaternion.identity);
			ThrowableProjectile proj = throwableProj.GetComponent<ThrowableProjectile>();
			if (proj != null)
			{
				proj.owner = gameObject;
				proj.direction = new Vector2(transform.localScale.x, 0f);
			}
			StartCoroutine(NextDecision(0.5f));
		}
	}

	public void Run()
	{
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
		if (doOnceDecision)
		{
			anim.SetBool("IsWaiting", true);
			StartCoroutine(NextDecision(1f));
		}
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
		canAttack = false;
		yield return new WaitForSeconds(time);
		canAttack = true;
	}

	IEnumerator Dash()
	{
		anim.SetBool("IsDashing", true);
		isDashing = true;
		yield return new WaitForSeconds(0.1f);
		isDashing = false;
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
		capsule.size = new Vector2(1f, 0.25f);
		capsule.offset = new Vector2(0f, -0.8f);
		capsule.direction = CapsuleDirection2D.Horizontal;
		anim.SetBool("IsDead", true);
		yield return new WaitForSeconds(0.25f);
		m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
		yield return new WaitForSeconds(1f);

		GameManager.Instance.EnemyDefeated();

		Destroy(gameObject);
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
		Gizmos.DrawWireSphere(transform.position, rangeDist);
		Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
		Gizmos.DrawWireSphere(transform.position, meleeDist);
	}
}
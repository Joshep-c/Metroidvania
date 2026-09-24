using System.Collections;
using UnityEngine;

public class Attack : MonoBehaviour
{
	public float dmgValue = 4;
	public GameObject throwableObject;
	public Transform attackCheck;
	public Animator animator;
	public bool canAttack = true;
	public bool isTimeToCheck = false;

	public GameObject cam;
	[SerializeField] private float attackRadius = 1.4f;
	[SerializeField] private float throwCooldown = 0.4f;
	private float nextThrowTime;
	private SpriteRenderer slashRenderer;
	private PlayerMovement playerInput;

	private void Awake()
	{
		if (animator == null) animator = GetComponent<Animator>();
		playerInput = GetComponent<PlayerMovement>();
		if (attackCheck == null) attackCheck = transform.Find("AttackCheck");
		if (attackCheck != null)
		{
			GameObject effect = new GameObject("AttackSlash");
			effect.transform.SetParent(attackCheck, false);
			effect.transform.localPosition = Vector3.zero;
			slashRenderer = effect.AddComponent<SpriteRenderer>();
			slashRenderer.sprite = CombatVisuals.Slash;
			SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();
			if (playerRenderer != null)
			{
				slashRenderer.sortingLayerID = playerRenderer.sortingLayerID;
				slashRenderer.sortingOrder = playerRenderer.sortingOrder + 2;
			}
			slashRenderer.enabled = false;
		}
	}

	void Update()
	{
		if (playerInput != null && playerInput.AttackPressedThisFrame && canAttack)
		{
			canAttack = false;
			animator.SetBool("IsAttacking", true);
			if (slashRenderer != null) StartCoroutine(ShowSlash());
			StartCoroutine(AttackCooldown());
		}

		if (playerInput != null && playerInput.ThrowPressedThisFrame && throwableObject != null && Time.time >= nextThrowTime)
		{
			nextThrowTime = Time.time + throwCooldown;
			GameObject throwableWeapon = Instantiate(throwableObject, transform.position + new Vector3(Mathf.Sign(transform.localScale.x) * 0.9f, -0.25f), Quaternion.identity);
			ThrowableWeapon weapon = throwableWeapon.GetComponent<ThrowableWeapon>();
			if (weapon != null) weapon.direction = new Vector2(Mathf.Sign(transform.localScale.x), 0f);
			Collider2D playerCollider = GetComponent<Collider2D>();
			Collider2D weaponCollider = throwableWeapon.GetComponent<Collider2D>();
			if (playerCollider != null && weaponCollider != null)
				Physics2D.IgnoreCollision(playerCollider, weaponCollider);
			throwableWeapon.name = "ThrowableWeapon";
		}
	}

	private IEnumerator ShowSlash()
	{
		slashRenderer.enabled = true;
		float elapsed = 0f;
		const float duration = 0.25f;
		while (elapsed < duration)
		{
			float progress = elapsed / duration;
			slashRenderer.transform.localScale = Vector3.one * Mathf.Lerp(1.8f, 2.8f, progress);
			slashRenderer.color = new Color(1f, 1f, 1f, 1f - progress);
			elapsed += Time.deltaTime;
			yield return null;
		}
		slashRenderer.enabled = false;
	}

	private void OnDisable()
	{
		if (slashRenderer != null) slashRenderer.enabled = false;
	}

	IEnumerator AttackCooldown()
	{
		yield return new WaitForSeconds(0.3f);
		canAttack = true;
	}

	public void DoDashDamage()
	{
		if (attackCheck == null) return;
		Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, attackRadius);
		for (int i = 0; i < collidersEnemies.Length; i++)
		{
			Collider2D hit = collidersEnemies[i];
			if (hit.CompareTag("Enemy"))
			{
				float damage = Mathf.Sign(hit.transform.position.x - transform.position.x) * Mathf.Abs(dmgValue);
				Ally ally = hit.GetComponentInParent<Ally>();
				Met_Enemy patrol = hit.GetComponentInParent<Met_Enemy>();
				if (ally != null) ally.ApplyDamage(damage);
				else if (patrol != null) patrol.ApplyDamage(damage);
				CameraFollow follow = cam != null ? cam.GetComponent<CameraFollow>() : Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
				if (follow != null) follow.ShakeCamera();
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (attackCheck == null) return;
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(attackCheck.position, attackRadius);
	}
}

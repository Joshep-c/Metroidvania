using UnityEngine;

[CreateAssetMenu(fileName = "SO_EnemyStats_Patrol", menuName = "Project/Enemy Stats Patrol")]
public class SO_EnemyStats_Patrol : ScriptableObject
{
    [Header("Health")]
    public float maxLife = 10f;

    [Header("Movement")]
    public float speed = 5f;

    [Header("Combat")]
    public float contactDamage = 2f;
    public float knockbackForceX = 500f;
    public float knockbackForceY = 100f;

    [Header("Timing")]
    public float hitStunDuration = 0.1f;
    public float deathFlattenDelay = 0.25f;
    public float deathDestroyDelay = 3f;
}

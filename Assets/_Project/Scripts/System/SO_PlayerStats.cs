using UnityEngine;

[CreateAssetMenu(fileName = "SO_PlayerStats", menuName = "Project/Player Stats")]
public class SO_PlayerStats : ScriptableObject
{
    [Header("Movement")]
    public float runSpeed = 40f;
    public float movementSmoothing = 0.05f;
    public bool airControl = false;

    [Header("Jump")]
    public float jumpForce = 400f;
    public bool canDoubleJump = true;

    [Header("Dash")]
    public float dashForce = 25f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 0.5f;

    [Header("Health")]
    public float maxLife = 10f;
    public float invincibilityDuration = 1f;
    public float stunDuration = 0.25f;

    [Header("Wall")]
    public float wallJumpForceMultiplier = 1.2f;
    public float doubleJumpForceMultiplier = 0.833f; // 1/1.2

    [Header("Fall")]
    public float limitFallSpeed = 25f;
}

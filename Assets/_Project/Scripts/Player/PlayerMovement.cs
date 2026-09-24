using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    public Met_CharacterController2D controller;
    public Animator animator;
    public float runSpeed = 40f;

    private InputActionMap playerActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction attackAction;
    private InputAction throwAction;
    private float horizontalMove;
    private bool jump;
    private bool dash;

    public bool AttackPressedThisFrame => attackAction != null && attackAction.WasPressedThisFrame();
    public bool ThrowPressedThisFrame => throwAction != null && throwAction.WasPressedThisFrame();
    public InputActionAsset InputActions => inputActions;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<Met_CharacterController2D>();
        if (animator == null) animator = GetComponent<Animator>();

        if (inputActions != null)
            playerActions = inputActions.FindActionMap("Player", false);
        if (playerActions == null)
        {
            Debug.LogError("El jugador necesita el mapa Player del Input System.", this);
            enabled = false;
            return;
        }

        moveAction = playerActions.FindAction("Move", true);
        jumpAction = playerActions.FindAction("Jump", true);
        dashAction = playerActions.FindAction("Dash", true);
        attackAction = playerActions.FindAction("Attack", true);
        throwAction = playerActions.FindAction("Throw", true);
    }

    private void OnEnable()
    {
        if (playerActions != null) playerActions.Enable();
    }

    private void OnDisable()
    {
        if (playerActions != null) playerActions.Disable();
        horizontalMove = 0f;
        jump = false;
        dash = false;
    }

    private void Update()
    {
        float direction = moveAction.ReadValue<Vector2>().x;
        // El mando tiene zona muerta en el asset; este umbral evita pequeñas derivas residuales.
        if (Mathf.Abs(direction) < 0.2f) direction = 0f;
        horizontalMove = direction * runSpeed;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (jumpAction.WasPressedThisFrame()) jump = true;
        if (dashAction.WasPressedThisFrame()) dash = true;
    }

    public void OnFall()
    {
        animator.SetBool("IsJumping", true);
    }

    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
    }

    private void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
        jump = false;
        dash = false;
    }
}

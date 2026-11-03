using UnityEngine;
using CreakwoodManor.Controls;

namespace CreakwoodManor.Players
{
    /// <summary>
    /// Side-view platformer movement for the human (the kid).
    /// Walks, jumps, and feels forgiving: coyote time lets you jump just after
    /// leaving a ledge, the jump buffer catches presses slightly before landing,
    /// and letting go of jump early gives a shorter hop.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class HumanController : MonoBehaviour
    {
        [Header("Walking")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 60f;
        [SerializeField] private float deceleration = 70f;

        [Header("Jumping")]
        [SerializeField] private float jumpVelocity = 10f;
        [Tooltip("How much upward speed is kept when jump is released early.")]
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.1f;

        [Header("Ground Check")]
        [Tooltip("Empty child object placed at the human's feet.")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.1f);
        [SerializeField] private LayerMask groundLayers;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D body;
        private IPlayerInput input;

        private float moveInput;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private bool isJumping;

        public bool IsGrounded { get; private set; }

        /// <summary>Lets other systems (cutscenes, room resets) freeze the human.</summary>
        public bool CanMove { get; set; } = true;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.freezeRotation = true;

            input = GetComponent<IPlayerInput>();
            if (input == null)
            {
                Debug.LogError($"{name} needs an input component (like KeyboardPlayerInput set to Left).", this);
            }

            if (groundCheck == null)
            {
                Debug.LogWarning($"{name} has no Ground Check assigned; using its own position.", this);
                groundCheck = transform;
            }
        }

        private void Update()
        {
            if (input == null) return;

            moveInput = CanMove ? input.Move.x : 0f;

            // Remember a jump press for a short moment so it isn't lost
            // if the player presses jump just before touching the ground.
            if (CanMove && input.JumpPressed)
            {
                jumpBufferTimer = jumpBufferTime;
            }
            else
            {
                jumpBufferTimer -= Time.deltaTime;
            }

            if (spriteRenderer != null && Mathf.Abs(moveInput) > 0.01f)
            {
                spriteRenderer.flipX = moveInput < 0f;
            }
        }

        private void FixedUpdate()
        {
            IsGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayers);

            if (IsGrounded)
            {
                coyoteTimer = coyoteTime;
            }
            else
            {
                coyoteTimer -= Time.fixedDeltaTime;
            }

            Vector2 velocity = body.linearVelocity;

            // Walk: ease toward the target speed so movement feels smooth, not robotic.
            float targetSpeed = moveInput * moveSpeed;
            float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, rate * Time.fixedDeltaTime);

            // Jump if a press is buffered and we're grounded (or just left the ground).
            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                velocity.y = jumpVelocity;
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                isJumping = true;
            }

            // Short hop: releasing jump while rising cuts the jump.
            bool jumpHeld = input != null && input.JumpHeld;
            if (isJumping && !jumpHeld && velocity.y > 0f)
            {
                velocity.y *= jumpCutMultiplier;
                isJumping = false;
            }

            if (IsGrounded && velocity.y <= 0f)
            {
                isJumping = false;
            }

            body.linearVelocity = velocity;
        }

        private void OnDrawGizmosSelected()
        {
            Transform check = groundCheck != null ? groundCheck : transform;
            Gizmos.color = IsGrounded ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(check.position, groundCheckSize);
        }
    }
}

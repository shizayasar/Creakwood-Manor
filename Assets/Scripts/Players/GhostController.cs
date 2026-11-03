using UnityEngine;
using CreakwoodManor.Controls;

namespace CreakwoodManor.Players
{
    /// <summary>
    /// Free-floating 4-direction movement for the ghost. No gravity, gentle
    /// acceleration, and a soft bob on its sprite so it always feels floaty.
    /// Passing through walls and floors is handled by physics layers, not code:
    /// the Ghost layer simply doesn't collide with Ground or Walls.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class GhostController : MonoBehaviour
    {
        [Header("Floating")]
        [SerializeField] private float maxSpeed = 4.5f;
        [SerializeField] private float acceleration = 20f;
        [Tooltip("Lower than acceleration so the ghost drifts to a stop.")]
        [SerializeField] private float deceleration = 12f;

        [Header("Bob (visual only)")]
        [Tooltip("Child object holding the ghost's sprite. The bob moves this, not the body.")]
        [SerializeField] private Transform visual;
        [SerializeField] private float bobHeight = 0.08f;
        [SerializeField] private float bobSpeed = 2.5f;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D body;
        private IPlayerInput input;
        private Vector2 moveInput;
        private Vector3 visualStartPosition;

        /// <summary>
        /// Turned off while the ghost is possessing an object, so its
        /// arrow keys drive the object instead.
        /// </summary>
        public bool CanMove { get; set; } = true;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;

            input = GetComponent<IPlayerInput>();
            if (input == null)
            {
                Debug.LogError($"{name} needs an input component (like KeyboardPlayerInput set to Right).", this);
            }

            if (visual != null)
            {
                visualStartPosition = visual.localPosition;
            }
        }

        private void Update()
        {
            if (input == null) return;

            // Clamp so diagonals aren't faster than straight lines.
            moveInput = CanMove ? Vector2.ClampMagnitude(input.Move, 1f) : Vector2.zero;

            if (spriteRenderer != null && Mathf.Abs(moveInput.x) > 0.01f)
            {
                spriteRenderer.flipX = moveInput.x < 0f;
            }

            if (visual != null)
            {
                float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                visual.localPosition = visualStartPosition + new Vector3(0f, offset, 0f);
            }
        }

        private void FixedUpdate()
        {
            Vector2 targetVelocity = moveInput * maxSpeed;
            float rate = targetVelocity.sqrMagnitude > 0.0001f ? acceleration : deceleration;

            body.linearVelocity = Vector2.MoveTowards(
                body.linearVelocity,
                targetVelocity,
                rate * Time.fixedDeltaTime);
        }
    }
}

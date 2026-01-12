using System.ComponentModel;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.MovementLogic
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Movement : MonoBehaviour
    {
        [Header("Движение влево-вправо")]
        [SerializeField] public float speed = 4f;
    
        [Header("Прыжок")]
        [SerializeField] public float jumpForce = 7f;
        [SerializeField] public int maxJumps = 2;
        
        [Header("Sprint")]
        [SerializeField] public float sprintSpeed = 10f;
        
        [Description("Время бега в секундах")]
        [SerializeField] public float sprintTime = 2f;
    
        [Header("Логика отскока от стен")]
        [SerializeField] public float wallSlideSpeed = 2f;
        [SerializeField] public Vector2 wallJumpForce = new(5f, 10f);
        
        [Header("Collision checks")]
        [SerializeField] private Transform wallCheck;
        [SerializeField] public float wallCheckDistance = 0.5f;
        [SerializeField] private Vector2 wallBoxSize = new(1.5f, 0.6f);

        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundBoxSize = new(0.4f, 0.4f);

        [Header("Collision layers")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        
        [SerializeField] private Rigidbody2D _rb;
        
        [Header("состояния")]
        [SerializeField] private bool _isSprinting;
        [SerializeField] private bool _isGrounded;
        [SerializeField] private bool _wasGrounded;
        [SerializeField] private bool _isTouchingWall;

        [SerializeField] private Vector2 _inputVector;
        [SerializeField] private float _absDirection = 1f;
        [SerializeField] private int _jumpCount;
        
        [SerializeField] public event System.Action<bool> OnGroundedStateChanged;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.freezeRotation = true;
        }

        private void OnEnable()
        {
            moveAction.action.Enable();
            jumpAction.action.Enable();
            sprintAction.action.Enable();
            
            moveAction.action.performed += OnMove;
            moveAction.action.canceled  += OnMove;
            jumpAction.action.performed += OnJump;
            sprintAction.action.performed += OnSprint;
        }
    
        private void OnDisable()
        {
            moveAction.action.performed -= OnMove;
            moveAction.action.canceled  -= OnMove;
            jumpAction.action.performed -= OnJump;
            sprintAction.action.performed -= OnSprint;

            moveAction.action.Disable();
            jumpAction.action.Disable();
            sprintAction.action.Disable();
        }

        public void FixedUpdate()
        {
            UpdateState();
            HandleMovement();
        }
        
        public void OnDrawGizmosSelected(){
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundBoxSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(wallCheck.position, wallBoxSize);
        }
        
        private void UpdateState()
        {
            if (_inputVector.x != 0 && !_isSprinting)
                _absDirection = Mathf.Sign(_inputVector.x);

            _wasGrounded = _isGrounded;
            _isGrounded = Physics2D.OverlapBox(groundCheck.position, groundBoxSize, 0f, groundLayer);
            _isTouchingWall = Physics2D.OverlapBox(wallCheck.position, wallBoxSize, 0f, wallLayer);

            if (_isGrounded != _wasGrounded)
                OnGroundedStateChanged?.Invoke(_isGrounded);
    
            if (_isGrounded && !_wasGrounded)
                _jumpCount = 0;
        }

        private void HandleMovement()
        {
            if (!_isSprinting)
                _rb.linearVelocity = new Vector2(_inputVector.x * speed, _rb.linearVelocity.y);

            if (_isTouchingWall && !_isGrounded && _rb.linearVelocity.y < 0)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -wallSlideSpeed);
        }
        
        private void OnMove(InputAction.CallbackContext context)
            => _inputVector = moveAction.action.ReadValue<Vector2>();


        private void OnJump(InputAction.CallbackContext context) 
        {
            if (_isTouchingWall && !_isGrounded) 
            {
                var jumpDirection = new Vector2(-_absDirection * wallJumpForce.x, wallJumpForce.y);
                _rb.linearVelocity = Vector2.zero;
                _rb.AddForce(jumpDirection, ForceMode2D.Impulse);
                _jumpCount = 1;
                return;
            }

            if (_isGrounded)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f); 
                _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                _jumpCount = 1;
            }
            else if (_jumpCount < maxJumps)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f); 
                _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                _jumpCount++;
            }
        }

        private void OnSprint(InputAction.CallbackContext context)
        {
            if (_isSprinting) return;
            StartCoroutine(SprintCoroutine());
        }
    
        private IEnumerator SprintCoroutine()
        {
            _isSprinting = true;
            _rb.linearVelocity = new Vector2(_absDirection * sprintSpeed, 0);
            _rb.gravityScale = 0f;
            Debug.Log(_absDirection);
            yield return new WaitForSeconds(sprintTime/2);
            _isSprinting = false;
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            _rb.gravityScale = 3f;
        }
    }
}

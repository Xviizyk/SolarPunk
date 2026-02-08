using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour, PlayerActions.IPlayerInputActions
{
    [Header("Connections")]
    private Rigidbody2D _rb;
    private Collider2D _col;
    
    [Header("Actions")]
    private PlayerActions _input;

    [Header("Moving system")]
    private bool _isLeft;
    private bool _canDash = true;
    private bool _isDashing;
    [SerializeField] private bool _onWall;
    [SerializeField] private bool _onGround;
    private bool _isSprinting;
    private Vector2 _movingInput;
    private LayerMask CheckLayerMask;
    private float _sprintSpeed = 1f;
    public float MovingSpeed;
    public float JumpHeight;
    public float MaxFallingSpeed;
    public float SprintBooster;
    public float DashSpeed;
    public float DashTime;
    
    [Header("Collision system")]
    public Vector2 WallCheckLeftTopCornerHitbox;
    public Vector2 WallCheckRightBottomCornerHitbox;
    
    #region Basic
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _input = new PlayerActions();

        _input.PlayerInput.SetCallbacks(this);

        CheckLayerMask = LayerMask.GetMask("Wall", "Ground");
    }

    private void OnDestroy() => _input.Dispose();
    
    private void OnEnable() => _input.Enable();

    private void OnDisable() => _input.Disable();

    private void FixedUpdate()
    { 
        HandleGravity();
        WallCheck();
        GroundCheck();
    }
    #endregion

    #region Handle

    private void HandleGravity()
    {
        if (_rb.linearVelocity.y < -MaxFallingSpeed && !_isDashing) 
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -MaxFallingSpeed);

        if (_rb.linearVelocity.y > MaxFallingSpeed && !_isDashing) 
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, MaxFallingSpeed);
    }

    private IEnumerator HandleDash()
    {
        _canDash = false;
        _isDashing = true;

        float originalGravity = _rb.gravityScale;
        Vector2 originalLinearVelocity = _rb.linearVelocity;
        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;

        float dir = _isLeft ? -1 : 1;
        _rb.linearVelocity = new Vector2(DashSpeed * dir, 0f);

        yield return new WaitForSeconds(DashTime);

        _rb.gravityScale = originalGravity;
        _rb.linearVelocity = new Vector2(originalLinearVelocity.x, 0f);
        _canDash = true;
        _isDashing = false;
    }
    #endregion

    #region OnFunctions
    public void OnDash(InputAction.CallbackContext context)
    {
        if (_canDash) 
            StartCoroutine(HandleDash());
    }

    public void OnMoving(InputAction.CallbackContext context)
    {
        _movingInput = context.ReadValue<Vector2>();
        if ((!_onWall || _onGround) && !_isDashing)
            _rb.linearVelocity = new Vector2(_movingInput.x * MovingSpeed * _sprintSpeed, _rb.linearVelocity.y);
        _isLeft = _movingInput.x < 0 ? true : false;
    }

    public void OnSit(InputAction.CallbackContext context) => Debug.Log("Sit");

    public void OnJump(InputAction.CallbackContext context)
    {
        if (_onGround) 
            _rb.AddForce(Vector2.up * JumpHeight, ForceMode2D.Impulse);
    }

    public void OnSprint(InputAction.CallbackContext context) => _sprintSpeed = SprintBooster;

    // надо потом будет разделить PlayerInput на MovementInput и CombatInput
    public void OnLMB(InputAction.CallbackContext context) {}
    public void OnRMB(InputAction.CallbackContext context) {}
    public void OnReload(InputAction.CallbackContext context) {}
    
    #endregion

    #region Collision check
    private void GroundCheck()
    {
        RaycastHit2D hit = Check.CheckGround(
            transform.position, 
            CheckLayerMask
        );

        _onGround = hit ? false : true;
    }

    private void WallCheck()
    {
        Collider2D hit = Check.CheckWall(
            transform.position, 
            WallCheckLeftTopCornerHitbox.x, 
            WallCheckLeftTopCornerHitbox.y, 
            WallCheckRightBottomCornerHitbox.x, 
            WallCheckRightBottomCornerHitbox.y, 
            CheckLayerMask
        );

        _onWall = hit ? false : true;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        RaycastHit2D hit = Check.CheckGround(
            transform.position, 
            CheckLayerMask
        );

        Gizmos.color = _onGround ? Color.green : Color.red;
        Vector3 hitPoint = _onGround ? (Vector3)hit.point : transform.position + Vector3.down * 2000f; 

        Gizmos.DrawLine(transform.position, hitPoint);
        Gizmos.DrawWireSphere(hitPoint, 0.1f);
        
        Vector2 LeftTopPoint = new Vector2(transform.position.x - WallCheckLeftTopCornerHitbox.x, transform.position.y + WallCheckLeftTopCornerHitbox.y);
        Vector2 RightBottomPoint = new Vector2(transform.position.x + WallCheckRightBottomCornerHitbox.x, transform.position.y - WallCheckRightBottomCornerHitbox.y);
        Vector2 RightTopPoint = new Vector2(RightBottomPoint.x, LeftTopPoint.y);
        Vector2 LeftBottonPoint = new Vector2(LeftTopPoint.x, RightBottomPoint.y);

        Gizmos.color = WallCheck() ? Color.green : Color.red;
        Gizmos.DrawLine(LeftTopPoint, RightTopPoint);
        Gizmos.DrawLine(RightTopPoint, RightBottomPoint); 
        Gizmos.DrawLine(RightBottomPoint, LeftBottonPoint);
        Gizmos.DrawLine(LeftBottonPoint, LeftTopPoint);
    }
}
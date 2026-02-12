using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour, PlayerActions.IMovementInputActions
{
    [Header("Connections")]
    private Rigidbody2D _rb;
    private Collider2D _col;
    private Settings _set;
    
    [Header("Actions")]
    private PlayerActions _input;

    [Header("Moving system")]
    [SerializeField] private int _jumpCount;
    [SerializeField] private bool _isLeft;
    [SerializeField] private bool _canDash = true;
    [SerializeField] private bool _isDashing;
    [SerializeField] private bool _onWall;
    [SerializeField] private bool _onGround;
    [SerializeField] private bool _isSprinting;
    [SerializeField] private Vector2 _movingInput;
    [SerializeField] private LayerMask CheckLayerMask;
    [SerializeField] private float _sprintSpeed = 1f;
    public float DownGravityScale;
    public float UpGravityScale;
    public float Acceleration;
    public float Deceleration;
    public float GroundSpeed;
    public float GroundFriction;
    public float AirSpeed;
    public float AirControl;
    public float AirFriction;
    public float MovingSpeed;
    public float JumpHeight;
    public float MaxFallingSpeed;
    public float SprintBooster;
    public float DashSpeed;
    public float DashTime;
    public int MaxJumpCount;
    
    [Header("Collision system")]
    public Vector2 WallCheckLeftTopCornerHitbox;
    public Vector2 WallCheckRightBottomCornerHitbox;
    
    #region Basic    
    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _set = GetComponent<Settings>();
        _input = new PlayerActions();

        _input.Enable();

        CheckLayerMask = LayerMask.GetMask("Wall", "Ground");

        _input.MovementInput.SetCallbacks(this);
    }

    private void OnDisable() => _input.Disable();

    private void OnDestroy() => _input.Dispose();

    private void FixedUpdate()
    { 
        HandleGravity();
        WallCheck();
        GroundCheck();
        GetMovingInput();

        if (_isDashing) return;
        
        if (_onGround)
            GroundMove();
        else 
            AirMove();
    }
    #endregion

    #region Moving system
    private void GetMovingInput()
    {
        _movingInput = _input.MovementInput.Moving.ReadValue<Vector2>();
    }

    private void GroundMove()
    {
        float targetSpeed = _movingInput.x * GroundSpeed * _sprintSpeed;

        _rb.linearVelocity = new Vector2(
            Mathf.MoveTowards(
                _rb.linearVelocity.x,
                targetSpeed,
                GroundFriction * Time.fixedDeltaTime
            ),
            _rb.linearVelocity.y
        );
    }

    private void AirMove()
    {
        float targetSpeed = _movingInput.x * AirSpeed;

        _rb.linearVelocity = new Vector2(
            Mathf.Lerp(
                _rb.linearVelocity.x,
                targetSpeed,
                AirControl * Time.fixedDeltaTime
            ),
            _rb.linearVelocity.y
        );
    }
    #endregion

    #region Handle
    // private void HandleMoving()
    // {
    //     if (_isDashing) return;

    //     float targetSpeed = _movingInput.x * MovingSpeed * _sprintSpeed;
    //     float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? Acceleration : Deceleration;
    //     float newSpeed = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, accelRate * Time.deltaTime);

    //     _rb.linearVelocity = new Vector2(newSpeed, _rb.linearVelocity.y);
        
    //     if (Mathf.Abs(newSpeed) > 0.01f)
    //         _isLeft = newSpeed < 0;
    // }

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
        _rb.gravityScale = 0f;

        float dir = _isLeft ? -1 : 1;

        _rb.linearVelocity += new Vector2(dir * DashSpeed, 0f);

        yield return new WaitForSeconds(DashTime);

        _rb.gravityScale = originalGravity;
        _isDashing = false;

        yield return new WaitForSeconds(0.2f);
        _canDash = true;
    }
    #endregion

    #region OnFunctions
    public void OnDash(InputAction.CallbackContext context)
    {
        if (_canDash && context.started) 
            StartCoroutine(HandleDash());
    }

    public void OnMoving(InputAction.CallbackContext context)
    {
        // I don't want to write anything, because movement will have stopped. (Вкратце, получится фигня)
    }

    public void OnSit(InputAction.CallbackContext context) => Debug.Log("Sit");

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        if (_jumpCount <= 0) return;

        _rb.linearVelocity = new Vector2(
            _rb.linearVelocity.x,
            0f
        );

        _rb.AddForce(Vector2.up * JumpHeight, ForceMode2D.Impulse);

        _jumpCount--;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (!_set.IsSprintTogglable && context.started)
            _sprintSpeed = SprintBooster;

        if (!_set.IsSprintTogglable && context.canceled)
            _sprintSpeed = 1.0f;

        if (_set.IsSprintTogglable && context.started)
            _sprintSpeed = _sprintSpeed == SprintBooster ? 1.0f : SprintBooster;
    }
    #endregion

    #region Collision check
    private void GroundCheck()
    {
        RaycastHit2D hit = Check.CheckGround(
            transform.position, 
            CheckLayerMask
        );

        _onGround = Mathf.Abs(hit.point.y - transform.position.y) < 1.0f ? true : false;

        if (_onGround) _jumpCount = MaxJumpCount;
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

        _onWall = hit ? true : false;

        if (_onWall) _jumpCount = MaxJumpCount;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        RaycastHit2D hit = Check.CheckGround(
            transform.position, 
            CheckLayerMask
        );

        Gizmos.color = !_onGround ? Color.green : Color.red;
        Vector3 hitPoint = _onGround ? (Vector3)hit.point : transform.position + Vector3.down * 2000f; 

        Gizmos.DrawLine(transform.position, hitPoint);
        Gizmos.DrawWireSphere(hitPoint, 0.1f);
        
        Vector2 LeftTopPoint = new Vector2(transform.position.x - WallCheckLeftTopCornerHitbox.x, transform.position.y + WallCheckLeftTopCornerHitbox.y);
        Vector2 RightBottomPoint = new Vector2(transform.position.x + WallCheckRightBottomCornerHitbox.x, transform.position.y - WallCheckRightBottomCornerHitbox.y);
        Vector2 RightTopPoint = new Vector2(RightBottomPoint.x, LeftTopPoint.y);
        Vector2 LeftBottonPoint = new Vector2(LeftTopPoint.x, RightBottomPoint.y);

        Gizmos.color = _onWall ? Color.green : Color.red;
        Gizmos.DrawLine(LeftTopPoint, RightTopPoint);
        Gizmos.DrawLine(RightTopPoint, RightBottomPoint); 
        Gizmos.DrawLine(RightBottomPoint, LeftBottonPoint);
        Gizmos.DrawLine(LeftBottonPoint, LeftTopPoint);
    }
}
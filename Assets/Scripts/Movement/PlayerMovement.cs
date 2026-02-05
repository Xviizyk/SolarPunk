using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Connections")]
    private Rigidbody2D _rb;
    private Collider2D _col;
    
    [Header("Actions")]
    public InputActionAsset PlayerActionAsset;
    private InputActionMap _playerActionMap;
    private InputAction _movingAction;
    private InputAction _sprintAction;
    private InputAction _sitAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;

    [Header("Moving system")]
    private bool _isLeft; //0 - right, 1 - left
    private bool _canDash = true;
    private bool _isDashing;
    private bool _onWall;
    private bool _onGround;
    private bool _isSprinting;
    private Vector2 _movingInput;
    private LayerMask GroundLayerMask;
    private LayerMask WallLayerMask;
    private float _sprintSpeed = 1f;
    public string GroundLayerName;
    public string WallLayerName;
    public float MovingSpeed;
    public float JumpHeight;
    public float MaxFallingSpeed;
    public float SprintBooster;
    public float DashSpeed;
    public float DashTime;
    
    [Header("Collision system")]
    public Vector2 WallCheckLeftTopCornerHitbox;
    public Vector2 WallCheckRightBottomCornerHitbox;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

        WallLayerMask = LayerMask.GetMask("Wall");
        GroundLayerMask = LayerMask.GetMask("Ground");
    }
    
    private void OnEnable()
    {
        _playerActionMap = PlayerActionAsset?.FindActionMap("Player");

        _movingAction = _playerActionMap.FindAction("Moving");
        _sprintAction = _playerActionMap.FindAction("Sprint");
        _dashAction = _playerActionMap.FindAction("Dash");
        _jumpAction = _playerActionMap.FindAction("Jump");
        _sitAction = _playerActionMap.FindAction("Sit");

        _playerActionMap.Enable();
    }
    private void OnDisable()
    {
        _playerActionMap.Disable();
    }

    private void Update()
    {
        HandleSprint();
    }

    private void FixedUpdate()
    {
        HandleInput(); 
        HandleGravity();
    }

    private void HandleInput()
    {
        _movingInput = _movingAction.ReadValue<Vector2>();

        if (_dashAction.triggered && _canDash) StartCoroutine(Dash());
        if (_jumpAction.triggered) Jump();
        if (_sitAction.triggered) Sit();

        _isLeft = _movingInput.x < 0 ? true : false;

        Move();
    }

    private void HandleGravity()
    {
        RaycastHit2D hit = Check.CheckGround(
            transform.position, 
            GroundLayerMask
        );

        if (hit) {
            float dist = Mathf.Abs(hit.point.y - transform.position.y);
            // Debug.Log($"Hit point by y: {hit.point.y}. Distance without y position: {dist}");
            _onGround = dist > 1f ? false : true;
        }

        if (_rb.linearVelocity.y < -MaxFallingSpeed && !_isDashing) _rb.linearVelocity = new Vector2( _rb.linearVelocity.x, -MaxFallingSpeed);
        if (_rb.linearVelocity.y > MaxFallingSpeed && !_isDashing) _rb.linearVelocity = new Vector2( _rb.linearVelocity.x, MaxFallingSpeed);
    }

    private void HandleSprint()
    {
        if (_sprintAction.WasPressedThisFrame()) _isSprinting = !_isSprinting;
        _sprintSpeed = 1f;
        if (_isSprinting) _sprintSpeed = SprintBooster;
    }

    private void Move()
    {
        if ((!_onWall || _onGround) && !_isDashing) _rb.linearVelocity = new Vector2(_movingInput.x * MovingSpeed * _sprintSpeed, _rb.linearVelocity.y);
    }

    private void Jump()
    {
        float dir = 1;
        if (_rb.linearVelocity.y > 0) dir = -dir;
        if (_onGround) _rb.AddForce(Vector3.up * JumpHeight * dir, ForceMode2D.Impulse);
    }

    private IEnumerator Dash()
    {
        _canDash = false;
        _isDashing = true;

        float originalGravity = _rb.gravityScale;
        Vector2 originalLinearVelocity = _rb.linearVelocity;
        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;

        float dir = _isLeft ? -1 : 1;
        _rb.linearVelocity = new Vector2(transform.localScale.x * DashSpeed * dir, 0f);

        yield return new WaitForSeconds(DashTime);

        _rb.gravityScale = originalGravity;
        _rb.linearVelocity = new Vector2(originalLinearVelocity.x, 0f);
        _canDash = true;
        _isDashing = false;
    }

    private void Sit()
    {
        Debug.Log("Sit");
    }

    private void CheckWall()
    {
        _onWall = Wall();
    }

    private RaycastHit2D Ground()
    {
        return Check.CheckGround(
            transform.position, 
            (1 << 6) | (1 << 7)
        );
    }

    private Collider2D Wall()
    {
        return Check.CheckWall(
            transform.position, 
            WallCheckLeftTopCornerHitbox.x, 
            WallCheckLeftTopCornerHitbox.y, 
            WallCheckRightBottomCornerHitbox.x, 
            WallCheckRightBottomCornerHitbox.y, 
            (1 << 6) | (1 << 7)
        );
    }

    private void OnDrawGizmosSelected()
    {
        RaycastHit2D groundHit = Ground();
        Gizmos.color = groundHit ? Color.green : Color.red;
        Vector3 hitPoint = groundHit ? (Vector3)groundHit.point : transform.position + Vector3.down * 2000f; 

        Gizmos.DrawLine(transform.position, hitPoint);
        Gizmos.DrawWireSphere(hitPoint, 0.1f);
        
        Vector2 LeftTopPoint = new Vector2(transform.position.x - WallCheckLeftTopCornerHitbox.x, transform.position.y + WallCheckLeftTopCornerHitbox.y);
        Vector2 RightBottomPoint = new Vector2(transform.position.x + WallCheckRightBottomCornerHitbox.x, transform.position.y - WallCheckRightBottomCornerHitbox.y);
        Vector2 RightTopPoint = new Vector2(RightBottomPoint.x, LeftTopPoint.y);
        Vector2 LeftBottonPoint = new Vector2(LeftTopPoint.x, RightBottomPoint.y);

        Gizmos.color = Wall() ? Color.green : Color.red;
        Gizmos.DrawLine(LeftTopPoint, RightTopPoint);
        Gizmos.DrawLine(RightTopPoint, RightBottomPoint); 
        Gizmos.DrawLine(RightBottomPoint, LeftBottonPoint);
        Gizmos.DrawLine(LeftBottonPoint, LeftTopPoint);
    }
}
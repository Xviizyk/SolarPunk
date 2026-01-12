using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

public class ModernMovement : MonoBehaviour
{
    [Header("Main settings")]
    public float movementSpeed;
    public float jumpForce;
    public int maxAmountJumps;
    public float sprintRange;
    public float sprintTime;
    public float wallSlideDownSpeed;
    public float wallJumpForce;

    [Header("Check points settings")]
    public Transform wallCheckPosition;
    public Transform groundCheckPosition;
    public float wallCheckDistance;
    public float groundCheckDistance;

    [Header("Layers")]
    public string groundLayerName;
    public string wallLayerName;

    [Header("Player actions")]
    public InputActionAsset generalActions;
    [SerializeField] private InputActionMap computerPlayerActions;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction sprintAction;
    [SerializeField] private InputAction movementAction;

    [Header("Private states")]
    [SerializeField] private bool isDashing;
    [SerializeField] private int jumpAmount;
    [SerializeField] private bool isLeft;
    [SerializeField] private bool isSprint;
    [SerializeField] private bool onGround;
    [SerializeField] private bool wasGround;
    [SerializeField] private bool onWall;
    [SerializeField] private Vector2 inputDirection;

    [Header("Gravity system")]
    public float m_MassOfBody; // The mass of the body (player)
    public float g_GravitationalAcceleration; // Acceleration due to gravity
    public float G_UniversalGravitationalConstant; // Universal Gravitational Constant
    public float r_DistanceBetweenBodies; // Distances between centres of bodies
    public float F_universalGravitation;
    public float M_MassOfCelestialBody; // The mass of the celestial body; For example:   Mass of Mars:  639000000000000000000000 kg (6,39E23 kg)   Mass of Earth: 5972000000000000000000000 kg (5,972E24 kg)   Mass of Sun:   1989000000000000000000000000000 kg (1,989E30 kg)
    public float v_speedOfFalling; // speed of falling
    [SerializeField] private float t_timeFromStartJumping; // How much time has gone, since body jumped
    [SerializeField] private float v_speedOfFallingBuffer; // just speed of falling
    [SerializeField] private bool isUpGravity; // Body is moving top or bottom

    [Header("Moving system")]
    public float x_jumpMoving;
    public float y_jumpMoving;
    public float minTimeDashed;
    public float maxTimeDashed;
    public float dashDistance;
    public float dashTime;
    [SerializeField] private Vector2 dashTargetPos;
    [SerializeField] private Vector2 dashStartPos;
    [SerializeField] private float dashTimeHandler;
    [SerializeField] private bool isWaitDashing;
    [SerializeField] private float dashWaitingTime;
    [SerializeField] private float lastTimeDashed;

    private void OnEnable()
    {
        computerPlayerActions = generalActions.FindActionMap("PC");
        jumpAction = computerPlayerActions.FindAction("Jump");
        sprintAction = computerPlayerActions.FindAction("Sprint");
        movementAction = computerPlayerActions.FindAction("Moving");

        jumpAction.Enable();
        sprintAction.Enable();
        movementAction.Enable();
    }

    private void OnDisable()
    {
        jumpAction.Disable();
        sprintAction.Disable();
        movementAction.Disable();
    }

    private void Start()
    {
        G_UniversalGravitationalConstant = 6.67430e-11f;
    }

    private void FixedUpdate()
    {
        // foreach (float mass in Weapon.inventoryWeapons)
        // {
        //     m_MassOfBody += mass;
        // }
        HandleMovement();
        HandleJump();
        HandleSprint();
        HandlePhysics();
    }

    #region Movement mechanics

    
    private void HandleJump()
    {
        if (jumpAction.triggered)
        {
            if (jumpAmount < maxAmountJumps)
            {
                v_speedOfFalling = -jumpForce;
                jumpAmount++;
            }
            else if (onWall && !onGround)
            {
                v_speedOfFalling = -jumpForce;
                if (isLeft) x_jumpMoving = -x_jumpMoving;
                transform.position = Vector2.Lerp(transform.position, new Vector2(transform.position.x + x_jumpMoving, transform.position.y + y_jumpMoving), 1.0f);
            }
        }
    }

    private void HandleSprint()
    {
        // короче, вся эта функция должна типа реализовать: если чел зажал спринт, то скорость увеличивается, а елси после нажатия спринта не прошло и секунды и чел снова нажал, то персонаж по Vector2.Lerp двигается в сторону (сам реализу, просто комменатриями скажи где надо)
        if (isWaitDashing) lastTimeDashed += Time.fixedDeltaTime; 
        if (sprintAction.WasPressedThisFrame())
        {
            if (isWaitDashing && lastTimeDashed >= minTimeDashed && lastTimeDashed <= maxTimeDashed)
            {
                if (!isDashing) StartCoroutine(DashHandler());
                isWaitDashing = false;
                lastTimeDashed = 0;
            }

            else
            {
                isWaitDashing = true;
                lastTimeDashed = 0;
            }
        }
        if (lastTimeDashed > maxTimeDashed)
        {
            isWaitDashing = false;
        }
        isSprint = sprintAction.IsPressed();
    }

    private void HandleMovement()
    {
        inputDirection = movementAction.ReadValue<Vector2>();
        if (inputDirection.x < 0) isLeft = true;
        else if (inputDirection.x > 0) isLeft = false;
    }

    private IEnumerator DashHandler()
    {
        isDashing = true;
        float _time = 0f;
        dashStartPos = transform.position;
        if (isLeft) dashTargetPos = dashStartPos + (Vector2.left * dashDistance);
        if (!isLeft) dashTargetPos = dashStartPos + (Vector2.right * dashDistance);

        while (_time < dashTime)
        {
            _time += Time.deltaTime;
            float t = _time / dashTime;
            float ease = t*t*(3f-t-t);
            transform.position = Vector2.Lerp(dashStartPos, dashTargetPos, ease);
            yield return null;
        }
        transform.position = dashTargetPos;
        isDashing = false;
    }

    #endregion

    #region Realization of Gravity

    private void CheckSurroundings()
    {
        onGround = Physics2D.Raycast(groundCheckPosition.position, Vector2.down, groundCheckDistance, LayerMask.GetMask(groundLayerName));
        Vector2 wallDir = isLeft ? Vector2.left : Vector2.right;
        onWall = Physics2D.Raycast(wallCheckPosition.position, wallDir, wallCheckDistance, LayerMask.GetMask(wallLayerName));
        if (onGround) jumpAmount = 0;
    }

    private void HandlePhysics()
    {
        CheckSurroundings();
        Find_g(); 

        if (isDashing) return;

        if (!onGround)
        {
            if (onWall && v_speedOfFalling > 0)
            {
                v_speedOfFalling = wallSlideDownSpeed;
            }
            else
            {
                v_speedOfFalling += g_GravitationalAcceleration * Time.fixedDeltaTime;
            }
        }
        else
        {
            v_speedOfFalling = 0;
        }
        float moveX = inputDirection.x * movementSpeed;
        float moveY = -v_speedOfFalling;
        transform.Translate(new Vector2(moveX, moveY) * Time.fixedDeltaTime);
    }

    private void Gravitation()
    {
        Find_g();
        if (!onGround)
        {
            v_speedOfFalling += g_GravitationalAcceleration * Time.fixedDeltaTime;
        }
        else
        {
            v_speedOfFalling = 0;
        }
        v_speedOfFalling = Mathf.Clamp(v_speedOfFalling, -100f, 100f);
    }

    private float Find_t()
    {
        float height = Find_l();
        if (g_GravitationalAcceleration <= 0 || height <= 0) return 0;
        return Mathf.Sqrt((2 * height) / g_GravitationalAcceleration);
    }

    public float Find_l(Transform objectTransform = null)
    {
        if (objectTransform == null)
        {
            objectTransform = this.transform;
        }
        float distance = 0;
        RaycastHit2D hit = Physics2D.Raycast(objectTransform.position, -Vector2.up);
        if (hit.collider != null)
        {
            distance = Mathf.Abs(hit.point.y - transform.position.y);
        }
        return distance;
    }

    private void Find_g()
    {
        g_GravitationalAcceleration = (G_UniversalGravitationalConstant * M_MassOfCelestialBody) / (r_DistanceBetweenBodies * r_DistanceBetweenBodies);
    }
    
    private void Find_G()
    {
        G_UniversalGravitationalConstant = (F_universalGravitation * (r_DistanceBetweenBodies * r_DistanceBetweenBodies)) / (M_MassOfCelestialBody * m_MassOfBody);
    }

    private void Find_M()
    {
        M_MassOfCelestialBody = (g_GravitationalAcceleration * (r_DistanceBetweenBodies * r_DistanceBetweenBodies)) / G_UniversalGravitationalConstant;
    }

    private void Find_r()
    {
        r_DistanceBetweenBodies = Mathf.Sqrt(G_UniversalGravitationalConstant * M_MassOfCelestialBody) / g_GravitationalAcceleration;
    }

    private void Find_F()
    {
        F_universalGravitation = G_UniversalGravitationalConstant * ((M_MassOfCelestialBody * m_MassOfBody) / (r_DistanceBetweenBodies * r_DistanceBetweenBodies));
    }

    private void Find_v()
    {
        v_speedOfFallingBuffer = v_speedOfFalling;
        v_speedOfFalling = Mathf.Sqrt(2 * g_GravitationalAcceleration * Find_l());
    }

    #endregion

    //ужас...
}
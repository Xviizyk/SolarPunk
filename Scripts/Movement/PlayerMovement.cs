using Godot;
using System;

public partial class PlayerMovement : CharacterBody2D
{
    private bool _canDash = true;
	private bool _moved = false;
	private bool _isGrabbed = false;
    private bool _isLeft = false;
    private bool _isDashing = false;
	private bool _sliding = false;
	private bool _isGrabbing = false;
	private bool _printing = false;

    private Vector2 _movingInput = Vector2.Zero;

    private float _speedMultiply = 1.0f;
    private float MaxFallingSpeed = 1000;
    private float _delta = 0.0f;
    private float _deltaDash = 0.0f;
    private float _jumpTimer = 0.0f;

    private int _jumpCount = 0;

	private string _lastAnimation = "Idle";
	private string _wallState = "Normal"; /* Normal | Grab | Climb */

	[Export] public RayCast2D Left_body_ray;
	[Export] public RayCast2D Left_head_ray;
	[Export] public RayCast2D Right_body_ray;
	[Export] public RayCast2D Right_head_ray;

    [Export] public SpriteFrames _spriteFrames;
    [Export] public AnimatedSprite2D _animatedSprite;
	[Export] public CollisionShape2D _hitbox;
	[Export] public CapsuleShape2D _hitboxShape;
    
    [Export] public float Acceleration = 1000.0f;
    [Export] public float Deceleration = 2000.0f; 
    [Export] public float GroundSpeed = 300.0f;
    [Export] public float AirSpeed = 350.0f;
    [Export] public float JumpVelocity = -400.0f;
    [Export] public float SprintBooster = 1.5f;
    [Export] public float DashSpeed = 1000.0f;
    [Export] public float DashTime = 0.2f;
	[Export] public float JumpTimer = 0.2f;

	[Export] public Vector2 WallClimbingHitbox = new Vector2(20.0f, 100.0f); //Radius and Height
	[Export] public Vector2 WallClimbingHitboxPosition = new Vector2(-45.0f, -52.0f);
	[Export] public Vector2 WallClimbHitboxPosition = new Vector2(15.0f, 30.0f);
	[Export] public float WallClimbingHitboxRotation = 0;
	[Export] public float WallClimbGlobalPositionMove = -15.0f;

	[Export] public Vector2 SlideHitbox = new Vector2(29.0f, 120.0f); //Radius and Height
	[Export] public Vector2 SlideHitboxPosition = new Vector2(0.0f, 15.0f);
	[Export] public float SlideHitboxRotation = 90.0f;

	[Export] public Vector2 NormalHitbox = new Vector2(30.0f, 90.0f); //Radius and Height
	[Export] public Vector2 NormalHitboxPosition = new Vector2(0.0f, 0.0f);
	[Export] public float NormalHitboxRotation = 0.0f;

	[Export] public Vector2 WallGrabGlobalPsoitionPlusPlus = new Vector2(40.0f, -55.0f);

    [Export] public int MaxJumpCount = 1;

	[Export] public bool AllowDash = true;
	[Export] public bool AllowMove = true;
	[Export] public bool AllowSprint = true;
	[Export] public bool AllowJump = true;
	[Export] public bool AllowSit = true;
	[Export] public bool AllowGrab = true;

	public override void _Ready()
	{
		_animatedSprite = GetNode<AnimatedSprite2D>("BodySprite/AnimatedSprite2D");
		_spriteFrames = _animatedSprite.SpriteFrames as SpriteFrames;
		_hitbox = GetNode<CollisionShape2D>("CapsuleCollision");
		_hitboxShape = _hitbox.Shape as CapsuleShape2D;
	}

    public override void _PhysicsProcess(double delta)
    {
        _delta = (float)delta;  
		_jumpTimer -= _delta;

        _movingInput = Input.GetVector("Left", "Right", "Up", "Down");

        if (_movingInput.X != 0)
			_isLeft = _movingInput.X < 0;
			

		if (_isGrabbed && Velocity == Vector2.Zero)
			_isLeft = (Left_body_ray.IsColliding() && !Left_head_ray.IsColliding());


		if (_jumpTimer < 0.0f) 
			_jumpTimer = 0.0f;

        
        HandleGravity();
        HandleMove();

		EdgeGrab();

		if (IsOnFloor())
			_isGrabbed = false;

		if (_animatedSprite.Animation != "Wall_climb_third")
			Animate();

		else if (_animatedSprite.Frame+1 == _spriteFrames.GetFrameCount(_animatedSprite.Animation))
			Animate();

        MoveAndSlide();
    }

    private void HandleMove()
    {
        if (Input.IsActionJustPressed("Dash") && _canDash && AllowDash) 
            Dash(DashSpeed, DashTime);

        if (_isDashing) 
            return;

        if (AllowJump)
			OnJump();

        if (AllowSprint)
			OnSprint();

        if (AllowSit)
			OnSit();
		
		if (!AllowMove)
			return;

        if (IsOnFloor())
            GroundMove();
        else
            AirMove();
    }

    private void GroundMove()
    {
        float targetSpeed = _movingInput.X * GroundSpeed * _speedMultiply;

        float step = Mathf.Abs(targetSpeed) > 0.1f ? Acceleration : Deceleration;

        Velocity = new Vector2(
            Mathf.MoveToward(
                Velocity.X,
                targetSpeed,
                _delta * step
            ),
            Velocity.Y
        );
    }

    private void AirMove()
    {
        float targetSpeed = _movingInput.X * AirSpeed;

        Velocity = new Vector2(
            Mathf.MoveToward(
                Velocity.X,
                targetSpeed,
                _delta * Deceleration
            ),
            Velocity.Y
        );
    }

    private void HandleGravity()
    {        
        if (!_isDashing && AllowJump && _wallState != "Grab")
			Velocity += GetGravity() * (float)_delta;

        if (IsOnFloor()) 
            _jumpCount = MaxJumpCount;

        if (Velocity.Y > MaxFallingSpeed && !_isDashing)
            Velocity = new Vector2(Velocity.X, MaxFallingSpeed);
    }

    private void OnSit()
    {
		if (_jumpTimer > 0.0f)
			return;

        if (Input.IsActionPressed("Sit")) 
        {
            if (!IsOnFloor()) 
                return;

            if (_speedMultiply > 1.0f && Mathf.Abs(Velocity.X) > 10.0f && Input.IsActionJustPressed("Sit"))
			{
				Dash(DashSpeed * 0.5f, DashTime * 0.5f);
				_sliding = true;
			}

            else
			{
				_speedMultiply = 0.5f;
			}
        }
    }

    private void OnJump()
    {
        if (!Input.IsActionJustPressed("Jump"))
            return;

		if (_jumpTimer > 0.0f)
			return;

        if (_jumpCount <= 0)
            return;

        Velocity = new Vector2(Velocity.X, JumpVelocity);

        _jumpTimer = JumpTimer;

        _jumpCount--;
    }

    private void OnSprint()
    {
        if (!Input.IsActionPressed("Sprint"))
        {
            if (!Input.IsActionPressed("Sit"))
                _speedMultiply = 1.0f;
            return;
        }
        _speedMultiply = SprintBooster;
    }
    
    private async void Dash(float spd, float t)
    {
        _canDash = false;
        _isDashing = true;

        MotionMode = MotionModeEnum.Floating;

		float dir = _isLeft ? -1.0f : 1.0f;

        Velocity = new Vector2(dir * spd, 0f);

        await ToSignal(GetTree().CreateTimer(t), SceneTreeTimer.SignalName.Timeout);
        
		if (!IsInstanceValid(this)) return;

        MotionMode = MotionModeEnum.Grounded;
        _isDashing = false;

        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);

		if (!IsInstanceValid(this)) return;

        _canDash = true;
		_sliding = false;
    }

	private void EdgeGrab()
	{
		if ((_isGrabbing || IsOnFloor() || _isDashing) && AllowJump)
			return;

		_isGrabbed = true;
		bool leftCanGrab = (Left_body_ray.IsColliding() && !Left_head_ray.IsColliding());
		bool rightCanGrab = (Right_body_ray.IsColliding() && !Right_head_ray.IsColliding());

		if (!(leftCanGrab || rightCanGrab))
			return;

		Velocity = Vector2.Zero;

		if (!_isGrabbing)
		{
			SetHitbox(WallClimbingHitbox, WallClimbHitboxPosition * new Vector2(_isLeft ? 1 : -1, 1.0f), NormalHitboxRotation);
			PlayAnimation("Wall_climb_first");
			GlobalPosition += new Vector2(_moved ? 0 : WallClimbGlobalPositionMove * (_isLeft ? -1 : 1), 0.0f);
			_moved = true;
		}
		if (Input.IsActionJustPressed("Jump") && !_isGrabbing)
		{
			SetHitbox(NormalHitbox, WallClimbingHitboxPosition * new Vector2(_isLeft ? 1 : -1, 1.0f), NormalHitboxRotation);_wallState = "Grab";
			AllowJump = false;
			EdgeClimb();
		}
		_wallState = "Normal";
	}

	private async void EdgeClimb()
	{
		_wallState = "Climb";
		_isGrabbing = true;

		PlayAnimation("Wall_climb_third");

		await ToSignal(GetTree().CreateTimer(_spriteFrames.GetFrameCount("Wall_climb_third") / (_spriteFrames.GetAnimationSpeed("Wall_climb_third") * Mathf.Abs(_animatedSprite.GetPlayingSpeed())) - 1.0f/10f), "timeout");

		PlayAnimation("Idle");
		GlobalPosition += new Vector2(WallGrabGlobalPsoitionPlusPlus.X * (_isLeft ? -1 : 1), WallGrabGlobalPsoitionPlusPlus.Y);

		_wallState = "Normal";
		MotionMode = MotionModeEnum.Grounded;
		AllowJump = true;
		SetHitbox(NormalHitbox, NormalHitboxPosition, NormalHitboxRotation);
		_isGrabbing = false;
		_isGrabbed = false;
		_moved = false;
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
	}

	private void SetHitbox(Vector2 sz, Vector2 pos, float rot)
	{
		_hitbox.Position = pos;
		_hitbox.RotationDegrees = rot;
		_hitboxShape.Radius = sz.X;
		_hitboxShape.Height = sz.Y;
	}

	private void Animate()
	{
		bool leftCanGrab = (Left_body_ray.IsColliding() && !Left_head_ray.IsColliding());
		bool rightCanGrab = (Right_body_ray.IsColliding() && !Right_head_ray.IsColliding());

		TimedPrint($"Hitbox:\nHeight: {_hitboxShape.Height}, Radius: {_hitboxShape.Radius}\n\nWall System:\n_isGrabbed: {_isGrabbed}, WallState: {_wallState}\n\nRayCast:\nleftCanGrab: {leftCanGrab}, rightCamGrab: {rightCanGrab}\n\nOther:\nAnimation: {_lastAnimation}, FPS: {1.0f / _delta}", 0.01f);
	
		_animatedSprite.FlipH = _isLeft;

		if (_isDashing && _sliding)
		{
			SetHitbox(SlideHitbox, SlideHitboxPosition, SlideHitboxRotation);
		}
		else if (!_isGrabbed)
		{
			SetHitbox(NormalHitbox, NormalHitboxPosition, NormalHitboxRotation);
		}

		if (_isDashing)
		{
			PlayAnimation(_sliding ? "Slide_begin" : "Dash");
			return;
		}

		if (!IsOnFloor() && _lastAnimation != "Wall_climb_first" && _lastAnimation != "Wall_climb_third")
		{
			PlayAnimation(Velocity.Y < 0 ? "Jump" : "Fall");
			return;
		}

		if (_speedMultiply > 1.0f && Mathf.Abs(Velocity.X) > 10)
			PlayAnimation("Run");

		else if (Mathf.Abs(Velocity.X) > 5)
			PlayAnimation("Walk");

		else if (_wallState == "Normal" && (!_isGrabbed || IsOnFloor()))
			PlayAnimation("Idle");
		}

	private void PlayAnimation(string anim)
	{
		if (_lastAnimation == anim && _lastAnimation != "Jump")
			return;
		
		_animatedSprite.Play(anim);
		_lastAnimation = _animatedSprite.Animation;
	}

	private async void TimedPrint(string text, float time)
	{
		if (_printing) return;
		_printing = true;
		await ToSignal(GetTree().CreateTimer(time), "timeout");
		GD.Print(text);
		_printing = false;
	}
}

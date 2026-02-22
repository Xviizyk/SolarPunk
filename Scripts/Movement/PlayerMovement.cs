using Godot;
using System;

public partial class PlayerMovement : CharacterBody2D
{
    private bool _isLeft = false;
    private bool _canDash = true;
    private bool _isDashing = false;
    private bool _onWall = false;
    private bool _onGround = false;
    private bool _isSprinting = false;
	private bool _sliding = false;

    private Vector2 _movingInput = Vector2.Zero;

    private float _speedMultiply = 1.0f;
    private float MaxFallingSpeed = 1000;
    private float _delta = 0.0f;
    private float _deltaDash = 0.0f;
    private float _jumpTimer = 0.0f;

    private int _jumpCount = 0;

	private string _lastAnimation = "Idle";

    private AnimatedSprite2D _animatedSprite;
	private CollisionShape2D _hitbox;
    
    [Export] public float Acceleration = 1000.0f;
    [Export] public float Deceleration = 2000.0f; 
    [Export] public float GroundSpeed = 300.0f;
    [Export] public float AirSpeed = 350.0f;
    [Export] public float JumpVelocity = -400.0f;
    [Export] public float SprintBooster = 1.5f;
    [Export] public float DashSpeed = 1000.0f;
    [Export] public float DashTime = 0.2f;
	[Export] public float JumpTimer = 0.2f;

	[Export] public Vector2 WallClimbHitbox = new Vector2(20.0f, 100.0f); //Radius and Height
	[Export] public Vector2 SlideHitbox = new Vector2(29.0f, 120.0f); //Radius and Height
	[Export] public Vector2 NormalHitbox = new Vector2(30.0f, 90.0f); //Radius and Height

	[Export] public Vector2 WallClimbHitboxPosition = new Vector2(-15.0f, 30.0f);
	[Export] public Vector2 SlideHitboxPosition = new Vector2(0.0f, 15.0f);
	[Export] public Vector2 NormalHitboxPosition = new Vector2(0.0f, 0.0f);

	[Export] public float WallClimbHitboxRotation = 0;
	[Export] public float SlideHitboxRotation = 90.0f;
	[Export] public float NormalClimbHitboxRotation = 0.0f;

    [Export] public int MaxJumpCount = 1;

	[Export] public bool AllowDash = true;
	[Export] public bool AllowMove = true;
	[Export] public bool AllowSprint = true;
	[Export] public bool AllowJump = true;
	[Export] public bool AllowSit = true;

    public override void _Ready()
    {
        _hitbox = GetNode<CollisionShape2D>("CollisionShape2D");
        _animatedSprite = GetNode<AnimatedSprite2D>("BodySprite/AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        _delta = (float)delta;  
		_jumpTimer -= _delta;

        _movingInput = Input.GetVector("Left", "Right", "Up", "Down");

        if (_movingInput.X != 0) 
			_isLeft = _movingInput.X < 0 ? true : false;

		if (_jumpTimer < 0.0f) 
			_jumpTimer = 0.0f;

		Animate();
        
        HandleGravity();
        HandleMove();

        MoveAndSlide();
    }

    private void HandleMove()
    {
        if (Input.IsActionJustPressed("Dash") && _canDash && AllowDash) 
            Dash(DashSpeed, DashTime);

        if (_isDashing) 
            return;

        if (_jumpTimer == 0.0f && AllowJump)
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
        Velocity += GetGravity() * (float)_delta;

        if (Velocity.Y < -MaxFallingSpeed && !_isDashing)
            Velocity = new Vector2(Velocity.X, -MaxFallingSpeed);

        if (Velocity.Y > MaxFallingSpeed && !_isDashing)
            Velocity = new Vector2(Velocity.X, MaxFallingSpeed);
    }

    private void OnSit()
    {
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
                _speedMultiply = 0.5f;
        }
    }

    private void OnJump()
    {
        if (Velocity.Y >= 0 && IsOnFloor()) 
            _jumpCount = MaxJumpCount;

        if (!Input.IsActionJustPressed("Jump"))
            return;

        if (_jumpCount <= 0)
            return;

        Velocity = new Vector2(
            Velocity.X,
            0f
        );

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

        float originalGravity = MotionMode == MotionModeEnum.Grounded ? 1.0f : 0.0f;
        float dir = _isLeft ? -1f : 1f;

        MotionMode = MotionModeEnum.Floating;
        Velocity = new Vector2(dir * spd, 0f);

        await ToSignal(GetTree().CreateTimer(t), SceneTreeTimer.SignalName.Timeout);
        
        MotionMode = MotionModeEnum.Grounded;
        _isDashing = false;

        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);

        _canDash = true;
		_sliding = false;
    }

    private async void Animate()
    {
		// GD.Print($"Velocity.X: {Velocity.X}, Velocity.Y: {Velocity.Y}, _jumpTimer: {_jumpTimer}, Animation: {_animatedSprite.Animation}");
		_animatedSprite.FlipH = _isLeft;

		_hitbox.Position = new Vector2(NormalHitboxPosition.X, NormalHitboxPosition.Y);
		_hitbox.Rotation = new float(NormalClimbHitboxRotation);
		_hitbox.Shape.Height = new float(NormalHitbox.X);
		_hitbox.Shape.Radius = new float(NormalHitbox.Y);
		
		if (_isDashing)
		{
			GD.Print();
		}

		else if (!IsOnFloor() && !_isDashing)
		{
			if (Velocity.Y < 0)
				if (_animatedSprite.Animation == "Jump" && Input.IsActionJustPressed("Jump") && AllowJump && _jumpTimer == 0 && _jumpCount != 0) 
				{
					_animatedSprite.Stop();
					_animatedSprite.Play("Jump");
				}
				else
					_animatedSprite.Play("Jump");
			else
			{
				if (_animatedSprite.Animation == "Jump")
					return;
				if (_animatedSprite.Animation != "Fall_Loop" && _lastAnimation != "Jump" && _isDashing)
					_animatedSprite.Play("Fall");
				await ToSignal(_animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);
				if (!IsOnFloor() && Velocity.Y > 0) 
					_animatedSprite.Play("Fall_Loop");
			}
		}
		
		else if (!_isDashing && _lastAnimation == "Slide_begin") {
			_animatedSprite.Play("Slide_end");
		}

		else if (_isDashing) {
			if (_sliding) 
			{
				_hitbox.Position = new Vector2(SlideHitboxPosition.X, SlideHitboxPosition.Y);
				_hitbox.Rotation = new float(SlideHitboxRotation);
				_hitbox.Shape.height = new float(SlideHitbox.X);
				_hitbox.Shape.radius = new float(SlideHitbox.Y);

				_animatedSprite.Play("Slide_begin");
			}
		}

		else
		{
			if (_isSprinting || Mathf.Abs(Velocity.X) > GroundSpeed * _speedMultiply)
				_animatedSprite.Play("Run");
			else if (Mathf.Abs(Velocity.X) > 0.0f)
				_animatedSprite.Play("Walk");
			else	
				_animatedSprite.Play("Idle");
		}

		_lastAnimation = _animatedSprite.Animation;
    }
}

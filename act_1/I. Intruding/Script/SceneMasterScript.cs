using Godot;
using System;

[Export] public Sprite2D RightGroundGradient;
[Export] public Sprite2D LeftGroundGradient;
[Export] public CharacterBody2D Player;

private Vector2 _input = (0.0f, 0.0f);

public partial class SceneMasterScript : Node2D
{
	public override void _PhysicsProcess(double delta)
	{
		_input = Input.GetVector("Left", "Right", "Up", "Down");
	}
}

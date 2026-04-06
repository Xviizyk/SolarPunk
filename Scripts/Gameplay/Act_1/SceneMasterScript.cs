using Godot;

public partial class SceneMasterScript : Node2D
{
	private bool _printing = false;

	[Export] public Node2D GradientObject;
	[Export] public CharacterBody2D Player;
	[Export] public float MaxDistanceFromCenter = 3500.0f;
	
	public override void _PhysicsProcess(double delta)
	{
		GradientObject.GlobalPosition = new Vector2(Player.GlobalPosition.X, GradientObject.GlobalPosition.Y);

		if (Player.GlobalPosition.X > MaxDistanceFromCenter)
			Player.GlobalPosition = new Vector2(-MaxDistanceFromCenter+10.0f, Player.GlobalPosition.Y);
		
		if (Player.GlobalPosition.X < -MaxDistanceFromCenter)
			Player.GlobalPosition = new Vector2(MaxDistanceFromCenter-10.0f, Player.GlobalPosition.Y);
	}
}

using Godot;

[Export] public Node2D GradientObject;
[Export] public CharacterBody2D Player;
[Export] public MaxDistanceGlobalPosition = 1000.0f;

public partial class SceneMasterScript : Node2D
{
	public override void _PhysicsProcess(double delta)
	{
		GradientObject.GlobalPosition = new Vector2(Player.GlobalPosition.X, GradientObject.GlobalPosition.Y);

        if (Player.GlobalPosition.X > MaxDistanceFromCenter)
            Player.GlobalPosition = new Vector2(-MaxDistanceGlobalPosition+10.0f, Player.GlobalPosition.Y
        
        if (Player.GlobalPosition.X < -MaxDistanceFromCenter)
            Player.GlobalPosition = new Vector2(MaxDistanceGlobalPosition-10.0f, Player.GlobalPosition.Y);
	}
}

using Godot;

public partial class SceneMasterScript : Node2D
{
	private bool _printing = false;

	[Export] public Node2D GradientObject;
	[Export] public CharacterBody2D Player;
	[Export] public float MaxDistanceFromCenter = 4000.0f;
	
	public override void _PhysicsProcess(double delta)
	{
		GradientObject.GlobalPosition = new Vector2(Player.GlobalPosition.X, GradientObject.GlobalPosition.Y);

		if (Player.GlobalPosition.X > MaxDistanceFromCenter)
			Player.GlobalPosition = new Vector2(-MaxDistanceFromCenter+10.0f, Player.GlobalPosition.Y);
		
		if (Player.GlobalPosition.X < -MaxDistanceFromCenter)
			Player.GlobalPosition = new Vector2(MaxDistanceFromCenter-10.0f, Player.GlobalPosition.Y);

		TimedPrint($"Player: {Player.GlobalPosition}\nGradient: {GradientObject.GlobalPosition}\n\n", 0.5f);
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

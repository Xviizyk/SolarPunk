using Godot;

public partial class DialogueTest : Node2D
{
	[Export] CanvasLayer canvas;
	private Dialogue controller;

	public override void _Ready()
	{
		controller = canvas.GetNodeOrNull<Dialogue>("Dialogue");
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("debug_start_dialogue_phrase1")) StartTestDialogue1();
	}
	
	public void StartTestDialogue1()
	{		
		if (controller == null)
		{
			GD.PrintErr("Критическая ошибка: Скрипт DialogueTest не видит узел Dialogue!");
			return;
		}
		
		var portrait = GD.Load<Texture2D>("res://Sprites/Canvas/Avatar/Unknown.png");
		
		controller.SetupDialogue(
			portrait, 
			new Color(0.2f, 0.2f, 0.2f), 
			"UNKNOWN", 
			"ADMIN", 
			"Система: [font_size=12][color=red]Активна[/color]. BBCode [b]работает[/b]![/font_size]", 
			2, 
			"left", 
			true
		);
	}
}

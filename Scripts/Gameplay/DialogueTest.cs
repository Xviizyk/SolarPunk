using Godot;

public partial class DialogueController : Node2D
{
    public void StartTestDialogue()
    {
        var controller = GetNode<DialogueController>("DialogueCanvas");
        var portrait = GD.Load<Texture2D>("res://path_to_avatar.png");
        
        controller.SetupDialogue(
            portrait, 
            "TETO", 
            "COMMANDER", 
            "Внимание! <red>Критическая ошибка</red> в системе. Но <b>TETO</b> теперь работает.", 
            new Color(0.1f, 0.5f, 0.8f), 
            2, 
            "left", 
            true
        );
    }
}
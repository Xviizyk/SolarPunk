using Godot;
using System;

public partial class Dialogue : Node2D
{
    private bool _allowDash = false;
	private bool _allowMove = false;
	private bool _allowSprint = false;
	private bool _allowJump = false;
	private bool _allowSit = false;
	private bool _allowGrab = false;

    [Export] public string GlitchTextAnimationChars = "!?№%@$#*&^+-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    [Export] public CharacterBody2D Player;
    [Export] public float DialogueWindowTextTime;
    [Export] public float DialogueWindowHeaderTimeLate;

    private PlayerMovement MovementScript;

    public override void _PhysicsProcess()
    {
        
    }

    private void DialogueStart(string Text, string Header)
    {
        Player.InputLock = true;
    }

    private void DialogueEnd()
    {
        Player.InputLock = false;
    }
}


// using Godot;
// using System;
// using System.Threading.Tasks;

// public partial class Dialogue : Control
// {
//     [Export] public PlayerMovement Player;
//     [Export] public RichTextLabel ContentLabel;
//     [Export] public Label HeaderLabel;
//     [Export] public Panel Background;

//     private string _targetText = "";
//     private bool _isTyping = false;
//     private readonly string _chars = "!?№%@$#*&^+-0123456789"; // Символы для "глюка"
//     private Random _rnd = new Random();

//     public override void _Ready()
//     {
//         // Скрываем всё в начале
//         Background.Modulate = new Color(1, 1, 1, 0);
//         HeaderLabel.Modulate = new Color(1, 1, 1, 0);
//         ContentLabel.Text = "";
//     }

//     public async void ShowDialogue(string text, string author)
//     {
//         if (_isTyping) return;
//         _isTyping = true;

//         // Блокируем игрока (используем твой InputLock)
//         if (Player != null) Player.InputLock = true;

//         // 1. Анимация появления фона (Fade In)
//         var tween = CreateTween();
//         tween.TweenProperty(Background, "modulate:a", 1.0f, 0.3f);
//         await ToSignal(tween, "finished");

//         // 2. Анимация появления автора (Вылет слева)
//         HeaderLabel.Text = author;
//         HeaderLabel.Position = new Vector2(-100, HeaderLabel.Position.Y); // Стартуем левее
//         var headerTween = CreateTween().SetParallel(true);
//         headerTween.TweenProperty(HeaderLabel, "modulate:a", 1.0f, 0.2f);
//         headerTween.TweenProperty(HeaderLabel, "position:x", 20f, 0.4f).SetTrans(Tween.TransitionType.Back);
//         await ToSignal(headerTween, "finished");

//         // 3. Эффект расшифровки текста
//         await RevealText(text);

//         _isTyping = false;
//     }

//     private async Task RevealText(string finalTarget)
//     {
//         ContentLabel.Text = "";
//         string currentDisplay = "";

//         for (int i = 0; i < finalTarget.Length; i++)
//         {
//             // Эффект "перебора" символа перед тем как зафиксировать нужную букву
//             for (int j = 0; j < 3; j++) // 3 итерации шума на одну букву
//             {
//                 char randomChar = _chars[_rnd.Next(_chars.Length)];
//                 ContentLabel.Text = currentDisplay + randomChar;
//                 await Task.Delay(20); // Задержка "глюка"
//             }

//             currentDisplay += finalTarget[i];
//             ContentLabel.Text = currentDisplay;
//         }
//     }
// }
using Godot;
using System;
using System.Text;
using System.Threading.Tasks;

public partial class DialogueController : Control
{
    [Export] public float GlitchSpeed = 0.03f;
    [Export] public int TicksPerChar = 3;
    [Export] public string GlitchChars = "!@#$%&*<>?[]{}~ABCDEFGHIJKLMNOPQRSTUVWXYZ啊吧从";

    private Label _nameLabel;
    private Label _messageLabel;
    private Control _dialogueBox;
    
    private string _targetText = "";
    private bool _isTyping = false;
    private bool _skipRequested = false;

    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("DialogueBox/HBoxContainer/TextContainer/Background/Header_Message/Header_Text/Header_Text");
        _messageLabel = GetNode<Label>("DialogueBox/HBoxContainer/TextContainer/Background/Background_Text/Message");
        _dialogueBox = GetNode<Control>("DialogueBox");

        float screenWidth = GetViewportRect().Size.X;
        _dialogueBox.Position = new Vector2(-screenWidth, _dialogueBox.Position.Y);
    }

    public async void SetupDialogue(string name, string message, Color themeColor)
    {
        if (_isTyping) return;

        _nameLabel.Text = name;
        _targetText = message;
        _messageLabel.Text = "";
        _isTyping = true;
        _skipRequested = false;
        _messageLabel.SelfModulate = themeColor;

        Tween tween = CreateTween();
        tween.TweenProperty(_dialogueBox, "position:x", 0f, 0.4f)
             .SetTrans(Tween.TransitionType.Back)
             .SetEase(Tween.EaseType.Out);
        
        await ToSignal(tween, "finished");
        
        await RunGlitchEffect();
    }

    private async Task RunGlitchEffect()
    {
        StringBuilder displayedText = new StringBuilder();
        Random random = new Random();

        for (int i = 0; i < _targetText.Length; i++)
        {
            if (_skipRequested) break;

            for (int tick = 0; tick < TicksPerChar; tick++)
            {
                if (_skipRequested) break;
                
                char randomChar = GlitchChars[random.Next(GlitchChars.Length)];
                _messageLabel.Text = displayedText.ToString() + randomChar;
                
                await ToSignal(GetTree().CreateTimer(GlitchSpeed), "timeout");
            }

            displayedText.Append(_targetText[i]);
            _messageLabel.Text = displayedText.ToString();
        }

        _messageLabel.Text = _targetText;
        _isTyping = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept"))
        {
            if (_isTyping) _skipRequested = true;
            else CloseDialog();
        }
    }

    public void CloseDialog()
    {
        float screenWidth = GetViewportRect().Size.X;
        Tween tween = CreateTween();
        tween.TweenProperty(_dialogueBox, "position:x", screenWidth + 100, 0.4f)
             .SetTrans(Tween.TransitionType.Back)
             .SetEase(Tween.EaseType.In);
    }
}
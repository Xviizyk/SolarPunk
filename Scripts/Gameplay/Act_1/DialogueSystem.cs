// using Godot;
// using System;
// using System.Text;
// using System.Threading.Tasks;

// public partial class DialogueSystem : CanvasLayer
// {
// 	[Export] public float GlitchSpeed = 0.03f;
// 	[Export] public int TicksPerChar = 3;
// 	[Export] public string GlitchChars = "!@#$%&*<>?[]{}~ABCDEFGHIJKLMNOPQRSTUVWXYZ啊吧从";

// 	private Control _dialogBox;
// 	private Label _nameLabel;
// 	private Label _textLabel;
// 	private Panel _namePanel;
// 	private Panel _textPanel;
	
// 	private StyleBoxFlat _textStyle;
// 	private StyleBoxFlat _nameStyle;

// 	private string _targetText = "";
// 	private bool _isTyping = false;
// 	private bool _skip = false;

// 	public override void _Ready()
// 	{
// 		_dialogBox = GetNode<Control>("DialogBox");
// 		_nameLabel = GetNode<Label>("DialogBox/NameBackground/NameText");
// 		_textLabel = GetNode<Label>("DialogBox/TextBackground/DialogText");
// 		_namePanel = GetNode<Panel>("DialogBox/NameBackground");
// 		_textPanel = GetNode<Panel>("DialogBox/TextBackground");

// 		_textStyle = (StyleBoxFlat)_textPanel.GetThemeStylebox("normal").Duplicate();
// 		_textPanel.AddThemeStyleboxOverride("normal", _textStyle);
// 		_nameStyle = (StyleBoxFlat)_namePanel.GetThemeStylebox("normal").Duplicate();
// 		_namePanel.AddThemeStyleboxOverride("normal", _nameStyle);

// 		float screenWidth = GetViewport().GetVisibleRect().Size.X;
// 		_dialogBox.Position = new Vector2(-screenWidth, _dialogBox.Position.Y);
// 	}

// 	public override void _Input(InputEvent @event)
// 	{
// 		if (@event.IsActionPressed("ui_accept"))
// 		{
// 			if (_isTyping) _skip = true;
// 			else CloseDialog();
// 		}

// 		if (@event.IsActionPressed("Sit"))
// 		{
// 			StartDialog("WWWWWWWWW", "Test message with glitch effect...", new Color(0.1f, 0.6f, 1.0f));
// 		}
// 	}

// 	public async void StartDialog(string charName, string message, Color color)
// 	{
// 		if (_isTyping)
// 			return;

// 		_nameLabel.Text = charName;
// 		_targetText = message;
// 		_textLabel.Text = "";
// 		_isTyping = true;
// 		_skip = false;
// 		_textStyle.BorderColor = color;
// 		_nameStyle.BorderColor = color;
// 		Tween tween = CreateTween();
// 		tween.TweenProperty(_dialogBox, "position:x", 0f, 0.4f)
// 			.SetTrans(Tween.TransitionType.Back)
// 			.SetEase(Tween.EaseType.Out);
		
// 		await ToSignal(tween, "finished");
// 		await RunGlitchEffect();
// 	}

// 	private async Task RunGlitchEffect()
// 	{
// 		StringBuilder displayedText = new StringBuilder();
// 		Random random = new Random();

// 		for (int i = 0; i < _targetText.Length; i++)
// 		{
// 			if (_skip)
// 				break;

// 			for (int tick = 0; tick < TicksPerChar; tick++)
// 			{
// 				if (_skip)
// 					break;
// 				char randomChar = GlitchChars[random.Next(GlitchChars.Length)];
// 				_textLabel.Text = displayedText.ToString() + randomChar;
// 				await ToSignal(GetTree().CreateTimer(GlitchSpeed), "timeout");
// 			}

// 			displayedText.Append(_targetText[i]);
// 			_textLabel.Text = displayedText.ToString();
// 		}

// 		_textLabel.Text = _targetText;
// 		_isTyping = false;
// 		_skip = false;
// 	}

// 	public void CloseDialog()
// 	{
// 		float screenWidth = GetViewport().GetVisibleRect().Size.X;
// 		Tween tween = CreateTween();
// 		tween.TweenProperty(_dialogBox, "position:x", screenWidth + 100, 0.4f)
// 			.SetTrans(Tween.TransitionType.Back)
// 			.SetEase(Tween.EaseType.In);
// 	}
// }

using Godot;
using System;

public partial class DialogueBox : Control
{
	[Export] public RichTextLabel MessageLabel;
	[Export] public Label NameLabel;
	[Export] public double TextSpeed = 0.05;

	private Tween _textTween;

	public override void _Ready()
	{
		DisplayText("TESTCHARACTER", "THIS IS TEST MESSAGE. GLORY TO TETO!");
	}

	public void DisplayText(string name, string text)
	{
		NameLabel.Text = name;
		MessageLabel.Text = text;
		
		MessageLabel.VisibleRatio = 0;

		if (_textTween != null && _textTween.IsRunning())
			_textTween.Kill();

		_textTween = CreateTween();
		_textTween.TweenProperty(MessageLabel, "visible_ratio", 1.0f, (float)(text.Length * TextSpeed))
				  .SetTrans(Tween.TransitionType.Linear);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept"))
		{
			if (MessageLabel.VisibleRatio < 1.0f)
			{
				_textTween.Kill();
				MessageLabel.VisibleRatio = 1.0f;
			}
			else
			{
				GD.Print("Next message...");
			}
		}
	}
}

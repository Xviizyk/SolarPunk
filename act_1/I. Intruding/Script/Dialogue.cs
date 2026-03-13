using Godot;
using System;
using System.Text;
using System.Threading.Tasks;

public partial class DialogueSystem : CanvasLayer
{
	[Export] public float GlitchSpeed = 0.03f;
	[Export] public int TicksPerChar = 3;
	[Export] public string GlitchChars = "!@#$%&*<>?[]{}~ABCDEFGHIJKLMNOPQRSTUVWXYZ€‚ƒ„…†‡ˆ‰ŠŒ‹•–—˜™š›œžŸ¡¢Ž";

	private Control _dialogBox;
	private Label _nameLabel;
	private Label _textLabel;

	private string _targetText = "";

	private bool _isTyping = false;
	private bool _skip = false;

	public override void _Ready()
	{
		_dialogBox = GetNode<Control>("DialogBox");
		_nameLabel = GetNode<Label>("DialogBox/NameBackground/NameText");
		_textLabel = GetNode<Label>("DialogBox/TextBackground/DialogText");

		float screenWidth = GetViewport().GetVisibleRect().Size.X;

		_dialogBox.Position = new Vector2(-screenWidth, _dialogBox.Position.Y);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept") && _isTyping)
			_skip = true;
	}

	public async void StartDialog(string charName, string message)
	{
		_nameLabel.Text = charName;
		_targetText = message;
		_textLabel.Text = "";
		_isTyping = true;
		_skip = false;

		Tween tween = CreateTween();
		tween.TweenProperty(_dialogBox, "position:x", 0, 0.5f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		await ToSignal(tween, "finished");
		await RunGlitchEffect();
	}

	private async Task RunGlitchEffect()
	{
		StringBuilder displayedText = new StringBuilder();
		Random random = new Random();

		for (int i = 0; i < _targetText.Length; i++)
		{
			if (_skip)
			{
				_textLabel.Text = _targetText;
				break;
			}

			for (int tick = 0; tick < TicksPerChar; tick++)
			{
				char randomChar = GlitchChars[random.Next(GlitchChars.Length)];
				_textLabel.Text = displayedText.ToString() + randomChar;
				await Task.Delay(TimeSpan.FromSeconds(GlitchSpeed));

				if (_skip)
					break;
			}

			displayedText.Append(_targetText[i]);
			_textLabel.Text = displayedText.ToString();
		}

		_isTyping = false;
	}

	public void CloseDialog()
	{
		float screenWidth = GetViewport().GetVisibleRect().Size.X;
		Tween tween = CreateTween();
		tween.TweenProperty(_dialogBox, "position:x", screenWidth, 0.5f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
	}
}

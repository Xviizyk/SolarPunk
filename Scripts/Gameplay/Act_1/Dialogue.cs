using Godot;
using System;
using System.Threading.Tasks;

public partial class Dialogue : Control
{
	[Export] public float GlitchSpeed = 0.03f;
	[Export] public int TicksPerChar = 2;

	private RichTextLabel _nameLabel;
	private RichTextLabel _rankLabel;
	private RichTextLabel _messageLabel;
	private TextureRect _portrait;
	private Control _dialogueBox;
	private HBoxContainer _hBox;
	
	private bool _isTyping = false;
	private bool _skipRequested = false;
	private bool _skippable = true;

	public override void _Ready()
	{
		_dialogueBox = GetNode<Control>("DialogueBox");
		_nameLabel = GetNode<RichTextLabel>("DialogueBox/TextContainer/Background/Header_Message/Header_Text/Header_Text");
		_messageLabel = GetNode<RichTextLabel>("DialogueBox/TextContainer/Background/Background_Text/Message");
		_rankLabel = GetNode<RichTextLabel>("DialogueBox/PortraitContainer/Background/Header_Avatar/Header_Text/Header_Text");
		_portrait = GetNode<TextureRect>("DialogueBox/PortraitContainer/Background/Character");

		_messageLabel.BbcodeEnabled = true;
		_dialogueBox.GlobalPosition = new Vector2(-_dialogueBox.Size.X - 500, _dialogueBox.GlobalPosition.Y);
	}

	/// <summary>
	/// Function for start dialogue window
	/// Usage: SetupDialogue(Texture2D texture, Color color, string speaker_name, string speaker_role, string speaker_phrase, int vertical_pos, string horizontal_pos_side, bool skipable);
	/// </summary>
	public async void SetupDialogue(Texture2D texture, Color themeColor, string name = "ERROR: NAME WASN'T FOUND.", string role = "ERROR: ROLE WASN'T FOUND.", string message = "ERROR: TEXT WASN'T FOUND.", int vPos = 0, string side = "left", bool skippable = true)
	{
		if (_isTyping) return;

		_portrait.Texture = texture;
		_nameLabel.Text = name;
		_rankLabel.Text = role;
		_messageLabel.Text = message;
		_messageLabel.VisibleCharacters = 0;
		_skippable = skippable;

		if (themeColor != new Color(0f, 0f, 0f)) ApplyThemeColor(themeColor);
		AdjustLayout(vPos, side);

		await ToSignal(GetTree(), "process_frame");

		Vector2 finalPos = _dialogueBox.GlobalPosition;
		_dialogueBox.GlobalPosition = new Vector2(-_dialogueBox.Size.X - 100, finalPos.Y);

		Tween tween = CreateTween();
		tween.TweenProperty(_dialogueBox, "global_position:x", finalPos.X, 0.4f)
			 .SetTrans(Tween.TransitionType.Back)
			 .SetEase(Tween.EaseType.Out);
		
		await ToSignal(tween, "finished");
		await RunGlitchEffect();
	}

	/// <summary>
	/// Makes glitch effect before real character.
	/// </summary>
	private async Task RunGlitchEffect()
	{
		_isTyping = true;
		_skipRequested = false;
		int totalChars = _messageLabel.GetTotalCharacterCount();

		for (int i = 0; i <= totalChars; i++)
		{
			if (_skippable && _skipRequested) break;
			for (int tick = 0; tick < TicksPerChar; tick++)
			{
				if (_skippable && _skipRequested) break;
				await ToSignal(GetTree().CreateTimer(GlitchSpeed), "timeout");
			}
			_messageLabel.VisibleCharacters = i;
		}
		_messageLabel.VisibleCharacters = -1; 
		_isTyping = false;
	}

	private void ApplyThemeColor(Color color)
	{
		string[] backgrounds = { "DialogueBox/TextContainer/Background",
								 "DialogueBox/TextContainer/Background/Background_Text",
								 "DialogueBox/TextContainer/Background/Header_Message",
								 "DialogueBox/TextContainer/Background/Header_Message/Header_Text",
								 "DialogueBox/PortraitContainer/Background",
								 "DialogueBox/PortraitContainer/Background/Header_Avatar",
								 "DialogueBox/PortraitContainer/Background/Header_Avatar/Header_Text"
		};
		
		foreach (var path in backgrounds)
		{
			GetNode<Control>(path).SelfModulate = color;
		}
	}

	private void AdjustLayout(int vPos, string side)
	{
		var portraitContainer = GetNode<Control>("DialogueBox/PortraitContainer");

		switch (vPos)
		{
			case 0: _dialogueBox.SetAnchorsAndOffsetsPreset(LayoutPreset.TopWide); break;
			case 1: _dialogueBox.SetAnchorsAndOffsetsPreset(LayoutPreset.Center); break;
			default: _dialogueBox.SetAnchorsAndOffsetsPreset(LayoutPreset.BottomWide); break;
		}
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
		tween.TweenProperty(_dialogueBox, "global_position:x", screenWidth + 100, 0.4f)
			 .SetTrans(Tween.TransitionType.Back)
			 .SetEase(Tween.EaseType.In);
	}
}

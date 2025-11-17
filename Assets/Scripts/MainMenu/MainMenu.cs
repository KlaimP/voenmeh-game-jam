using Godot;
using System;

public partial class MainMenu : Control
{
	[Export]
	public AudioStream HoverSound { get; set; }
	
	[Export]
	public AudioStream ClickSound { get; set; }
	
	[Export]
	public Button PlayButton { get; set; }
	[Export]
	public Button ExitButton { get; set; }
	[Export]
	public Button EducationButton { get; set; }
	private AudioStreamPlayer audioPlayer;

	public override void _Ready()
	{
		// Создаем аудиоплеер для UI звуков
		audioPlayer = new AudioStreamPlayer();
		AddChild(audioPlayer);
		
		var playButton = PlayButton;
		var exitButton = ExitButton;
		var educationButton = EducationButton;

		playButton.Pressed += OnPlayButtonPressed;
		exitButton.Pressed += OnExitButtonPressed;
		educationButton.Pressed += OnEducationButtonPressed;

		// Подключаем сигналы наведения мыши
		playButton.MouseEntered += () => OnButtonHover();
		exitButton.MouseEntered += () => OnButtonHover();
		educationButton.MouseEntered += () => OnButtonHover();
		
		// ВОСПРОИЗВОДИМ МУЗЫКУ ГЛАВНОГО МЕНЮ
		if (MusicManager.Instance != null)
		{
			MusicManager.Instance.PlayDefaultMusic();
		}
	}

	private void OnButtonHover()
	{
		if (HoverSound != null)
		{
			audioPlayer.Stream = HoverSound;
			audioPlayer.VolumeDb = -20.0f;
			audioPlayer.Play();
		}
	}

	private void PlayClickSound()
	{
		if (ClickSound != null)
		{
			audioPlayer.Stream = ClickSound;
			audioPlayer.VolumeDb = -20.0f;
			audioPlayer.Play();
		}
	}

	private async void OnPlayButtonPressed()
	{
		PlayClickSound();
		await ToSignal(GetTree().CreateTimer(0.1), "timeout");
		GetTree().ChangeSceneToFile("res://Assets/Scripts/MainMenu/LevelSelection.tscn");
	}

	private async void OnEducationButtonPressed()
    {
        PlayClickSound();
		await ToSignal(GetTree().CreateTimer(0.1), "timeout");
		GetTree().ChangeSceneToFile("res://Assets/Scripts/MainMenu/Education.tscn");
    }

	private void OnExitButtonPressed()
	{
		PlayClickSound();
		GetTree().Quit();
	}
}

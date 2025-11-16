using Godot;
using System;

public partial class MainMenu : Control
{
	[Export]
	public AudioStream HoverSound { get; set; }
	
	[Export]
	public AudioStream ClickSound { get; set; }
	
	private AudioStreamPlayer audioPlayer;

	public override void _Ready()
	{
		// Создаем аудиоплеер для UI звуков
		audioPlayer = new AudioStreamPlayer();
		AddChild(audioPlayer);
		
		var playButton = GetNode<Button>("PlayButton");
		var exitButton = GetNode<Button>("ExitButton");

		playButton.Pressed += OnPlayButtonPressed;
		exitButton.Pressed += OnExitButtonPressed;
		
		// Подключаем сигналы наведения мыши
		playButton.MouseEntered += () => OnButtonHover();
		exitButton.MouseEntered += () => OnButtonHover();
		
		// Запускаем музыку при загрузке главного меню
		MusicManager.Instance.PlayMusic();
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

	private void OnExitButtonPressed()
	{
		PlayClickSound();
		GetTree().Quit();
	}
}
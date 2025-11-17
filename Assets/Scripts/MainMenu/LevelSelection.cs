using Godot;
using System;
using System.Threading.Tasks;

public partial class LevelSelection : Control
{
	[Export]
	public Button Level1Button { get; set; }
	
	[Export]
	public Button Level2Button { get; set; }
	
	[Export]
	public Button Level3Button { get; set; }
	
	[Export]
	public Button Level4Button { get; set; }
	
	[Export]
	public Button Level5Button { get; set; }
	
	[Export]
	public Button BackButton { get; set; }

	// Добавляем звуковые ресурсы
	[Export]
	public AudioStream HoverSound { get; set; }
	
	[Export]
	public AudioStream ClickSound { get; set; }
	
	// AudioStreamPlayer для воспроизведения
	private AudioStreamPlayer audioPlayer;

	public override void _Ready()
	{
		// Создаем аудиоплеер
		audioPlayer = new AudioStreamPlayer();
		AddChild(audioPlayer);
		
		// Подключаем сигналы кнопок уровней
		Level1Button.Pressed += () => OnLevelButtonPressed(1);
		Level2Button.Pressed += () => OnLevelButtonPressed(2);
		Level3Button.Pressed += () => OnLevelButtonPressed(3);
		Level4Button.Pressed += () => OnLevelButtonPressed(4);
		Level5Button.Pressed += () => OnLevelButtonPressed(5);
		
		BackButton.Pressed += OnBackButtonPressed;
		
		// Подключаем сигналы наведения для всех кнопок
		ConnectHoverSignals();

		// Убеждаемся, что музыка играет
		MusicManager.Instance.PlayMusic();
	}

	private void ConnectHoverSignals()
	{
		Level1Button.MouseEntered += () => OnButtonHover();
		Level2Button.MouseEntered += () => OnButtonHover();
		Level3Button.MouseEntered += () => OnButtonHover();
		Level4Button.MouseEntered += () => OnButtonHover();
		Level5Button.MouseEntered += () => OnButtonHover();
		BackButton.MouseEntered += () => OnButtonHover();
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

	private async void OnLevelButtonPressed(int levelNumber)
	{
		// Воспроизводим звук клика
		PlayClickSound();
		
		GD.Print($"Загружается уровень {levelNumber}");
		
		// Небольшая задержка для воспроизведения звука перед сменой сцены
		await ToSignal(GetTree().CreateTimer(0.1), "timeout");
		
		// Здесь загружаем соответствующий уровень
		switch (levelNumber)
		{
			case 1:
				GetTree().ChangeSceneToFile("res://Assets/Scripts/Levels/Level1/LvL1.tscn");
				break;
			case 2:
				GetTree().ChangeSceneToFile("res://Assets/Scripts/Levels/Level2/LvL2.tscn");
				break;
			case 3:
				GetTree().ChangeSceneToFile("res://Assets/Scripts/Levels/Level3/LvL3.tscn");
				break;
			case 4:
				GetTree().ChangeSceneToFile("res://Assets/Scripts/Levels/Level4/LvL4.tscn");
				break;
			case 5:
				GetTree().ChangeSceneToFile("res://Assets/Scripts/Levels/Level5/LvL5.tscn");
				break;
		}
	}

	private async void OnBackButtonPressed()
	{
		// Воспроизводим звук клика
		PlayClickSound();
		
		// Небольшая задержка для воспроизведения звука перед сменой сцены
		await ToSignal(GetTree().CreateTimer(0.1), "timeout");

		// Возвращаемся в главное меню
		GetTree().ChangeSceneToFile("res://Assets/Scripts/MainMenu/MainMenu.tscn");
	}
}

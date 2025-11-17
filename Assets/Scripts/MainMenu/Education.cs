using Godot;

public partial class Education : Control
{
    [Export] public Button BackButton { get; set; }
    [Export] public Label PageIndicator { get; set; }
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

		BackButton.Pressed += OnBackButtonPressed;
		BackButton.MouseEntered += () => OnButtonHover();

		// Убеждаемся, что музыка играет
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
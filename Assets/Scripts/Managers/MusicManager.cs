using Godot;

public partial class MusicManager : Node
{
    private AudioStreamPlayer musicPlayer;
    
    [Export] public AudioStream BackgroundMusic { get; set; }
    
    public static MusicManager Instance { get; private set; }
    
    // Текущая играющая музыка
    public AudioStream CurrentMusic { get; private set; }
    
    public float MusicVolume
    {
        get => musicPlayer.VolumeDb;
        set => musicPlayer.VolumeDb = value;
    }
    
    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
            musicPlayer = new AudioStreamPlayer();
            AddChild(musicPlayer);
            
            ProcessMode = ProcessModeEnum.Always;
            
            // Подписываемся на окончание музыки для зацикливания
            musicPlayer.Finished += OnMusicFinished;
            
            // Загружаем музыку если она назначена в инспекторе
            if (BackgroundMusic != null)
            {
                CurrentMusic = BackgroundMusic;
                musicPlayer.Stream = BackgroundMusic;
                musicPlayer.Autoplay = true;
                musicPlayer.VolumeDb = -35.0f;
            }
        }
        else
        {
            QueueFree();
        }
    }
    
    // Обработчик сигнала окончания музыки
    private void OnMusicFinished()
    {
        // Перезапускаем музыку, когда она заканчивается
        musicPlayer.Play();
    }
    
    public void PlayMusic()
    {
        if (!musicPlayer.Playing && BackgroundMusic != null)
        {
            CurrentMusic = BackgroundMusic;
            musicPlayer.Stream = BackgroundMusic;
            musicPlayer.Play();
        }
    }
    
    public void StopMusic()
    {
        musicPlayer.Stop();
    }
    
    // Метод для смены музыки
    public void SetMusic(AudioStream newMusic)
    {
        // Если это та же музыка, ничего не делаем
        if (newMusic == CurrentMusic && musicPlayer.Playing)
            return;
        
        CurrentMusic = newMusic;
        musicPlayer.Stream = newMusic;
        musicPlayer.Play();
    }
    
    // Метод для возврата к музыке по умолчанию (главного меню)
    public void PlayDefaultMusic()
    {
        SetMusic(BackgroundMusic);
    }
}
using Godot;

public partial class MusicManager : Node
{
    private AudioStreamPlayer musicPlayer;
    
    [Export] public AudioStream BackgroundMusic { get; set; }
    
    public static MusicManager Instance { get; private set; }
    
    // Громкость музыки (можно регулировать)
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
            
            // Устанавливаем режим обработки для работы между сценами
            ProcessMode = ProcessModeEnum.Always;
            
            // Загружаем музыку если она назначена в инспекторе
            if (BackgroundMusic != null)
            {
                // В Godot 4 зацикливание настраивается через AudioStreamPlayer
                musicPlayer.Stream = BackgroundMusic;
                musicPlayer.Autoplay = true;
                musicPlayer.VolumeDb = -35.0f;
                
                // Подключаем сигнал для зацикливания
                musicPlayer.Finished += OnMusicFinished;
            }
        }
        else
        {
            // Если уже есть экземпляр, удаляем дубликат
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
            musicPlayer.Stream = BackgroundMusic;
            musicPlayer.Play();
        }
    }
    
    public void StopMusic()
    {
        musicPlayer.Stop();
    }
    
    public void SetMusic(AudioStream newMusic)
    {
        bool wasPlaying = musicPlayer.Playing;
        
        musicPlayer.Stream = newMusic;
        
        if (wasPlaying)
        {
            musicPlayer.Play();
        }
    }
}
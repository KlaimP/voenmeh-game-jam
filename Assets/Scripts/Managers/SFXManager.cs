// SFXManager.cs - добавьте в Autoload
using Godot;
using System.Collections.Generic;

public partial class SFXManager : Node
{
    private List<AudioStreamPlayer> audioPlayers = new List<AudioStreamPlayer>();
    private int poolSize = 5; // Количество одновременных звуков

    public static SFXManager Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Создаем пул аудиоплееров для одновременного воспроизведения нескольких звуков
            for (int i = 0; i < poolSize; i++)
            {
                AudioStreamPlayer player = new AudioStreamPlayer();
                AddChild(player);
                audioPlayers.Add(player);
            }
            
            ProcessMode = ProcessModeEnum.Always;
        }
        else
        {
            QueueFree();
        }
    }

    public void PlaySound(AudioStream sound, float volumeDb = -20.0f)
    {
        if (sound == null) return;

        // Ищем свободный аудиоплеер
        foreach (var player in audioPlayers)
        {
            if (!player.Playing)
            {
                player.Stream = sound;
                player.VolumeDb = volumeDb;
                player.Play();
                return;
            }
        }
        
        // Если все заняты, создаем дополнительный (на всякий случай)
        GD.Print("SFXManager: Все аудиоплееры заняты, создаем дополнительный");
        AudioStreamPlayer newPlayer = new AudioStreamPlayer();
        AddChild(newPlayer);
        audioPlayers.Add(newPlayer);
        newPlayer.Stream = sound;
        newPlayer.VolumeDb = volumeDb;
        newPlayer.Play();
    }
}
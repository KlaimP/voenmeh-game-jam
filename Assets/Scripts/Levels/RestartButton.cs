using Godot;

public partial class RestartButton : Button
{
    public override void _Ready()
    {
        this.Pressed += OnRestartPressed;
    }
    
    private void OnRestartPressed()
    {
        var levelScene = GetTree().CurrentScene as LevelScene;
        if (levelScene != null)
        {
            levelScene.RestartLevel();
        }
        else
        {
            GD.PrintErr("LevelScene не найден!");
        }
    }
}
using Godot;

public partial class LevelCompleteButton : Button
{
    public override void _Ready()
    {
        this.Pressed += OnCheckPressed;
    }
    
    private void OnCheckPressed()
    {
        var levelScene = GetTree().CurrentScene as LevelScene;
        if (levelScene != null)
        {
            levelScene.OnLevelCompletionCheck();
        }
    }
}
using Godot;
using System.Threading.Tasks;

/* Объект - Ящик
   Наследование: GridObject
*/
public partial class BoxObject : GridObject
{
    // Инициализация
    public override void _Ready()
    {
        ObjectType = "BOX";
        IsSolid = true;     // Твёрдый
        CanBePushed = true; // Можно двигать
        base._Ready();
    }

    // Переопределяем анимацию разрушения для ящика
    public override async Task PlayDestructionEffects()
    {
        GD.Print("🎁 Специальная анимация разрушения ящика!");
        
        // Например, вращение при исчезновении
        var tween = CreateTween();
        tween.Parallel().TweenProperty(this, "scale", Vector2.Zero, 0.3f);
        tween.Parallel().TweenProperty(this, "rotation", Mathf.Pi * 2, 0.3f);
        tween.Parallel().TweenProperty(this, "modulate", new Color(1, 0.5f, 0, 0.5f), 0.3f);
        
        await ToSignal(tween, "finished");
    }

    // Или можно переопределить весь метод разрушения
    public override async Task OnDestroyed()
    {
        GD.Print($"🎁 Ящик разрушается со спецэффектами в {GridPosition}");
        await PlayDestructionEffects();
    }
}
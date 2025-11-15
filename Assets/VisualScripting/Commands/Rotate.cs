using Godot;
using System;

namespace VoenmehGameJam.Scripts
{
    public class Rotate : BlockScriptClass
    {
        public Rotate() : base("Rotate", "Поворачивает робота на 90 градусов по часовой стрелке\n") { }

        public override void Execute(Robot robot)
        {
            GD.Print("Script: \"Rotate\"");
            if (next != null)
            {
                next.Execute(robot);
            }
        }
    }
}

using Godot;
using System;

namespace VoenmehGameJam.Scripts
{
    public class RotateCounterСlockwise : BlockScriptClass
    {
        public RotateCounterСlockwise() : base("RotateLeft()", "Поворачивает робота на 90 градусов по часовой стрелке\n") { }

        public async override void Execute(Robot robot)
        {
            GD.Print("Script: \"Rotate -90\"");

            await robot.TurnLeft();

            if (next != null)
            {
                next.Execute(robot);
            }
        }
    }
}

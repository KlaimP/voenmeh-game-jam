using Godot;
using System;

namespace VoenmehGameJam.Scripts
{
	public class RotateСlockwise : BlockScriptClass
	{
		public RotateСlockwise() : base("RotateRight()", "Поворачивает робота на 90 градусов по часовой стрелке\n") { }

		public async override void Execute(Robot robot)
		{
			GD.Print("Script: \"Rotate 90\"");
			await robot.TurnRight();
			if (next != null)
			{
				next.Execute(robot);
			}
		}
	}
}

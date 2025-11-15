using Godot;
using System;

namespace VoenmehGameJam.Scripts
{
	public class ForwardMove : BlockScriptClass
	{
		public ForwardMove() : base("ForwardMove", "Команда заставляет робота переместиться вперёд на одну клетку по сетке.\n") { }

		public async override void Execute(Robot robot)
		{
			GD.Print("Script: \"ForwardMove\"");
			await robot.MoveForward();


			if (next != null)
			{
				next.Execute(robot);
			}
		}
	}
}

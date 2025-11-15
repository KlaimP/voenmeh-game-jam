using Godot;
using System;


namespace VoenmehGameJam.Scripts
{
	public class Start : BlockScriptClass
	{
		public Start() : base("Start()", "Начинает выполнение программы\r") { }

		public override void Execute(Robot robot)
		{
			GD.Print("Script: \"Start\"");
			if (next != null)
			{
				next.Execute(robot);
			}
		}
	}
}

using Godot;
using System;


namespace VoenmehGameJam.Scripts
{
	public class Start : BlockScriptClass
	{
		public Start() : base("Start", "Начинает выполнение программы") { }

		public override void Execute(Robot robot)
		{
			if (next != null)
			{
				next.Execute(robot);
			}
		}
	}
}

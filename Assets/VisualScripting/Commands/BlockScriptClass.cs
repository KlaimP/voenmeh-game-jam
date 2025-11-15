using Godot;
using Godot.Collections;
using System;

namespace VoenmehGameJam.Scripts
{
	public abstract class BlockScriptClass
	{
		public BlockScriptClass? next;
		public string Name { get; } = "";
		public string Description { get; } = "";

		protected BlockScriptClass(string name, string description)
		{
			Name = name;
			Description = description;
		}
		public abstract void Execute(Robot robot);

	}
}

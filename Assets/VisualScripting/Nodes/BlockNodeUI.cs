using Godot;
using System;
using VoenmehGameJam.Scripts;

public interface IBlockNode
{
	BlockScriptClass Block { get; }
}

public partial class BlockNodeUI : GraphNode, IBlockNode
{
	public BlockScriptClass Block { get; private set; }

	public void Initialize(BlockScriptClass block, bool leftPort, bool rightPort)
	{
		Block = block;
		this.Title = block.Name;

		this.SetSlot(0, leftPort, 0, Colors.Green, rightPort, 0, Colors.Red);
	}
}

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

	public void Initialize(BlockScriptClass block)
	{
		Block = block;
		this.Title = block.Name;

		this.SetSlot(0, true, 0, Colors.Green, true, 0, Colors.Red);
	}
}

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

		if(block is ForwardMove forwardMove)
		{
			HSplitContainer splitContainer = (HSplitContainer)ResourceLoader.Load<PackedScene>("res://Assets/Scripts/VisualScripting/Nodes/Childs/graphNodeChild.tscn").Instantiate();
			this.AddChild(splitContainer);

			SpinBox stepsSpinBox = splitContainer.GetNode<SpinBox>("SpinBox");
			Label label = splitContainer.GetNode<Label>("Label");

			label.Text = "Steps";
			// Устанавливаем текущее значение
			stepsSpinBox.Value = forwardMove.Step;

			stepsSpinBox.ValueChanged += (val) =>
			{
				forwardMove.Step = (int)val;
				GD.Print($"ForwardMove Steps updated: {forwardMove.Step}");
			};
		}
	}
}

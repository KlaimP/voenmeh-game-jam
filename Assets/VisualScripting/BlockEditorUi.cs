using Godot;
using System;
using System.Collections.Generic;
using VoenmehGameJam.Scripts;

public partial class BlockEditorUi : Control
{
	[Export]
	public PackedScene GraphNode;

	[Export]
	private OptionButton SelectCommandButton;

	[Export]
	private Button AddButton;

	[Export]
	private Button StartGame;

	private GraphEdit graphEdit;

	private Start startScript = new Start();


	private GlobalSignals globals;

	public override void _Ready()
	{
		globals = GetNode("/root/GlobalSignals") as GlobalSignals;
		globals.StartGame += StartGameReceived;

		graphEdit = GetNode<GraphEdit>("GraphEdit");

		graphEdit.ConnectionRequest += OnConnectionRequest;
		graphEdit.DisconnectionRequest += OnDisconnectionRequest;
		StartGame.ButtonDown += () =>
		{
			globals.EmitSignal(GlobalSignals.SignalName.StartGame);
		};
		

		BlockNodeUI StartBlock = (BlockNodeUI)GraphNode.Instantiate();

		StartBlock.Initialize(startScript, false, true);

		graphEdit.AddChild(StartBlock);

		SelectCommandButton.AddItem("ForwardMove");
		SelectCommandButton.AddItem("Rotate");
		AddButton.ButtonDown += OnCommandSelected;
	}

	private void StartGameReceived()
	{
		startScript.Execute(new Robot());
	}

	private void OnCommandSelected()
	{
		int index = SelectCommandButton.Selected;

		string commandName = SelectCommandButton.GetItemText((int)index);
		GD.Print("Click	");

		BlockScriptClass command = commandName switch
		{
			"ForwardMove" => new ForwardMove(),
			"Rotate" => new Rotate(),
			_ => null
		};

		if (command == null)
		{
			GD.Print("Command null");
			return;
		}

		BlockNodeUI newBlock = (BlockNodeUI)GraphNode.Instantiate();
		newBlock.Initialize(command, true, true);
		graphEdit.AddChild(newBlock);

	}

	private void OnConnectionRequest(Godot.StringName fromNode, long fromSlot, Godot.StringName toNode, long toSlot)
	{
		GD.Print("OnConnectionRequest");

		var fromGraphNode = graphEdit.GetNode<GraphNode>(fromNode.ToString());
		var toGraphNode = graphEdit.GetNode<GraphNode>(toNode.ToString());

		var fromBlock = (fromGraphNode as IBlockNode)?.Block;
		var toBlock = (toGraphNode as IBlockNode)?.Block;

		foreach (var child in graphEdit.GetChildren())
		{
			if (child is BlockNodeUI node)
			{
				var blockChild = (BlockNodeUI)child;
				if (blockChild.Block.next == toBlock)
				{
					GD.Print("Нода: " + node.Title);
					return;
				}
			}
		}

		//var fromBlock = (fromNode as IBlockNode)?.Block;
		//var toBlock = (toNode as IBlockNode)?.Block;

		if (fromBlock.next != null)
		{
			GD.Print("Уже есть связь");
		} 
		else if (fromBlock != null && toBlock != null)
		{
			fromBlock.next = toBlock;
			graphEdit.ConnectNode(fromNode, (int)fromSlot, toNode, (int)toSlot);
			GD.Print($"{fromBlock.Name} {fromSlot} -> {toBlock.Name} {toSlot}");
		}
	}

	private void OnDisconnectionRequest(Godot.StringName fromNode, long fromSlot, Godot.StringName toNode, long toSlot)
	{
		GD.Print("OnDisconnectionRequest");

		var fromGraphNode = graphEdit.GetNode<GraphNode>(fromNode.ToString());
		var toGraphNode = graphEdit.GetNode<GraphNode>(toNode.ToString());

		var fromBlock = (fromGraphNode as IBlockNode)?.Block;
		var toBlock = (toGraphNode as IBlockNode)?.Block;

		if (fromBlock != null && toBlock != null)
		{
			fromBlock.next = null;
			graphEdit.DisconnectNode(fromNode, (int)fromSlot, toNode, (int)toSlot);
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("Delete"))
		{
			GD.Print("Delete");
			foreach(var x in graphEdit.GetChildren())
			{
				if(x is GraphNode)
				{
					GraphNode node = (GraphNode)x;
					BlockNodeUI nodeUI = (BlockNodeUI)x;
					if (node.Selected)
					{
						graphEdit.RemoveChild(x);
						nodeUI.Block.next = null;
					}
				}
			}

		}
	}
}

using Godot;
using System;
using System.Collections.Generic;
using VoenmehGameJam.Scripts;

public partial class BlockEditorUi : GraphEdit
{
	[Export]
	public PackedScene GraphNode;

	[Export]
	private OptionButton SelectCommandButton;

	[Export]
	private Button AddButton;

	[Export]
	private Button StartGame;

	[Export]
	public Robot Robot;

	private Start startScript = new Start();


	private GlobalSignals globals;

	public override void _Ready()
	{
		globals = GetNode("/root/GlobalSignals") as GlobalSignals;
		globals.StartGame += StartGameReceived;


		this.ConnectionRequest += OnConnectionRequest;
		this.DisconnectionRequest += OnDisconnectionRequest;
		StartGame.ButtonDown += () =>
		{
			globals.EmitSignal(GlobalSignals.SignalName.StartGame);
		};
		

		BlockNodeUI StartBlock = (BlockNodeUI)GraphNode.Instantiate();

		StartBlock.Initialize(startScript, false, true);

		this.AddChild(StartBlock);

		SelectCommandButton.AddItem("ForwardMove");
		SelectCommandButton.AddItem("Rotate");
		AddButton.ButtonDown += OnCommandSelected;
	}

	private void StartGameReceived()
	{
		startScript.Execute(Robot);
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
		this.AddChild(newBlock);

	}

	private void OnConnectionRequest(Godot.StringName fromNode, long fromSlot, Godot.StringName toNode, long toSlot)
	{
		GD.Print("OnConnectionRequest");

		var fromGraphNode = this.GetNode<GraphNode>(fromNode.ToString());
		var toGraphNode = this.GetNode<GraphNode>(toNode.ToString());

		var fromBlock = (fromGraphNode as IBlockNode)?.Block;
		var toBlock = (toGraphNode as IBlockNode)?.Block;

		foreach (var child in this.GetChildren())
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
			this.ConnectNode(fromNode, (int)fromSlot, toNode, (int)toSlot);
			GD.Print($"{fromBlock.Name} {fromSlot} -> {toBlock.Name} {toSlot}");
		}
	}

	private void OnDisconnectionRequest(Godot.StringName fromNode, long fromSlot, Godot.StringName toNode, long toSlot)
	{
		GD.Print("OnDisconnectionRequest");

		var fromGraphNode = this.GetNode<GraphNode>(fromNode.ToString());
		var toGraphNode = this.GetNode<GraphNode>(toNode.ToString());

		var fromBlock = (fromGraphNode as IBlockNode)?.Block;
		var toBlock = (toGraphNode as IBlockNode)?.Block;

		if (fromBlock != null && toBlock != null)
		{
			fromBlock.next = null;
			this.DisconnectNode(fromNode, (int)fromSlot, toNode, (int)toSlot);
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("Delete"))
		{
			GD.Print("Delete");
			foreach(var x in this.GetChildren())
			{
				if(x is GraphNode)
				{
					GraphNode node = (GraphNode)x;
					BlockNodeUI nodeUI = (BlockNodeUI)x;
					if (node.Selected && nodeUI.Title != "Start")
					{
						GD.Print(nodeUI.Name);
						this.RemoveChild(x);
						nodeUI.Block.next = null;
					}
				}
			}

		}
	}
}

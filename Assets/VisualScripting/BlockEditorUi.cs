using Godot;
using System;
using System.Collections.Generic;
using VoenmehGameJam.Scripts;

public partial class BlockEditorUi : Control
{
	[Export]
	public PackedScene GraphNode;

	private GraphEdit graphEdit;

	private Start startScript = new Start();
	public override void _Ready()
	{
		graphEdit = GetNode<GraphEdit>("GraphEdit");

		graphEdit.ConnectionRequest += OnConnectionRequest;
		graphEdit.DisconnectionRequest += OnDisconnectionRequest;
		

		BlockNodeUI StartBlock = (BlockNodeUI)GraphNode.Instantiate();

		StartBlock.Initialize(startScript);

		graphEdit.AddChild(StartBlock);

		
		ForwardMove forwardScript = new ForwardMove();

		BlockNodeUI ForwardBlock = (BlockNodeUI)GraphNode.Instantiate();

		ForwardBlock.Initialize(forwardScript);

		graphEdit.AddChild(ForwardBlock);

		
		ForwardMove forwardScript2 = new ForwardMove();

		BlockNodeUI ForwardBlock2 = (BlockNodeUI)GraphNode.Instantiate();

		ForwardBlock2.Initialize(forwardScript2);

		graphEdit.AddChild(ForwardBlock2);

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
}

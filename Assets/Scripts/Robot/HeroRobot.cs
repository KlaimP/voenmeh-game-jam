using Godot;
using System;

public partial class HeroRobot : Node2D
{
	[Export]
	public Node2D UpNode { get; set; }
	[Export]
	public Node2D DownNode { get; set; }
	[Export]
	public Node2D RightNode {  get; set; }
	[Export]
	public Node2D LeftNode {  get; set; }


	public void SetUpDirection()
	{
		UpNode.Visible = true;
		DownNode.Visible = false;
		RightNode.Visible = false;
		LeftNode.Visible = false;
	}
	public void SetDownDirection()
	{
		DownNode.Visible = true;
		UpNode.Visible = false;
		RightNode.Visible = false;
		LeftNode.Visible = false;
	}
	public void SetRightDirection()
	{
		RightNode.Visible = true;
		LeftNode.Visible = false;
		UpNode.Visible= false;
		DownNode.Visible= false;
	}
	public void SetLeftDirection()
	{
		LeftNode.Visible = true;
		DownNode.Visible = false;
		RightNode.Visible = false;
		UpNode.Visible = false;
	}
}

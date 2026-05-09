using Godot;
using System;

public partial class Camera : Camera2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Get the Parent Game Node
		Game game = this.GetParent<Game>();

		// Center Camera on the Map
		int x = (game.MapWidth / 2) * game.Map.TileSet.TileSize.X;
		int y = (game.MapHeight / 2) * game.Map.TileSet.TileSize.Y;
		this.Position = new Vector2(x, y);
	}
}

using Godot;
using System;

public partial class Spirit : Sprite2D
{
    // Spirit Properties
    public Vector2I MapPos { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    // Setter for Spirit's Map Position given a World Position
    public void SetSpiritMapPosition(Vector2 worldPos)
	{
        // Get the Parent Game Node
        Game game = GetParent().GetParent<Game>();
        Vector2I tileSize = game.Map.TileSet.TileSize; // Tile Size

        // Convert World Position to Map Position
        int mapX = (int)Mathf.Floor(worldPos.X / tileSize.X);
        int mapY = (int)Mathf.Floor(worldPos.Y / tileSize.Y);
        Vector2I tilePos = game.ClampToMapBounds(new Vector2I(mapX, mapY));

        // Set the Spirit's Map Position
        this.MapPos = tilePos;
        this.Position = tilePos * tileSize;
    }

    // Setter for Spirit's Map Position given a Map Position
    public void SetSpiritMapPosition(Vector2I tilePos)
    {
        // Get the Parent Game Node
        Game game = GetParent().GetParent<Game>();
        Vector2I tileSize = game.Map.TileSet.TileSize; // Tile Size

        // Set the Spirit's Map Position
        this.MapPos = tilePos;
        this.Position = tilePos * tileSize;
    }
}

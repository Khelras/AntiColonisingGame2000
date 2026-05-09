using Godot;
using System;

public partial class Game : Node2D
{
	// Map Dimensions (in Tiles)
	[Export] public int MapWidth = 12;
	[Export] public int MapHeight = 8;

	// Reference to the TileMapLayer
	[Export] public TileMapLayer Map;

	// Tile IDs
	public enum TileType
	{
		Healthy = 0,
		Damaged = 1,
		Village = 2,
		Outpost = 3
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (this.Map == null)
		{
			GD.PushWarning("TileMapLayer reference is not set. Please assign it in the inspector.");
		} else
		{
			GD.Print("TileMapLayer reference is set successfully.");
			GD.Print("Game Ready. Map size: " + MapWidth + "x" + MapHeight);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public override void _Input(InputEvent @event)
	{
		// Check for Mouse Button
		if (@event is InputEventMouseButton mouseEvent)
		{
			// Left Mouse Button
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				// Pressed
				if (mouseEvent.Pressed)
				{
					// Mouse Position in Map Coordinates
					Vector2I mapCoords = Map.LocalToMap(Map.GetGlobalMousePosition());

					// Check if the clicked coordinates are within the map bounds
					if (mapCoords.X < 0 || mapCoords.X >= MapWidth || mapCoords.Y < 0 || mapCoords.Y >= MapHeight) return;

					// Example: Get the tile type at the clicked coordinates
					TileType clickedTileType = this.GetTileAtMapCoord(mapCoords);
					GD.Print("Clicked Tile Type: " + clickedTileType);
				}
			}
		}
	}

	// Helper: Clamp a Map Coordinate to the Map Bounds
	public Vector2I ClampToMapBounds(Vector2I mapCoords)
	{
		int clampedX = Godot.Mathf.Clamp(mapCoords.X, 0, MapWidth - 1);
		int clampedY = Godot.Mathf.Clamp(mapCoords.Y, 0, MapHeight - 1);
		return new Vector2I(clampedX, clampedY);
	}

	// Helper: Get the Tile Type at a given Map Coordinates
	public TileType GetTileAtMapCoord(Vector2I mapCoords)
	{
		// Ensure a TileMapLayer Reference Exists
		if (Map == null) return TileType.Healthy;

		// Clamped Map Coordindates
		Vector2I coords = this.ClampToMapBounds(mapCoords);

		// Getting the Tile ID at the given coordinates
		int sourceID = Map.GetCellSourceId(coords);
		GD.Print("Source ID at " + coords + ": " + sourceID);

		// Return the Tile Type based on the Source ID
		switch (sourceID)
		{
			case 0: // Healthy Tile
				return TileType.Healthy;
			case 1: // Damaged Tile
				return TileType.Damaged;
			case 2: // Village Tile
				return TileType.Village;
			case 3: // Outpost Tile
				return TileType.Outpost;
			default: // Unknown
				GD.PushWarning("Unknown source ID: " + sourceID + " at coordinates: " + coords);
				return TileType.Healthy; // Default fallback
		}
	}

	// Helper: Set a Tile at a given Map Cooridates
	public void SetTileAtMapCoord(Vector2I mapCoords, TileType tileType)
	{
		// Ensure a TileMapLayer Reference Exists
		if (Map == null) return;

		// Clamped Map Coordindates
		Vector2I coords = this.ClampToMapBounds(mapCoords);

		// Setting the Tile
		Map.EraseCell(coords); // Clears the Tile
		Map.SetCell(coords, (int)tileType, Vector2I.Zero, 0); // Set the Tile
	}
}

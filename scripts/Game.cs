using Godot;
using System;
using System.Collections.Generic;

public partial class Game : Node2D
{
	// Map Dimensions (in Tiles)
	[Export] public int MapWidth = 12;
	[Export] public int MapHeight = 8;

	// References
	[Export] public TileMapLayer Map;
	[Export] public Camera2D Camera;
	[Export] public Control ActionPopUp;

    // Tile IDs
    public enum TileType
	{
		Healthy = 0,
		Damaged = 1,
		Village = 2,
		Outpost = 3
	}

    // Turn State
    public enum  TurnState
    {
        Player,
		Enemy,
		Evaluate
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		// Check if Map Reference is Set
		if (this.Map == null)
		{
            // DEBUG: If the Map reference is not set, print a warning to the console
            GD.PushWarning("TileMapLayer reference is not set. Please assign it in the inspector.");
		} 
		else
		{
            // DEBUG: Print the name of the Map to confirm the reference is set correctly
			GD.Print("Map reference is set successfully.");
		}

        // Check if ActionPopUp Reference is Set
        if (this.ActionPopUp == null)
        {
            // DEBUG: If the Action Pop-Up reference is not set, print a warning to the console
            GD.PushWarning("ActionPopUp reference is not set. Please assign it in the inspector.");
        }
		else
		{
            // DEBUG: Print the name of the Action Pop-Up to confirm the reference is set correctly
            GD.Print("ActionPopUp reference is set successfully.");
			this.ActionPopUp.Visible = false; // Hide the Action Pop-Up at the start of the Game
        }

        // DEBUG: Game is Ready, print the Map Size for debugging purposes
        GD.Print("Game Ready. Map size: " + MapWidth + "x" + MapHeight);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    // Public Helper: Clamp a Map Coordinate to the Map Bounds
    public Vector2I ClampToMapBounds(Vector2I mapCoords)
	{
		int clampedX = Godot.Mathf.Clamp(mapCoords.X, 0, MapWidth - 1);
		int clampedY = Godot.Mathf.Clamp(mapCoords.Y, 0, MapHeight - 1);
		return new Vector2I(clampedX, clampedY);
	}

    // Public Helper: Get the Tile Type at a given Map Coordinates
    public TileType GetTileAtMapCoord(Vector2I mapCoords)
	{
		// Ensure a TileMapLayer Reference Exists
		if (this.Map == null) return TileType.Healthy;

		// Clamped Map Coordindates
		Vector2I coords = this.ClampToMapBounds(mapCoords);

		// Getting the Tile ID at the given coordinates
		int sourceID = this.Map.GetCellSourceId(coords);
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

	// Public Helper: Set a Tile at a given Map Cooridates
	public void SetTileAtMapCoord(Vector2I mapCoords, TileType tileType)
	{
		// Ensure a TileMapLayer Reference Exists
		if (this.Map == null) return;

		// Clamped Map Coordindates
		Vector2I coords = this.ClampToMapBounds(mapCoords);

		// Setting the Tile
		this.Map.EraseCell(coords); // Clears the Tile
		this.Map.SetCell(coords, (int)tileType, Vector2I.Zero, 0); // Set the Tile
	}
}
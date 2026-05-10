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
	[Export] public Node SpiritsRoot;

	// Spirits
	private readonly List<Spirit> _spirits = new List<Spirit>();
	private Spirit _selectedSpirit;

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
		// Check if Map Reference is Set
		if (this.Map == null)
		{
			GD.PushWarning("TileMapLayer reference is not set. Please assign it in the inspector.");
		} 
		else
		{
			GD.Print("Map reference is set successfully.");
		}

		// Collect all Spirit Children Nodes and add it to the List of Spirits
		if (this.SpiritsRoot != null)
		{
            // Iterate through the children of SpiritsRoot and add any Spirit nodes to the List of Spirits
            foreach (var child in this.SpiritsRoot.GetChildren())
			{
                // Check if the child is of type Spirit before adding it to the list
                if (child is Spirit spirit)
				{
					// Add the Spirit to the List of Spirits
					GD.Print("Found Spirit: " + spirit.Name);
					
                    this._spirits.Add(spirit);
									}
                // If the child is not of type Spirit, log a warning
                else
                {
					GD.PushWarning("Child node '" + child.Name + "' is not of type Spirit and will be ignored.");
				}
            }
        }
		else
		{
			GD.PushWarning("SpiritsRoot reference is not set. Please assign it in the inspector.");
		}

        // Game is Ready, print the Map Size for debugging purposes
        GD.Print("Game Ready. Map size: " + MapWidth + "x" + MapHeight);
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
					// Ensure a TileMapLayer Reference Exists
					if (this.Map == null) return;

					// Mouse Position in Map Coordinates
					Vector2I mapCoords = Map.LocalToMap(Map.GetGlobalMousePosition());

					// Check if the clicked coordinates are within the map bounds
					if (mapCoords.X < 0 || mapCoords.X >= MapWidth || mapCoords.Y < 0 || mapCoords.Y >= MapHeight) return;

					// Example: Click on a Tile with a Spirit on it (Multiple Edition :O)
					Spirit spirit = this.FindSpiritAtMapCoord(mapCoords);
					if (spirit != null)
					{
						GD.Print("Clicked on a tile with '" + spirit.Name + "' at Map Position: " + mapCoords);
						// You can add logic here to select the Spirit, show info, etc.
					}
					else
					{
						GD.Print("Clicked on an empty tile at: " + mapCoords);
						// You can add logic here to place a new Spirit, change the tile, etc.
					}
				}
			}
		}
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

    // Private Helper: Find the Spirit at a given Map Coordinates (if any)
	private Spirit FindSpiritAtMapCoord(Vector2I mapCoords)
	{
		// Ensure a TileMapLayer Reference Exists
		if (this.Map == null) return null;

		// Clamped Map Coordindates
		Vector2I coords = this.ClampToMapBounds(mapCoords);

		// Iterate through all Spirits and check their Map Positions
		foreach (Spirit spirit in this._spirits)
		{
            // Check if this Spirit is at the given coordinates
            if (spirit.GetSpiritMapPosition() == coords)
			{
                // Spirit found at the given coordinates
                return spirit;
			}
		}

		// Otherwise, No Spirit found at the given coordinates
		return null;
	}
}
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
	[Export] public Node SpiritsRoot;
	[Export] public Control ActionPopUp;
	[Export] public Button MoveButton;
	[Export] public Button HealButton;

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

	// Player State
	public enum PlayerState
	{
		Idle,
		SpiritSelected,
		MovingSpirit,
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
					spirit.SetSpiritMapPosition(spirit.GlobalPosition);
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

        // Check if ActionPopUp Reference is Set
        if (this.ActionPopUp == null)
        {
            GD.PushWarning("ActionPopUp reference is not set. Please assign it in the inspector.");
        }
		else
		{
			GD.Print("ActionPopUp reference is set successfully.");
			this.ActionPopUp.Visible = false; // Hide the Action Pop-Up at the start of the Game
        }

        // Check if MoveButton Reference is Set
        if (this.MoveButton == null)
        {
            GD.PushWarning("MoveButton reference is not set. Please assign it in the inspector.");
        }
		else
		{
			GD.Print("MoveButton reference is set successfully.");
        }

        // Check if HealButton Reference is Set
        if (this.HealButton == null)
        {
            GD.PushWarning("HealButton reference is not set. Please assign it in the inspector.");
        }
		else
		{
			GD.Print("HealButton reference is set successfully.");
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

					// Check if the Map Coordinates of the Mouse are within the Map Bounds
					if (mapCoords.X < 0 || mapCoords.X >= MapWidth || mapCoords.Y < 0 || mapCoords.Y >= MapHeight) return;

					// Check if the Tile has a Spirit on it
					Spirit spirit = this.FindSpiritAtMapCoord(mapCoords);
					if (spirit != null)
					{
                        // DEBUG: Print the name of the Spirit and the Map Coordinates where it was clicked
                        GD.Print("Clicked on a tile with '" + spirit.Name + "' at Map Position: " + mapCoords);

                        // Show the Action Pop-Up for the Spirit and select it
                        this.ShowPopup(spirit);
					}
                    else
                    {
                        // Check if the Mouse is NOT hovering over the Action Pop-Up Buttons, therefore clicking on an empty Tile
                        if (this.MoveButton.IsHovered() == false && this.HealButton.IsHovered() == false)
						{
                            // DEBUG: Print the Map Coordinates of the empty tile that was clicked
                            GD.Print("Clicked on an empty tile at: " + mapCoords);

							// Hide the Action Pop-Up
                            this.HidePopup();
                        }
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
            if (spirit.MapPos == coords)
			{
                // Spirit found at the given coordinates
                return spirit;
			}
		}

		// Otherwise, No Spirit found at the given coordinates
		return null;
	}

    // Private Helper: Show the Action Pop-Up from a selected Spirit
	private void ShowPopup(Spirit spirit)
	{
		// Ensure there is an Action Pop-Up Reference
		if (spirit == null) return;

        // Select the given Spirit
        this._selectedSpirit = spirit;

        // Convert the Spirit's Global Position to Camera's Local Space
        Vector2 spiritCameraPosition = this.Camera.GetCanvasTransform() * (spirit.GlobalPosition + spirit.GetRect().GetCenter());

        // Position the Action Pop-Up under the selected Spirit
        Vector2 actionPopUpPosition = spiritCameraPosition + new Vector2(-this.ActionPopUp.GetRect().Size.X / 2, 30.0f);

        // Move and Show the Action Pop-Up
        this.ActionPopUp.GlobalPosition = actionPopUpPosition;
		this.ActionPopUp.Visible = true;

        GD.Print("Showing Action Pop-Up for '" + spirit.Name + "' at position: " + this.ActionPopUp.GlobalPosition);
    }

    // Private Helper: Hide the Action Pop-Up
    private void HidePopup()
	{
        // Hide the Action Pop-Up
        if (this.ActionPopUp != null) this.ActionPopUp.Visible = false;

        // Deselect the Spirit
        this._selectedSpirit = null;
    }

    // Signal Handler: Called when the Move Button is Pressed
    public void OnMoveButtonPressed()
	{
		if (this._selectedSpirit == null)
		{
			GD.PushWarning("No spirit selected. Cannot move.");
			return;
		}

        // DEBUG: Print the name of the selected Spirit when the Move Button is pressed
        GD.Print("Move button pressed for '" + this._selectedSpirit.Name + "'. Implement movement logic here.");

        // TODO: Implement movement logic here (e.g., show valid move tiles, allow player to select a tile, etc.)
    }

    // Signal Handler: Called when the Heal Button is Pressed
    public void OnHealButtonPressed()
	{
		if (this._selectedSpirit == null)
		{
			GD.PushWarning("No spirit selected. Cannot heal.");
			return;
		}

        // DEBUG: Print the name of the selected Spirit when the Heal Button is pressed
        GD.Print("Heal button pressed for '" + this._selectedSpirit.Name + "'. Implement healing logic here.");

        // TODO: Implement healing logic here (e.g., heal the spirit, change the tile state, etc.)
    }
}
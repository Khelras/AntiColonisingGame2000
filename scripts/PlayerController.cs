using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerController : Node
{
    // References
    [Export] public Game GameManager;
    [Export] public Button MoveButton;
    [Export] public Button HealButton;

    // Spirits
    private readonly List<Spirit> _spirits = new List<Spirit>();
    private Spirit _selectedSpirit;

    // Player State
    public enum PlayerState
    {
        Idle,
        SpiritSelected,
        MovingSpirit,
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        // Check if Game Reference is Set
        if (this.GameManager == null)
        {
            // DEBUG: If the Game Reference is not set, print a warning to the console
            GD.PushWarning("Game reference is not set. Please assign it in the inspector.");
        }
        else
        {
            // DEBUG: If the Game Reference is set successfully, print a confirmation message to the console
            GD.Print("Game reference is set successfully.");
        }

        // Check if MoveButton Reference is Set
        if (this.MoveButton == null)
        {
            // DEBUG: If the Move Button reference is not set, print a warning to the console
            GD.PushWarning("MoveButton reference is not set. Please assign it in the inspector.");
        }
        else
        {
            // DEBUG: Print the name of the Move Button to confirm the reference is set correctly
            GD.Print("MoveButton reference is set successfully.");
        }

        // Check if HealButton Reference is Set
        if (this.HealButton == null)
        {
            // DEBUG: If the Heal Button reference is not set, print a warning to the console
            GD.PushWarning("HealButton reference is not set. Please assign it in the inspector.");
        }
        else
        {
            // DEBUG: Print the name of the Heal Button to confirm the reference is set correctly
            GD.Print("HealButton reference is set successfully.");
        }

        // Collect all Spirit Children Nodes and add it to the List of Spirits
        foreach (var child in this.GetChildren())
        {
            // Check if the child is of type Spirit before adding it to the list
            if (child is Spirit spirit)
            {
                // DEBUG: Print the name of the Spirit that was found
                GD.Print("Found Spirit: " + spirit.Name);

                // Add the Spirit to the List of Spirits
                spirit.SetSpiritMapPosition(spirit.GlobalPosition);
                this._spirits.Add(spirit);
            }
            // If the child is not of type Spirit, log a warning
            else
            {
                // DEBUG: If a child node is not of type Spirit, print a warning to the console
                GD.PushWarning("Child node '" + child.Name + "' is not of type Spirit and will be ignored.");
            }
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    // Input
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
                    // Ensure a Game Reference Exists
                    if (this.GameManager == null) return;

                    // Get the Map
                    TileMapLayer map = this.GameManager.Map;

                    // Mouse Position in Map Coordinates
                    Vector2I mapCoords = map.LocalToMap(map.GetGlobalMousePosition());

                    // Check if the Map Coordinates of the Mouse are within the Map Bounds
                    if (mapCoords.X < 0 || 
                        mapCoords.X >= this.GameManager.MapWidth || 
                        mapCoords.Y < 0 || 
                        mapCoords.Y >= this.GameManager.MapHeight) return;

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

    // Private Helper: Find the Spirit at a given Map Coordinates (if any)
    private Spirit FindSpiritAtMapCoord(Vector2I mapCoords)
    {
        // Ensure a Game Reference Exists
        if (this.GameManager == null) return null;

        // Clamped Map Coordindates
        Vector2I coords = this.GameManager.ClampToMapBounds(mapCoords);

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
        // Ensure a Game Reference Exists
        if (this.GameManager == null) return;

        // Select the given Spirit
        this._selectedSpirit = spirit;

        
        // Position the Action Pop-Up under the selected Spirit
        Vector2 spiritCameraPosition = // Convert the Spirit's Global World Position to Camera's Local Space
            this.GameManager.Camera.GetCanvasTransform() * (spirit.GlobalPosition + spirit.GetRect().GetCenter());
        Vector2 actionPopUpPosition =
            spiritCameraPosition + new Vector2(-this.GameManager.ActionPopUp.GetRect().Size.X / 2, 30.0f);

        // Move and Show the Action Pop-Up
        this.GameManager.ActionPopUp.GlobalPosition = actionPopUpPosition;
        this.GameManager.ActionPopUp.Visible = true;
    }

    // Private Helper: Hide the Action Pop-Up
    private void HidePopup()
    {
        // Ensure a Game Reference Exists
        if (this.GameManager == null) return;

        // Hide the Action Pop-Up
        if (this.GameManager.ActionPopUp != null) this.GameManager.ActionPopUp.Visible = false;

        // Deselect the Spirit
        this._selectedSpirit = null;
    }

    // Signal Handler: Called when the Move Button is Pressed
    public void OnMoveButtonPressed()
    {
        // Ensure there is a Selected Spirit
        if (this._selectedSpirit == null)
        {
            // DEBUG: If there is no selected Spirit when the Move Button is pressed, print a warning to the console
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
        // Ensure there is a Selected Spirit
        if (this._selectedSpirit == null)
        {
            // DEBUG: If there is no selected Spirit when the Heal Button is pressed, print a warning to the console
            GD.PushWarning("No spirit selected. Cannot heal.");
            return;
        }

        // DEBUG: Print the name of the selected Spirit when the Heal Button is pressed
        GD.Print("Heal button pressed for '" + this._selectedSpirit.Name + "'. Implement healing logic here.");

        // TODO: Implement healing logic here (e.g., heal the spirit, change the tile state, etc.)
    }
}

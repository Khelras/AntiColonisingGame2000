using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerController : Node
{
    // References
    [Export] public Game GameManager;
    [Export] public Button MoveButton;
    [Export] public Button HealButton;
    [Export] public Label PlayerStateLabel;

    // Spirits
    private readonly List<Spirit> _spirits = new List<Spirit>();
    private Spirit _selectedSpirit;

    // Player States
    private Dictionary<string, Action> _playerStates; // Hash-Map of all Player States
    private Stack<Action> _playerState; // Stack-Based Player State Machine

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

        // Check if PlayerStateLabel Reference is Set
        if (this.PlayerStateLabel == null)
        {
            // DEBUG: If the Player State Label reference is not set, print a warning to the console
            GD.PushWarning("PlayerStateLabel reference is not set. Please assign it in the inspector.");
        }
        else
        {
            // DEBUG: Print the name of the Player State Label to confirm the reference is set correctly
            GD.Print("PlayerStateLabel reference is set successfully.");
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

        // All Player States
        this._playerStates = new Dictionary<string, Action>
        {
            { "Idle", IdleState },
            { "SpiritSelected", SpiritSelectedState },
            { "MovingSpirit", MovingSpiritState }
        };

        // Initialize the Stack-Based Player State Machine with the Idle State
        this._playerState = new Stack<Action>();
        this._playerState.Push(this._playerStates["Idle"]);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // Ensure a PlayerStateLabel Reference Exists
        if (this.PlayerStateLabel != null)
        {
            // Update the Player State Label to show the current Player State
            this.PlayerStateLabel.Text = "Player State: " + this._playerState.Peek().Method.Name;
        }
    }

    // Input Handling and Detection
    public override void _Input(InputEvent @event)
    {
        // Check for Mouse Button
        if (@event is InputEventMouseButton mouseEvent)
        {
            // Left Mouse Button
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                // Pressed
                if (mouseEvent.Pressed) this.OnMouseLeftButtonPressed();
            }

            // Right Mouse Button
            if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                // Pressed
                if (mouseEvent.Pressed) this.OnMouseRightButtonPressed();
            }
        }
    }

    // Input: Mouse Left Button Pressed Function
    private void OnMouseLeftButtonPressed()
    {
        // Ensure a Game Reference Exists
        if (this.GameManager == null) return;

        // Call the current Player State function from the top of the Stack
        this._playerState.Peek().Invoke();
    }

    // Input: Mouse Right Button Pressed Function
    private void OnMouseRightButtonPressed()
    {
        // Ensure a Game Reference Exists
        if (this.GameManager == null) return;

        // DEBUG: Print to the console that the Right Mouse Button was Pressed and the Player State is being Refreshed
        GD.Print("Right mouse button pressed. Refreshing player state.");

        // Refresh the Player State
        this.RefreshPlayerState();
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
    private void HidePopup(bool deselect = true)
    {
        // Ensure a Game Reference Exists
        if (this.GameManager == null) return;

        // Hide the Action Pop-Up
        if (this.GameManager.ActionPopUp != null) this.GameManager.ActionPopUp.Visible = false;

        if (deselect == true)
        {
            // Deselect the Spirit
            this._selectedSpirit = null;
        }
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

        // Change the Player State to MovingSpirit
        this._playerState.Push(this._playerStates["MovingSpirit"]);

        // Hide the Action Pop-Up but keep the Spirit selected
        this.HidePopup(false);
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

    // Private Helper: Clear the Player State Stack and return to Idle State
    private void RefreshPlayerState()
    {
        // Clear the Player State Stack
        this._playerState.Clear();

        // Push the Idle State back onto the Stack
        this._playerState.Push(this._playerStates["Idle"]);

        // Hide the Action Pop-Up and Deselect any selected Spirit
        this.HidePopup();
    }

    // Player State: Idle
    private void IdleState()
    {
        // Get the Map
        TileMapLayer map = this.GameManager.Map;

        // Mouse Position in Map Coordinates
        Vector2I mapCoords = map.LocalToMap(map.GetGlobalMousePosition());

        // Check if the Map Coordinates of the Mouse are within the Map Bounds
        if (mapCoords.X < 0 ||
            mapCoords.X >= this.GameManager.MapWidth ||
            mapCoords.Y < 0 ||
            mapCoords.Y >= this.GameManager.MapHeight) return;

        // Check if the Clicked Tile has a Spirit on it
        Spirit spirit = this.FindSpiritAtMapCoord(mapCoords);
        if (spirit != null)
        {
            // DEBUG: Print the name of the Spirit and the Map Coordinates where it was clicked
            GD.Print("Clicked on a tile with '" + spirit.Name + "' at Map Position: " + mapCoords);

            // Update the Player State to SpiritSelected
            this._playerState.Push(this._playerStates["SpiritSelected"]);

            // Show the Action Pop-Up for the Spirit and select it
            this.ShowPopup(spirit);
        }
        // Otherwise, the Tile is empty
        else
        {
            // DEBUG: Print the Map Coordinates of the empty tile that was clicked
            GD.Print("Clicked on an empty tile at: " + mapCoords);
        }
    }

    // Player State: Spirit Selected
    private void SpiritSelectedState()
    {
        // Ensure there is a Selected Spirit
        if (this._selectedSpirit == null)
        {
            // Go back a State
            this._playerState.Pop();
            return;
        } 

        // Get the Map
        TileMapLayer map = this.GameManager.Map;

        // Mouse Position in Map Coordinates
        Vector2I mapCoords = map.LocalToMap(map.GetGlobalMousePosition());

        // Check if the Map Coordinates of the Mouse are within the Map Bounds
        if (mapCoords.X < 0 ||
            mapCoords.X >= this.GameManager.MapWidth ||
            mapCoords.Y < 0 ||
            mapCoords.Y >= this.GameManager.MapHeight) return;

        // Check if the Mouse is NOT hovering over the Action Pop-Up Buttons, therefore a Tile is being Clicked
        if (this.MoveButton.IsHovered() == false && this.HealButton.IsHovered() == false)
        {
            // Check if the Clicked Tile has a Spirit on it
            Spirit spirit = this.FindSpiritAtMapCoord(mapCoords);
            if (spirit != null)
            {
                // DEBUG: Print the name of the Spirit and the Map Coordinates where it was clicked
                GD.Print("Clicked on a tile with '" + spirit.Name + "' at Map Position: " + mapCoords);

                // Show the Action Pop-Up for the Spirit and select it
                this.ShowPopup(spirit);
            }
            // Otherwise, the Tile is empty
            else
            {
                // DEBUG: Print the Map Coordinates of the empty tile that was clicked
                GD.Print("Clicked on an empty tile at: " + mapCoords);

                // Go back a State
                this._playerState.Pop();

                // Hide the Action Pop-Up
                this.HidePopup();
            }
        }
    }

    // Player State: Moving Spirit
    private void MovingSpiritState()
    {
        // Get the Map
        TileMapLayer map = this.GameManager.Map;

        // Mouse Position in Map Coordinates
        Vector2I mapCoords = map.LocalToMap(map.GetGlobalMousePosition());

        // Check if the Map Coordinates of the Mouse are within the Map Bounds
        if (mapCoords.X < 0 ||
            mapCoords.X >= this.GameManager.MapWidth ||
            mapCoords.Y < 0 ||
            mapCoords.Y >= this.GameManager.MapHeight) return;

        // Check if the Clicked Tile has a Spirit on it
        Spirit spirit = this.FindSpiritAtMapCoord(mapCoords);
        if (spirit != null)
        {
            // DEBUG: Print saying cannot move the selected Spirit to the clicked tile because there is already a Spirit on it
            GD.Print("Cannot move '" + this._selectedSpirit.Name + "' to Map Position: " + mapCoords + " as there is already a Spirit on that Tile.");
        }
        // Otherwise, the Tile is empty
        else
        {
            // DEBUG: Print the Map Coordinates of the empty Tile that was clicked and move the selected Spirit to that Tile
            GD.Print("Clicked on an empty tile at: " + mapCoords + ", moving '" + this._selectedSpirit.Name + "' to that position.");

            // Move the selected Spirit to the new Map Coordinates
            this._selectedSpirit.SetSpiritMapPosition(mapCoords);

            // Refresh the Player State
            this.RefreshPlayerState();
        }
    }
}
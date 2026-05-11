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
	[Export] public AIController AI;
	[Export] public Control ActionPopUp;
	[Export] public Label TurnStateLabel;
	[Export] public Label TurnsSurpassedLabel;
	[Export] public Label LandHealthLabel;

    // Tile IDs
    public enum TileType
	{
		Healthy = 0,
		Damaged = 1,
		Village = 2,
		OutpostHalf = 3,
		Outpost = 4
	}
    
	// Game State
	public enum GameState
	{
		Playing,
		Win,
		Lose
	}

    // Turn State
    public enum  TurnState
    {
        Player,
		Enemy,
		Evaluate
    }

	// Game Properties
	public GameState CurrentGameState;
	public Queue<TurnState> TurnStateOrder;
	public int TurnsSurpassed { get; private set; }
    public const int TotalLandHealth = 100;
	public int CurrentLandHealth { get; private set; }

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

        // Check if AI Reference is Set
        if (this.AI == null)
        {
            // DEBUG: If the AI reference is not set, print a warning to the console
            GD.PushWarning("AI reference is not set. Please assign it in the inspector.");
        }
        else
        {
            // DEBUG: Print the name of the AI to confirm the reference is set correctly
            GD.Print("AI reference is set successfully.");
        }

        // Check if TurnStateLabel Reference is Set
        if (this.TurnStateLabel == null)
        {
            // DEBUG: If the Turn State Label reference is not set, print a warning to the console
            GD.PushWarning("TurnStateLabel reference is not set. Please assign it in the inspector.");
        }
        else
        {
            // DEBUG: Print the name of the Turn State Label to confirm the reference is set correctly
            GD.Print("TurnStateLabel reference is set successfully.");
        }

        // Check if TurnsSurpassedLabel Reference is Set
        if (this.TurnsSurpassedLabel == null)
        {
            // DEBUG: If the Turns Surpassed Label reference is not set, print a warning to the console
            GD.PushWarning("TurnsSurpassedLabel reference is not set. Please assign it in the inspector.");
        }
        else
        {
            // DEBUG: Print the name of the Turns Surpassed Label to confirm the reference is set correctly
            GD.Print("TurnsSurpassedLabel reference is set successfully.");
        }

        // Check if LandHealthLabel Reference is Set
        if (this.LandHealthLabel == null)
        {
            // DEBUG: If the Land Health Label reference is not set, print a warning to the console
            GD.PushWarning("LandHealthLabel reference is not set. Please assign it in the inspector.");
        }
        else
        {
            // DEBUG: Print the name of the Land Health Label to confirm the reference is set correctly
            GD.Print("LandHealthLabel reference is set successfully.");
        }

        // Initialisation of Game Properties 
        this.CurrentGameState = GameState.Playing;
        this.TurnStateOrder = new Queue<TurnState>();
		this.TurnStateOrder.Enqueue(TurnState.Player);
		this.TurnStateOrder.Enqueue(TurnState.Enemy);
		this.TurnStateOrder.Enqueue(TurnState.Evaluate);
		this.TurnsSurpassed = 0;
        this.CurrentLandHealth = TotalLandHealth;

        // DEBUG: Game is Ready, print the Map Size for debugging purposes
        GD.Print("Game Ready. Map size: " + MapWidth + "x" + MapHeight);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        // Ensure a TurnStateLabel Reference Exists
        if (this.TurnStateLabel != null)
        {
            // Update the Player State Label to show the current Player State
            this.TurnStateLabel.Text = "Current Turn State: " + this.TurnStateOrder.Peek().ToString();
        }

        // Ensure a TurnsSurpassedLabel Reference Exists
        if (this.TurnsSurpassedLabel != null)
        {
            // Update the Player State Label to show the current Player State
            this.TurnsSurpassedLabel.Text = "Turns Surpassed: " + this.TurnsSurpassed;
        }

        // Ensure a LandHealthLabel Reference Exists
        if (this.LandHealthLabel != null)
        {
            // Update the Land Health Label to show the current Land Health
            this.LandHealthLabel.Text = "Land Health: " + this.CurrentLandHealth + "/" + Game.TotalLandHealth;
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

		// Return the Tile Type based on the Source ID
		switch (sourceID)
		{
			case 0: // Healthy Tile
				return TileType.Healthy;
			case 1: // Damaged Tile
				return TileType.Damaged;
			case 2: // Village Tile
				return TileType.Village;
			case 3: // Outpost-Half Tile
				return TileType.OutpostHalf;
			case 4: // Outpost Tile
				return TileType.Outpost;
			default: // Unknown
				GD.PushWarning("Unknown source ID: " + sourceID + " at coordinates: " + coords);
				return TileType.Healthy; // Default fallback
		}
	}

	// Public Helper: Set a Tile at a given Map Coordinates
	public void SetTileAtMapCoord(Vector2I mapCoords, TileType tileType)
	{
		// Ensure a TileMapLayer Reference Exists
		if (this.Map == null) return;

		// Clamped Map Coordinates
		Vector2I coords = this.ClampToMapBounds(mapCoords);

		// Setting the Tile
		this.Map.EraseCell(coords); // Clears the Tile
		this.Map.SetCell(coords, (int)tileType, Vector2I.Zero, 0); // Set the Tile
	}
	
	// Public Helper: Damage the Land
	public void DamageLand()
	{
		// Damage the Land for 5
		this.CurrentLandHealth -= 5;
		
		// Check if Land Health reached 0 or below
		if (this.CurrentLandHealth <= 0)
		{
			// Clamp to 0
			this.CurrentLandHealth = 0;
			
			// Game lost
			this.CurrentGameState = GameState.Lose;
		}
	}
	
	// Public Helper: Heal the Land
	public void HealLand()
	{
		// Heal the Land for 5
		this.CurrentLandHealth += (this.CurrentLandHealth + 5 > 100) ? 100 : this.CurrentLandHealth + 5;
	}

	// Signal Handler: Called when the End Turn Button is Pressed
	public void OnEndTurnButtonPressed()
	{
		// Ensure it is currently the Player's Turn
		if (this.TurnStateOrder.Peek() != TurnState.Player) return;

		// DEBUG: Print the current Turn State before ending the turn
		GD.Print("Ending Turn '" + this.TurnStateOrder.Peek() + "'.");

		// End the Current Turn and Move to the Next Turn State
		this.TurnStateOrder.Enqueue(this.TurnStateOrder.Dequeue());

        // Start Enemy Turn
        this.EnemyTurn();
	}

    // Turn State: Enemy Turn Function
    private void EnemyTurn()
    {
        // Start the Enemy Turn via the AI Controller
        this.AI.StartEnemyTurn();
    }

    // Turn State: Evaluate Function
    private void EvaluateTurn()
    {
        // TODO: Evaluate at the End of the Turn order
        
        // Step 1: Check for Spirits Healing the Land and removing Outposts
        
        // Step 2: Find all the fully-built Outposts and Evaluate their Damages
        
        // Step 3: Find all half-built Outposts and turn them into 
        
        // Step 4: Evaluate the Win-Lost Condition or Proceed to the next Round of Turns
    }
}
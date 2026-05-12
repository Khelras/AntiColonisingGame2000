using Godot;
using System;

public partial class AIController : Node
{
    // References
    [Export] public Game GameManager;
    [Export] public Timer ThinkingTimer;

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

        // Check if Thinking Timer Reference is Set
        if (this.ThinkingTimer == null)
        {
            // DEBUG: If the Thinking Timer reference is not set, print a warning to the console
            GD.PushWarning("Thinking Timer reference is not set. Please assign it in the inspector.");
        }
        else
        {
            // DEBUG: Print the name of the Thinking Timer to confirm the reference is set correctly
            GD.Print("Thinking Timer reference is set successfully.");
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    // Function to Start the Enemy's Turn
    public void StartEnemyTurn()
    {
        // Ensure a Game Mananger and Thinking Timer Reference Exists
        if (this.GameManager == null || this.ThinkingTimer == null)
        {
            // DEBUG: If either the Game Manager or Thinking Timer reference is not set, print a warning to the console and return early
            GD.PushWarning("Cannot start Enemy Turn: Game Manager or Thinking Timer reference is not set.");
            return;
        }

        // DEBUG: Print the AI is "Thinking" which indicates the start of the Thinking Timer
        GD.Print("Enemy AI is Thinking...");

        // Start the Thinking Timer to simulate the AI "Thinking"
        this.ThinkingTimer.Start();
    }

    // Function called when the Thinking Timer times out, indicating the AI has finished "thinking"
    public void OnThinkingTimerTimeout()
    {
        // Stop the Thinking Timer to prevent it from continuously timing out
        this.ThinkingTimer.Stop();

        // Place around 1-3 Random Outposts on the Map
        Random random = new Random();
        int randomAmount = random.Next(1, 4);

        // Loop 1-3 Times to Randomly place the Outposts
        for (int i = 0; i < randomAmount; i++)
        {
            // Random Outposts Properties
            int randomOutpostMapX, randomOutpostMapY;
            Vector2I randomOutpostMapPos = new Vector2I();
            
            // Do-While Loop to Generate Random Map Coordinates until a Suitable Tile is found to place the Outpost
            Game.TileType checkedTile = this.GameManager.GetTileAtMapCoord(randomOutpostMapPos);
            do
            {
                // Calculate a Random Position
                randomOutpostMapX = random.Next(0, this.GameManager.MapWidth);
                randomOutpostMapY = random.Next(0, this.GameManager.MapHeight);
                randomOutpostMapPos = new Vector2I(randomOutpostMapX, randomOutpostMapY);
                
                // Re-Check the Tile at the new Random Position
                checkedTile = this.GameManager.GetTileAtMapCoord(randomOutpostMapPos);
            } while (checkedTile != Game.TileType.Healthy && checkedTile != Game.TileType.Damaged);
            
            // DEBUG: Print the Map Position of the Randomly Placed Outpost
            GD.Print("Placing Outpost at Map Position: " + randomOutpostMapPos);
            
            // Place the Outpost at the Randomly Generated Coordinates
            this.GameManager.SetTileAtMapCoord(randomOutpostMapPos, Game.TileType.OutpostHalf);
            
            // Damage the Land
            this.GameManager.DamageLand();
        }
        
        // End Turn and Go to Next Turn
        this.GameManager.NextTurn();
    }
}

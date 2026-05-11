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

        // Place a Random Outpost on the Map
        Random random = new Random();
        int randomOutpostMapX, randomOutpostMapY;
        Vector2I randomOutpostMapPos = new Vector2I();

        // Do-While Loop to Generate Random Map Coordinates until a Suitable Tile is found to place the Outpost
        bool condition = 
            this.GameManager.GetTileAtMapCoord(randomOutpostMapPos) == Game.TileType.Village ||
            this.GameManager.GetTileAtMapCoord(randomOutpostMapPos) == Game.TileType.OutpostHalf ||
            this.GameManager.GetTileAtMapCoord(randomOutpostMapPos) == Game.TileType.Outpost;
        do {
            randomOutpostMapX = random.Next(0, this.GameManager.MapWidth);
            randomOutpostMapY = random.Next(0, this.GameManager.MapHeight);
            randomOutpostMapPos = new Vector2I(randomOutpostMapX, randomOutpostMapY);
        } while (condition);

        // Place the Outpost at the Randomly Generated Coordinates
        this.GameManager.SetTileAtMapCoord(randomOutpostMapPos, Game.TileType.OutpostHalf);
        
        // Damage the Land
        this.GameManager.DamageLand();

        // DEBUG: Print the current Turn State before ending the turn
        GD.Print("Ending Turn '" + this.GameManager.TurnStateOrder.Peek() + "'.");

        // End the Current Turn and Move to the Next Turn State
        this.GameManager.TurnStateOrder.Enqueue(this.GameManager.TurnStateOrder.Dequeue());
    }
}

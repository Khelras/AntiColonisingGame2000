using Godot;
using System;

public partial class Spirit : Sprite2D
{
    // Spirit Sprite References
    static readonly CompressedTexture2D Normal = GD.Load<CompressedTexture2D>("res://assets/Spirit.png");
    static readonly CompressedTexture2D Expended = GD.Load<CompressedTexture2D>("res://assets/SpiritHasHealed.png");

    // Reference to Player Controller
    PlayerController Player;

    // Spirit Properties
    public Vector2I MapPos { get; private set; }
    public bool IsExpended { get; private set; } = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        // Get the Parent Node Player Controller
        this.Player = GetParent<PlayerController>();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    // Setter for Spirit's Map Position given a World Position
    public void SetSpiritMapPosition(Vector2 worldPos)
	{
        // Get the Game object from the Parent Player Controller
        Game game = this.Player.GameManager;
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
        // Get the Game object from the Parent Player Controller
        Game game = this.Player.GameManager;
        Vector2I tileSize = game.Map.TileSet.TileSize; // Tile Size

        // Set the Spirit's Map Position
        this.MapPos = tilePos;
        this.Position = tilePos * tileSize;
    }

    // Public Helper: Reset the Spirit to its Normal State
    public void ResetSpirit()
    {
        // Reset the Spirit's Texture to the Normal version
        this.IsExpended = false;
        this.Texture = Spirit.Normal; 
    }

    // Heal Function
    public void Heal()
    {
        // Get the Game object from the Parent Player Controller
        Game game = this.Player.GameManager;
        
        // Check that the Spirit is on an Outpost Tile and Heal that Tile, destroying the Outpost.
        if (game.GetTileAtMapCoord(this.MapPos) == Game.TileType.Outpost)
        {
            // DEBUG: Print Spirit Healing
            GD.Print("Spirit used Heal and was successful. All actions Expended.");
            this.IsExpended = true; // Set the Spirit as Expended
            this.Texture = Spirit.Expended; // Change the Spirit's Texture to the Expended version
        } 
        else
        {
            // DEBUG: Print Invalid Heal Attempt
            GD.Print("Spirit Heal failed. Not on an Outpost Tile.");
        }
    }
}

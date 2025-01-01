﻿using ConsoleGameEngine;

public class Enemy
{
    private readonly ConsoleEngine engine;
    public Point Position { get; private set; }
    public int Type { get; private set; } // 1 for normal, 2 for split, 3 for following, 4 for new type
    public bool IsActive { get; set; } = true;
    public int PlayerY { get; set; } // Add this property to track player's Y position
    public static int escEnemy { get; set; } = 0; // Make escEnemy static to track across all enemies

    private static Random random = new Random();
    private static int spawnCooldown = 200; // Decrease cooldown in frames for faster spawning
    private static int currentCooldown = 0;
    private static int enemySpeed = 1; // Set initial speed to 0 for slower start

    public Enemy(ConsoleEngine engine, Point initialPosition, int type, int playerY = 0) // Update constructor
    {
        this.engine = engine;
        this.Position = initialPosition;
        this.Type = type;
        this.PlayerY = playerY; // Initialize PlayerY
    }

    public void Update()
    {
        if (!IsActive) return;

        // Prevent type 0 life cube from moving
        if (Type == 0) return;

        // Move the enemy from right to left
        Position = new Point(Position.X - 1, Position.Y);

        // If the enemy is of type 3, follow the player's Y coordinate
        if (Type == 3)
        {
            Position = new Point(Position.X, PlayerY - 4); // Update Y position to player's Y
        }

        // Deactivate if it goes off-screen
        if (Position.X < 0)
        {
            IsActive = false;
            escEnemy++; // Increment escEnemy counter
        }
    }

    public void Render()
    {
        int width = Type == 1 ? 6 : (Type == 0 ? 3 : 12);
        int height = Type == 1 ? 4 : (Type == 0 ? 2 : 8);
        int color = Type == 1 ? 2 : (Type == 0 ? 4 : (Type == 4 ? 6 : 3)); // Set color for type 0 and new type 4 enemy

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                engine.SetPixel(new Point(Position.X + x, Position.Y + y), color, ConsoleCharacter.Full);
            }
        }
    }

    public int OnHit(List<Enemy> enemies)
    {
        int scoreValue = Type == 1 ? 1 : 5; // 1 point for small enemy, 5 points for large enemy

        if (Type == 2)
        {
            // Split into two smaller enemies
            enemies.Add(new Enemy(engine, new Point(Position.X + 2, Position.Y - 2), 1));
            enemies.Add(new Enemy(engine, new Point(Position.X + 2, Position.Y + 5), 1));
        }
        else if (Type == 4)
        {
            // Split into two type 2 or type 3 enemies
            enemies.Add(new Enemy(engine, new Point(Position.X + 3, Position.Y - 6), random.Next(2, 4)));
            enemies.Add(new Enemy(engine, new Point(Position.X + 3, Position.Y + 9), random.Next(2, 4)));
        }

        // Only deactivate if the enemy is not type 0
        if (Type != 0) // type 0 is life cube
        {
            IsActive = false; // Deactivate if it's not a type 0 enemy
        }

        // Chance to spawn a life cube (5%)
        if (random.NextDouble() < 0.05)
        {
            SpawnLifeCube(Position.X, Position.Y, enemies); // Call the method to spawn a life cube
        }

        return scoreValue;
    }

    public void SpawnLifeCube(int x, int y, List<Enemy> enemies)
    {
        enemies.Add(new Enemy(engine, new Point(x, y), 0));
    }

    public List<Point> GetEnemyPoints()
    {
        List<Point> enemyPoints = new List<Point>();
        int width = Type == 1 ? 6 : 12;
        int height = Type == 1 ? 4 : 8;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                enemyPoints.Add(new Point(Position.X + x, Position.Y + y));
            }
        }

        return enemyPoints;
    }

    public static void ManageEnemies(ConsoleEngine engine, List<Enemy> enemies, int screenWidth, int screenHeight, int playerScore)
    {
        // Update existing enemies
        foreach (var enemy in enemies)
        {
            enemy.Update();
        }

        // Remove inactive enemies
        enemies.RemoveAll(e => !e.IsActive);

        // Adjust enemy speed based on player score
        if (playerScore >= 100)
        {
            enemySpeed = 1 + (playerScore / 100); // Increase speed for every 100 points
        }

        // Adjust spawn rate based on player score
        if (playerScore % 250 == 0 && playerScore > 0) // Change cooldown every 250 points
        {
            spawnCooldown = Math.Max(100, 200 - (playerScore / 10)); // Decrease cooldown to a minimum of 20 frames
        }

        // Handle enemy spawning
        if (currentCooldown <= 0)
        {
            int type;
            double randomValue = random.NextDouble();
            if (randomValue < 0.5)
            {
                type = 1; // 50% chance to spawn type 1
            }
            else if (randomValue < 0.8)
            {
                type = 2; // 30% chance to spawn type 2
            }
            else if (randomValue < 0.95)
            {
                type = 3; // 15% chance to spawn type 3
            }
            else
            {
                type = 4; // 5% chance to spawn type 4
            }

            int yPosition = random.Next(23, screenHeight - (type == 1 ? 12 : 18)); // Ensure enemy spawns within vertical bounds
            enemies.Add(new Enemy(engine, new Point(screenWidth - 1, yPosition), type));
            currentCooldown = spawnCooldown / enemySpeed; // Adjust cooldown based on speed
        }
        else
        {
            currentCooldown--;
        }
    }

    public int OnCollisionWithPlayer() // New method to handle collision with player
    {
        if (Type == 0) // Check if this is a type 0 enemy
        {
            // Increase player life logic here
            return 1; // Return 1 to indicate life increase
        }
        return 0; // No life increase for other types
    }

    private bool IsBulletCollidingWithEnemy(Point bullet, Enemy enemy)
    {
        if (enemy.Type == 0) // Prevent bullets from colliding with type 0 enemies
        {
            return false;
        }

        int width = enemy.Type == 1 ? 6 : 12;
        int height = enemy.Type == 1 ? 4 : 8;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (bullet.Equals(new Point(enemy.Position.X + x, enemy.Position.Y + y)))
                {
                    return true;
                }
            }
        }
        return false;
    }
}

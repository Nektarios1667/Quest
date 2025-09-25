namespace Quest.Managers;



public class Attack(int damage, RectangleF hitbox)
{
    public int Damage { get; } = damage;
    public RectangleF Hitbox { get; } = hitbox;
}
public class PlayerManager : IContainer
{
    public int SelectedSlot { get; set; }
    public Inventory? SelectedInventory { get; set; }
    public Inventory Inventory { get; }
    public IContainer? OpenedContainer { get; private set; }
    public Tile? TileBelow { get; private set; }
    public Direction PlayerDirection { get; private set; }
    public List<Attack> Attacks { get; private set; } = [];
    private float moveX, moveY;
    public PlayerManager()
    {
        Inventory = new(6, 4, isPlayer: true);
        Inventory.SetSlot(0, new DiamondSword(1));
        Inventory.SetSlot(1, new Lantern(1));
        Inventory.SetSlot(2, new Pickaxe(1));
        Inventory.SetSlot(3, new PhiCoin(10));
        Inventory.SetSlot(4, new DeltaCoin(5));
        Inventory.SetSlot(5, new GammaCoin(2));
        Inventory.SetSlot(6, new WoodKey(1));
        Inventory.SetSlot(7, new IronKey(1));
        Inventory.SetSlot(8, new DiamondKey(1));
        Inventory.SetSlot(9, new EmeraldKey(1));
        Inventory.SetSlot(10, new RubyKey(1));
        Inventory.SetSlot(11, new MagicKey(1));
        Inventory.SetSlot(14, new Apple(1));
        Inventory.SetSlot(15, new Bread(1));
        Inventory.SetSlot(16, new Skull(1));
    }
    public void Update(GameManager gameManager)
    {
        if (StateManager.State != GameState.Game && StateManager.State != GameState.Editor) return;
        if (StateManager.OverlayState == OverlayState.Pause) return;

        // Update player position
        UpdatePositions(gameManager);

        // Toggle inventory
        if (InputManager.KeyPressed(Keys.I))
        {
            if (OpenedContainer != null) CloseContainer();
            else OpenContainer(this);
        }

        // Loot
        DebugManager.StartBenchmark("UpdateLoot");
        // Check if can pick up and search
        if (Inventory.IsFull()) return;
        for (int l = 0; l < gameManager.LevelManager.Level.Loot.Count; l++)
        {
            Loot loot = gameManager.LevelManager.Level.Loot[l];
            // Pick up loot
            if (PointTools.DistanceSquared(CameraManager.PlayerFoot, loot.Location + new Point(20, 20)) <= Constants.TileSize.X * Constants.TileSize.Y * .5f)
            {
                gameManager.UIManager.LootNotifications.AddNotification($"+{loot.DisplayName}");
                Inventory.AddItem(Item.ItemFromName(loot.Item, loot.Amount));
                gameManager.LevelManager.Level.Loot.Remove(loot);
                LightingManager.RemoveLight($"Loot_{loot.UID}");
                SoundManager.PlaySound("Pickup", pitch: RandomManager.RandomFloat() / 2 - .25f);
            }
        }
        DebugManager.EndBenchmark("UpdateLoot");

        // Movement
        if (OpenedContainer == null)
        {
            // Movement
            DebugManager.StartBenchmark("UpdateMovement");
            moveX = 0; moveY = 0;
            moveX += InputManager.AnyKeyDown(Keys.A, Keys.Left) ? -Constants.PlayerSpeed : 0;
            moveX += InputManager.AnyKeyDown(Keys.D, Keys.Right) ? Constants.PlayerSpeed : 0;
            moveY += InputManager.AnyKeyDown(Keys.W, Keys.Up) ? -Constants.PlayerSpeed : 0;
            moveY += InputManager.AnyKeyDown(Keys.S, Keys.Down) ? Constants.PlayerSpeed : 0;
            Move(gameManager, new(moveX, moveY));
            if (moveX > 0) PlayerDirection = Direction.Right;
            else if (moveX < 0) PlayerDirection = Direction.Left;
            else if (moveY > 0) PlayerDirection = Direction.Down;
            else if (moveY < 0) PlayerDirection = Direction.Up;
            else PlayerDirection = Direction.Forward;
            DebugManager.EndBenchmark("UpdateMovement");

            // Remove attacks
            DebugManager.StartBenchmark("UpdateAttacks");
            Attacks.Clear();
            if (InputManager.LMouseClicked) Inventory.Equipped?.PrimaryUse(this);
            else if (InputManager.RMouseClicked) Inventory.Equipped?.SecondaryUse(this);
            DebugManager.EndBenchmark("UpdateAttacks");
        }


        // Inventory
        DebugManager.StartBenchmark("InventoryUpdate");
        Inventory.Update(gameManager, this);
        OpenedContainer?.Inventory.Update(gameManager, this);
        DebugManager.EndBenchmark("InventoryUpdate");

        // Player lighting
        if (Inventory.Equipped is Light light)
            LightingManager.SetLight("PlayerLightItem", Constants.Middle + CameraManager.CameraOffset.ToPoint(), light.LightStrength, light.LightColor, 0.7f);
        else
            LightingManager.RemoveLight("PlayerLightItem");
    }
    public void Draw(GameManager gameManager)
    {
        switch (StateManager.State)
        {
            case GameState.Game or GameState.Editor:
                DrawPlayer(gameManager);
                if (DebugManager.DrawHitboxes)
                {
                    DrawPlayerHitbox(gameManager);
                    foreach (Attack attack in Attacks)
                        FillRectangle(gameManager.Batch, new(attack.Hitbox.Position.ToPoint() - CameraManager.Camera.ToPoint() + Constants.Middle, new Point((int)attack.Hitbox.Width, (int)attack.Hitbox.Height)), Constants.DebugPinkTint);
                }

                break;
        }

    }
    public void DrawPlayer(GameManager gameManager)
    {
        // Get image source
        int sourceRow = 0;
        if (PlayerDirection == Direction.Forward) sourceRow = 0;
        else if (PlayerDirection == Direction.Left) sourceRow = 1;
        else if (PlayerDirection == Direction.Right) sourceRow = 3;
        else if (PlayerDirection == Direction.Down) sourceRow = 2;
        else if (PlayerDirection == Direction.Up) sourceRow = 4;
        // Draw player
        Point pos = Constants.Middle - Constants.MageHalfSize + CameraManager.CameraOffset.ToPoint();
        Rectangle source = GetAnimationSource(TextureID.BlueMage, gameManager.GameTime, duration: sourceRow == 0 ? .5f : .25f, row: sourceRow);
        DrawTexture(gameManager.Batch, TextureID.BlueMage, pos, source: source);
        // Draw equipped item
        if (Inventory.Equipped != null)
        {
            Point itemPos = Constants.Middle + CameraManager.CameraOffset.ToPoint() - (PlayerDirection == Direction.Left ? Constants.MageDrawShift : Point.Zero);
            DrawTexture(gameManager.Batch, Inventory.Equipped.Texture, itemPos, scale: 2, effects: PlayerDirection == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
    }
    public void DrawPlayerHitbox(GameManager gameManager)
    {
        Point[] points = new Point[4];
        for (int c = 0; c < Constants.PlayerCorners.Length; c++)
            points[c] = CameraManager.PlayerFoot + Constants.PlayerCorners[c] - CameraManager.Camera.ToPoint() + Constants.Middle;
        FillRectangle(gameManager.Batch, new Rectangle(points[0].X, points[0].Y, points[1].X - points[0].X, points[2].Y - points[1].Y), Constants.DebugPinkTint);
    }
    public void Move(GameManager gameManager, Vector2 move)
    {
        // Move
        if (move == Vector2.Zero) return;
        Vector2 finalMove = Vector2.Normalize(move) * gameManager.DeltaTime * Constants.PlayerSpeed;

        // Stuck in block
        if (IsColliding(gameManager)) return;

        // Check bump
        CheckBumping(gameManager, finalMove);

        // Check collision for x
        CameraManager.CameraDest += new Vector2(finalMove.X, 0);
        if (IsColliding(gameManager))
            CameraManager.CameraDest -= new Vector2(finalMove.X, 0);
        // Check collision for y
        CameraManager.CameraDest += new Vector2(0, finalMove.Y);
        if (IsColliding(gameManager)) CameraManager.CameraDest -= new Vector2(0, finalMove.Y);

        // On tile enter
        UpdatePositions(gameManager);
        if (TileBelow == null) return;
        TileBelow.OnPlayerEnter(gameManager, this);

        // Debug
        if (DebugManager.CollisionDebug) TileBelow.Marked = true;
    }
    public bool IsColliding(GameManager gameManager)
    {
        // Check if level loaded
        if (gameManager.LevelManager.Level == null) return false;
        // Check 4 corners
        UpdatePositions(gameManager);
        for (int o = 0; o < Constants.PlayerCorners.Length; o++)
        {
            // Check if the player collides with a tile
            Point coord = (CameraManager.PlayerFoot + Constants.PlayerCorners[o]) / Constants.TileSize;
            TileBelow = gameManager.LevelManager.GetTile(coord);
            if (TileBelow == null || !TileBelow.IsWalkable) return true;
        }
        return false;
    }
    public void CheckBumping(GameManager gameManager, Vector2 finalMove)
    {
        Rectangle playerBounds = new((CameraManager.CameraDest + finalMove + new Vector2(-Constants.PlayerBox.X / 2, Constants.PlayerBox.Y)).ToPoint(), Constants.PlayerBox);
        Point topLeftTile = playerBounds.Location / Constants.TileSize;
        Point bottomRightTile = (playerBounds.Location + playerBounds.Size) / Constants.TileSize;

        for (int y = topLeftTile.Y; y <= bottomRightTile.Y; y++)
        {
            for (int x = topLeftTile.X; x <= bottomRightTile.X; x++)
            {
                Tile? tile = gameManager.LevelManager.GetTile(new Point(x, y));
                if (tile != null && !tile.IsWalkable)
                {
                    tile.OnPlayerCollide(gameManager, this);
                    return;
                }
            }
        }
    }

    public void OpenContainer(IContainer container)
    {
        Inventory.Opened = true;
        OpenedContainer = container;
        container.Inventory.Opened = true;
        StateManager.OverlayState = OverlayState.Container;
        SoundManager.PlaySound("Click");
    }
    public void CloseContainer()
    {
        if (OpenedContainer != null || Inventory.Opened)
            SoundManager.PlaySound("Click");

        if (OpenedContainer != null)
            OpenedContainer.Inventory.Opened = false;
        Inventory.Opened = false;
        OpenedContainer = null;
        StateManager.OverlayState = OverlayState.None;
    }
    public void UpdatePositions(GameManager gameManager)
    {
        TileBelow = gameManager.LevelManager.GetTile(CameraManager.TileCoord);
    }
    public void DamagePlayer(GameManager gameManager, int damage)
    {
        gameManager.UIManager.HealthBar.CurrentValue -= damage;
        if (gameManager.UIManager.HealthBar.CurrentValue <= 0)
        {
            gameManager.UIManager.HealthBar.CurrentValue = 0;
            StateManager.State = GameState.Death;
        }
    }
    public void AddAttack(Attack attack)
    {
        Attacks.Add(attack);
    }
}

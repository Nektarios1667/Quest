using Quest.Gui;
namespace Quest.Managers;

public class Attack(int damage, RectangleF hitbox)
{
    public int Damage { get; } = damage;
    public RectangleF Hitbox { get; } = hitbox;
}
public class PlayerManager
{
    public Inventory Inventory { get; }
    public Inventory ContainerInventory { get; }
    public IContainer? OpenedContainer { get; set; } = null;
    public int SelectedSlot { get; set; }
    public Inventory SelectedInventory { get; set; }
    public Tile? TileBelow { get; private set; }
    public List<Tile> TileBumps { get; private set; } = [];
    public Direction PlayerDirection { get; private set; }
    public List<Attack> Attacks { get; private set; } = [];
    private float moveX, moveY;
    public PlayerManager()
    {
        Inventory = new(6, 4, isPlayer: true);
        ContainerInventory = new(Chest.Size.X, Chest.Size.Y, isPlayer: false);
        SelectedInventory = Inventory;
    }
    public void Update(GameManager gameManager)
    {
        if (StateManager.State != GameState.Game && StateManager.State != GameState.Editor) return;
        if (StateManager.OverlayState == OverlayState.Pause) return;

        // Update player position
        TileBumps.Clear();
        UpdatePositions(gameManager);

        // Toggle inventory
        if (InputManager.KeyPressed(Keys.I))
        {
            if (Inventory.Opened) CloseContainer();
            else OpenInventory();
        }
        if (InputManager.KeyPressed(Keys.Escape))
            CloseContainer();

        // Loot
        DebugManager.StartBenchmark("UpdateLoot");
        CheckForLoot(gameManager);
        DebugManager.EndBenchmark("UpdateLoot");

        // Movement
        if (!Inventory.Opened)
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
        ContainerInventory.Update(gameManager, this);
        DebugManager.EndBenchmark("InventoryUpdate");

        // NPC
        UpdateNPCInteractions(gameManager);

        // Player lighting
        if (Inventory.Equipped is Light light)
            LightingManager.SetLight("PlayerLightItem", CameraManager.PlayerFoot - CameraManager.Camera.ToPoint() + Constants.Middle, light.LightStrength, light.LightColor);
        else
            LightingManager.RemoveLight("PlayerLightItem");
    }
    public void UpdateNPCInteractions(GameManager gameManager)
    {
        // Process NPC dialogs
        if (NPC.DialogBox == null)
        {
            NPC.DialogBox = new Dialog(gameManager.UIManager.Gui, new(Constants.Middle.X - 600, Constants.NativeResolution.Y - 300), new(1200, 200), new Color(100, 100, 100) * 0.5f, Color.White, "", PixelOperator, borderColor: new Color(40, 40, 40) * 0.5f) { IsVisible = false };
            gameManager.UIManager.Gui.Widgets.Add(NPC.DialogBox);
        }
        (NPC npc, float dist) interacting = new(NPC.Null, float.MaxValue);
        if (NPC.NPCsNearby.Count > 0)
        {
            interacting = NPC.NPCsNearby[0];
            for (int n = 1; n < NPC.NPCsNearby.Count; n++)
            {
                if (NPC.NPCsNearby[n].dist < interacting.dist)
                    interacting = NPC.NPCsNearby[n];
            }
            // Same NPC
            string text = NPC.DialogBox.Text;
            if (text.Contains(']') && text[1..text.IndexOf(']')] == interacting.npc.Name)
                NPC.DialogBox.SetText(interacting.npc.GetFullDialog(), respeak: DialogRespeak.Auto);
            else
                NPC.DialogBox.SetText(interacting.npc.GetFullDialog(), respeak: DialogRespeak.Always);
            NPC.DialogBox.IsVisible = true;
        }
        else
            NPC.DialogBox.IsVisible = false;

        // Shop
        if (NPC.NPCsNearby.Count > 0)
        {
            if (InputManager.KeyPressed(Keys.D1)) interacting.npc.Buy(0, Inventory, gameManager);
            if (InputManager.KeyPressed(Keys.D2)) interacting.npc.Buy(1, Inventory, gameManager);
            if (InputManager.KeyPressed(Keys.D3)) interacting.npc.Buy(2, Inventory, gameManager);
            if (InputManager.KeyPressed(Keys.D4)) interacting.npc.Buy(3, Inventory, gameManager);
            if (InputManager.KeyPressed(Keys.D5)) interacting.npc.Buy(4, Inventory, gameManager);
        }

        NPC.NPCsNearby.Clear();
    }
    public void CheckForLoot(GameManager gameManager)
    {
        // Check if can pick up and search
        if (Inventory.IsFull()) return;
        for (int l = 0; l < gameManager.LevelManager.Level.Loot.Count; l++)
        {
            Loot loot = gameManager.LevelManager.Level.Loot[l];
            if (gameManager.GameTime - loot.Birth < 3) continue; // Prevent picking up things just dropped
            // Pick up loot
            if (PointTools.DistanceSquared(CameraManager.PlayerFoot, loot.Location + new Point(20, 20)) <= Constants.TileSize.X * Constants.TileSize.Y * .5f)
            {
                gameManager.UIManager.LootNotifications.AddNotification($"+{loot.DisplayName}");
                (bool success, Item leftover) = Inventory.AddItem(new(loot.Item.Type, loot.Item.Amount));
                if (success)
                {
                    loot.Dispose();
                    gameManager.LevelManager.Level.Loot.Remove(loot);
                }
                else if (leftover.Amount < loot.Item.Amount)
                    loot.Item.Amount = leftover.Amount;
                else
                    continue;
                // 
                LightingManager.RemoveLight($"Loot_{loot.UID}");
                SoundManager.PlaySound("Pickup", pitch: RandomManager.RandomFloat() / 2 - .25f);
            }
        }
    }
    public void Draw(GameManager gameManager)
    {
        if (StateManager.State != GameState.Game) return;

        // Draw player
        DrawPlayer(gameManager);
        if (DebugManager.DrawHitboxes)
        {
            DrawPlayerHitbox(gameManager);
            foreach (Attack attack in Attacks)
                FillRectangle(gameManager.Batch, new(attack.Hitbox.Position.ToPoint() - CameraManager.Camera.ToPoint() + Constants.Middle, new Point((int)attack.Hitbox.Width, (int)attack.Hitbox.Height)), Constants.DebugPinkTint);
        }

        // Draw marked tile
        if (TileBelow != null && DebugManager.CollisionDebug)
        {
            Point belowDest = TileBelow.Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
            gameManager.Batch.FillRectangle(new(belowDest.ToVector2(), Constants.TileSize), Color.Red * 0.5f);
        }
        if (TileBumps != null && DebugManager.CollisionDebug) {
            foreach (Tile tile in TileBumps)
            {
                Point bumpDest = tile.Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
                gameManager.Batch.FillRectangle(new(bumpDest.ToVector2(), Constants.TileSize), Color.Blue * 0.5f);
            }
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
        DrawTexture(gameManager.Batch, TextureID.BlueMage, pos, scale: Constants.PlayerScale, source: source);
        // Draw equipped item
        if (Inventory.Equipped != null)
        {
            bool left = PlayerDirection == Direction.Left;
            var leftShift = left ? new(TextureManager.Metadata[Inventory.Equipped.Texture].Size.X * 2, 0) : Point.Zero;
            Point itemPos = Constants.Middle + CameraManager.CameraOffset.ToPoint() - leftShift + Constants.MageItemShift.Scaled(left ? -1 : 1);
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
                // Check
                Tile? tile = gameManager.LevelManager.GetTile(new Point(x, y));
                if (tile == null || tile.IsWalkable) continue;
                
                // Bump
                tile.OnPlayerCollide(gameManager, this);
                TileBumps.Add(tile);
            }
        }
    }
    public void CommitContainerChanges(IContainer container)
    {
        if (container.Items == null) return;
        for (int x = 0; x < ContainerInventory.Width; x++)
            for (int y = 0; y < ContainerInventory.Height; y++)
                container.Items[x, y] = ContainerInventory.Items[x, y];
    }
    public void OpenInventory()
    {
        Inventory.Opened = true;
        StateManager.OverlayState = OverlayState.Container;
        SoundManager.PlaySound("Click");
    }

    public void OpenContainer(IContainer container)
    {
        if (container.Items == null) return;

        Inventory.Opened = true;

        OpenedContainer = container;
        ContainerInventory.SetItems(container.Items);
        ContainerInventory.Opened = true;

        StateManager.OverlayState = OverlayState.Container;
        SoundManager.PlaySound("Click");
    }
    public void CloseContainer()
    {
        if (Inventory.Opened)
            SoundManager.PlaySound("Click");

        if (OpenedContainer != null)
            CommitContainerChanges(OpenedContainer);
        OpenedContainer = null;
        ContainerInventory.SetItems(null);
        ContainerInventory.Opened = false;
        Inventory.Opened = false;
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

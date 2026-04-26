using Quest.Gui;
using Quest.Interaction;
namespace Quest.Managers;

public class PlayerManager : IEntity
{
    public ushort UID => 0;
    // Events
    public event Action<int>? EquippedSlotChanged;
    // Properties
    // Player stats
    public StatusManager StatusManager { get; } = new();
    private int _health = Constants.PlayerBaseHealth;
    public int Health
    {
        get => _health;
        set
        {
            _health = value;
            Game.OverlayManager.HealthBar.CurrentValue = value;
        }
    }
    private int _maxHealth = Constants.PlayerBaseHealth;
    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = value;
            Game.OverlayManager.HealthBar.MaxValue = value;
        }
    }
    public int Speed => (int)(Constants.PlayerBaseSpeed * StatusManager.GetSpeedMult());
    // Inventory and UI
    public NotificationArea StatusArea { get; } = new(new(5, 5), 400, PixelOperatorSubtitle, color: Color.Gray, hAlign: HorizontalAlignment.Left, vAlign: VerticalAlignment.Top);
    public bool InventoryOpen { get; set; } = false;
    public Container Inventory { get; }
    public UserInterface InventoryUI { get; }
    public UserInterface? OpenedInterface { get; set; } = null;
    private int equippedSlot = 0;
    public int EquippedSlot
    {
        get => equippedSlot;
        set { equippedSlot = value; EquippedSlotChanged?.Invoke(equippedSlot); }
    }
    public Item? HoveredItem { get; private set; }
    public Item? EquippedItem => EquippedSlot >= 0 && EquippedSlot < Inventory.Items.Length ? Inventory.Items[EquippedSlot] : null;
    public (UserInterface ui, int idx)? MouseSelection { get; set; } // Item being moved with mouse and its original inventory
    // Position and collision
    public RectangleF Bounds => GetHitbox();

    public Tile? TileBelow { get; private set; }
    public List<Tile> TileBumps { get; private set; } = [];
    public Direction PlayerDirection { get; private set; }
    private float moveX, moveY;
    private GameManager Game = null!;
    public PlayerManager()
    {
        Inventory = new(new Item[6 * 4]);
        InventoryUI = UserInterface.InventoryUI;
        InventoryUI.BindContainer(Inventory);

        InventoryUI.OnSlotClick += SlotClicked;
        InventoryUI.OnSlotDrop += SlotDropped;
        InventoryUI.OnSlotHover += SlotHovered;
    }

    public void Update(GameManager gameManager)
    {
        Game = gameManager;
        if (StateManager.State != GameState.Game && StateManager.State != GameState.Editor) return;
        if (StateManager.OverlayState == OverlayState.Pause) return;

        // Potions
        StatusManager.Update(gameManager, this);

        // Update player position
        TileBumps.Clear();
        UpdatePositions(gameManager);

        // Check projectiles
        CheckProjectiles(gameManager);

        // Toggle inventory
        if (InputManager.BindPressed(InputAction.ToggleInventory) && StateManager.OverlayState != OverlayState.Typing)
        {
            if (InventoryOpen)
            {
                CloseInventory();
                CloseInterface();
            }
            else OpenInventory();
        }
        if (InputManager.BindPressed(InputAction.Back))
        {
            CloseInventory();
            CloseInterface();
        }

        // Loot
        DebugManager.StartBenchmark("UpdateLoot");
        CheckForLoot(gameManager);
        DebugManager.EndBenchmark("UpdateLoot");

        // Movement
        if (!InventoryOpen)
        {
            // Movement
            if (!CameraManager.FreeCam)
                UpdateMovements(gameManager);

            // Item use
            DebugManager.StartBenchmark("UpdateAttacks");
            if (InputManager.LMouseClicked) EquippedItem?.PrimaryUse(gameManager, this);
            else if (InputManager.RMouseClicked) EquippedItem?.SecondaryUse(gameManager, this);
            DebugManager.EndBenchmark("UpdateAttacks");
        }


        // Inventory
        DebugManager.StartBenchmark("InventoryUpdate");

        StatusArea.Update(GameManager.DeltaTime);

        // Change equipped item with hotkeys
        if (!InventoryOpen)
        {
            if (InputManager.BindPressed(InputAction.Hotbar1)) EquippedSlot = 0;
            if (InputManager.BindPressed(InputAction.Hotbar2)) EquippedSlot = 1;
            if (InputManager.BindPressed(InputAction.Hotbar3)) EquippedSlot = 2;
            if (InputManager.BindPressed(InputAction.Hotbar4)) EquippedSlot = 3;
            if (InputManager.BindPressed(InputAction.Hotbar5)) EquippedSlot = 4;
            if (InputManager.BindPressed(InputAction.Hotbar6)) EquippedSlot = 5;
            // Change equipped item with scroll
            if (InputManager.ScrolledUp) EquippedSlot = (EquippedSlot - 1 + Chest.Size.X) % Chest.Size.X;
            if (InputManager.ScrolledDown) EquippedSlot = (EquippedSlot + 1) % Chest.Size.X;
        }

        // 
        InventoryUI.GetSlot(EquippedSlot).Mark(Color.Salmon);
        HoveredItem = null;

        // Inventory updates
        InventoryUI.Update(InventoryOpen ? null : "hotbar");
        OpenedInterface?.Update();

        DebugManager.EndBenchmark("InventoryUpdate");

        // NPC
        UpdateNPCInteractions(gameManager);

        // Player lighting
        if (EquippedItem is Light light)
            LightingManager.SetLight("PlayerLightItem", CameraManager.PlayerFoot - CameraManager.Camera.ToPoint() + Constants.Middle, light.LightStrength);
        else
            LightingManager.RemoveLight("PlayerLightItem");
    }
    public void UpdateMovements(GameManager gameManager)
    {
        DebugManager.StartBenchmark("UpdateMovement");
        moveX = 0; moveY = 0;
        moveX += InputManager.BindDown(InputAction.MoveLeft) ? -Speed : 0;
        moveX += InputManager.BindDown(InputAction.MoveRight) ? Speed : 0;
        moveY += InputManager.BindDown(InputAction.MoveUp) ? -Speed : 0;
        moveY += InputManager.BindDown(InputAction.MoveDown) ? Speed : 0;
        Move(gameManager, new(moveX, moveY));
        if (moveX > 0) PlayerDirection = Direction.Right;
        else if (moveX < 0) PlayerDirection = Direction.Left;
        else if (moveY > 0) PlayerDirection = Direction.Down;
        else if (moveY < 0) PlayerDirection = Direction.Up;
        else PlayerDirection = Direction.Forward;
        DebugManager.EndBenchmark("UpdateMovement");
    }
    public void CheckProjectiles(GameManager gameManager)
    {
        DebugManager.StartBenchmark("ProjectileCollisions");

        // Iterate projectiles
        IEntity[] entities = [.. gameManager.LevelManager.Level.Enemies.Values, this];
        for (int p = gameManager.LevelManager.Level.Projectiles.Count - 1; p >= 0; p--)
        {
            Projectile proj = gameManager.LevelManager.Level.Projectiles[p];
            // Iterate entities
            foreach (IEntity entity in entities)
            {
                // Checks
                if (entity.UID == proj.OwnerUID) continue;
                if (proj.Bounds.Intersects(entity.Bounds))
                {
                    // Damage enemy / player
                    if (entity is Enemy enemy)
                    {
                        enemy.Hurt((int)(proj.Damage * StatusManager.GetDamageMult()));
                        Heal(gameManager, (int)(proj.Damage * StatusManager.GetLifestealMult()));
                    }
                    else if (entity is PlayerManager)
                        Hurt(gameManager, (int)(proj.Damage * StatusManager.GetDefenseMult()));

                    // Destroy
                    proj.Destroy();
                    break;
                }
            }

            // Clean up dead projectiles
            if (!proj.IsAlive)
                gameManager.LevelManager.Level.Projectiles.RemoveAt(p);
        }

        DebugManager.EndBenchmark("ProjectileCollisions");
    }
    public void UpdateNPCInteractions(GameManager gameManager)
    {
        // Process NPC dialogs
        if (NPC.DialogBox == null)
        {
            NPC.DialogBox = new Dialog(gameManager.OverlayManager.Gui, new(1200, 200), new Color(100, 100, 100) * 0.5f, Color.White, "", PixelOperator, borderColor: new Color(40, 40, 40) * 0.5f) { IsVisible = false };
            gameManager.OverlayManager.Gui.Widgets.Add(NPC.DialogBox);
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
            if (GameManager.GameTime - loot.Birth < 3) continue; // Prevent picking up things just dropped
            // Pick up loot
            if (PointTools.DistanceSquared(CameraManager.PlayerFoot, loot.Position + new Point(20, 20)) <= Constants.TileSize.X * Constants.TileSize.Y * .5f)
            {
                gameManager.OverlayManager.LootNotifications.AddNotification($"+{loot.DisplayName}");
                Item adding = Item.Create(loot.Item.Type, loot.Item.Amount, loot.Item.CustomName);
                Item leftover = Inventory.AddItem(adding);
                if (leftover.Amount <= 0)
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
                SoundManager.PlaySound("Pickup", pitchVariation: 0.25f);
            }
        }
    }
    public void Draw(GameManager gameManager)
    {
        if (StateManager.State != GameState.Game) return;

        // Draw player
        DrawPlayer(gameManager);

        // Draw marked tile
        if (TileBelow != null && DebugManager.CollisionDebug)
        {
            Point belowDest = TileBelow.Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
            gameManager.Batch.FillRectangle(new(belowDest.ToVector2(), Constants.TileSize), Color.Red * 0.5f);
        }
        if (TileBumps != null && DebugManager.CollisionDebug)
        {
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
        int sourceRow = (int)PlayerDirection;
        // Draw player
        Point pos = Constants.Middle - Constants.MageHalfSize + CameraManager.CameraOffset.ToPoint();
        Rectangle source = GetAnimationSource(TextureID.BlueMage, GameManager.GameTime, duration: sourceRow == 0 ? .5f : .25f, row: sourceRow);
        DrawTexture(gameManager.Batch, TextureID.BlueMage, pos, scale: Constants.PlayerScale, source: source);
        // Draw equipped item
        if (EquippedItem != null)
        {
            bool left = PlayerDirection == Direction.Left;
            var leftShift = left ? new(TextureManager.Metadata[EquippedItem.Texture].Size.X * 2, 0) : Point.Zero;
            Point itemPos = Constants.Middle + CameraManager.CameraOffset.ToPoint() - leftShift + Constants.MageItemShift.Scaled(left ? -1 : 1);
            DrawTexture(gameManager.Batch, EquippedItem.Texture, itemPos, scale: 2, effects: PlayerDirection == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
        // Hitbox
        DebugManager.DrawHitbox(gameManager.Batch, this);
    }
    public static RectangleF GetHitbox()
    {
        Point[] points = new Point[4];
        for (int c = 0; c < Constants.PlayerCorners.Length; c++)
            points[c] = CameraManager.PlayerFoot + Constants.PlayerCorners[c];
        return new RectangleF(points[0].X, points[0].Y, points[1].X - points[0].X, points[2].Y - points[1].Y);
    }
    public void Move(GameManager gameManager, Vector2 move)
    {
        // Move
        if (move == Vector2.Zero) return;
        Vector2 finalMove = Vector2.Normalize(move) * GameManager.DeltaTime * Speed;

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

        // Decal
        if (gameManager.LevelManager.Level.Decals.TryGetValue(CameraManager.TileCoord.ToByteCoord(), out var dec))
            dec.OnPlayerEnter(gameManager, this);
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
    public void OpenInventory()
    {
        InventoryOpen = true;
        StateManager.OverlayState = OverlayState.Container;
        SoundManager.PlaySound("Click");
    }
    public void CloseInventory()
    {
        InventoryOpen = false;
        StateManager.OverlayState = OverlayState.None;
        SoundManager.PlaySound("Click");
    }

    public void OpenInterface(UserInterface ui)
    {
        CloseInterface();
        InventoryOpen = true;

        OpenedInterface = ui;
        OpenedInterface.OnSlotClick += SlotClicked;
        OpenedInterface.OnSlotDrop += SlotDropped;
        OpenedInterface.OnSlotHover += SlotHovered;

        StateManager.OverlayState = OverlayState.Container;
        SoundManager.PlaySound("Click");
    }
    public void CloseInterface()
    {
        if (OpenedInterface == null) return;

        OpenedInterface.OnSlotClick -= SlotClicked;
        OpenedInterface.OnSlotDrop -= SlotDropped;
        OpenedInterface.OnSlotHover -= SlotHovered;
        OpenedInterface = null;
    }
    public void SlotClicked(int slot, UserInterface ui)
    {
        if (MouseSelection == null)
        {
            if (ui.BoundContainer?.Items[slot] == null) return;
            MouseSelection = (ui, slot);
        }
        else if (MouseSelection.Value.ui.BoundContainer != null && ui.BoundContainer != null)
        {
            bool success = Container.MoveItemUI(MouseSelection.Value.ui, MouseSelection.Value.idx, ui, slot, split: InputManager.RMouseDown);
            if (success)
                MouseSelection = null;
        }
    }
    public void SlotDropped(int slot, UserInterface ui)
    {
        if (ui.BoundContainer?.Items[slot] == null) return;
        if (!InventoryOpen) return;
        Item? item = ui.BoundContainer.Items[slot];
        if (item == null) return;

        Game.LevelManager.Level.Loot.Add(new Loot(new(item.Type, item.Amount, item.CustomName), CameraManager.PlayerFoot + new Point(0, 20), GameManager.GameTime));
        ui.BoundContainer.SetSlot(slot, null);
    }
    public void SlotHovered(int slot, UserInterface ui)
    {
        Item? hovered = ui.BoundContainer?.Items[slot];
        HoveredItem = hovered;
    }
    public void UpdatePositions(GameManager gameManager)
    {
        TileBelow = gameManager.LevelManager.GetTile(CameraManager.TileCoord);
    }
    public void Hurt(GameManager gameManager, int damage)
    {
        Health -= damage;
        gameManager.OverlayManager.LootNotifications.AddNotification($"-{damage}", Color.Orange, duration: 2);
        if (Health <= 0)
        {
            Health = 0;
            StateManager.OverlayState = OverlayState.Death;
        }
    }
    public void Heal(GameManager gameManager, int health)
    {
        health = Math.Min(health, MaxHealth - Health);
        Health += health;
        if (health > 0)
            gameManager.OverlayManager.LootNotifications.AddNotification($"+{health}", Color.Green, duration: 2);
    }
}

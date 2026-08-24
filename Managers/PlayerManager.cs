using Quest.Gui;
using Quest.Interaction;
namespace Quest.Managers;

public class PlayerManager : IEntity, IStatusEffectable
{
    public ushort UID => 0;
    // Status effect
    public Dictionary<StatusEffect, float> StatusEffects { get; set; } = new();
    // Events
    public event Action<int>? EquippedSlotChanged;
    // Properties
    // Health
    private int _health = Constants.PlayerBaseHealth;
    public int Health
    {
        get => _health;
        set { _health = value; Game.OverlayManager.HealthBar.CurrentValue = value; }
    }
    private int _maxHealth = Constants.PlayerBaseHealth;
    public int MaxHealth
    {
        get => _maxHealth;
        set { _maxHealth = value; Game.OverlayManager.HealthBar.MaxValue = value; }
    }
    // Hunger
    private int _hunger = Constants.PlayerBaseHunger;
    public int Hunger
    {
        get => _hunger;
        set { _hunger = value; Game.OverlayManager.HungerBar.CurrentValue = value; }
    }
    private int _maxHunger = Constants.PlayerBaseHunger;
    public int MaxHunger
    {
        get => _maxHunger;
        set { _maxHunger = value; Game.OverlayManager.HungerBar.MaxValue = value; }
    }
    public bool IsAlive => _health > 0;
    public int Speed => (int)(Constants.PlayerBaseSpeed * StatusManager.GetSpeedMult(this));
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
        set
        {
            equippedSlot = value;
            EquippedSlotChanged?.Invoke(equippedSlot);
            TimerManager.TryRemove("MeleeAttack");
        }
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

        TimerManager.SetTimer("PlayerHungerLoss", Constants.SecondsPerHungerLoss, null);
    }

    public void Update(GameManager gameManager)
    {
        Game = gameManager;
        if (gameManager.StateManager.State != GameState.Game && gameManager.StateManager.State != GameState.Editor) return;
        if (gameManager.StateManager.OverlayState is OverlayState.Pause or OverlayState.Death) return;

        // Health and hunger
        StatusManager.Update(gameManager, this);
        UpdateHealth(gameManager);

        // Update player position
        TileBumps.Clear();
        UpdatePositions(gameManager);

        // Check projectiles
        CheckProjectiles(gameManager);

        // Toggle inventory
        if (InputManager.BindPressed(InputAction.ToggleInventory) && gameManager.StateManager.OverlayState != OverlayState.GUI)
        {
            if (InventoryOpen)
            {
                CloseInventory(gameManager);
                CloseInterface();
            }
            else OpenInventory(gameManager);
        }
        if (InputManager.KeyPressed(Keys.Escape))
        {
            CloseInventory(gameManager);
            CloseInterface();
        }

        // Toggle info
        if (InputManager.BindPressed(InputAction.ToggleWorldInfo) && gameManager.StateManager.OverlayState != OverlayState.GUI)
            gameManager.OverlayManager.ToggleWorldInfobox(gameManager.LevelManager.Level.Metadata);


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
            if (InputManager.BindDown(InputAction.PrimaryUse)) EquippedItem?.PrimaryUse(gameManager, this);
            else if (InputManager.BindDown(InputAction.SecondaryUse)) EquippedItem?.SecondaryUse(gameManager, this);
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
        InventoryUI.Update(gameManager, InventoryOpen ? null : "hotbar");
        OpenedInterface?.Update(gameManager);

        DebugManager.EndBenchmark("InventoryUpdate");

        // NPC
        UpdateNPCInteractions(gameManager);

        // Player lighting
        if (EquippedItem is Light light)
            LightingManager.SetLight("PlayerLightItem", CameraManager.TileCoord, light.LightStrength);
        else
            LightingManager.RemoveLight("PlayerLightItem");
    }
    public void UpdateHealth(GameManager gameManager)
    {
        // Hunger
        if (TimerManager.IsCompleteOrMissing("PlayerHungerLoss"))
        {
            TimerManager.SetTimer("PlayerHungerLoss", Constants.SecondsPerHungerLoss, null);
            Hunger -= StatusManager.GetCravingsMult(this);
        }
        // Natural regen
        if (Hunger > MaxHunger * 0.8f && Health < MaxHealth && TimerManager.IsCompleteOrMissing("PlayerNaturalRegen"))
        {
            TimerManager.SetTimer("PlayerNaturalRegen", Constants.SecondsPerNaturalRegen, null);
            Heal(gameManager, Constants.NaturalRegenRate);
            Hunger -= 1;
        }
        // Starvation
        if (Hunger < MaxHunger * 0.2f && TimerManager.IsCompleteOrMissing("PlayerStarvation"))
        {
            StatusManager.AddStatusEffect(this, StatusEffect.Weakness, Constants.SecondsPerStarvation + 1);
            StatusManager.AddStatusEffect(this, StatusEffect.Slowness, Constants.SecondsPerStarvation + 1);
            TimerManager.SetTimer("PlayerStarvation", Constants.SecondsPerStarvation, null);
            if (Hunger <= 0)
                Hurt(gameManager, Constants.StarvationRate);
        }
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
                        enemy.Hurt(gameManager, (int)(proj.Damage * StatusManager.GetDamageMult(this)));
                        Heal(gameManager, (int)(proj.Damage * StatusManager.GetLifestealMult(this)));
                    }
                    else if (entity is PlayerManager)
                    {
                        Hurt(gameManager, (int)(proj.Damage * StatusManager.GetDefenseMult(this)));
                    }

                    // Status effects
                    if (entity is IStatusEffectable effectable)
                    {
                        var projEffect = proj.GetProjectileEffect();
                        if (projEffect != null)
                            StatusManager.AddStatusEffect(effectable, projEffect.Value.effect, projEffect.Value.duration);
                    }

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
            NPC.DialogBox = new Dialog(gameManager.OverlayManager.Gui, null, new(1200, 200), new Color(100, 100, 100) * 0.5f, Color.White, "", PixelOperator, borderColor: new Color(40, 40, 40) * 0.5f) { IsVisible = false };
            gameManager.OverlayManager.Gui.Widgets.Add(NPC.DialogBox);
        }
        (NPC npc, float distSq) interacting = new(NPC.Null, float.MaxValue);
        if (NPC.NPCsNearby.Count > 0)
        {
            interacting = NPC.NPCsNearby[0];
            for (int n = 1; n < NPC.NPCsNearby.Count; n++)
            {
                if (NPC.NPCsNearby[n].distSq < interacting.distSq)
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
        if (gameManager.StateManager.State != GameState.Game) return;

        // Draw player
        DrawPlayer(gameManager);

        // Draw marked tile
        if (TileBelow != null && DebugManager.CollisionDebug)
        {
            Point belowDest = CameraManager.TileToScreen(TileBelow.Location);
            gameManager.Batch.FillRectangle(new(belowDest.ToVector2(), Constants.TileSize), Color.Red * 0.5f);
        }
        if (TileBumps != null && DebugManager.CollisionDebug)
        {
            foreach (Tile tile in TileBumps)
            {
                Point bumpDest = CameraManager.TileToScreen(tile.Location);
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
        DrawTexture(gameManager.Batch, TextureID.BlueMage, pos, scale: new(Constants.PlayerScale), source: source);
        // Draw equipped item
        if (EquippedItem != null)
        {
            bool left = PlayerDirection == Direction.Left;
            var leftShift = left ? new(TextureManager.Metadata[EquippedItem.Texture].Size.X * 2, 0) : Point.Zero;
            Point itemPos = Constants.Middle + CameraManager.CameraOffset.ToPoint() + TextureManager.Metadata[EquippedItem.Texture].Size - leftShift + Constants.MageItemShift.Scaled(left ? -1 : 1);
            float rotate = Math.Clamp(((float)Math.Pow(TimerManager.TryGetTimer("MeleeAttack")?.Progress ?? 0, 0.5f)) * (PlayerDirection == Direction.Left ? -1 : 1) * 2, -1f, 1f);
            DrawTexture(gameManager.Batch, EquippedItem.Texture, itemPos, scale: new(2), effects: PlayerDirection == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, rotation: rotate, origin: TextureManager.Metadata[EquippedItem.Texture].Size.ToVector2() / 2);
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
            Point coord = CameraManager.WorldToTile(CameraManager.PlayerFoot + Constants.PlayerCorners[o]);
            TileBelow = gameManager.LevelManager.GetTile(coord);
            if (TileBelow == null || !TileBelow.IsWalkable) return true;
        }
        return false;
    }
    public void CheckBumping(GameManager gameManager, Vector2 finalMove)
    {
        Rectangle playerBounds = new((CameraManager.CameraDest + finalMove + new Vector2(-Constants.PlayerBox.X / 2, Constants.PlayerBox.Y)).ToPoint(), Constants.PlayerBox);
        Point topLeftTile = CameraManager.WorldToTile(playerBounds.Location);
        Point bottomRightTile = CameraManager.WorldToTile(playerBounds.Location + playerBounds.Size);

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
    public void OpenInventory(GameManager gameManager)
    {
        InventoryOpen = true;
        gameManager.StateManager.OverlayState = OverlayState.Container;
        SoundManager.PlaySound("Click");
    }
    public void CloseInventory(GameManager gameManager)
    {
        InventoryOpen = false;
        gameManager.StateManager.OverlayState = OverlayState.None;
        SoundManager.PlaySound("Click");
    }

    public void OpenInterface(GameManager gameManager, UserInterface ui)
    {
        CloseInterface();
        InventoryOpen = true;

        OpenedInterface = ui;
        OpenedInterface.OnSlotClick += SlotClicked;
        OpenedInterface.OnSlotDrop += SlotDropped;
        OpenedInterface.OnSlotHover += SlotHovered;

        gameManager.StateManager.OverlayState = OverlayState.Container;
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

            // Shift click
            if ((InputManager.KeyDown(Keys.LeftShift) || InputManager.KeyDown(Keys.RightShift)))
            {
                // Null check
                if (OpenedInterface == null || OpenedInterface.BoundContainer == null) return;

                // Inventory to container / container to inventory
                var fromUI = ui == InventoryUI ? InventoryUI : OpenedInterface;
                var toUI = ui == InventoryUI ? OpenedInterface : InventoryUI;

                // Try every slot
                for (int i = 0; i < toUI.GetSlotElements().Length; i++)
                    if (Container.MoveItemUI(fromUI, slot, toUI, i, split: InputManager.RMouseDown, allowSwap: false))
                        break;

                // Clean
                OpenedInterface.BoundContainer.RemoveEmptyItems();
                Inventory.RemoveEmptyItems();
                return;
            }
            // Regular move
            else
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

        Game.LevelManager.Level.Loot.Add(new Loot(new(item.Type, item.Amount, item.CustomName), CameraManager.PlayerFoot + new Point(0, 20)));
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
            Die(gameManager);

        }
    }
    public void Die(GameManager gameManager)
    {
        Health = 0;

        CloseInterface();
        CloseInventory(gameManager);

        TimerManager.SetTimer("ScreenFadeOut", 2f, null);

        gameManager.StateManager.OverlayState = OverlayState.Death;
    }
    public void Heal(GameManager gameManager, int health)
    {
        health = Math.Min(health, MaxHealth - Health);
        Health += health;
        if (health > 0)
            gameManager.OverlayManager.LootNotifications.AddNotification($"+{health}", Color.Green, duration: 2);
    }
    public void Eat(GameManager gameManager, int hunger)
    {
        hunger = Math.Min(hunger, Constants.PlayerBaseHunger - Hunger);
        Hunger += hunger;
        if (hunger > 0)
            gameManager.OverlayManager.LootNotifications.AddNotification($"+{hunger}", Color.Goldenrod, duration: 2);
    }
}

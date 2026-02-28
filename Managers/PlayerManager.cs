using Quest.Gui;
using Quest.Interaction;
using System.Linq;
namespace Quest.Managers;

public class Attack(int damage, RectangleF hitbox)
{
    public int Damage { get; } = damage;
    public RectangleF Hitbox { get; } = hitbox;
}
public class PlayerManager
{
    // Events
    public event Action<Item?>? ItemSelected;
    // Properties
    // Inventory and UI
    public Container Inventory { get; }
    public UserInterface InventoryUI { get; }
    public UserInterface? OpenedInterface { get; set; } = null;
    public int EquippedSlot { get; set; } = 0;
    public Item? EquippedItem => EquippedSlot >= 0 && EquippedSlot < Inventory.Items.Length ? Inventory.Items[EquippedSlot] : null;
    public (UserInterface ui, int idx)? MouseSelection { get; set; } // Item being moved with mouse and its original inventory
    // Position and collision
    public Tile? TileBelow { get; private set; }
    public List<Tile> TileBumps { get; private set; } = [];
    public Direction PlayerDirection { get; private set; }
    public List<Attack> Attacks { get; private set; } = [];
    private float moveX, moveY;
    private GameManager Game;
    public PlayerManager()
    {
        Inventory = new(new Item[6*4]);
        InventoryUI = UserInterface.InventoryUI;
        InventoryUI.BindContainer(Inventory);
        InventoryUI.OnSlotClick += SlotClicked;
        InventoryUI.OnSlotDrop += SlotDropped;

        Inventory.SetSlot(0, new Item(ItemTypes.Apple, 1, "Green Apple"));
    }

    public void Update(GameManager gameManager)
    {
        Game = gameManager;
        if (StateManager.State != GameState.Game && StateManager.State != GameState.Editor) return;
        if (StateManager.OverlayState == OverlayState.Pause) return;

        // Update player position
        TileBumps.Clear();
        UpdatePositions(gameManager);

        // Toggle inventory
        if (InputManager.KeyPressed(Keys.I))
        {
            if (InventoryUI.IsVisible)
            {
                CloseInventory();
                CloseInterface();
            }
            else OpenInventory();
        }
        if (InputManager.KeyPressed(Keys.Escape))
        {
            CloseInventory();
            CloseInterface();
        }

        // Loot
        DebugManager.StartBenchmark("UpdateLoot");
        CheckForLoot(gameManager);
        DebugManager.EndBenchmark("UpdateLoot");

        // Movement
        if (!InventoryUI.IsVisible)
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
            if (InputManager.LMouseClicked) EquippedItem?.PrimaryUse(this);
            else if (InputManager.RMouseClicked) EquippedItem?.SecondaryUse(this);
            DebugManager.EndBenchmark("UpdateAttacks");
        }


        // Inventory
        DebugManager.StartBenchmark("InventoryUpdate");

        // Change equipped item with hotkeys
        if (InputManager.KeyPressed(Keys.D1)) EquippedSlot = 0;
        if (InputManager.KeyPressed(Keys.D2)) EquippedSlot = 1;
        if (InputManager.KeyPressed(Keys.D3)) EquippedSlot = 2;
        if (InputManager.KeyPressed(Keys.D4)) EquippedSlot = 3;
        if (InputManager.KeyPressed(Keys.D5)) EquippedSlot = 4;
        if (InputManager.KeyPressed(Keys.D6)) EquippedSlot = 5;
        // Change equipped item with scroll
        if (InputManager.ScrolledUp) EquippedSlot = (EquippedSlot - 1 + Chest.Size.X) % Chest.Size.X;
        if (InputManager.ScrolledDown) EquippedSlot = (EquippedSlot + 1) % Chest.Size.X;

        // 
        InventoryUI.GetSlot(EquippedSlot).Mark(Color.Salmon);

        // Inventory updates
        InventoryUI.Update();
        OpenedInterface?.Update();

        DebugManager.EndBenchmark("InventoryUpdate");


        // NPC
        UpdateNPCInteractions(gameManager);

        // Player lighting
        if (EquippedItem is Light light)
            LightingManager.SetLight("PlayerLightItem", CameraManager.PlayerFoot - CameraManager.Camera.ToPoint() + Constants.Middle, light.LightStrength, light.LightColor);
        else
            LightingManager.RemoveLight("PlayerLightItem");
    }
    public void UpdateNPCInteractions(GameManager gameManager)
    {
        // Process NPC dialogs
        if (NPC.DialogBox == null)
        {
            NPC.DialogBox = new Dialog(gameManager.OverlayManager.Gui, new(Constants.Middle.X - 600, Constants.NativeResolution.Y - 300), new(1200, 200), new Color(100, 100, 100) * 0.5f, Color.White, "", PixelOperator, borderColor: new Color(40, 40, 40) * 0.5f) { IsVisible = false };
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
        if (!Inventory.IsFull()) return;
        for (int l = 0; l < gameManager.LevelManager.Level.Loot.Count; l++)
        {
            Loot loot = gameManager.LevelManager.Level.Loot[l];
            if (gameManager.GameTime - loot.Birth < 3) continue; // Prevent picking up things just dropped
            // Pick up loot
            if (PointTools.DistanceSquared(CameraManager.PlayerFoot, loot.Location + new Point(20, 20)) <= Constants.TileSize.X * Constants.TileSize.Y * .5f)
            {
                gameManager.OverlayManager.LootNotifications.AddNotification($"+{loot.DisplayName}");
                Item leftover = Inventory.AddItem(new(loot.Item.Type, loot.Item.Amount));
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
        int sourceRow = (int)PlayerDirection;
        // Draw player
        Point pos = Constants.Middle - Constants.MageHalfSize + CameraManager.CameraOffset.ToPoint();
        Rectangle source = GetAnimationSource(TextureID.BlueMage, gameManager.GameTime, duration: sourceRow == 0 ? .5f : .25f, row: sourceRow);
        DrawTexture(gameManager.Batch, TextureID.BlueMage, pos, scale: Constants.PlayerScale, source: source);
        // Draw equipped item
        if (EquippedItem != null)
        {
            bool left = PlayerDirection == Direction.Left;
            var leftShift = left ? new(TextureManager.Metadata[EquippedItem.Texture].Size.X * 2, 0) : Point.Zero;
            Point itemPos = Constants.Middle + CameraManager.CameraOffset.ToPoint() - leftShift + Constants.MageItemShift.Scaled(left ? -1 : 1);
            DrawTexture(gameManager.Batch, EquippedItem.Texture, itemPos, scale: 2, effects: PlayerDirection == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
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
    public void OpenInventory()
    {
        InventoryUI.Show();
        StateManager.OverlayState = OverlayState.Container;
        SoundManager.PlaySound("Click");
    }
    public void CloseInventory()
    {
        InventoryUI.Hide();
        StateManager.OverlayState = OverlayState.None;
        SoundManager.PlaySound("Click");
    }

    public void OpenInterface(UserInterface ui)
    {
        CloseInterface();
        InventoryUI.Show();

        OpenedInterface = ui;
        OpenedInterface.OnSlotClick += SlotClicked;
        OpenedInterface.OnSlotDrop += SlotDropped;

        StateManager.OverlayState = OverlayState.Container;
        SoundManager.PlaySound("Click");
    }
    public void CloseInterface()
    {
        if (OpenedInterface == null) return;

        OpenedInterface.OnSlotClick -= SlotClicked;
        OpenedInterface.OnSlotDrop -= SlotDropped;
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
            Container.MoveItem(MouseSelection.Value.ui.BoundContainer, MouseSelection.Value.idx, ui.BoundContainer, slot, split: InputManager.RMouseDown);
            MouseSelection = null;
        }
    }
    public void SlotDropped(int slot, UserInterface ui)
    {
        if (ui.BoundContainer?.Items[slot] == null) return;
        Item? item = ui.BoundContainer.Items[slot];
        if (item == null) return;

        Game.LevelManager.Level.Loot.Add(new Loot(new(item.Type, item.Amount, item.CustomName), CameraManager.PlayerFoot + new Point(0, 20), Game.GameTime));
        ui.BoundContainer.SetSlot(slot, null);
    }
    public void UpdatePositions(GameManager gameManager)
    {
        TileBelow = gameManager.LevelManager.GetTile(CameraManager.TileCoord);
    }
    public void DamagePlayer(GameManager gameManager, int damage)
    {
        gameManager.OverlayManager.HealthBar.CurrentValue -= damage;
        if (gameManager.OverlayManager.HealthBar.CurrentValue <= 0)
        {
            gameManager.OverlayManager.HealthBar.CurrentValue = 0;
            StateManager.State = GameState.Death;
        }
    }
    public void AddAttack(Attack attack)
    {
        Attacks.Add(attack);
    }
}

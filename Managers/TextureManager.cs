﻿using Microsoft.Xna.Framework.Content;

namespace Quest.Managers;

public class Metadata(Point size, Point tileMap, string type)
{
    public Point Size { get; private set; } = size;
    public Point TileMap { get; private set; } = tileMap;
    public string Type { get; private set; } = type;
}
public static class TextureManager
{
    public enum TextureID
    {
        Null,
        Pixel,
        // Characters
        BlueMage,
        GrayMage,
        WhiteMage,
        PurpleWizard,
        WhiteWizard,
        Ghost,
        // Gui
        CursorArrow,
        DialogBox,
        GuiBackground,
        Slot,
        Speech,
        // Items
        Lantern,
        Pickaxe,
        PhiCoin,
        DeltaCoin,
        GammaCoin,
        Key,
        ActivePalantir,
        InactivePalantir,
        SteelSword,
        DiamondSword,
        // Tiles
        Dirt,
        Flooring,
        Grass,
        Sand,
        Sky,
        Stairs,
        StoneWall,
        Water,
        Darkness,
        Door,
        WoodPlanks,
        Stone,
        // Decals
        Torch,
        BlueTorch,
        WaterPuddle,
        BloodPuddle,
        Footprint,
        // Effecs
        Glow,
        Slash,
    }

    private static List<string> errors = [];
    public static Dictionary<TextureID, Texture2D> Textures { get; private set; } = [];
    public static Dictionary<TextureID, Metadata> Metadata { get; private set; } = [];
    private static Texture2D Pixel { get; set; } = null!;
    // Fonts
    public static SpriteFont PixelOperator { get; private set; } = null!;
    public static SpriteFont PixelOperatorBold { get; private set; } = null!;
    public static SpriteFont Arial { get; private set; } = null!;
    public static SpriteFont ArialSmall { get; private set; } = null!;

    private static ContentManager? Content { get; set; }
    public static void LoadTextures(ContentManager content)
    {
        Content = content;
        // Load all
        Textures[TextureID.Null] = content.Load<Texture2D>($"Images/Null");
        Textures[TextureID.Pixel] = content.Load<Texture2D>($"Images/Pixel");
        Textures[TextureID.BlueMage] = content.Load<Texture2D>($"Images/Characters/BlueMage");
        Textures[TextureID.GrayMage] = content.Load<Texture2D>($"Images/Characters/GrayMage");
        Textures[TextureID.WhiteMage] = content.Load<Texture2D>($"Images/Characters/WhiteMage");
        Textures[TextureID.PurpleWizard] = content.Load<Texture2D>($"Images/Characters/PurpleWizard");
        Textures[TextureID.WhiteWizard] = content.Load<Texture2D>($"Images/Characters/WhiteWizard");
        Textures[TextureID.Ghost] = content.Load<Texture2D>($"Images/Characters/Ghost");
        Textures[TextureID.CursorArrow] = content.Load<Texture2D>($"Images/Gui/CursorArrow");
        Textures[TextureID.DialogBox] = content.Load<Texture2D>($"Images/Gui/DialogBox");
        Textures[TextureID.GuiBackground] = content.Load<Texture2D>($"Images/Gui/GuiBackground");
        Textures[TextureID.Slot] = content.Load<Texture2D>($"Images/Gui/Slot");
        Textures[TextureID.Speech] = content.Load<Texture2D>($"Images/Gui/Speech");
        Textures[TextureID.Lantern] = content.Load<Texture2D>($"Images/Items/Lantern");
        Textures[TextureID.Pickaxe] = content.Load<Texture2D>($"Images/Items/Pickaxe");
        Textures[TextureID.PhiCoin] = content.Load<Texture2D>($"Images/Items/PhiCoin");
        Textures[TextureID.DeltaCoin] = content.Load<Texture2D>($"Images/Items/DeltaCoin");
        Textures[TextureID.GammaCoin] = content.Load<Texture2D>($"Images/Items/GammaCoin");
        Textures[TextureID.Key] = content.Load<Texture2D>($"Images/Items/Key");
        Textures[TextureID.ActivePalantir] = content.Load<Texture2D>($"Images/Items/ActivePalantir");
        Textures[TextureID.InactivePalantir] = content.Load<Texture2D>($"Images/Items/InactivePalantir");
        Textures[TextureID.SteelSword] = content.Load<Texture2D>($"Images/Items/SteelSword");
        Textures[TextureID.DiamondSword] = content.Load<Texture2D>($"Images/Items/DiamondSword");
        Textures[TextureID.Dirt] = content.Load<Texture2D>($"Images/Tiles/Dirt");
        Textures[TextureID.Flooring] = content.Load<Texture2D>($"Images/Tiles/Flooring");
        Textures[TextureID.Grass] = content.Load<Texture2D>($"Images/Tiles/Grass");
        Textures[TextureID.Sand] = content.Load<Texture2D>($"Images/Tiles/Sand");
        Textures[TextureID.Sky] = content.Load<Texture2D>($"Images/Tiles/Sky");
        Textures[TextureID.Stairs] = content.Load<Texture2D>($"Images/Tiles/Stairs");
        Textures[TextureID.StoneWall] = content.Load<Texture2D>($"Images/Tiles/StoneWall");
        Textures[TextureID.WoodPlanks] = content.Load<Texture2D>($"Images/Tiles/WoodPlanks");
        Textures[TextureID.Water] = content.Load<Texture2D>($"Images/Tiles/Water");
        Textures[TextureID.Darkness] = content.Load<Texture2D>($"Images/Tiles/Darkness");
        Textures[TextureID.Door] = content.Load<Texture2D>($"Images/Tiles/Door");
        Textures[TextureID.Stone] = content.Load<Texture2D>($"Images/Tiles/Stone");
        Textures[TextureID.Torch] = content.Load<Texture2D>($"Images/Decals/Torch");
        Textures[TextureID.BlueTorch] = content.Load<Texture2D>($"Images/Decals/BlueTorch");
        Textures[TextureID.WaterPuddle] = content.Load<Texture2D>($"Images/Decals/WaterPuddle");
        Textures[TextureID.BloodPuddle] = content.Load<Texture2D>($"Images/Decals/BloodPuddle");
        Textures[TextureID.Footprint] = content.Load<Texture2D>($"Images/Decals/Footprint");
        Textures[TextureID.Glow] = content.Load<Texture2D>($"Images/Effects/Glow");
        Textures[TextureID.Slash] = content.Load<Texture2D>($"Images/Effects/Slash");
        Pixel = Textures[TextureID.Pixel];

        foreach (var kv in Textures)
            if (kv.Value == null)
                Logger.Error($"Texture '{kv.Key}' failed to load.");
            else
                Logger.System($"Texture '{kv.Key}' successfully loaded.");
        Logger.System($"Successfully loaded {Textures.Count}/{Enum.GetValues(typeof(TextureID)).Length} textures.");

        // Metadata
        Metadata[TextureID.Null] = new(Textures[TextureID.Null].Bounds.Size, new(1, 1), "null");
        Metadata[TextureID.Pixel] = new(Textures[TextureID.Pixel].Bounds.Size, new(1, 1), "pixel");
        Metadata[TextureID.BlueMage] = new(Textures[TextureID.BlueMage].Bounds.Size, new(4, 5), "character");
        Metadata[TextureID.GrayMage] = new(Textures[TextureID.GrayMage].Bounds.Size, new(4, 5), "character");
        Metadata[TextureID.WhiteMage] = new(Textures[TextureID.WhiteMage].Bounds.Size, new(4, 5), "character");
        Metadata[TextureID.PurpleWizard] = new(Textures[TextureID.PurpleWizard].Bounds.Size, new(2, 1), "character");
        Metadata[TextureID.WhiteWizard] = new(Textures[TextureID.WhiteWizard].Bounds.Size, new(2, 1), "character");
        Metadata[TextureID.Ghost] = new(Textures[TextureID.Ghost].Bounds.Size, new(4, 1), "character");
        Metadata[TextureID.CursorArrow] = new(Textures[TextureID.CursorArrow].Bounds.Size, new(1, 1), "gui");
        Metadata[TextureID.DialogBox] = new(Textures[TextureID.DialogBox].Bounds.Size, new(1, 1), "gui");
        Metadata[TextureID.GuiBackground] = new(Textures[TextureID.GuiBackground].Bounds.Size, new(1, 1), "gui");
        Metadata[TextureID.Slot] = new(Textures[TextureID.Slot].Bounds.Size, new(1, 1), "gui");
        Metadata[TextureID.Speech] = new(Textures[TextureID.Speech].Bounds.Size, new(1, 4), "gui");
        Metadata[TextureID.Lantern] = new(Textures[TextureID.Lantern].Bounds.Size, new(1, 1), "item");
        Metadata[TextureID.Pickaxe] = new(Textures[TextureID.Pickaxe].Bounds.Size, new(1, 1), "item");
        Metadata[TextureID.PhiCoin] = new(Textures[TextureID.PhiCoin].Bounds.Size, new(1, 1), "item");
        Metadata[TextureID.DeltaCoin] = new(Textures[TextureID.DeltaCoin].Bounds.Size, new(1, 1), "item");
        Metadata[TextureID.GammaCoin] = new(Textures[TextureID.GammaCoin].Bounds.Size, new(1, 1), "item");
        Metadata[TextureID.Key] = new(Textures[TextureID.Key].Bounds.Size, new(1, 1), "item");
        Metadata[TextureID.ActivePalantir] = new(Textures[TextureID.ActivePalantir].Bounds.Size, new(1, 1), "item");
        Metadata[TextureID.InactivePalantir] = new(Textures[TextureID.InactivePalantir].Bounds.Size, new(1, 1), "item");
        Metadata[TextureID.SteelSword] = new(Textures[TextureID.SteelSword].Bounds.Size, new(1, 1), "item");
        Metadata[TextureID.DiamondSword] = new(Textures[TextureID.DiamondSword].Bounds.Size, new(1, 1), "item");
        Metadata[TextureID.Dirt] = new(Textures[TextureID.Dirt].Bounds.Size, new(4, 4), "tile");
        Metadata[TextureID.Flooring] = new(Textures[TextureID.Flooring].Bounds.Size, new(4, 4), "tile");
        Metadata[TextureID.Grass] = new(Textures[TextureID.Grass].Bounds.Size, new(4, 4), "tile");
        Metadata[TextureID.Sand] = new(Textures[TextureID.Sand].Bounds.Size, new(4, 4), "tile");
        Metadata[TextureID.Sky] = new(Textures[TextureID.Sky].Bounds.Size, new(4, 1), "tile");
        Metadata[TextureID.Stairs] = new(Textures[TextureID.Stairs].Bounds.Size, new(4, 1), "tile");
        Metadata[TextureID.StoneWall] = new(Textures[TextureID.StoneWall].Bounds.Size, new(4, 4), "tile");
        Metadata[TextureID.WoodPlanks] = new(Textures[TextureID.WoodPlanks].Bounds.Size, new(4, 4), "tile");
        Metadata[TextureID.Water] = new(Textures[TextureID.Water].Bounds.Size, new(4, 4), "tile");
        Metadata[TextureID.Darkness] = new(Textures[TextureID.Darkness].Bounds.Size, new(4, 4), "tile");
        Metadata[TextureID.Door] = new(Textures[TextureID.Door].Bounds.Size, new(4, 4), "tile");
        Metadata[TextureID.Stone] = new(Textures[TextureID.Stone].Bounds.Size, new(4, 4), "tile");
        Metadata[TextureID.Torch] = new(Textures[TextureID.Torch].Bounds.Size, new(6, 1), "decal");
        Metadata[TextureID.BlueTorch] = new(Textures[TextureID.BlueTorch].Bounds.Size, new(6, 1), "decal");
        Metadata[TextureID.WaterPuddle] = new(Textures[TextureID.WaterPuddle].Bounds.Size, new(1, 1), "decal");
        Metadata[TextureID.BloodPuddle] = new(Textures[TextureID.BloodPuddle].Bounds.Size, new(1, 1), "decal");
        Metadata[TextureID.Footprint] = new(Textures[TextureID.Footprint].Bounds.Size, new(1, 1), "decal");
        Metadata[TextureID.Glow] = new(Textures[TextureID.Glow].Bounds.Size, new(1, 1), "effect");
        Metadata[TextureID.Slash] = new(Textures[TextureID.Slash].Bounds.Size, new(1, 1), "effect");
        foreach (var kv in Metadata)
            if (kv.Value == null)
                Logger.Error($"Metadata for texture '{kv.Key}' failed to load.");
            else
                Logger.System($"Metadata for texture '{kv.Key}' successfully loaded.");
        Logger.System($"Successfully loaded {Metadata.Count}/{Textures.Count} texture Metadata.");

        // Fonts
        PixelOperator = Content.Load<SpriteFont>("Fonts/PixelOperator");
        PixelOperatorBold = Content.Load<SpriteFont>("Fonts/PixelOperatorBold");
        Arial = Content.Load<SpriteFont>("Fonts/Arial");
        ArialSmall = Content.Load<SpriteFont>("Fonts/ArialSmall");
        Logger.System("Successfully loaded fonts.");
    }
    public static TextureID ParseTextureString(string textureName)
    {
        return Enum.TryParse<TextureID>(textureName, true, out var tex) ? tex : TextureID.Null;
    }
    public static Texture2D GetTexture(TextureID id)
    {
        Texture2D tex = Textures.GetValueOrDefault(id, Textures[TextureID.Null]);
        if (tex == Textures[TextureID.Null] && !errors.Contains($"getfail-{id}"))
        {
            Logger.Error($"Texture with name '{id}' not found.");
            errors.Add($"getfail-{id}");
            return Textures[TextureID.Null];
        }
        return tex;
    }
    public static void DrawTexture(SpriteBatch batch, TextureID id, Point pos, Rectangle? source = null, Color color = default, float rotation = 0f, Vector2 origin = default, float scale = 1, SpriteEffects effects = SpriteEffects.None)
    {
        Texture2D tex = GetTexture(id);
        if (color == default) color = Color.White;
        if (origin == default) origin = Vector2.Zero;
        batch.Draw(tex, pos.ToVector2(), source, color, rotation, origin, scale, effects, 0f);
    }
    public static void FillRectangle(SpriteBatch batch, Point pos, Point size, Color color)
    {
        batch.Draw(Pixel, new Rectangle(pos.X, pos.Y, size.X, size.Y), color);
    }
    public static void FillRectangle(SpriteBatch batch, Rectangle rect, Color color)
    {
        batch.Draw(Pixel, rect, color);
    }
    public static void UnloadTexture(TextureID id)
    {
        if (!Textures.Remove(id) && !errors.Contains($"unloadfail-{id}"))
        {
            Logger.Error($"Texture with name '{id}' not found.");
            errors.Add($"unloadfail-{id}");
        }
    }
    public static Rectangle GetAnimationSource(TextureID texture, float time, float duration = 1, float start = 0, int row = 0)
    {
        // Get the texture ID from the content manager
        if (!Metadata.TryGetValue(texture, out Metadata? meta))
        {
            Logger.Error($"TextureManager.Metadata for texture '{texture}' not found.");
            return new(0, 0, 64, 64);
        }

        Point frameSize = meta.Size / meta.TileMap;
        float invDuration = 1 / duration;
        int frame = (int)((time - start) * invDuration) % meta.TileMap.X;
        return new Rectangle(frame * frameSize.X, row * frameSize.Y, frameSize.X, frameSize.Y);
    }
}
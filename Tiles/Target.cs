using Quest.Editor;
using Quest.Editor.Managers;
using System.IO;
using System.Linq;

namespace Quest.Tiles;

public class Target : TriggerTile
{
    public TextureID? RequiredProjectile { get; set; }
    public Target(Point location, string levelName, TileEffect effectType, ByteCoord effectCoord, LevelPath effectLevel) : base(TileTypeID.Target, location, levelName, effectType, effectCoord, effectLevel)
    {

    }
    public override void Draw(GameManager gameManager)
    {
        // Draw tile
        Point dest = CameraManager.TileToScreen(Location);
        Rectangle source = GetAnimationSource(Type.Texture, 0, row: Activated ? 1 : 0);
        DrawTexture(gameManager.Batch, Type.Texture, dest, source: source, scale: Constants.TileSizeScale);
    }
    public override void OnProjectileCollide(GameManager gameManager, Projectile proj)
    {
        if (RequiredProjectile == null || proj.Texture == RequiredProjectile)
            Activate(gameManager);
    }
    public override void WriteState(BinaryWriter writer, GameManager gameManager)
    {
        // If a target is even written to the save, then it means it's activated
    }
    public override void ReadState(BinaryReader reader, GameManager gameManager)
    {
        // If a target is even written to the save, then it means it's activated
    }
    public override void WriteLevelData(BinaryWriter writer)
    {
        base.WriteLevelData(writer);
        writer.Write((ushort)(RequiredProjectile ?? TextureID.Null));
    }
    public override void ReadLevelData(BinaryReader reader, LevelPath levelPath)
    {
        base.ReadLevelData(reader, levelPath);
        TextureID proj = (TextureID)reader.ReadUInt16();
        if (proj == TextureID.Null)
            RequiredProjectile = null;
        else
            RequiredProjectile = proj;
    }
    public override void Edit(EditorManager editorManager)
    {
        base.Edit(editorManager);

        // Input fields
        List<InputField> fields = [
            new("Required Projectile", null, dropdownOptions: [.. ProjectileTextures.Select(t => t.ToString()), "NONE"], placeholder: EffectType),
        ];


        // Window
        var (success, values) = PopupFactory.ShowInputForm("Target Editor", fields.ToArray());
        if (!success)
        {
            if (!PopupFactory.PopupOpen) Logger.Error("Target edit failed.");
            return;
        }

        // Set
        if (values[0].ToUpper() != "NONE")
            RequiredProjectile = Enum.Parse<TextureID>(values[0]);
        else
            RequiredProjectile = null;
    }
}

using Quest.Editor;
using System.IO;
using System.Linq;
namespace Quest.Utilities;
public static class BinaryWriterExtensions
{
    public static void Write(this BinaryWriter writer, Color color)
    {
        writer.Write(color.R);
        writer.Write(color.G);
        writer.Write(color.B);
        writer.Write(color.A);
    }
    public static void Write(this BinaryWriter writer, Point point)
    {
        writer.Write(point.X);
        writer.Write(point.Y);
    }
    public static void Write(this BinaryWriter writer, ByteCoord coord)
    {
        writer.Write(coord.X);
        writer.Write(coord.Y);
    }
    public static void Write(this BinaryWriter writer, NPC npc)
    {
        writer.Write(npc.Name);
        writer.Write(npc.Dialog);
        writer.Write(new ByteCoord(npc.Location));
        writer.WriteByteFloat(npc.Scale);
        writer.Write((ushort)npc.Texture);
    }
    public static void WriteByteFloat(this BinaryWriter writer, float value)
    {
        writer.Write(LevelEditor.IntToByte((int)(value * 10)));
    }
    public static void Write(this BinaryWriter writer, Loot loot)
    {
        writer.Write(loot.Item.Name);
        writer.Write(LevelEditor.IntToByte(loot.Item.Amount));
        writer.Write((ushort)loot.Location.X);
        writer.Write((ushort)loot.Location.Y);
    }
    public static void Write(this BinaryWriter writer, Decal decal)
    {
        writer.Write((byte)decal.Type);
        writer.Write(decal.Location);
    }
    public static void Write(this BinaryWriter writer, ILootGenerator generator)
    {
        string file = generator.FileName.Split('/', '\\').Last();
        if (file.IsNUL())
            writer.Write("_");
        else
            writer.Write(file);
    }
}

public static class BinaryReaderExtensions
{
    public static Color ReadColor(this BinaryReader reader)
    {
        return new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
    }
    public static Point ReadPoint(this BinaryReader reader)
    {
        return new Point(reader.ReadInt32(), reader.ReadInt32());
    }
    public static ByteCoord ReadByteCoord(this BinaryReader reader)
    {
        return new ByteCoord(reader.ReadByte(), reader.ReadByte());
    }
    public static NPC ReadNPC(this BinaryReader reader, GameManager gameManager)
    {
        string name = reader.ReadString();
        string dialog = reader.ReadString();
        Point location = reader.ReadByteCoord().ToPoint();
        float scale = reader.ReadByteFloat();
        ushort texID = reader.ReadUInt16();
        if (!Enum.IsDefined(typeof(TextureID), texID))
        {
            Logger.Error("Failed to read NPC. Invalid texture ID.");
            texID = (ushort)TextureID.Null;
        }
        TextureID texture = (TextureID)texID;
        return new NPC(gameManager.UIManager, texture, location, name, dialog, Color.White, scale);
    }
    public static float ReadByteFloat(this BinaryReader reader)
    {
        return reader.ReadByte() / 10f;
    }
    public static Loot ReadLoot(this BinaryReader reader, GameManager gameManager)
    {
        var type = ItemTypes.All[reader.ReadByte()];
        byte amount = reader.ReadByte();
        ushort x = reader.ReadUInt16();
        ushort y = reader.ReadUInt16();
        return new Loot(new ItemRef(type, amount), new Point(x, y), gameManager.GameTime);
    }
    public static Decal ReadDecal(this BinaryReader reader)
    {
        DecalType type = (DecalType)reader.ReadByte();
        Point location = reader.ReadByteCoord().ToPoint();
        return new Decal(location, type);
    }
}


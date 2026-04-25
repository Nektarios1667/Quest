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
        writer.Write(new ByteCoord(npc.Position));
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
        writer.Write((ushort)loot.Position.X);
        writer.Write((ushort)loot.Position.Y);
    }
    public static void Write(this BinaryWriter writer, Decal decal)
    {
        writer.Write((byte)decal.Type);
        writer.Write(decal.Location);
    }
    public static void Write(this BinaryWriter writer, Enemy enemy)
    {
        writer.Write(enemy.Health);
        writer.Write(enemy.Damage);
        writer.Write(enemy.AttackSpeed);
        writer.Write(enemy.Defense);
        writer.Write(enemy.Speed);
        writer.Write(enemy.ProjectileSpeed);
        writer.Write(enemy.ViewRange);
        writer.Write(enemy.AttackRange);
        writer.Write((ushort)enemy.Texture);
        writer.Write((ushort)enemy.ProjectileTexture);
        writer.Write((ushort)Math.Round(enemy.Position.X));
        writer.Write((ushort)Math.Round(enemy.Position.Y));
        writer.Write(enemy.UID);
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
    public static NPC ReadNPC(this BinaryReader reader)
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
        return new NPC(texture, location, name, dialog, Color.White, scale);
    }
    public static Enemy ReadEnemy(this BinaryReader reader)
    {
        ushort health = reader.ReadUInt16();
        ushort damage = reader.ReadUInt16();
        float attackSpeed = reader.ReadSingle();
        ushort defense = reader.ReadUInt16();
        ushort speed = reader.ReadUInt16();
        ushort projectileSpeed = reader.ReadUInt16();
        ushort viewRange = reader.ReadUInt16();
        ushort attackRange = reader.ReadUInt16();

        ushort texID = reader.ReadUInt16();
        if (!Enum.IsDefined(typeof(TextureID), texID))
        {
            Logger.Error("Failed to read Enemy. Invalid texture ID.");
            texID = (ushort)TextureID.Null;
        }

        ushort projTexID = reader.ReadUInt16();
        if (!Enum.IsDefined(typeof(TextureID), projTexID))
        {
            Logger.Error("Failed to read Enemy. Invalid projectile texture ID.");
            projTexID = (ushort)TextureID.Null;
        }

        Vector2 position = new(reader.ReadUInt16(), reader.ReadUInt16());
        ushort uid = reader.ReadUInt16();

        Enemy enemy = new(
            position,
            health,
            damage,
            attackSpeed,
            defense,
            speed,
            projectileSpeed,
            viewRange,
            attackRange,
            (TextureID)texID,
            (TextureID)projTexID,
            uid
        );
        return enemy;
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
        return new Loot(new ItemRef(type, amount), new Point(x, y), GameManager.GameTime);
    }
    public static Decal ReadDecal(this BinaryReader reader)
    {
        DecalType type = (DecalType)reader.ReadByte();
        Point location = reader.ReadByteCoord().ToPoint();
        return Decal.CreateDecal(type, location);
    }
}


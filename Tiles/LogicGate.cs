using Quest.Editor;
using Quest.Editor.Managers;
using System.IO;

namespace Quest.Tiles;

public enum LogicGateType : byte
{
    And,
    Or,
    Not,
    Xor,
    Nand,
    Nor,
    XNor,
}
public enum InputType { A, B }
public class LogicGate : TriggerTile
{
    public LogicGateType GateType { get; set; }
    public bool InputA { get; private set; } = false;
    public bool InputB { get; private set; } = false;
    public LogicGate(Point location, string levelName, TileEffect effectType, ByteCoord effectCoord, LevelPath effectLevel, LogicGateType gate) : base(TileTypeID.LogicGate, location, levelName, effectType, effectCoord, effectLevel)
    {
        GateType = gate;
    }
    public override void Draw(GameManager gameManager)
    {
        Point dest = CameraManager.TileToScreen(Location);
        int column = (int)GateType;
        int row = (InputA ? 1 : 0) + (InputB ? 2 : 0);
        Rectangle source = GetAnimationSource(Type.Texture, column, row: row);
        DrawTexture(gameManager.Batch, Type.Texture, dest, source: source, scale: Constants.TileSizeScale);
    }
    public void SetInput(GameManager gameManager, bool input, InputType inputType)
    {
        // Set
        if (inputType == InputType.A)
            InputA = input;
        else if (inputType == InputType.B)
            InputB = input;

        // Solve
        bool act = Solve(GateType, InputA, InputB);
        if (act != Activated)
            Activate(gameManager, true);
        Activated = act;
    }
    public override void WriteState(BinaryWriter writer, GameManager gameManager)
    {
        writer.Write(InputA);
        writer.Write(InputB);
    }
    public override void ReadState(BinaryReader reader, GameManager gameManager)
    {
        SetInput(gameManager, reader.ReadBoolean(), InputType.A);
        SetInput(gameManager, reader.ReadBoolean(), InputType.B);
    }
    public override void WriteLevelData(BinaryWriter writer)
    {
        base.WriteLevelData(writer);
        writer.Write((byte)GateType);
    }
    public override void ReadLevelData(BinaryReader reader, LevelPath levelPath)
    {
        base.ReadLevelData(reader, levelPath);
        GateType = (LogicGateType)reader.ReadByte();
    }
    public override void Edit(EditorManager editorManager)
    {
        base.Edit(editorManager);

        // Window
        var (success, values) = PopupFactory.ShowInputForm("Trigger Tile Editor", [
            new("Logic Gate", null, dropdownOptions: Enum.GetNames<LogicGateType>(), placeholder: GateType),
        ]);

        if (!success)
        {
            if (!PopupFactory.PopupOpen) Logger.Error("Trigger tile edit failed.");
            return;
        }

        GateType = Enum.Parse<LogicGateType>(values[0]);

    }
    public static bool Solve(LogicGateType gate, bool a, bool? b = null)
    {
        bool bValue = b ?? false;

        return gate switch
        {
            LogicGateType.And => a && bValue,
            LogicGateType.Or => a || bValue,
            LogicGateType.Not => !a,
            LogicGateType.Xor => a ^ bValue,
            LogicGateType.Nand => !(a && bValue),
            LogicGateType.Nor => !(a || bValue),
            LogicGateType.XNor => a == bValue,
            _ => false,
        };
    }
}

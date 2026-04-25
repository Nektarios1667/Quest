namespace Quest.Entities;
public interface IEntity
{
    RectangleF Bounds { get; }
    ushort UID { get; }
}

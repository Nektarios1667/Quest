namespace Quest.Entities;
public class Entity
{
    public Vector2 Position { get; set; }
    public Entity(Vector2 position)
    {
        Position = position;
    }
    public virtual void Update(GameManager gameManager)
    {

    }
    public virtual void Draw(GameManager gameManager)
    {

    }
}

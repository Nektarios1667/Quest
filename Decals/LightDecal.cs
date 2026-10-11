namespace Quest.Decals;
public class LightDecal(Point location) : Decal(location)
{
    public override void Draw(GameManager gameManager)
    {
        if (gameManager.StateManager.State == GameState.Editor)
            base.Draw(gameManager);
        LightingManager.SetLight($"LightDecal_{Location.X}_{Location.Y}", Location.ToPoint(), 10f);
    }
}

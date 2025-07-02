using Quest.Enemies;

namespace Quest.Managers;
public class EnemyManager
{
    public List<Enemy> Enemies { get; protected set; }
    public EnemyManager()
    {
        Enemies = [];
    }
    public void Update(GameManager gameManager, Attack[] attacks)
    {
        DebugManager.StartBenchmark("UpdateEnemies");
        // Enemy updates
        foreach (Enemy enemy in Enemies)
        {
            foreach (Attack attack in attacks)
                if (enemy.Hitbox.Intersects(attack.Hitbox)) enemy.Hurt(attack.Damage);
            enemy.Update(gameManager);
        }

        // Remove dead enemies
        for (int i = Enemies.Count - 1; i >= 0; i--)
        {
            if (!Enemies[i].IsAlive)
            {
                Enemies.RemoveAt(i);
            }
        }
        DebugManager.EndBenchmark("UpdateEnemies");
    }
    public void Draw(GameManager gameManager)
    {
        DebugManager.StartBenchmark("DrawEnemies");
        foreach (Enemy enemy in Enemies) enemy.Draw(gameManager);
        DebugManager.EndBenchmark("DrawEnemies");
    }

}

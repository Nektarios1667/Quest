using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Quest.Entities;
public class Enemy
{
    public IGameManager Game { get; private set; }
    public string Name { get; private set; }
    public int Health { get; private set; }
    public int Attack { get; private set; }
    public float AttackSpeed { get; private set; }
    public float AttackCooldown { get; private set; } = 0f;
    public float Defense { get; private set; }
    public int Speed { get; private set; }
    public int ViewRange { get; private set; }
    public int AttackRange { get; private set; }
    public TextureID Texture { get; private set; }
    public Vector2 Location { get; private set; }
    public string Mode { get; private set; }
    private Point tileSize { get; set; }
    public Enemy(IGameManager game, string name, int health, Point location, int attack, float attackSpeed, float defense, int speed, int viewRange, int attackRange, TextureID texture)
    {
        Game = game;
        Name = name;
        Health = health;
        Location = location.ToVector2();
        Attack = attack;
        AttackSpeed = attackSpeed;
        Defense = defense;
        Speed = speed;
        Texture = texture;
        ViewRange = viewRange;
        AttackRange = attackRange;
        Mode = "idle";
        tileSize = TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap;
    }
    public virtual void Update()
    {
        // View range
        float playerDistSq = Vector2.DistanceSquared(Location, Game.CameraDest);
        // Attack
        if (playerDistSq < AttackRange * AttackRange && AttackCooldown <= 0)
        {
            Mode = "attack";
            Game.DamagePlayer(Attack);
            AttackCooldown = AttackSpeed;
        }
        // Move
        else if (playerDistSq < ViewRange * ViewRange && playerDistSq != 0)
        {
            Mode = "move";
            Location += Vector2.Normalize(Game.CameraDest - Location) * Speed * Game.Delta;
        }
        else
            Mode = "idle";
        
        // Final
        if (AttackCooldown > 0) AttackCooldown -= Game.Delta;
    }
    public virtual void Draw()
    {
        Rectangle source = GetAnimationSource(Texture, Game.Time, duration: 0.5f);
        DrawTexture(Game.Batch, Texture, Location.ToPoint() - Game.Camera.ToPoint() + Constants.Middle, source: source);
    }
    public virtual void Damage(int damage)
    {
        if (damage <= Defense) Health -= (int)(damage * (1f - Defense / (Defense + 500)));
        else Health -= damage;
    }
}

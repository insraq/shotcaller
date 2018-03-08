using Godot;
using System;
using System.Linq;

public class Char : Area2D
{
    [Export]
    private Texture texture;
    [Export]
    private int hp;
    [Export]
    public Direction direction;
    [Export]
    private int attackRange;
    [Export]
    public int moveSpeed;
    [Export]
    private CharType charType;
    [Export]
    private int damage;
    [Export]
    private int attackPerSecond;

    private Sprite sprite;
    private Label label;
    private CharStatus charStatus;
    private Vector2 initialPosition;
    private int initialHp;
    private float timeSinceLastAttack;

    public override void _Ready()
    {
        sprite = (Sprite)GetNode("./Sprite");
        initialPosition = GetGlobalPosition();
        initialHp = hp;

        if (texture != null)
        {
            sprite.SetTexture(texture);
        }
        if (attackRange == 0)
        {
            GD.Print($"Please set attackRange for {GetName()}");
        }
        if (moveSpeed == 0)
        {
            GD.Print($"Please set moveSpeed for {GetName()}");
        }
        if (damage == 0)
        {
            GD.Print($"Please set moveSpeed for {GetName()}");
        }
        if (attackPerSecond == 0)
        {
            GD.Print($"Please set moveSpeed for {GetName()}");
        }
    }

    public override void _Process(float delta)
    {
        UpdateLabel();

        if (hp <= 0)
        {
            if (charType == CharType.Hero)
            {
                // Respawn
                hp = initialHp;
                SetGlobalPosition(initialPosition);
                return;
            }
            if (charType == CharType.Creep)
            {
                // Hide and kill
                SetGlobalPosition(new Vector2(-1000, -1000));
                QueueFree();
            }

        }

        timeSinceLastAttack += delta;

        var charsInRange = GetOverlappingAreas();

        var sorted = charsInRange
            .OfType<Char>()
            .Where(c => c.direction != direction)
            .OrderBy(c => c.charType == CharType.Hero ? 0 : 1)
            .ToList();

        if (sorted.Count > 0)
        {
            var enemy = sorted[0];
            var dist = enemy.GetGlobalPosition().DistanceTo(GetGlobalPosition());
            if (dist <= attackRange)
            {
                if (timeSinceLastAttack <= (1 / (float)attackPerSecond))
                {
                    return;
                }
                enemy.hp -= damage;
                timeSinceLastAttack = 0;
                charStatus = enemy.charType == CharType.Hero ? CharStatus.AttackHero : CharStatus.AttackCreep;
                return;
            }
        }

        int dir = direction == Direction.Left ? -1 : 1;
        SetGlobalPosition(GetGlobalPosition() + new Vector2(dir * moveSpeed * delta, 0));
        charStatus = CharStatus.Move;
    }

    private void UpdateLabel()
    {
        label = (Label)GetNode("./HP");
        label.SetText($"HP: {hp}\nSpd/Rng: {moveSpeed}/{attackRange}\nSts: {charStatus}");
    }

    public enum Direction
    {
        Left, Right
    }

    public enum CharStatus
    {
        AttackHero, AttackBuilding, AttackCreep, Move
    }

    public enum CharType
    {
        Hero, Creep
    }
}

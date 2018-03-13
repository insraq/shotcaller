using Godot;
using System.Linq;

public class Char : Area2D
{
    [Export]
    public Texture texture;
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
    private ProgressBar progressBar;
    private Label label;
    private CharStatus charStatus;
    private Vector2 initialPosition;
    private int initialHp;
    private float timeSinceLastAttack;
    private float timeSinceLastDeath;
    private Char attackTarget;

    public override void _Ready()
    {
        sprite = (Sprite)GetNode("./Sprite");
        progressBar = (ProgressBar)GetNode("./HP");
        initialPosition = GetGlobalPosition();
        timeSinceLastDeath = 5;
        initialHp = hp;

        if (texture != null)
        {
            sprite.SetTexture(texture);
        }
        if (attackRange == 0)
        {
            GD.Print($"Please set attackRange for {GetName()}");
            ((CircleShape2D)((CollisionShape2D)GetNode("./AttackRange")).Shape).Radius = attackRange;
        }
        if (moveSpeed == 0 && charType != CharType.Tower)
        {
            GD.Print($"Please set moveSpeed for {GetName()}");
        }
        if (damage == 0)
        {
            GD.Print($"Please set damage for {GetName()}");
        }
        if (attackPerSecond == 0)
        {
            GD.Print($"Please set attackPerSecond for {GetName()}");
        }
    }

    private void OnAreaEntered(Object area)
    {
        LookForAndSetPotentialAttackTarget();
    }

    private void OnAreaExited(Object area)
    {
        var character = (Char)area;
        LookForAndSetPotentialAttackTarget();
    }


    private void LookForAndSetPotentialAttackTarget()
    {
        var charsInRange = GetOverlappingAreas();

        var sorted = charsInRange
            .OfType<Char>()
            .Where(c => c.direction != direction && c.hp > 0) // We need hp > 0 because GetOverlappingAreas will include the exiting area
            .OrderBy(c => c.charType.AttackOrder())
            .ThenBy(c => c.hp)
            .ToList();

        if (sorted.Count > 0)
        {
            attackTarget = sorted[0];
        }
        else
        {
            attackTarget = null;
        }
    }

    public override void _Process(float delta)
    {
        timeSinceLastAttack += delta;
        timeSinceLastDeath += delta;
        UpdateView();

        if (hp <= 0)
        {
            if (charType == CharType.Hero)
            {
                // Respawn
                timeSinceLastDeath = 0;
                hp = initialHp;
                SetGlobalPosition(initialPosition);
                return;
            }
            else // For Creep and Tower
            {
                // Hide and kill
                SetGlobalPosition(new Vector2(-1000, -1000));
                QueueFree();
                return;
            }

        }

        if (timeSinceLastDeath < 5)
        {
            Modulate = new Color(1, 1, 1, 0.2f + 0.8f * timeSinceLastDeath / 5);
            return;
        }
        else
        {
            Modulate = new Color(1, 1, 1, 1);
        }

        if (attackTarget != null)
        {
            var dist = attackTarget.GetGlobalPosition().DistanceTo(GetGlobalPosition());
            if (dist <= attackRange)
            {
                if (timeSinceLastAttack <= (1 / (float)attackPerSecond))
                {
                    return;
                }
                attackTarget.hp -= damage;
                timeSinceLastAttack = 0;
                charStatus = attackTarget.charType.ToCharStatus();
                // When attack, skip moving
                return;
            }
        }

        if (charType != CharType.Tower)
        {
            Move(delta);
        }
    }

    private void Move(float delta)
    {
        int dir = direction == Direction.Left ? -1 : 1;
        SetGlobalPosition(GetGlobalPosition() + new Vector2(dir * moveSpeed * delta, 0));
        charStatus = CharStatus.Move;
    }

    private void UpdateView()
    {
        progressBar.SetValue((float)100 * hp / initialHp);
    }

    public enum Direction
    {
        Left, Right
    }

    public enum CharStatus
    {
        AttackHero, AttackTower, AttackCreep, Move
    }

    public enum CharType
    {
        Hero, Creep, Tower
    }
}

public static class Extensions
{
    public static Char.CharStatus ToCharStatus(this Char.CharType charType)
    {
        switch (charType)
        {
            case Char.CharType.Hero:
                return Char.CharStatus.AttackHero;
            case Char.CharType.Creep:
                return Char.CharStatus.AttackCreep;
            case Char.CharType.Tower:
                return Char.CharStatus.AttackTower;
            default:
                return Char.CharStatus.Move;
        }
    }

    // Smaller means higher priority
    public static int AttackOrder(this Char.CharType charType)
    {
        switch (charType)
        {
            case Char.CharType.Hero:
                return 0;
            case Char.CharType.Creep:
                return 1;
            case Char.CharType.Tower:
                return 2;
            default:
                return 999;
        }
    }
}
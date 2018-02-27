using Godot;
using System;

public class Char : Area2D
{
    [Export]
    private Texture texture;
    [Export]
    private int hp;
    [Export]
    private Direction direction;
    [Export]
    private int attackRange;
    [Export]
    private int moveSpeed;
    [Export]
    private CharType charType;

    private Sprite sprite;
    private Label label;
    private CharStatus charStatus;

    public override void _Ready()
    {
        sprite = (Sprite)GetNode("./Sprite");
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
    }

    public override void _Process(float delta)
    {
        UpdateLabel();

        var charsInRange = GetOverlappingAreas();
        foreach (var c in charsInRange)
        {
            if (c is Char @char)
            {
                var dist = @char.GetPosition().DistanceTo(GetPosition());
                if (dist <= attackRange)
                {
                    // TODO: Implement the correct attack order
                    if (@char.charType == CharType.Hero)
                    {
                        charStatus = CharStatus.AttackHero;
                        return;
                    }
                }
            }
        }

        int dir = direction == Direction.Left ? -1 : 1;
        SetPosition(GetPosition() + new Vector2(dir * moveSpeed * delta, 0));
        charStatus = CharStatus.Move;
    }

    private void UpdateLabel()
    {
        label = (Label)GetNode("./HP");
        label.SetText($"HP: {hp}\nSpd/Rng: {moveSpeed}/{attackRange}\nSts: {charStatus}");
    }

    enum Direction
    {
        Left, Right
    }

    enum CharStatus
    {
        AttackHero, AttackBuilding, AttackCreep, Move
    }

    enum CharType
    {
        Hero, Creep
    }
}

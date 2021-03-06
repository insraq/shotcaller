﻿using Godot;
using System.Collections.Generic;
using System.Linq;

public class Char : Area2D
{
    [Export] public Texture texture;
    [Export] private int hp;
    [Export] public Direction direction;
    [Export] private int attackRange;
    [Export] public int moveSpeed;
    [Export] private CharType charType;
    [Export] private int damage;
    [Export] private int attackPerSecond;
    [Node("./Sprite")] private Sprite sprite;
    [Node("./Selected")] private Sprite selectedSprite;
    [Node("./HP")] private readonly HPBar progressBar;
    [Node("./AttackRange")] private readonly CollisionShape2D attackRangeCollider;
    [Node("./Level/LevelLabelShadow")] private readonly Label levelLabelShadow;
    [Node("./Level/LevelLabel")] private readonly Label levelLabel;
    public bool selected;
    private Label label;
    private CharStatus charStatus;
    private Vector2 initialPosition;
    private int fullHp;
    private float timeSinceLastAttack;
    private float timeSinceLastDeath = 5;
    private Char attackTarget;
    private HashSet<Char> targets;
    private int level;
    private const int BoundingBoxIndex = 1;
    private const int RespawnTime = 5;
    private readonly PackedScene damageAnimation = (PackedScene)GD.Load("res://Damage.tscn");
    private int accumulatedDamage = 0;

    public int Hp
    {
        get => hp;
        set
        {
            if (value < hp)
            {
                Damage dmg = (Damage)damageAnimation.Instance();
                dmg.SetPosition(new Vector2(0, -50));
                AddChild(dmg);
                dmg.Play((value - hp).ToString());
            }
            hp = value;
        }
    }

    public override void _Ready()
    {
        this.WireNodes();
        initialPosition = GetGlobalPosition();
        fullHp = Hp;
        targets = new HashSet<Char>();

        if (charType == CharType.Hero)
        {
            SetLevel(1);
        }
        else
        {
            levelLabel.SetVisible(false);
            levelLabelShadow.SetVisible(false);
        }

        if (texture != null)
        {
            sprite.SetTexture(texture);
            selectedSprite.SetTexture(texture);
        }
        if (attackRange == 0)
        {
            GD.Print($"Please set attackRange for {GetName()}");
            ((CircleShape2D)attackRangeCollider.Shape).Radius = attackRange;
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

    private void SetLevel(int l)
    {
        level = l;
        levelLabel.SetText($"LVL{l}");
        levelLabelShadow.SetText($"LVL{l}");
    }

    private void OnAreaEntered(Object area)
    {
        if (area is Char c)
        {
            targets.Add(c);
            LookForAndSetPotentialAttackTarget();
        }
    }

    private void OnAreaExited(Object area)
    {
        if (area is Char c)
        {
            targets.Remove(c);
            LookForAndSetPotentialAttackTarget();
        }
    }

    private void LookForAndSetPotentialAttackTarget()
    {
        var sorted = targets
            .OfType<Char>()
            .Where(c => c.direction != direction && c.Hp > 0)
            .OrderBy(c => c.GetGlobalPosition().DistanceTo(GetGlobalPosition()))
            .ThenBy(c => c.Hp)
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

    private void OnInputEvent(Object viewport, Object e, int shapeIdx)
    {
        if (e is InputEvent ev && ev.IsActionPressed("select") && shapeIdx == BoundingBoxIndex)
        {
            switch (charType)
            {
                case CharType.Hero:
                    if (timeSinceLastDeath < RespawnTime) { return; }
                    if (selected) { Unselect(); }
                    else { Select(); }
                    return;
                case CharType.Creep:
                    return;
                case CharType.Tower:
                    var selections = GetTree()
                        .GetNodesInGroup("HeroSelection")
                        .OfType<Char>()
                        .Where(c => c.selected);
                    if (selections.Count() == 1)
                    {
                        Char currentSelected = selections.First();
                        if (currentSelected.direction == direction)
                        {
                            currentSelected.timeSinceLastDeath = 0;
                            currentSelected.Hp = currentSelected.fullHp;
                            currentSelected.Unselect();
                            currentSelected.SetGlobalPosition(GetGlobalPosition());
                        }
                    }
                    return;
                default:
                    return;
            }

        }
    }

    private void Unselect()
    {
        selectedSprite.Visible = false;
        selected = false;
    }

    private void Select()
    {
        GetTree().CallGroupFlags((int)SceneTree.GroupCallFlags.Realtime, "HeroSelection", "Unselect");
        selectedSprite.Visible = true;
        selected = true;
    }

    public override void _Process(float delta)
    {
        timeSinceLastAttack += delta;
        timeSinceLastDeath += delta;
        UpdateView();

        if (Hp <= 0)
        {
            if (charType == CharType.Hero)
            {
                // Respawn
                timeSinceLastDeath = 0;
                Hp = fullHp;
                Unselect();
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

        if (timeSinceLastDeath < RespawnTime)
        {
            Modulate = new Color(1, 1, 1, 0.2f + 0.8f * timeSinceLastDeath / RespawnTime);
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
                int randomizedDamage = (int)new System.Random().FloatRange(0.8f * damage, 1.2f * damage);
                attackTarget.Hp -= randomizedDamage;
                accumulatedDamage += randomizedDamage;
                if (accumulatedDamage / fullHp > level)
                {
                    SetLevel(accumulatedDamage / fullHp);
                    fullHp = (int)(fullHp * 1.1f);
                    hp = (int)(hp * 1.1f);
                    damage = (int)(damage * 1.1f);
                }
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
        var percent = 100f * Hp / fullHp;
        progressBar.SetProgress(percent);
    }

}
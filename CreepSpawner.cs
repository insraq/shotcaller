using Godot;
using System;

public class CreepSpawner : Node2D
{
    [Export]
    private Direction direction;
    [Export]
    private Texture texture;
    private PackedScene creep = (PackedScene)GD.Load("res://Creep.tscn");

    public override void _Ready()
    {
    }

    private void SpawnCreep()
    {
        AddChild(CreateCreep());
    }

    private Char CreateCreep()
    {
        var newCreep = (Char)creep.Instance();
        newCreep.direction = direction;
        newCreep.texture = texture;
        return newCreep;
    }
}
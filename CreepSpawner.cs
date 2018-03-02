using Godot;
using System;

public class CreepSpawner : Node2D
{
    [Export]
    private Char.Direction direction;
    private PackedScene creep;

    public override void _Ready()
    {
        creep = (PackedScene)GD.Load("res://Creep.tscn");
    }

    private void SpawnCreep()
    {
        var newCreep = (Char)creep.Instance();
        newCreep.direction = direction;
        AddChild(newCreep);
    }
}
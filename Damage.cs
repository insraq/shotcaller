using Godot;
using System;

public class Damage : Node2D
{
    [Node("./DamageAnimation")] private AnimationPlayer damageAnimation;
    [Node("./DamageLabel")] private Label damageAnimationLabel;

    public override void _Ready()
    {
        this.WireNodes();
    }

    public void Play(String text)
    {
        damageAnimationLabel.Text = text;
        damageAnimation.Play("Damage");
    }
}
using Godot;
using System;

public class HPBar : TextureProgress
{

    [Export] private Texture red;
    [Export] private Texture amber;
    [Export] private Texture green;
    public void SetProgress(float value)
    {
        if (value > 66.6f)
        {
            SetColor(green);
        }
        else if (value > 33.3f)
        {
            SetColor(amber);
        }
        else
        {
            SetColor(red);
        }
        SetValue(value);
    }

    private void SetColor(Texture color)
    {
        if (TextureProgress_ == color)
        {
            return;
        }
        TextureProgress_ = color;
    }

}

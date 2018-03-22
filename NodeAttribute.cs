using Godot;
using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class GetNodeAttribute : Attribute
{
    public string nodePath;

    public GetNodeAttribute(string np)
    {
        nodePath = np;
    }
}

using Godot;
using System;
using System.Reflection;

public static class Extensions
{
    public static CharStatus ToCharStatus(this CharType charType)
    {
        switch (charType)
        {
            case CharType.Hero:
                return CharStatus.AttackHero;
            case CharType.Creep:
                return CharStatus.AttackCreep;
            case CharType.Tower:
                return CharStatus.AttackTower;
            default:
                return CharStatus.Move;
        }
    }

    // Smaller means higher priority
    public static int AttackOrder(this CharType charType)
    {
        switch (charType)
        {
            case CharType.Hero:
                return 0;
            case CharType.Creep:
                return 1;
            case CharType.Tower:
                return 2;
            default:
                return 999;
        }
    }

    public static void WireNodes(this Node node)
    {
        FieldInfo[] info = node
            .GetType()
            .GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var f in info)
        {
            NodeAttribute attr = (NodeAttribute)Attribute.GetCustomAttribute(f, typeof(NodeAttribute));
            if (attr != null)
            {
                Node nodeInstnace = node.GetNode(attr.nodePath);
                if (nodeInstnace == null)
                {
                    throw new NullReferenceException($"Cannot find Node for NodePath '{attr.nodePath}'");
                }
                if (f.FieldType != nodeInstnace.GetType())
                {
                    throw new InvalidCastException($"For NodePath '{attr.nodePath}', expect Type '{f.FieldType}', but get '{nodeInstnace.GetType()}'");
                }
                f.SetValue(node, nodeInstnace);
            }
        }
    }
}
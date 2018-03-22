using Godot;

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
        System.Reflection.FieldInfo[] info = node.GetType().GetFields(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        foreach (var f in info)
        {
            NodeAttribute attr = (NodeAttribute)System.Attribute.GetCustomAttribute(f, typeof(NodeAttribute));
            if (attr != null)
            {
                f.SetValue(node, node.GetNode(attr.nodePath));
            }
        }
    }
}
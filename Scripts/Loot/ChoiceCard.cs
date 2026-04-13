using Godot;
using System;
using Godot.Collections;
using System.ComponentModel;

public partial class ChoiceCard : MarginContainer
{
    [Export]
    public NodePath buttonPath;
    [Export]
    public NodePath containerPath;
    public void writeCardInfo(Dictionary<string, string> cardInfo){
        Node container = GetNode<Node>(containerPath);
        foreach (var child in container.GetChildren())
        {
            if (child is Label label)
            {
                if (cardInfo.ContainsKey(label.Name))
                {
                    label.Text = cardInfo[label.Name];
                }
                if(label.Name == "Rarity")
                {
                    switch (cardInfo[label.Name])
                    {
                        case "Common":
                            label.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
                            break;
                        case "Uncommon":
                            label.AddThemeColorOverride("font_color", new Color(0.5f, 1f, 0.5f));
                            break;
                        case "Rare":
                            label.AddThemeColorOverride("font_color", new Color(0.4f, 0.6f, 1f));
                            break;
                        case "Epic":
                            label.AddThemeColorOverride("font_color", new Color(0.6f, 0.2f, 1f));
                            break;
                        case "Legendary":
                            label.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.2f));
                            break;
                    }
                }
            }
        }
    }
    public Button GetButton()
    {
        return GetNode<Button>(buttonPath);
    }
}

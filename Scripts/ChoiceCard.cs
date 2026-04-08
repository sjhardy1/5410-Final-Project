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
            }
        }
    }
    public Button GetButton()
    {
        return GetNode<Button>(buttonPath);
    }
}

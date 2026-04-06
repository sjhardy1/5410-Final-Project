using Godot;
using System;
using Godot.Collections;

public partial class ChoiceCard : MarginContainer
{
    public void writeCardInfo(Dictionary<string, string> cardInfo){
        Node container = GetNode<Node>("MarginContainer/Button/VBoxContainer");
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
    public override void _Ready()
	{
		Button button = GetNode<Button>("MarginContainer/Button");
		button.Pressed += () => {
			OnCardClicked?.Invoke();
		};
	}
	public Action OnCardClicked;
}

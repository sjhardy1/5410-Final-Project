using Godot;
using System;
using System.Collections.Generic;


public partial class UpkeepReport : CanvasLayer
{
    [Export] public NodePath buttonPath;
    [Export] public NodePath newFoodPath;
    [Export] public NodePath newWoodPath;
    private VBoxContainer contentBox;
    public void Initialize(Queue<string> foodChangeSources, Queue<int> foodChanges, Queue<string> woodChangeSources, Queue<int> woodChanges, int newFood, int newWood, bool insufficientWood, bool starvation)
    {
        contentBox = GetNode<VBoxContainer>("ContentBox");
        foreach(Node child in contentBox.GetChildren())
        {
            if (child is HBoxContainer && child.Name != "Totals")
            {
                child.QueueFree();
            }
        }
        GetNode<Button>(buttonPath).Pressed += () => QueueFree();
        int totalFoodChange = 0;
        while(foodChangeSources.Count > 0)
        {
            string source = foodChangeSources.Dequeue();
            int foodChange = foodChanges.Dequeue();
            totalFoodChange += foodChange;
            AddEntry(source, foodChange, "Food");
        }
        AddBreak();
        AddEntry("Total Food Upkeep", totalFoodChange, "Food");
        AddBreak();
        int totalWoodChange = 0;
        while(woodChangeSources.Count > 0){
            string source = woodChangeSources.Dequeue();
            int woodChange = woodChanges.Dequeue();
            totalWoodChange += woodChange;
            AddEntry(source, woodChange, "Wood");
        }
        AddBreak();
        AddEntry("Total Wood Upkeep", totalWoodChange, "Wood");
        if(insufficientWood)
        {
            AddWarningLabel("! Residents don't have enough wood to maintain their buildings. Wood upkeep efficiency greatly decreased.");
        }
        if(starvation)
        {
            AddWarningLabel("! Residents are starving and cannot work or fight effectively.");
        }
        GetNode<Label>(newFoodPath).Text = $"Food: {newFood}";
        GetNode<Label>(newWoodPath).Text = $"Wood: {newWood}";
    }
    public void AddEntry(string source, int number, string resourceType)
    {
        HBoxContainer entry = new HBoxContainer();
        Label sourceLabel = new Label();
        sourceLabel.Text = source;
        Label numberLabel = new Label();
        if(number >=0)
        {
            numberLabel.AddThemeColorOverride("font_color", new Color(0.2f, 0.8f, 0.2f));
            numberLabel.Text = "+" + number.ToString();
        } else if(number < 0)
        {
            numberLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.2f, 0.2f));
            numberLabel.Text = number.ToString();
        }
        numberLabel.HorizontalAlignment = HorizontalAlignment.Right;
        numberLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        Label typeLabel = new Label();
        typeLabel.Text = resourceType;
        entry.AddChild(sourceLabel);
        entry.AddChild(numberLabel);
        entry.AddChild(typeLabel);
        contentBox.AddChild(entry);
        contentBox.MoveChild(entry, contentBox.GetChildCount() - 4);
    }
    public void AddWarningLabel(string message)
    {
        Label warningLabel = new Label();
        warningLabel.Text = message;
        warningLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        warningLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.2f, 0.2f));
        warningLabel.AddThemeFontSizeOverride("font_size", 12);
        contentBox.AddChild(warningLabel);
        contentBox.MoveChild(warningLabel, contentBox.GetChildCount() - 4);
    }
    public void AddBreak()
    {
        HSeparator separator = new HSeparator();
        contentBox.AddChild(separator);
        contentBox.MoveChild(separator, contentBox.GetChildCount() - 4);
    }
}

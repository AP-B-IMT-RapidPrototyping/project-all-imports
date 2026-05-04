using Godot;
using System;
using System.Collections.Generic;

public partial class InventoryManager : Node
{
    public static InventoryManager Instance { get; private set; }
    
    private List<InventorySlot> items = new List<InventorySlot>();
    
    [Signal]
    public delegate void InventoryChangedEventHandler();
    
    public class InventorySlot
    {
        public string ItemName { get; set; }
        public int Value { get; set; }
        public bool IsImportant { get; set; }
        public int Count { get; set; }
    }
    
    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
        }
    }
    
    public void AddItem(StealableObject item)
    {
        // Check of we dit item al hebben
        InventorySlot existing = items.Find(s => s.ItemName == item.ItemName);
        
        if (existing != null)
        {
            existing.Count++;
        }
        else
        {
            items.Add(new InventorySlot
            {
                ItemName = item.ItemName,
                Value = item.Value,
                IsImportant = item.IsImportant,
                Count = 1
            });
        }
        
        EmitSignal(SignalName.InventoryChanged);
        GD.Print($"Inventory: {item.ItemName} toegevoegd. Totaal items: {GetTotalItems()}");
    }
    
    public int GetTotalItems()
    {
        int total = 0;
        foreach (var slot in items) total += slot.Count;
        return total;
    }
    
    public int GetTotalValue()
    {
        int total = 0;
        foreach (var slot in items) total += slot.Value * slot.Count;
        return total;
    }
    
    public List<InventorySlot> GetAllItems()
    {
        return items;
    }
    
    public bool HasItem(string itemName)
    {
        return items.Exists(s => s.ItemName == itemName);
    }
}
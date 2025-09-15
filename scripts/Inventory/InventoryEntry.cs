// container class
public class InventoryEntry {
	// inventory has a type <baseitem -> quan int>
	public string Key { get; }
	public BaseItem Item { get; private set; }
	public int Quantity { get; set; }
	
	public InventoryEntry(BaseItem item, int quantity, string key) {
		Item = item;
		Quantity = quantity;
		Key = key;
	}
}

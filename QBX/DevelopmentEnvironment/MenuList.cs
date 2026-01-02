using System.Collections;

namespace QBX.DevelopmentEnvironment;

public class MenuList<TItem>(string label) : MenuItem(label), IList<TItem>, IEnumerable<TItem>
	where TItem : MenuItem
{
	public List<TItem> Items = new List<TItem>();

	public Dictionary<string, TItem> ItemByAccelerator = new Dictionary<string, TItem>(StringComparer.OrdinalIgnoreCase);

	public int Count => Items.Count;
	public bool IsReadOnly => false;

	public TItem this[int index]
	{
		get => Items[index];
		set => Items[index] = value;
	}

	public IEnumerator<TItem> GetEnumerator() => Items.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
	public void Add(TItem menuItem) => Items.Add(menuItem);
	public int IndexOf(TItem item) => Items.IndexOf(item);

	public void EnsureAcceleratorLookUp()
	{
		if (ItemByAccelerator.Count == 0)
		{
			foreach (var item in Items)
			{
				string label = item.Label;

				int accelIndex = label.IndexOf('&');

				while ((accelIndex >= 0) && (accelIndex + 1 < label.Length) && (label[accelIndex + 1] == '&'))
					accelIndex = label.IndexOf('&', accelIndex + 2);

				if ((accelIndex >= 0) && (accelIndex + 1 < label.Length))
				{
					string accel = label.Substring(accelIndex + 1, 1);

					ItemByAccelerator[accel] = item;
				}
			}
		}
	}

	void IList<TItem>.Insert(int index, TItem item) => Items.Insert(index, item);
	void IList<TItem>.RemoveAt(int index) => Items.RemoveAt(index);
	void ICollection<TItem>.Clear() => Items.Clear();
	bool ICollection<TItem>.Contains(TItem item) => Items.Contains(item);
	void ICollection<TItem>.CopyTo(TItem[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);
	bool ICollection<TItem>.Remove(TItem item) => Items.Remove(item);
}

using System.Collections;
using System.Runtime.CompilerServices;

namespace QBX.Parser;

public class ListRange<T> : IEnumerable<T>, IList<T>
{
	IList<T> _list;
	int _offset;
	int _count;

	public ListRange(IList<T> list)
		: this(list, 0..)
	{
	}

	public ListRange(IList<T> list, Range range)
	{
		_list = list;
		(_offset, _count) = range.GetOffsetAndLength(_list.Count);
	}

	public ListRange(ListRange<T> listRange, Range range)
	{
		_list = listRange._list;
		(_offset, _count) = range.GetOffsetAndLength(listRange.Count);
		_offset += listRange._offset;
	}

	public ListRange(IList<T> list, int offset, int count)
	{
		_list = list;
		_offset = offset;
		_count = Math.Min(count, list.Count - offset);
	}

	public ListRange(ListRange<T> listRange, int offset, int count)
	{
		_list = listRange._list;
		_offset = offset + listRange._offset;
		_count = Math.Min(count, listRange.Count - offset);
	}

	public static implicit operator ListRange<T>(List<T> list) => new ListRange<T>(list);

	public ListRange<T> Slice(Range range) => new ListRange<T>(this, range);

	public ListRange<T> Slice(int offset, int count) => new ListRange<T>(this, offset, count);

	public IEnumerator<T> GetEnumerator()
	{
		return _list.Skip(_offset).Take(_count).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public int Count => _count;

	public bool IsReadOnly => false;

	public T this[int index]
	{
		get
		{
			if (index >= _count)
				throw new ArgumentOutOfRangeException(nameof(index));

			return _list[index + _offset];
		}
		set
		{
			if (index >= _count)
				throw new ArgumentOutOfRangeException(nameof(index));

			_list[index + _offset] = value;
		}
	}

	public int IndexOf(T item)
	{
		if (_list is List<T> concreteList)
			return concreteList.IndexOf(item, _offset, _count) - _offset;
		else
		{
			for (int i = 0; i < _count; i++)
			{
				var element = this[i];

				if (element?.Equals(item) ?? false)
					return i;
			}

			return -1;
		}
	}

	public void Insert(int index, T item)
	{
		if (index > _count)
			throw new ArgumentOutOfRangeException(nameof(index));

		_list.Insert(index + _offset, item);
		_count++;
	}

	public void RemoveAt(int index)
	{
		if (index >= _count)
			throw new ArgumentOutOfRangeException(nameof(index));

		_list.RemoveAt(index + _offset);
		_count--;
	}

	public void Add(T item)
	{
		Insert(_count, item);
	}

	public void Clear()
	{
		if (_list is List<T> concreteList)
		{
			concreteList.RemoveRange(_offset, _count);
			_count = 0;
		}
		else
		{
			while (_count > 0)
			{
				_list.RemoveAt(_offset);
				_count--;
			}
		}
	}

	public bool Contains(T item)
	{
		return IndexOf(item) >= 0;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		for (int i = 0; i < _count; i++)
			array[i] = this[i];
	}

	public bool Remove(T item)
	{
		int index = IndexOf(item);

		if (index >= 0)
		{
			RemoveAt(index);
			return true;
		}

		return false;
	}
}

public static class ListExtensionsForListRange
{
	public static ListRange<T> Slice<T>(this IList<T> list, int start) => new ListRange<T>(list, start, list.Count - start);
	public static ListRange<T> Slice<T>(this IList<T> list, int start, int count) => new ListRange<T>(list, start, count);
}

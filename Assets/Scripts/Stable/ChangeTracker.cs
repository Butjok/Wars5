using System;
using System.Collections.Generic;

public struct ChangeTracker<T> {

	public Action<T> onChange;
	public bool wasSet;
	private T value;

	public ChangeTracker(Action<T> onChange) {
		this.onChange = onChange;
		value = default;
		wasSet = false;
	}

	public T v {
		get => value;
		set {
			if (wasSet && EqualityComparer<T>.Default.Equals(value, this.value))
				return;
			wasSet = true;
			var old = this.value;
			this.value = value;
			onChange(old);
		}
	}

	public override string ToString() {
		return value.ToString();
	}
}

public class ListChangeTracker<T> : List<T> {

	public Action<int, T> onAdd, onRemove, onChange;

	public ListChangeTracker(Action<int, T> onAdd=null, Action<int, T> onRemove=null, Action<int, T> onChange=null) {
		this.onAdd = onAdd;
		this.onRemove = onRemove;
		this.onChange = onChange;
	}
	public new void Add(T value) {
		base.Add(value);
		onAdd?.Invoke(Count - 1, value);
	}
	public new void RemoveAt(int index) {
		var oldValue = base[index];
		base.RemoveAt(index);
		onRemove?.Invoke(index, oldValue);
	}
	public new T this[int index] {
		get => base[index];
		set {
			var oldValue = base[index];
			base[index] = value;
			onChange?.Invoke(index, oldValue);
		}
	}
}
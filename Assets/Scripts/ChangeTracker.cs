using System;
using System.Collections.Generic;

public struct ChangeTracker<T> {
	
	public Action<T> onChange;
	public ChangeTracker(Action<T> onChange) {
		this.onChange = onChange;
		value = default;
		wasSet = false;
	}

    public bool wasSet;
	private T value;
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
}
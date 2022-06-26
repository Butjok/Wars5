using System;
using System.Collections.Generic;

public struct ChangeTracker<T> {
	
	public Action<T> onChange;
	public ChangeTracker(Action<T> onChange) {
		this.onChange = onChange;
		value = default;
	}

	private T value;
	public T v {
		get => value;
		set {
			if (EqualityComparer<T>.Default.Equals(value, this.value))
				return;
			var old = this.value;
			this.value = value;
			onChange(old);
		}
	}
}
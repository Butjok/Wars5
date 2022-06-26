using System;

public struct Lazy<T> {

	public Func<T> evaluate;
	public bool Evaluated { get; private set; }

	private T value;
	public T v {
		get {
			if (!Evaluated) {
				value = evaluate();
				Evaluated = true;
			}
			return value;
		}
	}

	public Lazy(Func<T> evaluate) {
		this.evaluate = evaluate;
		Evaluated = false;
		value = default;
	}
}
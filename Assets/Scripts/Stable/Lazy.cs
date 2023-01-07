using System;
using UnityEngine;

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

public static class Lazy {
	public static Lazy<T> Resource<T>(string name) where T : UnityEngine.Object {
		return new Lazy<T>(() => Resources.Load<T>(name));
	}
}
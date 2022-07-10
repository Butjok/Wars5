public abstract class State : IState {

	public bool Started { get; set; }
	public bool Disposed { get; set; }

	public virtual void Start() { }
	public virtual void Update() { }
	public virtual void DrawGUI() { }
	public virtual void Dispose() { }
	public virtual void DrawGizmos() { }
	public virtual void DrawGizmosSelected() { }
	public virtual void OnBeforePush() { }
	public virtual void OnAfterPop() { }
}
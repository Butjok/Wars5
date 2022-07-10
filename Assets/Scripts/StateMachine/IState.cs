using System;

public interface IState : IDisposable {
	bool Started { get; set; }
	bool Disposed { get; set; }
	void Start();
	void Update();
	void DrawGUI();
	void DrawGizmos();
	void DrawGizmosSelected();
	void OnBeforePush();
	void OnAfterPop();
}
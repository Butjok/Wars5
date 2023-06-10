using UnityEngine;

public class CursorInteractor : MonoBehaviour {

    public enum Command { MouseDown, MouseDrag, MouseUp, MouseEnter, MouseExit, MouseOver, MouseUpAsButton }

    private void OnMouseDown() {
        Game.Instance.EnqueueCommand(Command.MouseDown, this);
    }
    private void OnMouseDrag() {
        Game.Instance.EnqueueCommand(Command.MouseDrag, this);
    }
    private void OnMouseUp() {
        Game.Instance.EnqueueCommand(Command.MouseUp, this);
    }
    private void OnMouseEnter() {
        Game.Instance.EnqueueCommand(Command.MouseEnter, this);
    }
    private void OnMouseExit() {
        Game.Instance.EnqueueCommand(Command.MouseExit, this);
    }
    private void OnMouseOver() {
        Game.Instance.EnqueueCommand(Command.MouseOver, this);
    }
    private void OnMouseUpAsButton() {
        Game.Instance.EnqueueCommand(Command.MouseUpAsButton, this);
    }
}
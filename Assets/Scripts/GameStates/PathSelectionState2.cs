using System.Collections.Generic;

public class PathSelectionState2 : IDisposableState {

    public const string prefix = "path-selection-state.";

    public const string cancel = prefix + "cancel";
    public const string move = prefix + "move";
    public const string reconstructPath = prefix + "reconstruct-path";
    public const string appendToPath = prefix + "append-to-path";
    
    public Level level;
    public PathSelectionState2(Level level) {
        this.level = level;
    }

    public IEnumerator<StateChange> Run {
        get {
            yield break;
        }
    }
    
    public void Dispose() {
        
    }
}
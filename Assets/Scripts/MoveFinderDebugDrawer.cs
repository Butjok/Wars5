

/*public class MoveFinderDebugDrawer : MonoBehaviour {

    public Vector3 textPosition = new Vector3(50, 100, 0);

    [Command]
    public bool Show { set => enabled = value; }
    
    private void Update() {
        using (Draw.ingame.WithLineWidth(2)) {

            if (Camera.main)
                using (Draw.ingame.InScreenSpace(Camera.main)) {
                    Draw.ingame.Label2D(textPosition, "move destinations", 14, LabelAlignment.TopLeft, Color.cyan);
                    Draw.ingame.Label2D(textPosition, "\ngoals", 14, LabelAlignment.TopLeft, Color.magenta);
                    Draw.ingame.Label2D(textPosition, "\n\nA* (starting from goals) closed nodes", 14, LabelAlignment.TopLeft, Color.yellow);
                }

            foreach (var position in MoveFinder.Destinations) {
                Draw.ingame.SolidPlane((Vector3)position.ToVector3Int(), Vector3.up, Vector2.one, Color.cyan);
                Draw.ingame.Label2D((Vector3)position.ToVector3Int(), MoveFinder.MoveNodes[position].g.ToString(), 14, LabelAlignment.Center, Color.black);
            }

            foreach (var position in MoveFinder.goals) {
                Draw.ingame.CrossXZ((Vector3)position.ToVector3Int(), .5f, Color.magenta);
            }

            foreach (var position in MoveFinder.PathClosedNodes) {
                Draw.ingame.SolidPlane((Vector3)position.ToVector3Int(), Vector3.up, Vector2.one, Color.yellow);
                Draw.ingame.Label2D((Vector3)position.ToVector3Int(), MoveFinder.PathNodes[position].g.ToString(), 14, LabelAlignment.Center, Color.black);
            }

            var path = MoveFinder.Path;
            var restPath = MoveFinder.RestPath;

            for (var i = 1; i < path.Count; i++) {
                Vector3 from = path[i - 1].ToVector3Int();
                Vector3 to = path[i].ToVector3Int();
                Draw.ingame.Arrow(from, to, Color.blue);
            }
            for (var i = 1; i < restPath.Count; i++) {
                Vector3 from = restPath[i - 1].ToVector3Int();
                Vector3 to = restPath[i].ToVector3Int();
                Draw.ingame.Arrow(from, to, Color.yellow);
            }
        }
    }
}*/
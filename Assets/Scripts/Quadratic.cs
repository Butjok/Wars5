using System.Collections.Generic;

public struct Quadratic {

    public float a, b, c;

    public float this[float x] => a * x * x + b * x + c;
    public float FirstDerivative(float x) => 2 * a * x + b;
    public float SecondDerivative => 2 * a;

    public static bool TryApproximate(IReadOnlyList<(float x, float y)> points, out Quadratic quadratic) {

        if (points.Count < 3) {
            quadratic = default;
            return false;
        }

        var p0 = points[0];
        var p1 = points[points.Count / 2];
        var p2 = points[^1];

        // http://www2.lawrence.edu/fast/GREGGJ/CMSC210/arithmetic/interpolation.html
        var diff10 = (p1.y - p0.y) / (p1.x - p0.x);
        var diff21 = (p2.y - p1.y) / (p2.x - p1.x);
        var c2 = p0.y;
        var b2 = diff10;
        var a2 = (diff21 - diff10) / (p2.x - p0.x);

        quadratic = new Quadratic {
            a = a2,
            b = -a2 * p0.x - a2 * p1.x + b2,
            c = a2 * p0.x * p1.x - b2 * p0.x + c2,
        };
        return true;
    }
}
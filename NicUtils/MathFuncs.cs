using System;

namespace NicUtils {
    
    public static class Sigmoid {
        public static double Value(double x) {
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        public static double Derivative(double x) {
            double value = Value(x);
            return DerivativeFromValue(value);
        }

        public static double DerivativeFromValue(double value) {
            return value * (1 - value);
        }
    }

    public class Distances2D {
        private static readonly float SQRT2 = MathF.Sqrt(2);

        public enum HeuristicType { EuclidianSquared, Euclidian, Octile, Manhattan }

        public static float GetDistance((float x, float y) coords1, (float x, float y) coords2, HeuristicType heuristicType) {
            float dx = MathF.Abs(coords1.x - coords2.x);
            float dy = MathF.Abs(coords1.y - coords2.y);
            return heuristicType switch {
                HeuristicType.EuclidianSquared => dx * dx + dy * dy,
                HeuristicType.Euclidian => MathF.Sqrt(dx * dx + dy * dy),
                HeuristicType.Octile => Math.Abs(dx - dy) + SQRT2 * Math.Min(dx, dy),
                _ => dx + dy,
            };
        }
    }

}

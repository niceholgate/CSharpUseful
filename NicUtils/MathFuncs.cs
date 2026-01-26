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
        private static readonly double SQRT2 = Math.Sqrt(2);

        public enum HeuristicType { EuclidianSquared, Euclidian, Octile, Manhattan }

        public static double GetDistance((double x, double y) coords1, (double x, double y) coords2, HeuristicType heuristicType) {
            double dx = Math.Abs(coords1.x - coords2.x);
            double dy = Math.Abs(coords1.y - coords2.y);
            return heuristicType switch {
                HeuristicType.EuclidianSquared => dx * dx + dy * dy,
                HeuristicType.Euclidian => Math.Sqrt(dx * dx + dy * dy),
                HeuristicType.Octile => Math.Abs(dx - dy) + SQRT2 * Math.Min(dx, dy),
                _ => dx + dy,
            };
        }
    }

}

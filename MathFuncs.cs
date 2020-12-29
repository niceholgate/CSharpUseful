using System;

namespace NicUtils {
    
    public class Sigmoid {
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
        public enum HeuristicType { Euclidian, Octile, Manhattan }
        //public static readonly string[] heuristicTypes = { "euclidian", "octile", "manhattan" };
        public static double GetDistance(double[] coords1, double[] coords2, HeuristicType heuristicType) {
            // Check that the coordinates are both of the same length
            if (coords1.Length != coords2.Length) { throw new ArgumentException($"Coordinates must of the same length, but instead are of lengths {coords1.Length} and {coords2.Length}."); }

            double sum = 0.0;
            if (heuristicType == HeuristicType.Euclidian) {
                for (int i = 0; i < coords1.Length; i++) { sum += Math.Pow((coords1[i] - coords2[i]), 2); }
                return Math.Sqrt(sum);
            } else if (heuristicType == HeuristicType.Octile) {
                double dx = Math.Abs(coords1[0] - coords2[0]);
                double dy = Math.Abs(coords1[1] - coords2[1]);
                return dx - dy + Math.Sqrt(2) * Math.Min(dx, dy);
            } else if (heuristicType == HeuristicType.Manhattan) {
                for (int i = 0; i < coords1.Length; i++) { sum += Math.Abs(coords1[i] - coords2[i]); }
                return sum;
            } else {
                throw new ArgumentException($"Unknown distance type requested, must be from among: " +
                                    $"{string.Join(", ", (string[])Enum.GetNames(typeof(HeuristicType)))}");
            }
        }

    }
    
}

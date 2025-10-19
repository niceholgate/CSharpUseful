using System;
using System.ComponentModel;


namespace NicUtils.ExtensionMethods {
    public static class MiscExtensions {

        public static (bool success, T result) AttemptConversion<T>(this object input) {
            if (input is T variable)
                return (true, variable);
            else
                try {
                    // Handling Nullable types i.e, int?, double?, bool? .. etc
                    if (Nullable.GetUnderlyingType(typeof(T)) != null) {
                        TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
                        T val = (T)conv.ConvertFrom(input);
                        if (val == null) { return (false, val); }
                        return (true, val);
                    } else {
                        return (true, (T)Convert.ChangeType(input, typeof(T)));
                    }
                } catch (Exception) {
                    return (false, default(T));
                }
        }
    }
}

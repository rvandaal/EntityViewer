using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiagramViewer {
    public static class AssemblyExtensions {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly) {
            // TODO: Argument validation
            try {
                return assembly.GetTypes();
            } catch (ReflectionTypeLoadException e) {
                return e.Types.Where(t => t != null);
            }
        }
    }
}

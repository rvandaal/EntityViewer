using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace GraphFramework {
    public interface IGraph {
        Point ProjectPoint(Point3D point);
        Point3D UnprojectPoint(Point pos2D, double z);
    }
}

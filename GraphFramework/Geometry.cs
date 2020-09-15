//using System;
//using System.Windows.Media.Media3D;

//namespace GraphFramework {
//    /// <summary>Specifies a geometry.</summary>
//    [Serializable]
//    public struct Geometry {
//        /// <summary>Constructs a new instance of a geometry.</summary>
//        /// <param name="origin">The point of origin.</param>
//        /// <param name="xaxis">The first axis.</param>
//        /// <param name="yaxis">The second axis.</param>
//        public Geometry(Point3D origin, Vector3D xaxis, Vector3D yaxis)
//            : this() {
//            Origin = origin;
//            Xaxis = xaxis;
//            Yaxis = yaxis;
//            Xdimension = 0;
//            Ydimension = 0;
//        }

//        /// <summary>Constructor of the geometry</summary>
//        public Geometry(Geometry geometry) : this(geometry.Origin, geometry.Xaxis, geometry.Yaxis) { }

//        /// <summary>Constructs a new instance of a geometry.</summary>
//        /// <param name="origin">The point of origin.</param>
//        /// <param name="xaxis">The first axis.</param>
//        /// <param name="yaxis">The second axis.</param>
//        /// <param name="xdimension">The first dimension.</param>
//        /// <param name="ydimension">The second dimension.</param>
//        public Geometry(Point3D origin, Vector3D xaxis, Vector3D yaxis, double xdimension, double ydimension)
//            : this() {
//            Origin = origin;
//            Xaxis = xaxis;
//            Yaxis = yaxis;
//            Xdimension = xdimension;
//            Ydimension = ydimension;
//        }

//        /// <summary>Gets or sets the origin of the geometry.</summary>
//        /// <param name="value">The point of origin.</param>
//        public Point3D Origin { get; set; }

//        /// <summary>Gets or sets the first axis of the geometry.</summary>
//        /// <param name="value">The first axis.</param>
//        public Vector3D Xaxis { get; set; }

//        /// <summary>Gets or sets the second axis of the geometry.</summary>
//        /// <param name="value">The second axis.</param>
//        public Vector3D Yaxis { get; set; }

//        /// <summary>Gets or sets the first dimension of the geometry.</summary>
//        /// <param name="value">The first dimension.</param>
//        public double Xdimension { get; set; }

//        /// <summary>Gets or sets the second dimension of the geometry.</summary>
//        /// <param name="value">The second dimension.</param>
//        public double Ydimension { get; set; }

//        /// <summary>Returns the transformation matrix.</summary>
//        public Matrix3D TransformMatrix {
//            get {
//                Vector3D normal = Vector3D.CrossProduct(Xaxis, Yaxis);
//                TranslateTransform3D translate = new TranslateTransform3D(Origin.X, Origin.Y, Origin.Z);

//                Matrix3D translateMatrix = translate.Value;
//                Matrix3D geoMatrix = new Matrix3D(Xaxis.X, Xaxis.Y, Xaxis.Z, 0,
//                                                  Yaxis.X, Yaxis.Y, Yaxis.Z, 0,
//                                                  normal.X, normal.Y, normal.Z, 0,
//                                                  0, 0, 0, 1);
//                return geoMatrix * translateMatrix;
//            }
//        }

//        public static Geometry operator *(Geometry geometry, Matrix3D transform) {
//            Matrix3D newTransform = geometry.TransformMatrix * transform;

//            return new Geometry(
//                new Point3D(newTransform.OffsetX, newTransform.OffsetY, newTransform.OffsetZ),
//                new Vector3D(newTransform.M11, newTransform.M12, newTransform.M13),
//                new Vector3D(newTransform.M21, newTransform.M22, newTransform.M23),
//                geometry.Xdimension, geometry.Ydimension);
//        }
//    }
//}

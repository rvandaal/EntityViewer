//using System;
//using System.Windows;
//using System.Windows.Media.Media3D;

//namespace GraphFramework {
//    public static class GeoMath {
//        // http://mathworld.wolfram.com/Plane.html
//        // http://paulbourke.net/geometry/planeline/

//        // Returns true if (point, vector) is parallel to the plane
//        //    if (point, vector) is on the plane, intersect is set to point, otherwise intersect is set to null.
//        // Returns false if (point, vector) is not parallel to the plane
//        //    intersect is set the inersection of (point, vector) through the plane.
//        public static bool Intersect(Point3D point, Vector3D vector, Geometry geometry, out Point3D? intersect) {
//            double distance;
//            return Intersect(point, vector, geometry, out intersect, out distance);
//        }

//        // Returns true if (point, vector) is parallel to the plane
//        //    if (point, vector) is on the plane, intersect is set to point, otherwise intersect is set to null.
//        // Returns false if (point, vector) is not parallel to the plane
//        //    intersect is set the inersection of (point, vector) through the plane.
//        public static bool Intersect(Point3D point, Vector3D vector, Geometry geometry, out Point3D? intersect,
//                                     out double distance) {
//            Vector3D normal = Vector3D.CrossProduct(geometry.Xaxis, geometry.Yaxis);
//            Point3D p0 = geometry.Origin;

//            double a = normal.X;
//            double b = normal.Y;
//            double c = normal.Z;
//            double d = -a * p0.X - b * p0.Y - c * p0.Z;
//            Point3D x1 = point;
//            Point3D x2 = point + vector;
//            //TODO: We have to look into the problem with uDenominator being almost 0
//            //because of rounding issues with double
//            double uDenominator = (a * (x1.X - x2.X) + b * (x1.Y - x2.Y) + c * (x1.Z - x2.Z));
//            if (Math.Abs(uDenominator) < 0.00000001) {
//                //normal is perpedicular to line. line parallel to geometry.

//                //if point on plane
//                if (a * x1.X + b * x1.Y + c * x1.Z + d == 0) {
//                    intersect = point;
//                    distance = 0;
//                    return true;
//                }
//                intersect = null;
//                distance = 0;
//                return true;
//            }
//            double u = (a * x1.X + b * x1.Y + c * x1.Z + d) / uDenominator;
//            intersect = x1 + u * vector;
//            distance = u;
//            return false;
//        }

//        // Returns true if planes are parallel to eachother
//        //     point and vector are set to null
//        // Return false if planes are not parallel to eachother
//        //     (point, vector) represents the intersection line of both planes
//        public static bool Intersection(Geometry projectedFrom, Geometry projectedTo, out Point3D? point,
//                                        out Vector3D? vector) {
//            Point3D? p1;
//            Point3D? p2;
//            // try first over the x-axis
//            bool parallel = Intersect(projectedFrom.Origin, projectedFrom.Xaxis, projectedTo, out p1);
//            if (!parallel) {
//                // not parallel => p1 has value and p2 can be calculated

//                Intersect(projectedFrom.Origin + projectedFrom.Yaxis * projectedFrom.Ydimension,
//                                     projectedFrom.Xaxis, projectedTo, out p2);
//            } else {
//                // try over the y-axis
//                parallel = Intersect(projectedFrom.Origin, projectedFrom.Yaxis, projectedTo, out p1);
//                if (!parallel) {
//                    // nor parallel => p1 has value and p2 can be calculated
//                    Intersect(projectedFrom.Origin + projectedFrom.Xaxis * projectedFrom.Xdimension,
//                                         projectedFrom.Yaxis, projectedTo, out p2);
//                } else {
//                    // no intersection
//                    point = null;
//                    vector = null;
//                    return true;
//                }
//            }
//            point = p1;
//            Vector3D v3 = (p2.Value - p1.Value);
//            v3.Normalize();
//            vector = v3;
//            return false;
//        }

//        public static bool IntersectFinite(Geometry projectedFrom, Geometry projectedTo, out Point3D? point1, out Point3D? point2) {

//            point1 = null;
//            point2 = null;

//            Point3D? point;
//            Vector3D? vector;
//            // First infinite intersection
//            if (!Intersection(projectedFrom, projectedTo, out point, out vector)) {
//                // Next determine finite intersection.
//                Matrix3D fromScreen = projectedTo.TransformMatrix;
//                Matrix3D toScreen = fromScreen.Inverted();

//                Point3D projectedPoint = point.Value * toScreen;
//                Vector3D projectedVector = vector.Value * toScreen;

//                PointF point2D = new PointF((float)projectedPoint.X, (float)projectedPoint.Y);
//                Vector vector2D = new Vector(projectedVector.X, projectedVector.Y);
//                PointF rectll = new PointF((float)-projectedTo.Xdimension / 2, (float)-projectedTo.Ydimension / 2);
//                PointF recttr = new PointF((float)projectedTo.Xdimension / 2, (float)projectedTo.Ydimension / 2);
//                PointF? p1;
//                PointF? p2;
//                if (LineRectangleIntersection(point2D, vector2D, rectll, recttr, out p1, out p2, 1e-3)) {
//                    point1 = new Point3D(p1.Value.X, p1.Value.Y, 0);
//                    point1 = point1 * fromScreen;
//                    point2 = new Point3D(p2.Value.X, p2.Value.Y, 0);
//                    point2 = point2 * fromScreen;
//                    return true;
//                }
//            }
//            return false;
//        }

//        public static void ProjectGeometryOnLine(Geometry geometry, Point3D pointOfLine, Vector3D vectorOfLine,
//                                                 out Point3D beginPoint, out Point3D endPoint) {
//            Point3D[] points = new Point3D[4];
//            double halfX = geometry.Xdimension / 2;
//            double halfY = geometry.Ydimension / 2;

//            points[0] = geometry.Origin + geometry.Xaxis * halfX + geometry.Yaxis * halfY;
//            points[1] = geometry.Origin - geometry.Xaxis * halfX + geometry.Yaxis * halfY;
//            points[2] = geometry.Origin + geometry.Xaxis * halfX - geometry.Yaxis * halfY;
//            points[3] = geometry.Origin - geometry.Xaxis * halfX - geometry.Yaxis * halfY;

//            for (int i = 0; i < points.Length; ++i) {
//                points[i] = ProjectPointOnLine(points[i], pointOfLine, vectorOfLine);
//            }

//            double maxDistance = double.MinValue;
//            int maxPointIndex1 = -1;
//            int maxPointIndex2 = -1;
//            for (int i = 0; i < 4; ++i) {
//                for (int j = i + 1; j < 4; ++j) {
//                    if ((points[i] - points[j]).Length > maxDistance) {
//                        maxPointIndex1 = i;
//                        maxPointIndex2 = j;
//                        maxDistance = (points[i] - points[j]).Length;
//                    }
//                }
//            }

//            beginPoint = points[maxPointIndex1];
//            endPoint = points[maxPointIndex2];

//            bool swap = beginPoint.X > endPoint.X;
//            if (beginPoint.X == endPoint.X && beginPoint.Y > endPoint.Y) {
//                swap = true;
//            }
//            if (beginPoint.X == endPoint.X && beginPoint.Y == endPoint.Y && beginPoint.Z > endPoint.Z) {
//                swap = true;
//            }

//            if (swap) {
//                Point3D t = beginPoint;
//                beginPoint = endPoint;
//                endPoint = t;
//            }
//        }

//        public static Point3D ProjectPointOnLine(Point3D point, Point3D pointOfLine, Vector3D vectorOfLine) {
//            Vector3D pointToPointOfLine = point - pointOfLine;
//            Vector3D vectorOfLineN = vectorOfLine.Normalized();
//            double dotProduct = Vector3D.DotProduct(vectorOfLineN, pointToPointOfLine);
//            return pointOfLine + dotProduct * vectorOfLineN;
//        }

//        public static bool ProjectPointOnFiniteLine(Point3D point, Point3D linep1, Point3D linep2, out Point3D result, out double dist) {
//            Vector3D v1 = linep2 - linep1;
//            Vector3D v2 = linep1 - point;
//            result = new Point3D();
//            dist = 0;
//            const double accuracy = 1e-12;
//            double v1Length = v1.Length;
//            double v2Length = v2.Length;

//            if (v1Length > accuracy) {
//                if (v2Length > accuracy) {
//                    v1.Normalize();
//                    double dot = Math.Abs(Vector3D.DotProduct(v1, v2));
//                    if (dot > -accuracy && (dot < (v1Length + accuracy))) {
//                        v1 = v1 * dot;
//                        v2 = v2 + v1;
//                        result = point + v2;
//                        dist = v2.Length;
//                        return true;
//                    }
//                } else {
//                    dist = 0;
//                    result = linep1;
//                }
//            }
//            return false;
//        }

//        public static Point3D ShortestPointToLine(Point3D point, Point3D pointOfLine, Vector3D vectorOfLine) {
//            Vector3D pointToPointOfLine = point - pointOfLine;
//            double linelength = vectorOfLine.Length;
//            Vector3D vectorOfLineN = vectorOfLine.Normalized();
//            double dotProduct = Vector3D.DotProduct(vectorOfLineN, pointToPointOfLine);

//            if (dotProduct < 0) {
//                return pointOfLine;
//            }
//            if (dotProduct > linelength) {
//                return pointOfLine + vectorOfLine;
//            }
//            return pointOfLine + dotProduct * vectorOfLineN;
//        }

//        public static double Radians2Degrees(double radians) {
//            return radians * 180.0 / Math.PI;
//        }

//        public static double Degrees2Radians(double degrees) {
//            return degrees * Math.PI / 180.0;
//        }

//        // Compute intersection of a line with a rectangle.
//        // Returns entry and exit points of intersection...
//        private static bool LineRectangleIntersection(
//            PointF origin,
//            Vector vector,
//            PointF rectOrigin,
//            PointF rectCorner,
//            out PointF? p1,
//            out PointF? p2,
//            double accuracy
//            ) {
//            double horEntry, horExit, verEntry, verExit, entry, exit;
//            bool horizontalIntersection;
//            bool verticalIntersection;

//            p1 = null;
//            p2 = null;

//            // Determine horizontal bounds on intersection...
//            if (accuracy < vector.X) {
//                horEntry = rectOrigin.X;
//                horExit = rectCorner.X;
//                horizontalIntersection = true;
//            } else if (vector.X < -accuracy) {
//                horEntry = rectCorner.X;
//                horExit = rectOrigin.X;
//                horizontalIntersection = true;
//            } else {
//                horEntry = 0.0;
//                horExit = 0.0;
//                horizontalIntersection = false;
//            }
//            // Determine vertical bounds on intersection...
//            if (accuracy < vector.Y) {
//                verEntry = rectOrigin.Y;
//                verExit = rectCorner.Y;
//                verticalIntersection = true;
//            } else if (vector.Y < -accuracy) {
//                verEntry = rectCorner.Y;
//                verExit = rectOrigin.Y;
//                verticalIntersection = true;
//            } else {
//                verEntry = 0.0;
//                verExit = 0.0;
//                verticalIntersection = false;
//            }
//            if (horizontalIntersection) {
//                horEntry = (horEntry - origin.X) / vector.X;
//                horExit = (horExit - origin.X) / vector.X;
//            }
//            if (verticalIntersection) {
//                verEntry = (verEntry - origin.Y) / vector.Y;
//                verExit = (verExit - origin.Y) / vector.Y;
//            }
//            // Merge horizontal and vertical results...
//            if (horizontalIntersection) {
//                if (verticalIntersection) {
//                    entry = Math.Max(verEntry, horEntry);
//                    exit = Math.Min(verExit, horExit);
//                } else {
//                    entry = horEntry;
//                    exit = horExit;
//                }
//            } else {
//                if (verticalIntersection) {
//                    entry = verEntry;
//                    exit = verExit;
//                } else {
//                    return (false);
//                }
//            }
//            if (exit < entry) return (false);
//            p1 = new PointF((float)(origin.X + (entry * vector.X)), (float)(origin.Y + (entry * vector.Y)));
//            p2 = new PointF((float)(origin.X + (exit * vector.X)), (float)(origin.Y + (exit * vector.Y)));
//            return (true);
//        }
//    }
//}
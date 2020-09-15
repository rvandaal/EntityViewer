using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Media3D;
using GraphFramework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UmlEditor {
    public class UmlDiagram : Diagram {

        private Random random = new Random();

        private readonly Dictionary<Type, Node> generatedNodes = new Dictionary<Type, Node>();

        struct ImageFileHeader {
            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        };

        public UmlDiagram() {
            var assembly = Assembly.GetAssembly(typeof (UmlDiagram));
            //var assembly = Assembly.LoadFrom(@"C:\Users\xxx\Dropbox\Projects\UmlEditor\UmlEditor\ExampleApplication\bin\Debug\ExampleApplication.exe");

            DateTime currentAssemblyTimeStamp = GetBuildDateTime(assembly); // not used yet

            List<Type> types = assembly.GetLoadableTypes().ToList();
            //List<Type> viewmodelTypes = assemblyViewmodels.GetLoadableTypes().ToList();
            //List<Type> controllerTypes = assemblyControllers.GetLoadableTypes().ToList();
            //List<Type> dataModelTypes = assemblyDataModels.GetLoadableTypes().ToList();

            //List<Type> types = uiTypes.Concat(viewmodelTypes).Concat(controllerTypes).Concat(dataModelTypes).ToList();
            //List<Type> types = assembly.GetLoadableTypes().ToList();

            foreach (var type in types) { //.Where(t => t.Name.ToLower().Contains("extension"))
                AddOrGetClassNode(type, types);
            }
            // size the nodes
            foreach (var node in Nodes.Cast<UmlClass>()) {
                var count = node.NumberOfInheritors;
                Size size = new Size(Math.Max(100, 5 * count), 40);
                node.Size = size;
            }
        }

        private Node AddOrGetClassNode(Type type, ICollection<Type> types) {
            //if (type.IsValueType || type.Name == "String" || !types.Contains(type)) return null;
            if (type.IsValueType || type.Name == "String") return null;

            var baseType = type.BaseType;
            if (!generatedNodes.ContainsKey(type)) {
                //
                // Add a node if it doesn't exist yet and cache it
                //
                var node = AddNode(type) as UmlClass;
                Debug.Assert(node != null);
                generatedNodes[type] = node;

                PropertyInfo[] fields = type.GetProperties();
                int i = 0;
                foreach (var field in fields) {
                    Type fieldType;
                    try {
                        fieldType = field.PropertyType;
                    } catch(Exception) {
                        continue;
                    }
                    if (i < 1 && !fieldType.IsValueType && fieldType.Name != "String") {//types.Contains(field.PropertyType)) {
                        //
                        // Yes, we want the comprised node in our diagram
                        //
                        Node comprisedNode = AddOrGetClassNode(fieldType, types);
                        if (comprisedNode != null) {
                            comprisedNode.Pos = node.Pos;
                            TranslateChildNodes(comprisedNode, new Vector3D(50, 0, 0));
                            var link = new UmlCompositionRelation("135");
                            link.EndNode = node; // node2 = parent, node1 = child
                            link.StartNode = comprisedNode;
                            node.Links.Add(link);
                            comprisedNode.Links.Add(link);
                            Links.Add(link);
                            i++;
                        }
                    }
                }
                //
                // Now, check the base types of this type
                //
                var parentNode = baseType != null && baseType != typeof(object) ? AddOrGetClassNode(baseType, types) : null;
                if (parentNode != null) {
                    var link = new UmlInheritanceRelation("90");
                    link.StartNode = node;
                    link.EndNode = parentNode;
                    parentNode.Pos = node.Pos + new Vector3D(0, -50, 0);
                    node.Links.Add(link);
                    parentNode.Links.Add(link);
                    Links.Add(link);
                }
                return node;
            }
            return generatedNodes[type];
        }

        private void TranslateChildNodes(Node node, Vector3D translateVector) {
            node.Pos += translateVector;
            //foreach (var link in node.Links.Where(l => l.EndNode == node)) {
            //    TranslateChildNodes(link.GetNeighbourNode(node), translateVector);
            //}
        }

        private Node AddNode(Type type) {
            var node = new UmlClass(this);
            node.Pos = new Point3D(random.Next(5000), random.Next(5000), 1);
            //node.Pos = new Point3D(500, 500, 1);
            node.Size = new Size(100, 50);
            node.Assembly = type.Assembly;
            node.Name = type.Name;
            Nodes.Add(node);
            return node;
        }

        public UmlClass CreateClass(string className) {
            var node = new UmlClass(this);
            node.Pos = new Point3D(random.Next(1000), random.Next(1000), 1);
            node.Size = new Size(100, 50);
            node.Name = className;
            Nodes.Add(node);
            return node;
        }

        protected override Node CreateNode() {
            return new UmlClass(this);
        }

        protected override Link CreateLink() {
            return new UmlRelation("0");
        }

        protected override void OnMouseMove(Point point, Point startingPoint) {
            base.OnMouseMove(point, startingPoint);
            var deltaVector = point - startingPoint;
            switch (CurrentOperation) {
                case DragOperation.CreatingNode:

                    // TODO: hier gaat het fout; Size wordt geset terwijl het hier om de Size2D gaat.
                    // Maar ook die kunnen we niet zomaar zetten, eigenlijk moeten we de corners al unprojecten 
                    // en daaruit volgt dan een size.

                    ((UmlClass)ActiveNode).Size = new Size(Math.Abs(deltaVector.X), Math.Abs(deltaVector.Y));
                    ActiveNode.Pos2D = startingPoint + deltaVector / 2;
                    break;
                case DragOperation.CreatingLink:
                    ActiveLink.ManualEndPoint = point;
                    break;
            }
        }

        internal void HandleMouseLeftButtonUpOnNodeBody(Node node) {
            HandleMouseLeftButtonUpOnConnector(node, new UmlAssociationRelation("0"));
        }

        internal void HandleMouseLeftButtonUpOnInheritanceConnector(Node node) {
            HandleMouseLeftButtonUpOnConnector(node, new UmlInheritanceRelation("0"));
        }

        internal void HandleMouseLeftButtonUpOnCompositionConnector(Node node) {
            HandleMouseLeftButtonUpOnConnector(node, new UmlCompositionRelation("0"));
        }

        internal void HandleMouseLeftButtonUpOnAssociationConnector(Node node) {
            HandleMouseLeftButtonUpOnConnector(node, new UmlAssociationRelation("0"));
        }

        private void HandleMouseLeftButtonUpOnConnector(Node node, UmlRelation relation) {
            if (ActiveLink != null) {
                //
                // Until now, ActiveLink was just a general UmlRelation,
                // because its desired type was not know yet. Now, we know
                // the desired type, so switch it for the right subtype.
                //
                Links.Remove(ActiveLink);
                ActiveLink.StartNode.Links.Remove(ActiveLink);
                Links.Add(relation);
                relation.StartNode = ActiveLink.StartNode;
                relation.StartNode.Links.Add(relation);
                relation.EndNode = node;
                node.Links.Add(relation);
                ActiveLink = null;
            }
            CurrentOperation = DragOperation.None;
        }


        static DateTime GetBuildDateTime(Assembly assembly) {
            var path = assembly.GetName().CodeBase;
            if (File.Exists(path)) {
                var buffer = new byte[Math.Max(Marshal.SizeOf(typeof(ImageFileHeader)), 4)];
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                    fileStream.Position = 0x3C;
                    fileStream.Read(buffer, 0, 4);
                    fileStream.Position = BitConverter.ToUInt32(buffer, 0); // COFF header offset
                    fileStream.Read(buffer, 0, 4); // "PE\0\0"
                    fileStream.Read(buffer, 0, buffer.Length);
                }
                var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try {
                    var coffHeader = (ImageFileHeader)Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(), typeof(ImageFileHeader));

                    return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1) + new TimeSpan(coffHeader.TimeDateStamp * TimeSpan.TicksPerSecond));
                } finally {
                    pinnedBuffer.Free();
                }
            }
            return new DateTime();
        }
    }
}

using System.IO;
using DiagramViewer.Models;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramClassOperation : UmlDiagramClassMember {

        public TypeDefinition TypeDefinition { get; set; }

        public UmlOperation UmlOperation { get; set; }
        public MethodDefinition Method { get { return UmlOperation.Method; } }
        public string BodyString {
            get {
                if(UmlModel.LoadedAssembly == null) {
                    return null;
                }
                AstBuilder astBuilder = new AstBuilder(new DecompilerContext(UmlModel.LoadedAssembly.MainModule) {CurrentType = TypeDefinition});
                astBuilder.AddMethod(Method);
                StringWriter output = new StringWriter();
                astBuilder.GenerateCode(new PlainTextOutput(output));
                string result = output.ToString();
                output.Dispose();
                return result;
            }
        }
        
        public UmlDiagramClassOperation(UmlOperation umlOperation, UmlClass umlClass) : base(umlOperation) {
            UmlOperation = umlOperation;
            TypeDefinition = umlClass.TypeDefinition;
        }

        public AccessModifier AccessModifier {
            get { return UmlOperation.AccessModifier; }
        }
    }
}

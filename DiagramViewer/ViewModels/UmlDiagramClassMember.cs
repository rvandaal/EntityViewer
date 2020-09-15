using DiagramViewer.Models;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramClassMember : ViewModelBase {
        private readonly UmlClassMember umlClassMember;

        public string Type { get { return umlClassMember.Type; } }
        public string Name { get { return umlClassMember.Name; } }

        private bool isVisibleInList = true;
        public bool IsVisibleInList {
            get { return isVisibleInList; }
            set { SetProperty(value, ref isVisibleInList, () => IsVisibleInList); }
        }

        public UmlDiagramClassMember(UmlClassMember umlClassMember) {
            this.umlClassMember = umlClassMember;
        }
    }
}

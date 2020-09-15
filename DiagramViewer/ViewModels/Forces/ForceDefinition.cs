
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DiagramViewer.ViewModels.Forces {
    /// <summary>
    /// Class that defines a type of force that can be exerted on a node or link.
    /// </summary>
    public abstract class ForceDefinition : ViewModelBase {
        protected ForceDefinition(string name) {
            Name = name;
            forceSettings = new List<ForceSetting>();
        }

        private bool isEnabled = true;
        public bool IsEnabled {
            get { return isEnabled; }
            set { SetProperty(value, ref isEnabled, () => IsEnabled); }
        }

        private string name;
        public string Name {
            get { return name; }
            set { SetPropertyClass(value, ref name, () => Name); }
        }

        private readonly List<ForceSetting> forceSettings;
        public ReadOnlyCollection<ForceSetting> ForceSettings { get { return forceSettings.AsReadOnly(); } }

        public bool HasSettings { get { return ForceSettings.Any(); } }

        protected ForceSetting AddForceSetting(string label, double minimum, double maximum, int precision, double initialValue) {
            var forceSetting = new ForceSetting(label, minimum, maximum, precision, initialValue);
            forceSettings.Add(forceSetting);
            return forceSetting;
        }

        public void UpdateForces(Diagram diagram, double contentWidth, double contentHeight) {
            if(IsEnabled) {
                UpdateForcesOverride(diagram, contentWidth, contentHeight);
            }
        }

        protected abstract void UpdateForcesOverride(Diagram diagram, double contentWidth, double contentHeight);
    }
}

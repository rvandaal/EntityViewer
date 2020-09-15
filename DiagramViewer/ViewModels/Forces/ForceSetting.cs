namespace DiagramViewer.ViewModels.Forces {
    public class ForceSetting : ViewModelBase {

        private string label;
        public string Label {
            get { return label; }
            set { SetPropertyClass(value, ref label, () => Label); }
        }

        private double parameterMinimum;
        public double ParameterMinimum {
            get { return parameterMinimum; }
            set { SetDoubleProperty(value, ref parameterMinimum, () => ParameterMinimum); }
        }

        private double parameterMaximum;
        public double ParameterMaximum {
            get { return parameterMaximum; }
            set { SetDoubleProperty(value, ref parameterMaximum, () => ParameterMaximum); }
        }

        private int precision;
        public int Precision {
            get { return precision; }
            set { SetProperty(value, ref precision, () => Precision); }
        }

        private double parameterValue;
        public double ParameterValue {
            get { return parameterValue; }
            set { SetDoubleProperty(value, ref parameterValue, () => ParameterValue); }
        }

        public ForceSetting(string label, double minimum, double maximum, int precision, double initialValue) {
            this.label = label;
            parameterMinimum = minimum;
            parameterMaximum = maximum;
            this.precision = precision;
            parameterValue = initialValue;
        }
    }
}

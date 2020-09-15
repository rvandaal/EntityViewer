using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExampleApplication {
    public class XVehicle {
        private double power;
    }

    public class XCar : XVehicle {
        private List<XWheel> wheels;
        private XSteer steer;
    }

    public class XWheel {
        private double diameter;
    }

    public class XSteer {
        
    }

    public class XHatchback : XCar {
        private bool isOpen;
    }
}

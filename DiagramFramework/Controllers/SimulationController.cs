using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiagramFramework.Controllers {
    public class SimulationController {
        //
        // Bevat dingen als ComputeForces / ApplyForces
        // Centreren op middelpunt hier of in UmlCanvas?

        // Is de simulator eigenlijk geen onderdeel van de UmlCanvas? Die is namelijk verantwoordelijk voor de layout
        // van de UmlClassItemContainers?
        // Wel wordt de simulatie beinvloed door specifieke nodes en links. Een inheritance link definieert bv andere krachten
        // dan een composition link. (Is dit zo? Je kunt op Link niveau een PreferredOrientation property maken)
        // Op node niveau wordt de afstootkracht bepaald door oa de afstand tot de andere node en hiervoor maakt de shape wel uit
        // Ofwel, SimulatorController heeft een link nodig naar de losse UmlDiagramController om UmlClassControllers en UmlRelationControllers te benaderen.
        //
        // Op de vraag of Simulator bij Canvas hoort, kun je je afvragen, kan de een zonder de ander bestaan:
        // - Een Simulator kan wel degelijk zonder canvas bestaan; misschien wil ik geen nodes laten zien, maar een andere view met alleen statistieken.
        // Dus wat mij betreft weet UmlCanvas niet wat een simulator is. Misschien wil ik wel een andere methode gebruiken om nodes te positioneren.

    }
}

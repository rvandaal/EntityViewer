using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace UmlViewer.Controls {
    public class UmlCanvas : Canvas {
        //
        // Hier komen de mouse events binnen; de mouse events van de uml classes bubblen ook hier naar toe
        // Als het om diagram brede operaties gaat, zoals create new uml class en drag een link naar een andere node,
        // dan moet dat vanuit hier worden afgehandeld.
        // 
        // Afspraak:
        // - View specifieke functionaliteit die niet opgeslagen hoef te worden wordt op viewmodel niveau afgehandeld (en niet op controller niveau). 
        // Bv het maken van een nieuwe link door te draggen, dit is niet iets wat je op wil slaan, hier hoeft geen controller
        // bij te pas te komen.
        // Wel moet de controller bepalen welk object er aangemaakt moet worden, deze ontvangt dus een event van het viewmodel 
        // dat het draggen klaar is en dat Node1=node a en Node2 = node b.
        // Het feit dat je een nieuw link aan het maken bent moet in de controller besloten worden omdat dit applicatie specifiek
        // is. Van de andere kant, als je een hele andere UI hebt, zou de applicatie code gelijk moeten blijven, hmm.
        //
        // Als er een link wordt gedragged naar een connector op een UmlClass, dan zou de UmlClassControl moeten communiceren
        // naar het UmlCanvas over welke knop de muis is, dit kan alleen de UmlClassControl weten. Dus UmlClassControl zou een 
        // property als IsMouseOverInheritanceConnector moeten bevatten. Of deze data zou ook als custom RoutedEventArgs gecommuniceerd
        // kunnen worden.
        //
        // DP's:
        // - Zoomfactor
        // - CurrentMouseOperation

    }
}

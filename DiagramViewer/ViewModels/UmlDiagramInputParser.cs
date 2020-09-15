using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DiagramViewer.Models;

namespace DiagramViewer.ViewModels {

    public static class UmlDiagramInputParser {
        //
        // Probleem: 
        // Meteen het diagram updaten nadat je een class hebt aangemaakt, is niet handig, omdat de text om de class
        // aan te maken onderdeel is van de operatie die opnieuw moet worden uitgevoerd om het diagram te maken.
        // Meteen een UmlDiagramClass aanmaken nadat je een UmlClass hebt gemaakt kan ook niet, want andere viewports
        // krijgen dan geen trigger om hetzelfde te doen.
        //
        // Oeps, denkfoutje gemaakt: UmlModel raist Modified, het UmlDiagram subscribed hierop en in de handler
        // synct hij de classes en re-evalueert de operaties die tot de view leiden.
        // Dit is fout, je hoeft tijdens het parsen van de input text alleen maar de classes te syncen, het uitvoeren van 
        // de operaties moet pas na het parsen van de input text gebeuren.
        //

        private enum State {
            None,
            ShowRelation1,
            ShowRelation2,
            Hide,
            Delete,
            Note,
            Inheritance,
            Association,
            AssociationN,
            Dependence,
            Aggregation,
            AggregationN,
            Composition,
            CompositionN,
            ImplementsInterface,
            FindKeyword,
            HasMethod
        }

        private static string[] ValueTypes = new[] {"void", "string", "int", "double", "float", "bool", "boolean", "long", "point", "point3d", "vector", "vector3d", "matrix", "matrix3d",
                                                    "int?", "double?", "float?", "bool?", "boolean?", "long?", "point?", "point3d?", "vector?", "vector3d?",
                                                    "observablecollection<string>"};

        public static void ProcessInput(string newInputText, UmlDiagram diagram, bool isPreview) {
            List<UmlClass> currentClassList = null;
            State currentState = State.None;
            bool invert = false;
            IEnumerable<string> tokens = Split(newInputText, ' ', '\r', '\n');
            foreach (string token in tokens) {
                ProcessToken(token, ref currentState, ref invert, ref currentClassList, diagram);
            }
        }

        /// <summary>
        /// Splits the string passed in by the delimiters passed in.
        /// Quoted sections are not split, and all tokens have whitespace
        /// trimmed from the start and end.
        /// </summary>
        private static IEnumerable<string> Split(string stringToSplit, params char[] delimiters) {
            //
            // a has "view model":b
            // is splitted to 'a', 'has', 'view model:b'
            //
            List<string> results = new List<string>();

            bool inQuote = false;
            StringBuilder currentToken = new StringBuilder();
            foreach (char currentCharacter in stringToSplit) {
                if (currentCharacter == '"') {
                    // When we see a ", we need to decide whether we are
                    // at the start or send of a quoted section...
                    inQuote = !inQuote;
                } else if (delimiters.Contains(currentCharacter) && inQuote == false) {
                    // We've come to the end of a token, so we find the token,
                    // trim it and add it to the collection of results...
                    string result = currentToken.ToString().Trim();
                    if (result != "") results.Add(result);

                    // We start a new token...
                    currentToken = new StringBuilder();
                } else {
                    // We've got a 'normal' character, so we add it to
                    // the curent token...
                    currentToken.Append(currentCharacter);
                }
            }

            // We've come to the end of the string, so we add the last token...
            string lastResult = currentToken.ToString().Trim();
            if (lastResult != "") results.Add(lastResult);

            return results;
        }

        private static void ProcessToken(
            string token,
            ref State state,
            ref bool invert,
            ref List<UmlClass> selectedClasses,
            UmlDiagram diagram
        ) {
            string lowerToken = token.ToLower();
            switch (lowerToken) {
                case "not":
                    invert = true;
                    break;
                case "clearmodel":
                    if (state == State.None) {
                        diagram.ClearModel();
                    }
                    break;
                case "cleardiagram":
                    if (state == State.None) {
                        diagram.ClearDiagram();
                    }
                    break;
                case "showmembers":
                    if (state == State.None) {
                        diagram.ShowsMembers = true;
                    }
                    break;
                case "hidemembers":
                    if (state == State.None) {
                        diagram.ShowsMembers = false;
                    }
                    break;
                case "find":
                    if (state == State.None) {
                        state = State.FindKeyword;
                    }
                    break;

                //case "showall":
                //    if (state == State.None) {
                //        diagram.ShowAll();
                //    }
                //    break;
                case "startattracting":
                    if (state == State.None) {
                        diagram.StartAttracting();
                    }
                    break;
                case "stopattracting":
                    if (state == State.None) {
                        diagram.StopAttracting();
                    }
                    break;
                case "pause":
                    diagram.PauseSimulation();
                    break;
                case "resume":
                    diagram.ResumeSimulation();
                    break;
                case "show":
                    //
                    // Show is the default command
                    //
                    break;
                case "showrelation":
                    if (state == State.None) {
                        state = State.ShowRelation1;
                    }
                    break;
                case "hide":
                    if (state == State.None) {
                        state = State.Hide;
                    }
                    break;
                case "delete":
                    if (state == State.None) {
                        state = State.Delete;
                    }
                    break;
                case "hasnote":
                    if (state == State.None) {
                        state = State.Note;
                    }
                    break;
                case "hasm":
                case "hasmethod":
                    if (state == State.None)
                    {
                        state = State.HasMethod;
                    }
                    break;
                case "is":
                    state = State.Inheritance;
                    break;
                case "has":
                    state = State.Association;
                    break;
                case "hasn":
                    state = State.AssociationN;
                    break;
                case "implements":
                    state = State.ImplementsInterface;
                    break;
                case "dependson":
                    state = State.Dependence;
                    break;
                case "owns":
                    state = State.Composition;
                    break;
                case "ownsn":
                    state = State.CompositionN;
                    break;
                case "contains":
                    state = State.Aggregation;
                    break;
                case "containsn":
                    state = State.AggregationN;
                    break;
                default:
                    string name = null;
                    if (state == State.Association || state == State.AssociationN ||
                        state == State.Aggregation || state == State.AggregationN ||
                        state == State.Composition || state == State.CompositionN) {
                        //
                        // Check for attributes and operations
                        // Attribute: a has name:type
                        // Operation: a has name():type
                        //
                        string type = null;
                        bool isOperation = false;
                        int indexOfColon = token.IndexOf(":", StringComparison.InvariantCulture);
                        if (indexOfColon > -1) {
                            //
                            // There is a ':' in the string, indicating an attribute or operation
                            //
                            name = token.Substring(0, indexOfColon);
                            if (name.EndsWith("()")) {
                                isOperation = true;
                                name = name.Substring(0, name.Length - 2);
                            }
                            if (indexOfColon + 1 < token.Length) {
                                type = token.Substring(indexOfColon + 1);
                            }
                            //
                            // If type == null (i.e. a has b:), add an attribute or operation and break.
                            //
                            if (type == null || ValueTypes.Contains(type.ToLower())) {
                                foreach (var umlClass in selectedClasses) {
                                    if (isOperation) {
                                        if (!invert) {
                                            diagram.CreateModelOperation(umlClass, name, type);
                                        } else {
                                            diagram.RemoveModelOperation(umlClass, name, type);
                                        }
                                    } else {
                                        if (!invert) {
                                            diagram.CreateModelAttribute(umlClass, name, type);
                                        } else {
                                            diagram.RemoveModelAttribute(umlClass, name, type);
                                        }
                                    }
                                }
                                break; // ?
                            }
                            // Type is not a value type, proceed with processing Collection.
                            token = type;
                            if (isOperation) {
                                name += "()";
                            }
                        }
                    }

                    // TODO: je hebt nu method nodes aangemaakt; wat je beter kunt doen:
                    // je kunt al operations maken met a has name():type. Nu hoef je alleen nog maar een switch maken waarmee je tussen operatie-in-class-blokje of apart-blokje-voor-operation
                    // kunt togglen. Maak er een view specifiek dingetje van dus ipv het model aan te passen.
                    // Niet triviaal trouwens: er moeten wel nodes bij komen als je de operaties als blokjes laat zien.
                    // Wel moet deze parser met Nodes gaan werken want je wil net zo goed notes kunnen selecteren. 

                    List<UmlClass> operand = null;
                    if (state != State.Note && state != State.FindKeyword && state != State.HasMethod) {
                        operand = ProcessCollection(token, diagram.Model).Cast<UmlClass>().ToList();
                    }

                    switch (state) {
                        case State.None:
                            diagram.Show(operand);
                            selectedClasses = operand;
                            break;
                        case State.Note:
                            foreach (var startClass in selectedClasses) {
                                diagram.CreateModelNote(startClass, token);
                            }
                            state = State.None;
                            break;
                        case State.HasMethod:
                            foreach (var startClass in selectedClasses)
                            {
                                diagram.CreateModelMethodNode(startClass, token);
                            }
                            state = State.None;
                            break;
                        case State.FindKeyword:
                            var classes =
                                FindClassesWithCertainFields(token, diagram.Model).Concat(
                                    FindClassesWithCertainProperties(token, diagram.Model)).ToList();
                            diagram.Show(classes);
                            state = State.None;
                            break;
                        case State.ShowRelation1:
                            selectedClasses = operand;
                            state = State.ShowRelation2;
                            break;
                        case State.ShowRelation2:
                            diagram.ShowRelation(selectedClasses, operand);
                            state = State.None;
                            break;
                        case State.Hide:
                            diagram.RemoveClassesFromDiagram(operand);
                            state = State.None;
                            break;
                        case State.Delete:
                            diagram.RemoveClassesFromModel(operand);
                            state = State.None;
                            break;
                        default:
                            diagram.Show(operand);
                            foreach (var startClass in selectedClasses) {
                                foreach (var endClass in operand) {
                                    switch (state) {
                                        case State.Inheritance:
                                            if (!invert) {
                                                diagram.CreateModelRelation(RelationType.Inheritance, startClass, endClass, false);
                                            } else {
                                                diagram.RemoveModelRelation(startClass, endClass, RelationType.Inheritance, name);
                                            }
                                            break;
                                        case State.Composition:
                                            if (!invert) {
                                                diagram.CreateModelRelation(RelationType.Composition, endClass, startClass, false, name);
                                            } else {
                                                diagram.RemoveModelRelation(endClass, startClass, RelationType.Composition, name);
                                            }
                                            break;
                                        case State.CompositionN:
                                            if (!invert) {
                                                diagram.CreateModelRelation(RelationType.Composition, endClass, startClass, true, name);
                                            } else {
                                                diagram.RemoveModelRelation(endClass, startClass, RelationType.Composition, name);
                                            }
                                            break;
                                        case State.Aggregation:
                                            if (!invert) {
                                                diagram.CreateModelRelation(RelationType.Aggregation, endClass, startClass, false, name);
                                            } else {
                                                diagram.RemoveModelRelation(endClass, startClass, RelationType.Aggregation, name);
                                            }
                                            break;
                                        case State.AggregationN:
                                            if (!invert) {
                                                diagram.CreateModelRelation(RelationType.Aggregation, endClass, startClass, true, name);
                                            } else {
                                                diagram.RemoveModelRelation(endClass, startClass, RelationType.Aggregation, name);
                                            }
                                            break;
                                        case State.Association:
                                            if (!invert) {
                                                diagram.CreateModelRelation(RelationType.Association, startClass, endClass, false, name);
                                            } else {
                                                diagram.RemoveModelRelation(startClass, endClass, RelationType.Association, name);
                                            }
                                            break;
                                        case State.AssociationN:
                                            if (!invert) {
                                                diagram.CreateModelRelation(RelationType.Association, startClass, endClass, true, name);
                                            } else {
                                                diagram.RemoveModelRelation(startClass, endClass, RelationType.Association, name);
                                            }
                                            break;
                                        case State.ImplementsInterface:
                                            if (!invert) {
                                                diagram.CreateModelRelation(RelationType.ImplementsInterface, startClass, endClass, false);
                                            } else {
                                                diagram.RemoveModelRelation(startClass, endClass, RelationType.ImplementsInterface, name);
                                            }
                                            break;
                                        case State.Dependence:
                                            if (!invert) {
                                                diagram.CreateModelRelation(RelationType.Dependence, startClass, endClass, false, name);
                                            } else {
                                                diagram.RemoveModelRelation(startClass, endClass, RelationType.Dependence, name);
                                            }
                                            break;
                                        case State.HasMethod:
                                            if (!invert) {
                                                diagram.CreateModelRelation(RelationType.HasMethod, startClass, endClass, false, name);
                                            } else  {
                                                diagram.RemoveModelRelation(startClass, endClass, RelationType.HasMethod, name);
                                            }
                                            break;
                                    }
                                }
                            }
                            state = State.None;
                            break;
                    }
                    break;
            }
        }

        private static List<UmlClass> ProcessCollection(string token, UmlModel model) {
            return ProcessUnion(token, model);
        }

        private static List<UmlClass> ProcessUnion(string token, UmlModel model) {
            List<List<UmlClass>> lists =
                token.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => ProcessSubtraction(t, model)).ToList();
            return lists.Aggregate((union, t) => union.Union(t).ToList());
        }

        private static List<UmlClass> ProcessSubtraction(string token, UmlModel model) {
            List<List<UmlClass>> lists =
                token.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => ProcessIntersection(t, model)).ToList();
            return lists.Aggregate((subtraction, t) => subtraction.Except(t).ToList());
        }

        private static List<UmlClass> ProcessIntersection(string token, UmlModel model) {
            List<List<UmlClass>> lists =
                token.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => ProcessAssociation(t, model)).ToList();
            return lists.Aggregate((intersection, t) => intersection.Intersect(t).ToList());
        }

        private static List<UmlClass> ProcessAssociation(string token, UmlModel model) {
            string[] tokens = token.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            List<UmlClass> totalList = ProcessAtomicCollection(tokens[0], model);
            foreach (var t in tokens.Skip(1)) {
                switch (t) {
                    case "sub":
                        totalList = totalList.SelectMany(u => u.SubClasses).Union(totalList).ToList();
                        break;
                    case "super":
                        totalList = totalList.SelectMany(u => u.SuperClasses).Union(totalList).ToList();
                        break;
                    case "impl":
                        totalList = totalList.SelectMany(u => u.Implementors).Union(totalList).ToList();
                        break;
                    case "compparent":
                        totalList = totalList.SelectMany(u => u.CompositionParentClasses).Union(totalList).ToList();
                        break;
                    case "com":
                        totalList = totalList.SelectMany(u => u.CompositionChildClasses).Union(totalList).ToList();
                        break;
                    case "assparent":
                        totalList = totalList.SelectMany(u => u.AssociationParentClasses).Union(totalList).ToList();
                        break;
                    case "ass":
                        totalList = totalList.SelectMany(u => u.AssociationChildClasses).Union(totalList).ToList();
                        break;

                    case "sub*":
                        totalList = totalList.SelectMany(u => u.Descendents).Union(totalList).ToList();
                        break;
                    case "super*":
                        totalList = totalList.SelectMany(u => u.Ancestors).Union(totalList).ToList();
                        break;
                    case "compparent*":
                        totalList = totalList.SelectMany(u => u.CompositionParentClasses).Union(totalList).ToList();
                        break;
                    case "com*":
                        totalList = totalList.SelectMany(u => u.CompositionChildClasses).Union(totalList).ToList();
                        break;
                    case "assparent*":
                        totalList = totalList.SelectMany(u => u.AssociationParentClasses).Union(totalList).ToList();
                        break;
                    case "ass*":
                        totalList = totalList.SelectMany(u => u.AssociationChildClasses).Union(totalList).ToList();
                        break;
                }
            }
            return totalList;
        }

        private static List<UmlClass> ProcessAtomicCollection(string token, UmlModel model) {
            return GetClassesOrCreateClass(token, model, true).ToList();
        }

        private static IEnumerable<UmlClass> GetClassesOrCreateClass(string name, UmlModel model, bool canCreateNew) {
            IEnumerable<UmlClass> classes = FindClasses(name, model).ToList();
            if (!classes.Any() && !ContainsWildcards(name) && canCreateNew) {
                var umlClass = model.CreateClassFromDiagram(name);
                classes = FindClasses(name, model).ToList();
            }
            return classes;
        }

        private static IEnumerable<UmlClass> FindClasses(string pattern, UmlModel model) {
            return model.Classes.Where(c => Regex.IsMatch(c.Name.ToLower(), "^" + pattern.ToLower().Replace("*", ".*") + "$"));
        }

        private static IEnumerable<UmlClass> FindClassesWithCertainFields(string pattern, UmlModel model) {
            return model.Classes.Where(c => c.Attributes.Any(a => Regex.IsMatch(a.Name.ToLower(), "^" + pattern.ToLower().Replace("*", ".*") + "$")));
        }

        private static IEnumerable<UmlClass> FindClassesWithCertainProperties(string pattern, UmlModel model) {
            return model.Classes.Where(c => c.Properties.Any(a => Regex.IsMatch(a.Name.ToLower(), "^" + pattern.ToLower().Replace("*", ".*") + "$")));
        }

        private static bool ContainsWildcards(string token) {
            return token.Contains("*") || token.Contains("?");
        }
    }
}

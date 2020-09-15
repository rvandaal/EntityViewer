using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiagramViewer.Utilities {

    /// <summary>
    /// Utility class that contains helper methods to find ancestors or children of a
    /// specified element in the visual tree.
    /// </summary>
    /// <remarks>
    /// Sources:
    /// <list>
    ///     <item><see href="http://www.hardcodet.net/2008/02/find-wpf-parent" /></item>
    ///     <item><see href="http://stackoverflow.com/questions/636383/wpf-ways-to-find-controls"/></item>
    ///     <item><see href="https://rachel53461.wordpress.com/2011/10/09/navigating-wpfs-visual-tree/"/></item>
    /// </list>
    /// </remarks>
    /// <seealso cref="DependencyObject">DependencyObject Class</seealso>
    /// <seealso cref="VisualTreeHelper">VisualTreeHelper Class</seealso>
    public static class UIFinder {

        #region Public methods

        /// <summary>
        /// Finds the first ancestor in the visual tree that matches the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the queried element.</typeparam>
        /// <param name="dependencyObject">
        /// The element from which the ancestor should be found.
        /// </param>
        /// <returns>
        /// The first ancestor that matches the specified type. If no matching element can be
        /// found, a <see langword="null"/> is returned.
        /// </returns>
        /// <remarks>
        /// The specified element itself is not included in the search.
        /// </remarks>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = @"The generic types are needed to create objects of those types,
            there are no parameters with the generic types that can be passed here."
        )]
        public static T FindAncestor<T>(this DependencyObject dependencyObject)
            where T : DependencyObject {
            return dependencyObject.FindAncestor<T>(null);
        }

        /// <summary>
        /// Finds the first ancestor in the logical tree that matches the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the queried element.</typeparam>
        /// <param name="dependencyObject">
        /// The element from which the ancestor should be found.
        /// </param>
        /// <returns>
        /// The first ancestor that matches the specified type. If no matching element can be
        /// found, a <see langword="null"/> is returned.
        /// </returns>
        /// <remarks>
        /// The specified element itself is not included in the search.
        /// </remarks>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = @"The generic types are needed to create objects of those types,
            there are no parameters with the generic types that can be passed here."
        )]
        public static T FindLogicalAncestor<T>(this DependencyObject dependencyObject)
            where T : DependencyObject {
            return dependencyObject.FindLogicalAncestor<T>(null);
        }

        /// <summary>
        /// Finds the first ancestor in the visual tree that matches the specified type and name.
        /// </summary>
        /// <typeparam name="T">The type of the queried element.</typeparam>
        /// <param name="dependencyObject">
        /// The element from which the ancestor should be found.
        /// </param>
        /// <param name="name">The name of the queried element.</param>
        /// <returns>
        /// The first ancestor that matches the specified type and name. If no matching element can
        /// be found, a <see langword="null"/> is returned.
        /// </returns>
        /// <remarks>
        /// The specified element itself is not included in the search.
        /// If the <paramref name="name"/> is null or empty, this method will return
        /// the first ancestor with the specified type.
        /// If an ancestor is not a <see cref="FrameworkElement"/>, it does not have a name to
        /// match with. If it still matches the type, it will be returned.
        /// </remarks>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = @"The generic types are needed to create objects of those types,
            there are no parameters with the generic types that can be passed here."
        )]
        public static T FindAncestor<T>(
            this DependencyObject dependencyObject,
            string name
        ) where T : DependencyObject {
            T foundAncestor = null;
            var parent = dependencyObject.GetVisualParent();
            while (parent != null) {
                var typedParent = parent as T;
                if (!string.IsNullOrEmpty(name)) {
                    var frameworkElement = parent as FrameworkElement;
                    if (
                        (typedParent != null) &&
                        (
                            (frameworkElement == null) ||
                            (frameworkElement.Name == name)
                        )
                    ) {
                        foundAncestor = typedParent;
                        break;
                    }
                } else if (typedParent != null) {
                    foundAncestor = typedParent;
                    break;
                }
                parent = parent.GetVisualParent();
            }
            return foundAncestor;
        }

        /// <summary>
        /// Finds the first ancestor in the logical tree that matches the specified type and name.
        /// </summary>
        /// <typeparam name="T">The type of the queried element.</typeparam>
        /// <param name="dependencyObject">
        /// The element from which the ancestor should be found.
        /// </param>
        /// <param name="name">The name of the queried element.</param>
        /// <returns>
        /// The first ancestor that matches the specified type and name. If no matching element can
        /// be found, a <see langword="null"/> is returned.
        /// </returns>
        /// <remarks>
        /// The specified element itself is not included in the search.
        /// If the <paramref name="name"/> is null or empty, this method will return
        /// the first ancestor with the specified type.
        /// If an ancestor is not a <see cref="FrameworkElement"/>, it does not have a name to
        /// match with. If it still matches the type, it will be returned.
        /// </remarks>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = @"The generic types are needed to create objects of those types,
            there are no parameters with the generic types that can be passed here."
        )]
        public static T FindLogicalAncestor<T>(
            this DependencyObject dependencyObject,
            string name
        ) where T : DependencyObject {
            T foundAncestor = null;
            var parent = dependencyObject.GetLogicalParent();
            while (parent != null) {
                var typedParent = parent as T;
                if (!string.IsNullOrEmpty(name)) {
                    var frameworkElement = parent as FrameworkElement;
                    if (
                        (typedParent != null) &&
                        (
                            (frameworkElement == null) ||
                            (frameworkElement.Name == name)
                        )
                    ) {
                        foundAncestor = typedParent;
                        break;
                    }
                } else if (typedParent != null) {
                    foundAncestor = typedParent;
                    break;
                }
                parent = parent.GetLogicalParent();
            }
            return foundAncestor;
        }

        /// <summary>
        /// Tries to find an ancestor in both the visual and logical trees that
        /// matches the specified type.
        /// </summary>
        /// <param name="child">
        /// The element from which the ancestor should be found.
        /// </param>
        /// <param name="parentType">The type of the queried element.</param>
        /// <param name="searchDepth">
        /// The amount of levels that are included in the search.
        /// </param>
        /// <returns>
        /// The first ancestor that matches the specified type. If the <paramref name="child"/>
        /// or <paramref name="parentType"/> is <see langword="null"/>, or if no matching
        /// element can be found, <see langword="null"/> is returned.
        /// </returns>
        /// <remarks>
        /// This method will search recursively until the <paramref name="parentType"/>
        /// is found, or until the searchDepth is reached. If the search depth is high,
        /// this method can significantly impact performance, so caution is advised.
        /// </remarks>
        public static object FindAncestorType(
            DependencyObject child,
            Type parentType,
            int searchDepth
        ) {

            if (child == null || parentType == null) {
                return null;
            }

            var searchIndexLogical = 0;
            var logicalParent = LogicalTreeHelper.GetParent(child);
            while (logicalParent != null && searchIndexLogical < searchDepth) {
                if (logicalParent.GetType() == parentType) {
                    return logicalParent;
                }
                logicalParent = LogicalTreeHelper.GetParent(logicalParent);
                searchIndexLogical++;
            }

            var searchIndexVisual = 0;
            var visualParent = VisualTreeHelper.GetParent(child);
            while (visualParent != null && searchIndexVisual < searchDepth) {
                if (visualParent.GetType() == parentType) {
                    return visualParent;
                }
                visualParent = VisualTreeHelper.GetParent(visualParent);
                searchIndexVisual++;
            }
            return null;
        }

        /// <summary>
        /// Tries to find an ancestor in both the visual and logical trees that
        /// matches the specified type. The maximum search depth is 3.
        /// </summary>
        /// <param name="child">
        /// The element from which the ancestor should be found.
        /// </param>
        /// <param name="parentType">The type of the queried element.</param>
        /// <returns>
        /// The first ancestor that matches the specified type. If no matching element can
        /// be found, a <see langword="null"/> is returned.
        /// </returns>
        public static object FindAncestorType(
            DependencyObject child,
            Type parentType
        ) {
            int searchDepth = 3;
            return FindAncestorType(child, parentType, searchDepth);
        }

        /// <summary>
        /// This method is an alternative to the WPF
        /// <see cref="VisualTreeHelper.GetParent"/> method and also
        /// supports content elements.
        /// </summary>
        /// <param name="dependencyObject">
        /// The element from which the parent should be retrieved.
        /// </param>
        /// <returns>
        /// The visual parent of the specified element, if available. Otherwise
        /// <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// For content elements, this method falls back to the logical tree to find a parent.
        /// </remarks>
        public static DependencyObject GetVisualParent(this DependencyObject dependencyObject) {
            DependencyObject parentObject = null;
            if (dependencyObject != null) {
                if (dependencyObject is ContentElement contentElement) {
                    var parent = ContentOperations.GetParent(contentElement);
                    if (parent != null) {
                        parentObject = parent;
                    } else {
                        var frameworkContentElement = contentElement as FrameworkContentElement;
                        parentObject = frameworkContentElement?.Parent;
                    }
                } else {
                    parentObject = VisualTreeHelper.GetParent(dependencyObject);
                }
            }
            return parentObject;
        }

        /// <summary>
        /// This method is an alternative to the WPF <see cref="LogicalTreeHelper.GetParent"/>
        /// method and also supports content elements.
        /// </summary>
        /// <param name="dependencyObject">
        /// The element from which the parent should be retrieved.
        /// </param>
        /// <returns>
        /// The visual parent of the specified element, if available.
        /// Otherwise, <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// For content elements, this method falls back to the logical tree to find a parent.
        /// </remarks>
        public static DependencyObject GetLogicalParent(this DependencyObject dependencyObject) {
            DependencyObject parentObject = null;
            if (dependencyObject != null) {
                if (dependencyObject is ContentElement contentElement) {
                    var parent = ContentOperations.GetParent(contentElement);
                    if (parent != null) {
                        parentObject = parent;
                    } else {
                        var frameworkContentElement = contentElement as FrameworkContentElement;
                        parentObject =
                            (frameworkContentElement != null) ?
                            frameworkContentElement.Parent :
                            null;
                    }
                } else {
                    parentObject = LogicalTreeHelper.GetParent(dependencyObject);
                }
            }
            return parentObject;
        }

        /// <summary>
        /// Finds a child of a given element in the visual tree.
        /// </summary>
        /// <param name="dependencyObject">
        /// The element from which a child should be retrieved.
        /// </param>
        /// <typeparam name="T">The type of the queried element.</typeparam>
        /// <remarks>
        /// The specified element itself is not included in the search.
        /// </remarks>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = @"The generic types are needed to create objects of those types,
            there are no parameters with the generic types that can be passed here."
        )]
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "It is very likely that this method is going to be used in the future."
        )]
        public static T FindChild<T>(
            this DependencyObject dependencyObject
        ) where T : DependencyObject {
            return dependencyObject.FindChild<T>(null);
        }

        /// <summary>
        /// Finds a child of a given element in the visual tree.
        /// </summary>
        /// <param name="dependencyObject">
        /// The element from which a child should be retrieved.
        /// </param>
        /// <typeparam name="T">The type of the queried element to be found.</typeparam>
        /// <param name="name">The name of the queried element to be found. </param>
        /// <remarks>
        /// The specified element itself is not included in the search.
        /// If the <paramref name="name"/> is null or empty, this method will return
        /// the first child with the specified type.
        /// If a child is not a <see cref="FrameworkElement"/>, it does not have a name to match
        /// with. If it still matches the type, it will be returned.
        /// </remarks>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = @"The generic types are needed to create objects of those types,
            there are no parameters with the generic types that can be passed here."
        )]
        public static T FindChild<T>(
            this DependencyObject dependencyObject,
            string name
        ) where T : DependencyObject {
            T foundChild = null;
            var childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (var i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child is T typedChild) {
                    if (string.IsNullOrEmpty(name)) {
                        foundChild = typedChild;
                        break;
                    } else {
                        var frameworkElement = typedChild as FrameworkElement;
                        if (frameworkElement != null) {
                            if (frameworkElement.Name == name) {
                                foundChild = typedChild;
                                break;
                            } else {
                                foundChild = FindChild<T>(child, name);
                                if (foundChild != null) {
                                    break;
                                }
                            }
                        }
                    }
                } else {
                    foundChild = FindChild<T>(child, name);
                    if (foundChild != null) {
                        break;
                    }
                }
            }
            return foundChild;
        }

        /// <summary>
        /// Finds the children of a given type <typeparamref name="T"/> in the visual tree.
        /// </summary>
        /// <param name="dependencyObject">
        /// The element from which the children should be retrieved.
        /// </param>
        /// <typeparam name="T">The type of the queried element to be found.</typeparam>
        /// <param name="children"><see cref="Collection{T}"/> with found elements.</param>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = @"The generic types are needed to create objects of those types,
            there are no parameters with the generic types that can be passed here."
        )]
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "It is very likely that this method is going to be used in the future."
        )]
        public static void FindChildren<T>(
            this DependencyObject dependencyObject,
            Collection<T> children
        ) where T : DependencyObject {
            dependencyObject.FindChildren(null, children);
        }

        /// <summary>
        /// Finds the children of a given type <typeparamref name="T"/> in the visual tree.
        /// </summary>
        /// <param name="dependencyObject">
        /// The element from which the children should be retrieved.
        /// </param>
        /// <typeparam name="T">The type of the queried element to be found.</typeparam>
        /// <param name="children"><see cref="Collection{T}"/> with found elements.</param>
        /// <param name="name">The name of the queried element to be found. </param>
        /// <remarks>
        /// The specified element itself is not included in the search.
        /// If the <paramref name="name"/> is null or empty, all children of type
        /// <typeparamref name="T"/> will be returned.
        /// </remarks>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = @"The generic types are needed to create objects of those types,
            there are no parameters with the generic types that can be passed here."
        )]
        public static void FindChildren<T>(
            this DependencyObject dependencyObject,
            string name,
            Collection<T> children
        ) where T : DependencyObject {
            if (children == null) {
                children = new Collection<T>();
            }
            var childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (var i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child is T typedChild) {
                    if (string.IsNullOrEmpty(name)) {
                        children.Add(typedChild);
                        typedChild.FindChildren(name, children);
                    } else {
                        var frameworkElement = typedChild as FrameworkElement;
                        if (frameworkElement != null) {
                            if (frameworkElement.Name == name) {
                                children.Add(typedChild);
                                typedChild.FindChildren(name, children);
                            } else {
                                child.FindChildren(name, children);
                            }
                        }
                    }
                } else {
                    child.FindChildren(name, children);
                }
            }
        }

        /// <summary>
        /// Finds the named template part for the specified control.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This extension method allows you to get a named template part for a control in code
        /// behind. This can be useful if you need to access a template part which you know is
        /// there, but which is otherwise inaccessible through the control's API.
        /// </para>
        /// <para>
        /// Please be aware that the named template parts will, in general, only be accessible
        /// after the control's template has been applied, i.e., after the method
        /// <see cref="FrameworkElement.OnApplyTemplate"/> has been called. Calling this extension
        /// method earlier than that may result in not being able to find the named template part,
        /// simply because the control has not been completed yet. Therefore, you should always
        /// check the result of this extension method for <see langword="null"/>.
        /// </para>
        /// </remarks>
        /// <param name="control">The control for which to find the named template part.</param>
        /// <param name="templatePartName">
        /// The name of the template part. Typically, a named template part starts with the
        /// prefix "PART_". This method does not assume that this convention if followed, though.
        /// </param>
        /// <returns>
        /// The named template part for the specified control, or <see langword="null"/> if no
        /// such template part can be found.
        /// </returns>
        public static object FindTemplatePart(
            this Control control,
            string templatePartName
        ) {
            object templatePart = null;
            var template = control.Template;
            if (template != null) {
                templatePart = template.FindName(templatePartName, control);
            }

            return templatePart;
        }

        /// <summary>
        /// Gets the nesting depth of a <see cref="TreeViewItem"/>.
        /// </summary>
        /// <param name="item">The tree view item for which to get the nesting depth.</param>
        /// <returns>The nesting depth for the specified tree view item.</returns>
        public static int GetDepth(this TreeViewItem item) {
            TreeViewItem parent;
            while ((parent = GetParent(item)) != null) {
                return GetDepth(parent) + 1;
            }
            return 0;
        }

        /// <summary>
        /// Gets the root of the visual tree that contains the specified dependency object.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> for which to get the root of the visual tree.
        /// </param>
        /// <returns>
        /// The root of the visual tree that contains the specified dependency object;
        /// or the dependency object itself if that turns out to be the root of the visual tree.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the specified <paramref name="dependencyObject"/> is <see langword="null"/>.
        /// </exception>
        public static DependencyObject GetRoot(this DependencyObject dependencyObject) {
            if (dependencyObject == null) {
                throw new ArgumentNullException(nameof(dependencyObject));
            }
            var parent = VisualTreeHelper.GetParent(dependencyObject);
            var root = dependencyObject;
            while (parent != null) {
                //
                // Travel up the visual tree as long as the parent of the current element is
                // not null. The root is the last element before we reach null.
                //
                root = parent;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return root;
        }


        /// <summary>
        /// Finds a child of a given element by type in the visual tree.
        /// </summary>
        /// <param name="type">The type of the queried element to be found.</param>
        /// <param name="element">The element in which the queried element to be found. </param>
        /// <remarks>
        /// If the <paramref name="element"/> is null or empty, this method will return null
        /// If a child is not a <see cref="FrameworkElement"/>, it does not have a name to match
        /// with. If it still matches the type, it will be returned.
        /// </remarks>
        /// <summary>
        /// Gets the child of the visual tree that contains the specified element type .
        /// </summary>
        public static Visual FindChildByType(Visual element, Type type) {
            if (element == null) {
                return null;
            }

            if (element.GetType() == type) {
                return element;
            }

            Visual foundElement = null;
            if (element is FrameworkElement frameworkElement) {
                frameworkElement.ApplyTemplate();
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++) {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = FindChildByType(visual, type);
                if (foundElement != null) {
                    break;
                }
            }

            return foundElement;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the parent of the specified <see cref="TreeViewItem"/>.
        /// </summary>
        /// <param name="item">The tree view item.</param>
        /// <returns>The parent of the tree view item.</returns>
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1800:DoNotCastUnnecessarily",
            Justification =
                "False positive, it is not possible to refactor this code to use only one cast."
        )]
        private static TreeViewItem GetParent(TreeViewItem item) {
            var parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView)) {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as TreeViewItem;
        }

        #endregion
    }
}

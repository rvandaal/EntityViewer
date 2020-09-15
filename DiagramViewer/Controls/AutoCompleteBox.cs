using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DiagramViewer.Utilities;

namespace DiagramViewer.Controls {
    public class AutoCompleteBox : TextBox {

        #region Private fields

        private Selector selector;
        private Popup popup;
        private bool isTextChangedByCode;
        private bool isSelectionChangedByNewText;
        private bool isSelectorLoaded;
        private ICollectionView collectionView;
        private bool shouldDropDownOpen;
        private bool allowedToFilter;

        #endregion

        #region Routed events

        public event RoutedEventHandler Committed {
            add { AddHandler(CommittedEvent, value); }
            remove { RemoveHandler(CommittedEvent, value); }
        }

        public static readonly RoutedEvent CommittedEvent =
            EventManager.RegisterRoutedEvent(
                "Committed",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(AutoCompleteBox)
            );

        #endregion

        #region Dependency properties

        #region TextItemsSource
        public ObservableCollection<string> TextItemsSource {
            get { return (ObservableCollection<string>)GetValue(TextItemsSourceProperty); }
            set { SetValue(TextItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty TextItemsSourceProperty =
            DependencyProperty.Register(
                "TextItemsSource",
                typeof(ObservableCollection<string>),
                typeof(AutoCompleteBox),
                new FrameworkPropertyMetadata(null, OnTextItemsSourceChanged)
            );
        #endregion

        #region IsDropDownOpen
        public bool IsDropDownOpen {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownOpenProperty =
            ComboBox.IsDropDownOpenProperty.AddOwner(
                typeof(AutoCompleteBox),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );
        #endregion

        #region IsCaseSensitive
        public bool IsCaseSensitive {
            get { return (bool)GetValue(IsCaseSensitiveProperty); }
            set { SetValue(IsCaseSensitiveProperty, value); }
        }

        public static readonly DependencyProperty IsCaseSensitiveProperty =
            DependencyProperty.Register(
                "IsCaseSensitive",
                typeof(bool),
                typeof(AutoCompleteBox),
                new FrameworkPropertyMetadata(false)
            );
        #endregion

        #region MatchesPascalCasing
        public bool MatchesPascalCasing {
            get { return (bool)GetValue(MatchesPascalCasingProperty); }
            set { SetValue(MatchesPascalCasingProperty, value); }
        }

        public static readonly DependencyProperty MatchesPascalCasingProperty =
            DependencyProperty.Register(
                "MatchesPascalCasing",
                typeof(bool),
                typeof(AutoCompleteBox),
                new FrameworkPropertyMetadata(true)
            );
        #endregion

        #region HorizontalOffset

        public double HorizontalPopupOffset {
            get { return (double)GetValue(HorizontalPopupOffsetProperty); }
            set { SetValue(HorizontalPopupOffsetProperty, value); }
        }

        public static readonly DependencyProperty HorizontalPopupOffsetProperty =
            DependencyProperty.Register("HorizontalPopupOffset", typeof(double), typeof(AutoCompleteBox),
                new FrameworkPropertyMetadata(0.0)
            );

        #endregion

        #endregion

        #region Constructors

        static AutoCompleteBox() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AutoCompleteBox),
                new FrameworkPropertyMetadata(typeof(AutoCompleteBox))
            );
        }

        public AutoCompleteBox() {
            AddHandler(MouseLeftButtonDownEvent, new RoutedEventHandler(OnMouseLeftButtonDown), true);
            DataContextChanged += OnDataContextChanged;
        }

        #endregion

        #region Public methods

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            if (selector != null) {
                selector.SelectionChanged -= OnSelectionChanged;
                selector.Loaded -= OnSelectorLoaded;
                selector.Unloaded -= OnSelectorUnloaded;
            }
            selector = Template.FindName("PART_Selector", this) as Selector;
            if (selector != null) {
                selector.SelectionChanged += OnSelectionChanged;
                selector.Loaded += OnSelectorLoaded;
                selector.Unloaded += OnSelectorUnloaded;
            }
            popup = Template.FindName("PART_Popup", this) as Popup;
            if (popup != null) {
                popup.Placement = PlacementMode.Custom;
                popup.CustomPopupPlacementCallback = GetPopupPlacement;
            }
        }

        private static CustomPopupPlacement[] GetPopupPlacement(Size popupSize, Size targetSize, Point offset) {
            return PopupPlacement.PlacePopup(
                popupSize, targetSize, offset, VerticalPlacement.Bottom, HorizontalPlacement.Left
            );
        }

        #endregion

        #region Protected methods

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) {
            base.OnLostKeyboardFocus(e);
            if (!IsKeyboardFocusWithin) {
                CloseDropDown();
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            base.OnPreviewKeyDown(e);
            switch (e.Key) {
                case Key.Escape:
                    if (IsDropDownOpen) {
                        Text = "";
                        CloseDropDown();
                        e.Handled = true;
                    }
                    break;
                case Key.Enter:
                    if (!string.IsNullOrEmpty(Text)) {
                        if (IsDropDownOpen) {
                            CloseDropDown();
                        }
                        //RaiseCommittedEvent();
                    }
                    //
                    // We handle the Enter key, because in general, pressing Enter on an AutoCompleteBox will put the focus
                    // on the next field.
                    //
                    //e.Handled = true;
                    break;
                case Key.Down:
                    if (!IsDropDownOpen) {
                        OpenDropDown();
                    } else if (collectionView != null) {
                        collectionView.MoveCurrentToNext();
                        if (collectionView.IsCurrentAfterLast) {
                            collectionView.MoveCurrentToLast();
                        }
                    }
                    e.Handled = true;
                    break;
                case Key.Up:
                    if (IsDropDownOpen && collectionView != null) {
                        collectionView.MoveCurrentToPrevious();
                        if (collectionView.IsCurrentBeforeFirst) {
                            collectionView.MoveCurrentToFirst();
                        }
                    }
                    e.Handled = true;
                    break;
            }
        }


        protected override void OnTextChanged(TextChangedEventArgs e) {
            base.OnTextChanged(e);
            //
            // If the text is changed because some item was selected, then don't open the dropdown.
            //
            if (isTextChangedByCode) {
                return;
            }
            isSelectionChangedByNewText = true;
            if (selector != null) {
                selector.SelectedIndex = -1;
            }
            if (collectionView == null) {
                return;
            }
            //
            // Refresh to perform filtering
            //
            allowedToFilter = true;
            collectionView.Refresh();
            allowedToFilter = false;

            SelectMatchingItem();

            if (shouldDropDownOpen && !Text.EndsWith(" ") && !Text.EndsWith(":")) {
                OpenDropDown();
            } else {
                CloseDropDown();
            }
            shouldDropDownOpen = false;
            isSelectionChangedByNewText = false;
        }

        #endregion

        #region Private methods

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            CloseDropDown();
        }

        private static void OnMouseLeftButtonDown(object sender, RoutedEventArgs e) {
            AutoCompleteBox autoCompleteBox = (AutoCompleteBox)sender;
            if (autoCompleteBox.IsDropDownOpen) {
                //
                // This code handles the case in which a selection was being made with the arrow keys and the user
                // clicked the current selected item after that. We cannot close the dropdown in the preview
                // mousedown, because then the item will not receive a bubbling mousedown and the item will
                // never be selected in the case that it was clicked with the mouse.
                //
                // This eventhandler is registered with handleEventsToo=true because the item will handle the 
                // MouseLeftButtonDown event and that handler will come first.
                //
                Visual dependencyObject = e.OriginalSource as Visual;
                if (dependencyObject.FindAncestor<ListBoxItem>() != null) {
                    autoCompleteBox.CloseDropDown();
                    //
                    // Focus back on the textbox, without opening the dropdown
                    //
                    autoCompleteBox.Focus();
                    autoCompleteBox.RaiseCommittedEvent();
                    e.Handled = true;
                }
            }
        }

        private static void OnTextItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            AutoCompleteBox autoComplete = (AutoCompleteBox)d;
            autoComplete.OnTextItemsSourceChanged();
        }

        private void OnTextItemsSourceChanged() {
            collectionView = CollectionViewSource.GetDefaultView(TextItemsSource);
            if (collectionView != null) {
                collectionView.Filter = item => {
                    if(!allowedToFilter) {
                        CloseDropDown();
                        return false;
                    }
                    if (string.IsNullOrWhiteSpace(Text)) {
                        return true;
                    }
                    string itemString = (string)item;
                    string capitalCharacters = new string(itemString.Where(c => char.IsUpper(c)).ToArray());

                    string text = Text;
                    if (!IsCaseSensitive) {
                        itemString = itemString.ToLower();
                        text = text.ToLower();
                        capitalCharacters = capitalCharacters.ToLower();
                    }
                    //
                    // 1. Sort the search words on their length (descending)
                    // 2. If there are multiple search words, the longest one exactly has to match with one of the item words
                    // 3. The rest may match partly with one of the item words
                    //
                    string itemWord = itemString.Split(
                            new[] { ' ' },
                            StringSplitOptions.RemoveEmptyEntries
                        ).Last();
                    string searchWord = text.Split(
                        new[] { ' ', ':' },
                        StringSplitOptions.RemoveEmptyEntries
                    ).Last();
                    bool result = itemWord.StartsWith(searchWord) || MatchesPascalCasing && capitalCharacters.StartsWith(searchWord);
                    if (result) {
                        shouldDropDownOpen = true;
                    }
                    return result;
                };
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            base.OnSelectionChanged(e);
            if (selector != null && selector.SelectedIndex > -1 && isSelectorLoaded) {
                isTextChangedByCode = true;
                if (!isSelectionChangedByNewText) {
                    //
                    // Only replace the last word in the query
                    //
                    int lastIndexOfDelimiter = Text.LastIndexOfAny(new[] {' ', ':'});
                    if(lastIndexOfDelimiter > -1) {
                        Text = Text.Substring(0, lastIndexOfDelimiter + 1) + (string)selector.SelectedItem;
                    } else {
                        Text = (string) selector.SelectedItem;
                    }
                    //var words = Text.Split(
                    //    new[] { ' ' },
                    //    StringSplitOptions.RemoveEmptyEntries
                    //);
                    //string tmp = words.Length > 1 ? string.Join(" ", words.Take(words.Length - 1)) + " " : "";
                    //Text = tmp + (string)selector.SelectedItem;
                    CaretIndex = Text.Length;
                }
                isTextChangedByCode = false;
            }
        }

        private void OnSelectorLoaded(object sender, EventArgs e) {
            isSelectorLoaded = true;
        }
        private void OnSelectorUnloaded(object sender, EventArgs e) {
            isSelectorLoaded = false;
        }

        private void OpenDropDown() {
            if (collectionView != null) {
                allowedToFilter = true;
                collectionView.Refresh();
                allowedToFilter = false;
                if (!IsDropDownOpen && TextItemsSource != null && !collectionView.IsEmpty) {
                    DetermineHorizontalPopupOffset();
                    Dispatcher.BeginInvoke(
                        new Action(() => {
                            if (IsKeyboardFocusWithin && !collectionView.IsEmpty) {
                                SetCurrentValue(IsDropDownOpenProperty, true);
                            }
                        }),
                        DispatcherPriority.Input
                    );
                }
            }
        }

        private void DetermineHorizontalPopupOffset() {
            double widthOffset = 10.0;// -9.0; //unknown why we need this offset
            var words = Text.Split(
                        new[] { ' ', ':' },
                        StringSplitOptions.RemoveEmptyEntries
                    );
            string textBeforeLastWord = words.Length > 1 ? string.Join(" ", words.Take(words.Length - 1)) + " " : null;
            if(string.IsNullOrEmpty(textBeforeLastWord)) {
                HorizontalPopupOffset = widthOffset;
            } else {
                var textBlock = new TextBlock {Text = textBeforeLastWord};
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double desiredWidth = textBlock.DesiredSize.Width;
                HorizontalPopupOffset = desiredWidth - 1 + widthOffset;
            }
        }

        private void CloseDropDown() {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }

        private bool SelectMatchingItem() {
            foreach (string item in TextItemsSource) {
                if ((IsCaseSensitive ? item == Text.Trim() : item.ToLower() == Text.Trim().ToLower())) {
                    if (selector != null) {
                        selector.SelectedItem = item;
                    }
                    return true;
                }
            }
            return false;
        }

        private void RaiseCommittedEvent() {
            var eventArgs = new RoutedEventArgs(CommittedEvent);
            RaiseEvent(eventArgs);
        }

        #endregion
    }
}

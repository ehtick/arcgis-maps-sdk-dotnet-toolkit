// /*******************************************************************************
//  * Copyright 2012-2018 Esri
//  *
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************/

using Esri.ArcGISRuntime.Toolkit.Internal;
#if WPF
#elif WINUI
using Microsoft.UI.Xaml.Media.Animation;
using Key = Windows.System.VirtualKey;
#elif WINDOWS_UWP
using Windows.UI.Xaml.Media.Animation;
using Key = Windows.System.VirtualKey;
#elif MAUI
using ScrollViewer = Microsoft.Maui.Controls.ScrollView;
using ContentControl = Microsoft.Maui.Controls.ContentView;
using FrameworkElement = Microsoft.Maui.Controls.View;
#endif


#if MAUI
namespace Esri.ArcGISRuntime.Toolkit.Maui.Primitives
#else
namespace Esri.ArcGISRuntime.Toolkit.Primitives
#endif
{
    /// <summary>
    /// Sub view for the PopupViewer control.
    /// </summary>
    public partial class NavigationSubView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSubView"/> class.
        /// </summary>
        public NavigationSubView()
        {
#if MAUI
            ControlTemplate = DefaultControlTemplate;
#else
            DefaultStyleKey = typeof(NavigationSubView);
#endif
        }

        private void UpdateView()
        {
            if (GetTemplateChild("NavigateBack") is FrameworkElement back)
            {
#if MAUI
                back.IsVisible = _navigationStack.Count > 0;
#else
                back.Visibility = _navigationStack.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
#endif
            }
            if (GetTemplateChild("NavigateUp") is FrameworkElement up)
            {
                bool isVisible = ShowHomeButton && _navigationStack.Count > 1;
#if MAUI
                up.IsVisible = isVisible;
#else
                up.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
#endif
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Home button should show when navigating more than one level deep.
        /// </summary>
        internal bool ShowHomeButton { get; set; } = false;

        private Stack<Tuple<object,double>> _navigationStack = new Stack<Tuple<object, double>>();

        /// <summary>
        /// Gets the current navigation stack
        /// </summary>
        public IEnumerable<object> NavigationStack
        {
            get
            {
                foreach(var t in _navigationStack)
                {
                    yield return t.Item1;
                }
            }
        }

        internal enum NavigationDirection
        {
            Forward,
            Backward,
            Reset
        }

        internal class NavigationEventArgs : EventArgs
        {
            private TaskCompletionSource<bool>? _deferredTaskCompletionSource;

            public NavigationEventArgs(object? from, object? to, NavigationDirection direction)
            {
                NavigatingFrom = from;
                NavigatingTo = to;
                Direction = direction;
            }
            public NavigationDirection Direction { get; }


            public object? NavigatingTo { get; }
            
            public object? NavigatingFrom { get; }
            
            public bool Cancel { get; set; }

            public NavigationDeferral GetDeferral()
            {
                if (_deferredTaskCompletionSource is not null)
                    throw new InvalidOperationException("Deferral has already been requested"); // Only one deferral currently supported

                _deferredTaskCompletionSource = new TaskCompletionSource<bool>();

                return new NavigationDeferral(() => _deferredTaskCompletionSource.TrySetResult(true));
            }
            public Task AwaitDeferralAsync()
            {
                return _deferredTaskCompletionSource?.Task ?? Task.CompletedTask;
            }
        }

        internal class NavigationDeferral
        {
            Action? _completed;

            internal NavigationDeferral(Action completed)
            {
                _completed = completed;
            }
            public void Complete()
            {
                var taskToComplete = Interlocked.Exchange(ref _completed, null);

                if (taskToComplete != null)
                    taskToComplete?.Invoke();
            }
        }

        internal event EventHandler<NavigationEventArgs>? OnNavigating;

        private async Task<bool> RaiseOnNavigatingAsync(object? content, NavigationDirection direction)
        {
            var handler = OnNavigating;
            if (handler is not null)
            {
                var args = new NavigationEventArgs(Content, content, direction);
                handler.Invoke(this, args);
                await args.AwaitDeferralAsync().ConfigureAwait(false);
                if (args.Cancel)
                    return false;
            }
            return true;
        }

        internal async Task<bool> Navigate(object? content, bool clearNavigationStack = false, bool skipRaisingEvent = false)
        {
            if (content is null && !clearNavigationStack)
                throw new ArgumentNullException(nameof(content));
            if (!skipRaisingEvent && !(await RaiseOnNavigatingAsync(content, clearNavigationStack ? NavigationDirection.Reset : NavigationDirection.Forward)))
            {
                return false;
            }
            double offset = 0;
            if (GetTemplateChild("ScrollViewer") is ScrollViewer sv)
            {
#if MAUI
                offset = sv.ScrollY;
#else
                offset = sv.VerticalOffset;
#endif
            }

            if (clearNavigationStack)
                _navigationStack.Clear();
            else if (Content is not null) // Move current content into the stack
                _navigationStack.Push(new Tuple<object, double>(Content, offset));
#if WINDOWS_XAML
            ContentTransitions = new TransitionCollection();
            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("NavigationSubViewForwardAnimation");
            bool success = false;
            if (animation != null && GetTemplateChild("Header") is UIElement header)
            {
                success = animation.TryStart(header);
            }
            if(!success)
            {
            if (_navigationStack.Count > 0)
                ContentTransitions.Add(new EntranceThemeTransition() { FromHorizontalOffset = 200, FromVerticalOffset = 0 });
            }
#endif
            SetContent(content);

#if MAUI
            (GetTemplateChild("ScrollViewer") as ScrollViewer)?.ScrollToAsync(0,0,false);
#elif WPF
            (GetTemplateChild("ScrollViewer") as ScrollViewer)?.ScrollToHome();
#elif WINDOWS_XAML
            (GetTemplateChild("ScrollViewer") as ScrollViewer)?.ChangeView(null, 0, null, disableAnimation: true);
#endif
            return true;
        }

        private void SetContent(object? content)
        {
            Content = content;
            if (GetTemplateChild("Header") is ContentControl cc1)
#if MAUI
                cc1.ContentData = content;
#else
                cc1.Content = content;
#endif
            if (GetTemplateChild("Content") is ContentControl cc2)
#if MAUI
                cc2.ContentData = content;
#else
                cc2.Content = content;
#endif
            UpdateView();
        }

        private async Task GoBack()
        {
            if (!IsBackNavigationEnabled || _navigationStack.Count == 0)
                return;

            if (!(await RaiseOnNavigatingAsync(_navigationStack.Peek().Item1, NavigationDirection.Backward)))
            {
                return;
            }
            var previousPage = _navigationStack.Pop();
            var content = previousPage.Item1;
            var lastOffset = previousPage.Item2;

#if WINDOWS_XAML
            ContentTransitions = new TransitionCollection
            {
                new EntranceThemeTransition() { FromHorizontalOffset = -200, FromVerticalOffset = 0 }
            };
#endif
            if (GetTemplateChild("ScrollViewer") is ScrollViewer sv)
            {
#if WINUI
                // Restore scroll offset on back navigation
                EventHandler<object>? handler = null;
                handler = (s, e) =>
                {
                    sv.LayoutUpdated -= handler;
                    sv.ChangeView(null, lastOffset, null, true);
                };
                sv.LayoutUpdated += handler;
#elif WPF
                ScrollChangedEventHandler? handler = null;
                handler = (s, e) =>
                {
                    if (e.ExtentHeight >= lastOffset)
                    {
                        sv.ScrollChanged -= handler;
                        sv.ScrollToVerticalOffset(lastOffset);
                    }
                };
                sv.ScrollChanged += handler; 
#elif MAUI
                EventHandler? handler = null;
                handler = (s, e) =>
                {
                    sv.Content.SizeChanged -= handler;
                    _ = sv.ScrollToAsync(0, lastOffset, false);
                };
                sv.Content.SizeChanged += handler;
#endif
            }
            SetContent(content);
        }

        private async Task GoUp()
        {
            if (!IsBackNavigationEnabled || _navigationStack.Count == 0)
                return;

            var content = _navigationStack.Last();
            if (!(await RaiseOnNavigatingAsync(content.Item1, NavigationDirection.Backward)))
            {
                return;
            }
            _navigationStack.Clear();
#if WINDOWS_XAML
            ContentTransitions = new TransitionCollection
            {
                new EntranceThemeTransition() { FromHorizontalOffset = 0, FromVerticalOffset = -200 }
            };
#endif
            SetContent(content.Item1);
        }

        /// <inheritdoc />
#if WINDOWS_XAML || MAUI
        protected override void OnApplyTemplate()
#elif WPF
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("NavigateBack") is Button backButton)
            {
#if MAUI
                backButton.Clicked += (s, e) => _ = GoBack();
#else
                backButton.Click += (s, e) => _ = GoBack();
#endif
            }
            if (GetTemplateChild("NavigateUp") is Button upButton)
            {
#if MAUI
                upButton.Clicked += (s, e) => _ = GoUp();
#else
                upButton.Click += (s, e) => _ = GoUp();
#endif
            }
            if (GetTemplateChild("Header") is ContentControl cc1)
#if MAUI
                cc1.ContentData = Content;
#else
                cc1.Content = Content;
#endif
            if (GetTemplateChild("Content") is ContentControl cc2)
#if MAUI
                cc2.ContentData = Content;
#else
                cc2.Content = Content;
#endif
            UpdateView();
            UpdateButtonEnableStates();
        }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public object? Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Content"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentProperty = PropertyHelper.CreateProperty<object, NavigationSubView>(nameof(Content), null);

        /// <summary>
        /// Gets or sets the template selector for the content.
        /// </summary>
        public DataTemplateSelector? ContentTemplateSelector
        {
            get { return GetValue(ContentTemplateSelectorProperty) as DataTemplateSelector; }
            set { SetValue(ContentTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ContentTemplateSelectorProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentTemplateSelectorProperty = PropertyHelper.CreateProperty<DataTemplateSelector, NavigationSubView>(nameof(ContentTemplateSelector), null);

        /// <summary>
        /// Gets or sets the template selector for the header.
        /// </summary>
        public DataTemplateSelector? HeaderTemplateSelector
        {
            get { return GetValue(HeaderTemplateSelectorProperty) as DataTemplateSelector; }
            set { SetValue(HeaderTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="HeaderTemplateSelectorProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateSelectorProperty = PropertyHelper.CreateProperty<DataTemplateSelector, NavigationSubView>(nameof(HeaderTemplateSelector), null);

        /// <summary>
        /// Gets or sets the vertical scrollbar visibility of the scrollviewer below the title.
        /// </summary>
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
            set => SetValue(VerticalScrollBarVisibilityProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="VerticalScrollBarVisibility"/> dependency property.
        /// </summary>
#if MAUI
        public static readonly BindableProperty VerticalScrollBarVisibilityProperty =
            BindableProperty.Create(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(NavigationSubView), ScrollBarVisibility.Default);
#else
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
            DependencyProperty.Register(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(NavigationSubView), new PropertyMetadata(ScrollBarVisibility.Auto));
#endif
        /// <summary>
        /// Gets or sets a value indicating whether the back buttons are enabled or not.
        /// </summary>
        public bool IsBackNavigationEnabled
        {
            get { return (bool)GetValue(IsBackNavigationEnabledProperty); }
            set { SetValue(IsBackNavigationEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsBackNavigationEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBackNavigationEnabledProperty =
            PropertyHelper.CreateProperty<bool, NavigationSubView>(nameof(IsBackNavigationEnabled), true, OnIsBackNavigationEnabledPropertyChanged);

        private static void OnIsBackNavigationEnabledPropertyChanged(NavigationSubView view, bool oldValue, bool newValue)
        {
            view.UpdateButtonEnableStates();
        }

        private void UpdateButtonEnableStates()
        {
            if (GetTemplateChild("NavigateBack") is Button back)
            {
                back.IsEnabled = IsBackNavigationEnabled;
            }
            if (GetTemplateChild("NavigateUp") is Button up)
            {
                up.IsEnabled = IsBackNavigationEnabled;
            }
        }
    }
}
﻿// /*******************************************************************************
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


using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping.FeatureForms;
using Esri.ArcGISRuntime.Toolkit.Internal;
using System.ComponentModel;
#if WPF || WINDOWS_XAML
using Esri.ArcGISRuntime.Toolkit.UI.Controls;
#elif MAUI
using Esri.ArcGISRuntime.Toolkit.Maui;
#endif


#if MAUI
namespace Esri.ArcGISRuntime.Toolkit.Maui.Primitives
#else
namespace Esri.ArcGISRuntime.Toolkit.Primitives
#endif
{
    /// <summary>
    /// Displays the list of Attachments in an <see cref="AttachmentsFormElement"/> object.
    /// </summary>
    public partial class AttachmentsFormElementView
    {
        private WeakEventListener<AttachmentsFormElementView, INotifyPropertyChanged, object?, PropertyChangedEventArgs>? _elementPropertyChangedListener;

        /// <summary>
        /// Initializes an instance of the <see cref="AttachmentsFormElementView"/> class.
        /// </summary>
        public AttachmentsFormElementView()
        {
#if MAUI
            ControlTemplate = DefaultControlTemplate;
#else
            DefaultStyleKey = typeof(AttachmentsFormElementView);
#endif
        }

        private void EvaluateExpressions()
        {
            var parent = FeatureFormView.GetFeatureFormViewParent(this);
            _ = parent?.EvaluateExpressions(parent?.FeatureForm);
        }

        /// <summary>
        /// Gets or sets the AttachmentsFormElement.
        /// </summary>
        public AttachmentsFormElement? Element
        {
            get { return (AttachmentsFormElement)GetValue(ElementProperty); }
            set { SetValue(ElementProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Element"/> dependency property.
        /// </summary>
#if MAUI
        public static readonly BindableProperty ElementProperty =
            BindableProperty.Create(nameof(Element), typeof(AttachmentsFormElement), typeof(AttachmentsFormElementView), null, propertyChanged: (s, oldValue, newValue) => ((AttachmentsFormElementView)s).OnElementPropertyChanged(oldValue as AttachmentsFormElement, newValue as AttachmentsFormElement));
#else
        public static readonly DependencyProperty ElementProperty =
            DependencyProperty.Register(nameof(Element), typeof(AttachmentsFormElement), typeof(AttachmentsFormElementView), new PropertyMetadata(null, (s, e) => ((AttachmentsFormElementView)s).OnElementPropertyChanged(e.OldValue as AttachmentsFormElement, e.NewValue as AttachmentsFormElement)));
#endif

        private async void OnElementPropertyChanged(AttachmentsFormElement? oldValue, AttachmentsFormElement? newValue)
        {
            if (oldValue is INotifyPropertyChanged inpcOld)
            {
                _elementPropertyChangedListener?.Detach();
                _elementPropertyChangedListener = null;
            }
            if (newValue is INotifyPropertyChanged inpcNew)
            {
                _elementPropertyChangedListener = new WeakEventListener<AttachmentsFormElementView, INotifyPropertyChanged, object?, PropertyChangedEventArgs>(this, inpcNew)
                {
                    OnEventAction = static (instance, source, eventArgs) => instance.Element_PropertyChanged(source, eventArgs),
                    OnDetachAction = static (instance, source, weakEventListener) => source.PropertyChanged -= weakEventListener.OnEvent,
                };
                inpcNew.PropertyChanged += _elementPropertyChangedListener.OnEvent;
            }
            UpdateVisibility();
            if (newValue != null)
            {
                try
                {
                    await newValue.FetchAttachmentsAsync();
                }
                catch (System.Exception) { }
            }
        }

        private void Element_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AttachmentsFormElement.IsVisible))
            {
                this.Dispatch(UpdateVisibility);
            }
        }

        private void UpdateVisibility()
        {
#if MAUI
            this.IsVisible = Element?.IsVisible == true;
#else
            this.Visibility = Element?.IsVisible == true ? Visibility.Visible : Visibility.Collapsed;
#endif
        }
    }
}

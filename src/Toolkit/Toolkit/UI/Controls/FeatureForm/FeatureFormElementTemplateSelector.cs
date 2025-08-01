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


#if MAUI
using Esri.ArcGISRuntime.Mapping.FeatureForms;
using Esri.ArcGISRuntime.Toolkit.Maui.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace Esri.ArcGISRuntime.Toolkit.Maui
{
    internal class FeatureFormElementTemplateSelector : DataTemplateSelector
    {
        private static DataTemplate DefaultFieldFormElementTemplate;
        private static DataTemplate DefaultGroupElementTemplate;
        private static DataTemplate DefaultTextFormElementTemplate;
        private static DataTemplate DefaultUtilityAssociationsFormElementTemplate;
        private static DataTemplate UnsupportedFormElementTemplate;

        [DynamicDependency(nameof(Esri.ArcGISRuntime.Mapping.FeatureForms.FormElement.IsVisible), "Esri.ArcGISRuntime.Mapping.FeatureForms.FormElement", "Esri.ArcGISRuntime")]
        [DynamicDependency(nameof(Esri.ArcGISRuntime.Mapping.FeatureForms.FormElement.Label), "Esri.ArcGISRuntime.Mapping.FeatureForms.FormElement", "Esri.ArcGISRuntime")]
        [DynamicDependency(nameof(Esri.ArcGISRuntime.Mapping.FeatureForms.FormElement.Description), "Esri.ArcGISRuntime.Mapping.FeatureForms.FormElement", "Esri.ArcGISRuntime")]
        [DynamicDependency(nameof(Esri.ArcGISRuntime.Mapping.FeatureForms.GroupFormElement.Elements), "Esri.ArcGISRuntime.Mapping.FeatureForms.GroupFormElement", "Esri.ArcGISRuntime")]
        static FeatureFormElementTemplateSelector()
        {
            DefaultFieldFormElementTemplate = new DataTemplate(() =>
            {
                var view = new FieldFormElementView() { Margin = new Thickness(0, 0, 0, 10) };
                view.SetBinding(FieldFormElementView.ElementProperty, Binding.SelfPath);
                view.SetBinding(FieldFormElementView.FeatureFormProperty, static (FeatureFormView view) => view.FeatureForm, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(FeatureFormView)));
                return view;
            });
            DefaultGroupElementTemplate = new DataTemplate(() =>
            {
                var view = new GroupFormElementView() { Margin = new Thickness(0, 0, 0, 10) };
                view.SetBinding(GroupFormElementView.ElementProperty, Binding.SelfPath);
                return view;
            });
            DefaultTextFormElementTemplate = new DataTemplate(() =>
            {
                var view = new TextFormElementView() { Margin = new Thickness(0, 0, 0, 10) };
                view.SetBinding(TextFormElementView.ElementProperty, Binding.SelfPath);
                return view;
            });

            DefaultUtilityAssociationsFormElementTemplate = new DataTemplate(() =>
            {
                var view = new UtilityAssociationsFormElementView() { Margin = new Thickness(0, 0, 0, 10) };
                view.SetBinding(UtilityAssociationsFormElementView.ElementProperty, Binding.SelfPath);
                return view;
            });
            UnsupportedFormElementTemplate = new DataTemplate(() =>
            {
                return new Grid() { IsVisible = false };
            });
        }

        public FeatureFormElementTemplateSelector()
        {
            FieldFormElementTemplate = DefaultFieldFormElementTemplate;
            GroupFormElementTemplate = DefaultGroupElementTemplate;
            TextFormElementTemplate = DefaultTextFormElementTemplate;
            UtilityAssociationsFormElementTemplate = DefaultUtilityAssociationsFormElementTemplate;
        }

        public DataTemplate FieldFormElementTemplate { get; set; }

        public DataTemplate GroupFormElementTemplate { get; set; }

        public DataTemplate TextFormElementTemplate { get; set; }

        public DataTemplate UtilityAssociationsFormElementTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is FieldFormElement)
            {
                return FieldFormElementTemplate;
            }
            else if(item is GroupFormElement)
            {
                return GroupFormElementTemplate;
            }
            else if (item is TextFormElement)
            {
                return TextFormElementTemplate;
            }
            else if (item is UtilityAssociationsFormElement)
            {
                return UtilityAssociationsFormElementTemplate;
            }
            return UnsupportedFormElementTemplate; // Renders empty / skips
        }
    }
}
#endif
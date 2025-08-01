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

#if MAUI
using Microsoft.Maui.Controls.Internals;
using Esri.ArcGISRuntime.Mapping.FeatureForms;
using System.Globalization;

namespace Esri.ArcGISRuntime.Toolkit.Maui.Primitives
{
    public partial class UtilityAssociationGroupResultView : TemplatedView
    {
        private static readonly ControlTemplate DefaultControlTemplate;

        static UtilityAssociationGroupResultView()
        {
            DefaultControlTemplate = new ControlTemplate(BuildDefaultTemplate);
        }

        private static object BuildDefaultTemplate()
        {
            var border = new Border() { StrokeThickness = 1, Margin = new Thickness(0, 4), VerticalOptions = LayoutOptions.Start };
            border.SetAppThemeColor(Border.StrokeProperty, Colors.Black, Colors.White);
            CollectionView cv = new CollectionView() { SelectionMode = SelectionMode.None };
            cv.SetBinding(CollectionView.ItemsSourceProperty, static (UtilityAssociationGroupResultView view) => view.GroupResult?.AssociationResults, source: RelativeBindingSource.TemplatedParent);
            cv.ItemTemplate = new DataTemplate(BuildDefaultItemTemplate);
            border.Content = cv;
            INameScope nameScope = new NameScope();
            NameScope.SetNameScope(border, nameScope);
            nameScope.RegisterName("ResultsList", cv);
            return border;
        }

        private static object BuildDefaultItemTemplate()
        {
            Grid layout = new Grid() { Padding = new Thickness(8, 0, 8, 0), MinimumHeightRequest = 40 };
            TapGestureRecognizer itemTapGesture = new TapGestureRecognizer();
            itemTapGesture.Tapped += Result_Tapped;
            layout.GestureRecognizers.Add(itemTapGesture);
            var view = new UtilityAssociationResultView();
            view.SetBinding(UtilityAssociationResultView.AssociationResultProperty, static (UtilityNetworks.UtilityAssociationResult result) => result);
            layout.Add(view);
            return layout;


        }
        private class FeatureDisplayNameConverter : IValueConverter
        {
            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if(value is Data.Feature feature)
                {
                    return new Mapping.Popups.Popup(feature, null).Title;
                }
                return value?.ToString();
            }

            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private static void Result_Tapped(object? sender, EventArgs e)
        {
            var cell = sender as View;
            if (cell?.BindingContext is UtilityNetworks.UtilityAssociationResult result)
            {
                var parent = FeatureFormView.GetFeatureFormViewParent(cell);
                parent?.NavigateToItem(new FeatureForm(result.AssociatedFeature)); 
            }
        }
    }
}
#endif
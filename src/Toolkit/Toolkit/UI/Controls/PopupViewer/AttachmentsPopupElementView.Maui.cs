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
using Microsoft.Maui.Controls.Internals;
using Esri.ArcGISRuntime.Mapping.Popups;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.InteropServices;
#if WINDOWS
using WinRT;
#endif

namespace Esri.ArcGISRuntime.Toolkit.Maui.Primitives
{
    /// <summary>
    /// Supporting control for the <see cref="Esri.ArcGISRuntime.Toolkit.Maui.PopupViewer"/> control,
    /// used for rendering a <see cref="AttachmentsPopupElement"/>.
    /// </summary>
    public partial class AttachmentsPopupElementView : TemplatedView
    {
        private static readonly ControlTemplate DefaultControlTemplate;

        /// <summary>
        /// Template name of the <see cref="CollectionView"/> attachment list.
        /// </summary>
        public const string AttachmentListName = "AttachmentList";

        static AttachmentsPopupElementView()
        {
            DefaultControlTemplate = new ControlTemplate(BuildDefaultTemplate);
        }

        private static object BuildDefaultTemplate()
        {
            StackLayout root = new StackLayout();
            Label roottitle = new Label();
            roottitle.SetBinding(Label.TextProperty, static (AttachmentsPopupElementView view) => view.Element?.Title, source: RelativeBindingSource.TemplatedParent);
            roottitle.SetBinding(VisualElement.IsVisibleProperty, static (AttachmentsPopupElementView view) => view.Element?.Title, source: RelativeBindingSource.TemplatedParent, converter: Internal.EmptyToFalseConverter.Instance);
            roottitle.Style = PopupViewer.GetPopupViewerTitleStyle();
            root.Add(roottitle);
            Label rootcaption = new Label();
            rootcaption.SetBinding(Label.TextProperty, static (AttachmentsPopupElementView view) => view.Element?.Description, source: RelativeBindingSource.TemplatedParent);
            rootcaption.SetBinding(VisualElement.IsVisibleProperty, static (AttachmentsPopupElementView view) => view.Element?.Description, source: RelativeBindingSource.TemplatedParent, converter: Internal.EmptyToFalseConverter.Instance);
            rootcaption.Style = PopupViewer.GetPopupViewerCaptionStyle();
            root.Add(rootcaption);
            root.Add(new Border() { StrokeThickness = 0, HeightRequest = 1, BackgroundColor = Colors.Gray, Margin = new Thickness(0,5) });
            CollectionView cv = new CollectionView() { SelectionMode = SelectionMode.None };
            cv.SetBinding(CollectionView.ItemsSourceProperty, static (AttachmentsPopupElementView view) => view.Element?.Attachments, source: RelativeBindingSource.TemplatedParent);
            cv.ItemTemplate = new DataTemplate(BuildDefaultItemTemplate);
            root.Add(cv);
            INameScope nameScope = new NameScope();
            NameScope.SetNameScope(root, nameScope);
            nameScope.RegisterName(AttachmentListName, cv);
            return root;
        }

        private static object BuildDefaultItemTemplate()
        {
            Grid layout = new Grid();
            TapGestureRecognizer itemTapGesture = new TapGestureRecognizer();
            itemTapGesture.Tapped += Attachment_Tapped;
            layout.GestureRecognizers.Add(itemTapGesture);
            layout.ColumnDefinitions.Add(new ColumnDefinition(30));
            layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            layout.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            layout.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            layout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            layout.SetBinding(Grid.BindingContextProperty, static (PopupAttachment attachment) => attachment, converter: AttachmentViewModelConverter.Instance);
            Image image = new Image() { WidthRequest = 24, HeightRequest = 24, HorizontalOptions = LayoutOptions.Start, Aspect = Aspect.Fill };
            layout.Add(image);
            image.SetBinding(Image.SourceProperty, static (AttachmentViewModel vm) => vm.Thumbnail);
            Grid.SetRowSpan(image, 2);

            Label name = new Label() { VerticalOptions = LayoutOptions.End };
            name.SetBinding(Label.TextProperty, static (AttachmentViewModel vm) => vm.Name);
            Grid.SetColumn(name, 1);
            layout.Add(name);

            Label size = new Label() { VerticalOptions = LayoutOptions.Start, TextColor = Colors.Gray, FontSize = 12 };
            size.SetBinding(Label.TextProperty, static (AttachmentViewModel vm) => vm.Size);
            Grid.SetColumn(size, 1);
            Grid.SetRow(size, 1);
            layout.Add(size);

            Image image2 = new Image() { WidthRequest = 18, HeightRequest = 18 };
            image2.SetBinding(Image.IsVisibleProperty, static (AttachmentViewModel vm) => vm.IsDownloadButtonVisible);
            image2.Source = new FontImageSource() { Glyph = ToolkitIcons.Inbox, Color = Colors.Gray, FontFamily = ToolkitIcons.FontFamilyName, Size = 18 };
            Grid.SetColumn(image2, 2);
            Grid.SetRowSpan(image2, 2);
            layout.Add(image2);

            ActivityIndicator indicator = new ActivityIndicator() { WidthRequest = 24, HeightRequest = 24, IsRunning = true };
            indicator.SetBinding(ActivityIndicator.IsRunningProperty, static (AttachmentViewModel vm) => vm.IsDownloading);
            Grid.SetColumn(indicator, 2);
            Grid.SetRowSpan(indicator, 2);
            layout.Add(indicator);
            var divider = new Border() { StrokeThickness = 0, HeightRequest = 1, BackgroundColor = Colors.Gray, Margin = new Thickness(0,5) };
            divider.VerticalOptions = LayoutOptions.End;
            Grid.SetRow(divider, 2);
            Grid.SetColumnSpan(divider, 3);
            layout.Add(divider);
            Border root = new Border() { StrokeThickness = 0, Content = layout };
            return root;
        }

        private static void Attachment_Tapped(object? sender, EventArgs e)
        {
            var cell = sender as View;
            Element? parent = cell?.Parent;
            while(parent is View && parent is not AttachmentsPopupElementView)
            {
                parent = parent.Parent;
            }
            if(parent is AttachmentsPopupElementView a && cell?.BindingContext is AttachmentViewModel vm)
            {
                a.OnAttachmentClicked(vm.Attachment);
            }
        }

        private PopupViewer? GetPopupViewerParent()
        {
            var parent = this.Parent;
            while(parent is not null && parent is not PopupViewer popup)
            {
                parent = parent.Parent;
            }
            return parent as PopupViewer;
        }

        internal class AttachmentViewModel : System.ComponentModel.INotifyPropertyChanged
        {
            public AttachmentViewModel(PopupAttachment attachment)
            {
                Attachment = attachment;
                Attachment.PropertyChanged += Attachment_PropertyChanged;
            }

            public PopupAttachment Attachment { get; }

            public string Name => Attachment.Name;

            public string Size
            {
                get
                {
                    var size = Attachment.Size;
                    if (size < 1024)
                        return $"{size} B";
                    else if (size < 1024 * 1024)
                        return $"{size / 1024} kB";
                    else
                        return $"{Math.Round(size / 1024d / 1024d, 1)} MB";
                }
            }

            private void Attachment_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(PopupAttachment.LoadStatus))
                {
                    OnPropertyChanged(nameof(IsDownloadButtonVisible));
                    OnPropertyChanged(nameof(IsDownloading));
                    if (Attachment.LoadStatus == LoadStatus.Loaded)
                    {
                        CreateThumbnail();
                    }
                }
            }

            public void Load()
            {
                _ = Attachment?.LoadAsync();
            }

            private ImageSource? _thumbnail;

            public ImageSource? Thumbnail
            {
                get
                {
                    if (_thumbnail is null)
                        CreateThumbnail();
                    return _thumbnail;
                }
            }

            public bool IsDownloadButtonVisible => !Attachment.IsLocal && Attachment.LoadStatus != LoadStatus.Loaded && Attachment.LoadStatus != LoadStatus.Loading;

            public bool IsDownloading => !Attachment.IsLocal && Attachment.LoadStatus == LoadStatus.Loading;

            private void CreateThumbnail()
            {
                if (Attachment.IsLocal || Attachment.LoadStatus == LoadStatus.Loaded)
                {
                    _thumbnail = ImageSource.FromStream(async (token) =>
                    {
                        var img = await Attachment.CreateThumbnailAsync(40, 40);
                        return await img!.GetEncodedBufferAsync(token);
                    });
                }
                else
                {
                    _thumbnail = new FontImageSource() { Glyph = ContentTypeToCalciteGlyph(Attachment.ContentType).ToString(), Color = Colors.Gray, FontFamily = ToolkitIcons.FontFamilyName, Size = 18 };
                }
                OnPropertyChanged(nameof(Thumbnail));
            }

            private static string ContentTypeToCalciteGlyph(string contentType)
            {
                contentType = contentType.ToLowerInvariant();
                if (contentType.StartsWith("image/"))
                    return ToolkitIcons.Image;
                if (contentType.StartsWith("video/"))
                    return ToolkitIcons.VideoWeb;
                if (contentType.StartsWith("audio/"))
                    return ToolkitIcons.FileSound;
                switch (contentType)
                {
                    case "application/pdf": return ToolkitIcons.FilePdf;
                    case "text/csv":
                    case "application/vnd.ms-excel": return ToolkitIcons.FileCsv;
                    case "application/msword":
                    case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                        return ToolkitIcons.FileWord;
                    default: return ToolkitIcons.File;
                }
            }

            private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        private class AttachmentViewModelConverter : IValueConverter
        {
            internal static AttachmentViewModelConverter Instance { get; } = new AttachmentViewModelConverter();
            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (value is PopupAttachment attachment)
                    return new AttachmentViewModel(attachment);
                return value;
            }

            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
        }
    }
}
#endif

﻿using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Toolkit.Maui;

namespace Toolkit.SampleApp.Maui.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [SampleInfo(Category = "BasemapGallery", Description = "Sceneview Basemap Appearance customization sample", ApiKeyRequired = true)]
    public partial class BasemapGallerySceneViewAppearanceSample : ContentPage
    {
        public BasemapGallerySceneViewAppearanceSample()
        {
            InitializeComponent();
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISImagery);
        }

        private async void Button_Add_Item(object? sender, EventArgs e)
        {
            if (Gallery.AvailableBasemaps is null) return;
            BasemapGalleryItem item = await BasemapGalleryItem.CreateAsync(new Basemap());
            item.Name = "With Thumbnail";
            item.Tooltip = Guid.NewGuid().ToString();
            item.Thumbnail = new Esri.ArcGISRuntime.UI.RuntimeImage(new Uri("https://www.esri.com/content/dam/esrisites/en-us/home/homepage-tile-arcgis-collaboration.jpg"));
            Gallery.AvailableBasemaps.Add(item);

            BasemapGalleryItem item2 = await BasemapGalleryItem.CreateAsync(new Basemap());
            item2.Name = "Without Thumbnail";
            Gallery.AvailableBasemaps.Add(item2);
        }

        private void Button_Remove_Item(object? sender, EventArgs e)
        {
            if (Gallery.AvailableBasemaps?.Any() == true)
            {
                Gallery.AvailableBasemaps.RemoveAt(Gallery.AvailableBasemaps.Count - 1);
            }
        }
    }
}
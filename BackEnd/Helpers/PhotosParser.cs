// ********************************************************
// Copyright (C) 2022 Louis S. Berman (louis@squideyes.com)
//
// This file is part of JonesRovers
//
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Common;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BackEnd
{
    public static class PhotosParser
    {
        private class Root
        {
            public PhotoInfo[] Photos { get; init; }
        }

        private class PhotoInfo
        {
            public int Id { get; init; }
            public int Sol { get; init; }
            public CameraInfo Camera { get; init; }
            public string Img_Src { get; init; }
            public string Earth_Date { get; init; }
        }

        private class CameraInfo
        {
            public string Name { get; init; }
        }

        public static List<Photo> GetPhotos(Manifest manifest, string json)
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };

            var root = JsonSerializer.Deserialize<Root>(json, options);

            var photos = new List<Photo>();

            foreach (var photoInfo in root.Photos)
            {
                photos.Add(new Photo()
                {
                    PhotoId = photoInfo.Id,
                    Manifest = manifest,
                    Sol = photoInfo.Sol,
                    ImageUri = new Uri(photoInfo.Img_Src),
                    Camera = photoInfo.Camera.Name.ToCamera()
                });
            }

            return photos;
        }
    }
}
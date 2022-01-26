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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace BackEnd
{
    public class PhotosClient
    {
        private const string BASE_URL = "https://api.nasa.gov/mars-photos/api/v1/";

        private readonly HttpClient client = new();

        private readonly string key;

        public PhotosClient(string key)
        {
            this.key = key ?? throw new ArgumentOutOfRangeException(nameof(key));
        }

        public async Task<Stream> GetPhotoStreamAsync(Photo photo)
        {
            if (photo == null)
                throw new ArgumentNullException(nameof(photo));

            return await client.GetStreamAsync(photo.ImageUri);
        }

        public async Task<List<Manifest>> GetManifestsAsync(
            Rover? roverFilter = null, DateOnly? minEarthDate = null,
            DateOnly? maxEarthDate = null)
        {
            async Task<List<Manifest>> GetManifests(Rover rover)
            {
                var url = new StringBuilder();

                url.Append(BASE_URL + "manifests/");
                url.Append(rover.ToString().ToLower());
                url.Append("?api_key=");
                url.Append(key);

                var json = await client.GetStringAsync(url.ToString());

                return ManifestParser.Parse(json);
            }

            var manifests = new ConcurrentBag<Manifest>();

            var fetcher = new TransformManyBlock<Rover, Manifest>(
                async rover =>
                {
                    var manifests = await GetManifests(rover);

                    var minDate = DateOnly.MinValue;
                    var maxDate = DateOnly.MaxValue;

                    if (minEarthDate.HasValue)
                        minDate = minEarthDate.Value;

                    if (maxEarthDate.HasValue)
                        maxDate = maxEarthDate.Value;

                    return manifests.Where(m =>
                        m.Date.Within(minDate, maxDate)).ToList();
                },
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = 3
                });

            var persister = new ActionBlock<Manifest>(
                manifest =>
                {
                    manifests.Add(manifest);
                },
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                }
            );

            fetcher.LinkTo(persister);

            if (roverFilter == null)
            {
                fetcher.Post(Rover.Curiosity);
                fetcher.Post(Rover.Opportunity);
                fetcher.Post(Rover.Spirit);
            }
            else
            {
                fetcher.Post(roverFilter.Value);
            }

            fetcher.Complete();

            await fetcher.Completion;

            return manifests.ToList();
        }

        public async Task<List<Photo>> GetPhotosAsync(
            Manifest manifest, Camera? camera = null)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            var url = new StringBuilder();

            url.Append(BASE_URL + "rovers/");
            url.Append(manifest.Rover.ToString().ToLower());
            url.Append("/photos?earth_date=");
            url.Append(manifest.Date.ToString("yyyy-MM-dd"));

            if (camera.HasValue)
                url.Append($"&camera={camera.Value.ToCode()}");

            url.Append($"&api_key={key}");

            var json = await client.GetStringAsync(url.ToString());

            return PhotosParser.GetPhotos(manifest, json);
        }
    }
}
// ********************************************************
// Copyright (C) 2022 Louis S. Berman (louis@squideyes.com)
//
// This file is part of JonesRovers
//
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace Common;

public sealed class CosmosJsonSerializer : CosmosSerializer
{
    private readonly JsonObjectSerializer serializer;

    public CosmosJsonSerializer(JsonSerializerOptions options)
    {
        serializer = new JsonObjectSerializer(options);
    }

    public override T FromStream<T>(Stream stream)
    {
        if (stream.CanSeek && stream.Length == 0)
            return default!;

        if (typeof(Stream).IsAssignableFrom(typeof(T)))
            return (T)(object)stream;

        using (stream)
            return (T)serializer.Deserialize(stream, typeof(T), default)!;
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();

        serializer.Serialize(stream, input, typeof(T), default);

        stream.Position = 0;

        return stream;
    }
}
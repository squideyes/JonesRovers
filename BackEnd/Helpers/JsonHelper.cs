// ********************************************************
// Copyright (C) 2022 Louis S. Berman (louis@squideyes.com)
//
// This file is part of JonesRovers
//
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackEnd;

internal static class JsonHelper
{
    private static JsonSerializerOptions options = null;

    public static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return options ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            Converters =
                {
                    new JsonStringEnumConverter(),
                    new JsonStringDateOnlyConverter()
                },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}
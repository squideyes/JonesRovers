// ********************************************************
// Copyright (C) 2022 Louis S. Berman (louis@squideyes.com)
//
// This file is part of JonesRovers
//
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using System.Text;

namespace Common;

public class Photo
{
    public int PhotoId { get; init; }
    public Rover Rover { get; init; }
    public DateOnly Date { get; init; }
    public Camera Camera { get; init; }
    public Uri? ImageUri { get; init; }

    public string GetFileName()
    {
        var sb = new StringBuilder();

        sb.Append(Rover.ToString().ToUpper());
        sb.Append('-');
        sb.Append(Date.ToString("yyyyMMdd"));
        sb.Append('-');
        sb.Append(Camera.ToCode());
        sb.Append('-');
        sb.Append(PhotoId.ToString("00000000"));
        sb.Append(Path.GetExtension(ImageUri?.AbsoluteUri));

        return sb.ToString();
    }

    public string GetBlobName()
    {
        var sb = new StringBuilder();

        sb.Append(Rover.ToString().ToUpper());
        sb.Append('/');
        sb.Append(Date.ToString("yyyy"));
        sb.Append('/');
        sb.Append(Camera.ToCode());
        sb.Append('/');
        sb.Append(GetFileName());

        return sb.ToString();
    }
}
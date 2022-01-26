// ********************************************************
// Copyright (C) 2022 Louis S. Berman (louis@squideyes.com)
//
// This file is part of JonesRovers
//
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

namespace Common;

public class Manifest
{
    public Rover Rover { get; init; }
    public DateOnly Date { get; init; }
    public int Photos { get; init; }
    public List<Camera>? Cameras { get; init; }

    public string Id => GetId(Rover, Date);

    public static string GetDateString(DateOnly date) => 
        date.ToString("yyyyMMdd");

    public override string ToString() => Id;

    public static string GetId(Rover rover, DateOnly date)=>
        $"{rover}_{GetDateString(date)}";
}
/*  Copyright (C) 2025 Piotr Kniaz

    This file is part of HBDPG-2.
    Repository: https://github.com/HBDPG-2/hbdpg-2-tui

    Licensed under the MIT License. See LICENSE file in the project root for details.
*/

namespace HBDPG2.Core;

public struct Result(string? password, double entropy, double elapsedTime, int attempt)
{
    public readonly string? Password { get => password; }
    public readonly double Entropy { get => entropy; }
    public readonly double ElapsedTime { get => elapsedTime; }
    public readonly int Attempt { get => attempt; }
}
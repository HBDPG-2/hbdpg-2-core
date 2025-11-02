/*  Copyright (C) 2025 Piotr Kniaz

    This file is part of HBDPG-2.
    Repository: https://github.com/HBDPG-2/hbdpg-2-tui

    Licensed under the MIT License. See LICENSE file in the project root for details.
*/

namespace HBDPG2.Core.Interfaces;

internal interface IGenerator
{
    Result GeneratePassword(char[]? passhrase1, char[]? passhrase2);
    Result GeneratePassword(char[]? passhrase1, char[]? passhrase2, int passwordLength);
    Result GeneratePassword(char[]? passhrase1, char[]? passhrase2, char[,]? customSymbols);
    Result GeneratePassword(char[]? passhrase1, char[]? passhrase2, int passwordLength, char[,]? customSymbols);
    bool ValidateCCT(char[,] symbols);
}
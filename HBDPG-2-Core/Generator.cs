/*  Copyright (C) 2025 Piotr Kniaz

    This file is part of HBDPG-2.
    Repository: https://github.com/HBDPG-2/hbdpg-2-tui

    Licensed under the MIT License. See LICENSE file in the project root for details.
*/

using HBDPG2.Core.Implementations;
using HBDPG2.Core.Interfaces;

namespace HBDPG2.Core;

public class Generator(SpecificationVersion specificationVersion)
{
    public async Task<Result> GeneratePassword()
    {
        return _generator.GeneratePassword(Passphrase1, Passphrase2, PasswordLength, CustomSymbols);
    }

    public string Passphrase1 { private get; set; } = string.Empty;
    public string Passphrase2 { private get; set; } = string.Empty;
    public int PasswordLength { private get; set; } = 32;
    public char[,]? CustomSymbols
    {
        private get => _customSymbols;
        set
        {
            if (value is null || (value is not null && _generator.ValidateCCT(value)))
            {
                _customSymbols = value;
            }
            else
            {
                throw new ArgumentException("Custom Character Table is invalid.");
            }
        }
    }
    public SpecificationVersion SpecificationVersion
    {
        get => _specificationVersion;
    }

    private char[,]? _customSymbols = null;
    private readonly IGenerator _generator = specificationVersion switch
    {
        SpecificationVersion.V10 => new V10(),
        _ => throw new NotSupportedException("The specified specification version is not supported.")
    };
    private readonly SpecificationVersion _specificationVersion = specificationVersion;
}
/*  Copyright (C) 2025 Piotr Kniaz

    This file is part of HBDPG-2.
    Repository: https://github.com/HBDPG-2/hbdpg-2-tui

    Licensed under the MIT License. See LICENSE file in the project root for details.
*/

// Core version: 1.0-beta

using Konscious.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using HBDPG2.Core.Interfaces;

namespace HBDPG2.Core.Implementations;

internal class V10 : IGenerator
{
    public Result GeneratePassword(char[]? passphrase1, char[]? passphrase2, int passwordLength, char[,]? customSymbols)
    {
        ValidateInputs(passphrase1, passphrase2, passwordLength, customSymbols);

        Stopwatch stopwatch = Stopwatch.StartNew();

        char[]? password = null;
        double entropy = 0;
        double elapsedTime;
        int attempt = 0;

        byte[]? hash = null;
        byte[]? nibbles = null;

        try
        {
            hash = GetHash(passphrase1!, passphrase2!, passwordLength);
            nibbles = GetNibbles(hash);

            for (; attempt < 16; attempt++)
            {
                byte[]? indexes = null;

                try
                {
                    indexes = GetIndexes(nibbles, attempt);
                    password = GetCharacters(indexes, customSymbols ?? _symbols);

                    if (CheckResult(password, ref entropy))
                    {
                        break;
                    }
                    else
                    {
                        if (password is not null)
                        {
                            Array.Clear(password);
                        }

                        password = null;
                        entropy = 0;
                    }
                }
                finally
                {
                    if (indexes is not null)
                    {
                        Array.Clear(indexes);
                    }
                }
            }
        }
        finally
        {
            if (hash is not null)
            {
                Array.Clear(hash);
            }

            if (nibbles is not null)
            {
                Array.Clear(nibbles);
            }
        }

        stopwatch.Stop();
        elapsedTime = (double)stopwatch.ElapsedMilliseconds / 1000;

        return new Result(password, entropy, elapsedTime, attempt);
    }

    public Result GeneratePassword(char[]? passphrase1, char[]? passphrase2) =>
        GeneratePassword(passphrase1, passphrase2, 32, null);

    public Result GeneratePassword(char[]? passphrase1, char[]? passphrase2, int passwordLength) =>
        GeneratePassword(passphrase1, passphrase2, passwordLength, null);

    public Result GeneratePassword(char[]? passphrase1, char[]? passphrase2, char[,]? customSymbols) =>
        GeneratePassword(passphrase1, passphrase2, 32, customSymbols);

    public bool ValidateCCT(char[,] symbols)
    {
        // ...
        return true;
    }
    
    private bool ValidateInputs(char[]? passphrase1, char[]? passphrase2, int passwordLength, char[,]? customSymbols)
    {
        if (passphrase1 is null || passphrase1.Length < 8)
        {
            throw new ArgumentException("Passphrase 1 must be at least 8 characters long.");
        }

        if (passphrase2 is null || passphrase2.Length < 8)
        {
            throw new ArgumentException("Passphrase 2 must be at least 8 characters long.");
        }

        if (customSymbols is not null && !ValidateCCT(customSymbols))
        {
            throw new ArgumentException("Custom Character Table is invalid.");
        }

        if (passwordLength < 16 || passwordLength > 64)
        {
            throw new ArgumentOutOfRangeException(nameof(passwordLength), "Password length must be between 16 and 64.");
        }

        return true;
    }

    private static byte[] GetHash(char[] passphrase1, char[] passphrase2, int passwordLength)
    {
        byte[]? passphrase1Bytes = null,
                passphrase2Bytes = null;

        try
        {
            passphrase1Bytes = Encoding.UTF8.GetBytes(passphrase1);
            passphrase2Bytes = Encoding.UTF8.GetBytes(passphrase2);

            using Argon2id hasher = new(passphrase1Bytes)
            {
                Salt = passphrase2Bytes,
                DegreeOfParallelism = 24,
                MemorySize = 256000,
                Iterations = 48
            };

            return hasher.GetBytes(passwordLength * 16);
        }
        finally
        {
            if (passphrase1Bytes is not null)
            {
                Array.Clear(passphrase1Bytes);
            }

            if (passphrase2Bytes is not null)
            {
                Array.Clear(passphrase2Bytes);
            }
        }
    }

    private static byte[] GetNibbles(byte[] bytes)
    {
        byte[] nibbles = new byte[bytes.Length * 2];

        for (int i = 0; i < bytes.Length; i++)
        {
            nibbles[i * 2] = (byte)(bytes[i] >> 4);
            nibbles[i * 2 + 1] = (byte)(bytes[i] & 0x0F);
        }

        return nibbles;
    }

    private static byte[] GetIndexes(byte[] nibbles, int shift)
    {
        const int blockSize = 16;

        byte[] indexes = new byte[nibbles.Length / 16];

        for (int i = 0 + shift, j = 0; i < nibbles.Length; i += blockSize, j++)
        {
            int blockSum = 0;

            for (int y = i; y < i + blockSize; y++)
            {
                if (y < nibbles.Length)
                {
                    blockSum += nibbles[y];
                }
                else
                {
                    blockSum += nibbles[y - nibbles.Length];
                }
            }

            int blockShift = blockSum % blockSize;

            if (i + blockShift < nibbles.Length)
            {
                indexes[j] = nibbles[i + blockShift];
            }
            else
            {
                indexes[j] = nibbles[i + blockShift - nibbles.Length];
            }
        }

        return indexes;
    }

    private static char[] GetCharacters(byte[] indexes, char[,] symbols)
    {
        char[] characters = new char[indexes.Length / 2];

        for (int i = 0; i < indexes.Length; i += 2)
        {
            characters[i / 2] = symbols[indexes[i], indexes[i + 1]];
        }

        return characters;
    }

    private static bool CheckResult(char[] password, ref double entropy)
    {
        HashSet<char> uniqueChars = [];
        int upperCaseCount = 0;
        int lowerCaseCount = 0;
        int digitCount = 0;
        int specialCharCount = 0;

        const int minUpperCaseCount = 2;
        const int minLowerCaseCount = 2;
        const int minDigitCount = 2;
        const int minSpecialCharCount = 2;
        double minEntropy;

        switch (password.Length)
        {
            case < 32:
                minEntropy = 60;
                break;
            case < 64:
                minEntropy = 140;
                break;
            case >= 64:
                minEntropy = 340;
                break;
        }

        foreach (char c in password)
        {
            uniqueChars.Add(c);

            switch (c)
            {
                case >= 'A' and <= 'Z':
                    upperCaseCount++;
                    break;
                case >= 'a' and <= 'z':
                    lowerCaseCount++;
                    break;
                case >= '0' and <= '9':
                    digitCount++;
                    break;
                default:
                    specialCharCount++;
                    break;
            }
        }

        entropy = password.Length * Math.Log2(uniqueChars.Count);

        if (upperCaseCount < minUpperCaseCount || lowerCaseCount < minLowerCaseCount
            || digitCount < minDigitCount || specialCharCount < minSpecialCharCount || entropy < minEntropy)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private static readonly char[,] _symbols = { { '3', 'U', 'I', '6', 'g', '9', '1', '8', 'n', 'W', '}', '2', '5', '\\', 'T', '}' },
                                                 { '5', '3', '2', 'V', '7', 'X', '2', '1', '3', '7', ',', '9', '_', '7', 'b', 'F' },
                                                 { 'H', '1', 'n', '5', '6', 'Q', 'C', ':', '+', '[', 'K', '0', 'e', '1', 'm', '0' },
                                                 { 'f', 'E', '/', 'e', '0', 'C', 'S', 'E', ',', '{', 'i', '\\', 'h', '%', 'G', 'z' },
                                                 { 'q', '9', '3', '4', 'B', 'B', '*', 'U', 'l', '5', '[', '0', 'R', 'D', '$', '8' },
                                                 { '8', '3', '8', '0', '9', 'p', '#', '4', '1', '0', '9', 'y', '+', '9', '2', '7' },
                                                 { '7', '7', '7', 'o', 'x', 'j', '.', '5', '#', 'A', '{', ';', 'h', '-', '1', '7' },
                                                 { 'M', 'X', 'O', '2', '2', '5', '6', '4', '1', 'J', '9', '&', 'y', '8', 'Y', 'o' },
                                                 { ')', '"', ']', '1', 'l', '4', '6', '0', '?', 'T', '$', 'F', 'd', '5', ';', 'I' },
                                                 { 'M', '6', 'c', 'Y', 'P', '5', '2', '2', '@', 'k', '%', 's', '&', '0', '|', 'r' },
                                                 { '|', '6', '^', 't', 'v', '6', 'k', '3', 'O', '9', 'x', '=', 'G', 'r', '0', '!' },
                                                 { '_', 'A', '8', '!', '^', '3', 'v', '4', 'W', '6', '\'', '3', 'D', '=', 'p', 'P' },
                                                 { 'a', '4', '\'', 'b', '4', '4', '-', 'j', '9', 'g', ':', 'J', '8', '(', 'u', '<' },
                                                 { 'Q', 'u', 'd', '>', 'w', 'Z', '>', 'S', 'w', ']', '1', '5', '<', '8', '7', 'a' },
                                                 { '3', '~', 'V', '.', '4', '*', 'R', '(', '?', 't', 'f', 'c', '8', 'K', 'm', ')' },
                                                 { 'N', 'Z', '@', 'z', '/', 's', '2', 'H', 'L', 'L', '~', 'N', '"', 'i', '6', 'q' } };
}
/*  Copyright (C) 2025 Piotr Kniaz

    This file is part of HBDPG-2.
    Repository: https://github.com/HBDPG-2/hbdpg-2-core

    Licensed under the MIT License. See LICENSE file in the project root for details.
*/

namespace HBDPG2.Core;

public class Result(char[]? password, double entropy, double elapsedTime, int attempt) : IDisposable
{
    public char[]? Password { get => password; private set => password = value; }
    public double Entropy { get => entropy; }
    public double ElapsedTime { get => elapsedTime; }
    public int Attempt { get => attempt; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (Password is not null)
                {
                    Array.Clear(Password);
                    Password = null;
                }
            }
            
            _disposed = true;
        }
    }

    private bool _disposed = false;

    ~Result()
    {
        Dispose(false);
    }
}
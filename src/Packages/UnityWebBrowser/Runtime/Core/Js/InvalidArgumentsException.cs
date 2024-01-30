﻿// UnityWebBrowser (UWB)
// Copyright (c) 2021-2024 Voltstro-Studios
// 
// This project is under the MIT license. See the LICENSE.md file for more details.

using System;

namespace VoltstroStudios.UnityWebBrowser.Core.Js
{
    public sealed class InvalidArgumentsException : Exception
    {
        public InvalidArgumentsException(string message) : base(message)
        {
        }
    }
}
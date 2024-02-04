﻿using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gubon.Gateway.Store.FreeSql
{
    public class FreeSqlReloadToken : IChangeToken
    {
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        public void OnReload() => tokenSource.Cancel();
        public bool ActiveChangeCallbacks { get; } = true;

        public bool HasChanged { get { return tokenSource.IsCancellationRequested; } }

        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
        {
            return tokenSource.Token.Register(callback, state);
        }
    }
}

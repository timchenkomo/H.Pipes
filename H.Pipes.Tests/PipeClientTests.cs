﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Pipes.Tests
{
    [TestClass]
    public class PipeClientTests
    {
        [TestMethod]
        public async Task ConnectTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await using var client = new PipeClient<string>("this_pipe_100%_is_not_exists");

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await client.ConnectAsync(cancellationTokenSource.Token));
        }
    }
}

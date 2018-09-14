using nng.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    using static nng.Tests.Util;

    [Collection("nng")]
    public class BusTests
    {
        IAPIFactory<IMessage> factory;

        public BusTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task Basic(string url)
        {
            using (var bus0 = factory.BusCreate(url, true))
            {
                await WaitReady();
                using (var bus1 = factory.BusCreate(url, false))
                {

                }
            }

            // Manually create listener/dialer
            using (var bus0 = factory.BusOpen())
            using (var listener0 = factory.ListenerCreate(bus0, url))
            {
                await WaitReady();
                using (var bus1 = factory.BusOpen())
                using (var dialer1 = factory.DialerCreate(bus1, url))
                {

                }
            }
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task Advanced(string url)
        {
            var readyToDial = new AsyncBarrier(3);
            var readyToSend = new AsyncBarrier(3);
            var messageReceipt = new AsyncCountdownEvent(2);
            var bus0Task = Task.Run(async () => {
                using (var bus = factory.BusCreate(url, true))
                using (var ctx = bus.CreateAsyncContext(factory))
                {
                    await readyToDial.SignalAndWait();
                    await readyToSend.SignalAndWait();
                    await ctx.Send(factory.CreateMessage());
                }
            });
            var bus1Task = Task.Run(async () => {
                await readyToDial.SignalAndWait();
                using (var bus = factory.BusCreate(url, false))
                using (var ctx = bus.CreateAsyncContext(factory))
                {
                    await readyToSend.SignalAndWait();
                    var _ = await ctx.Receive(CancellationToken.None);
                    messageReceipt.Signal();
                }
            });
            var bus2Task = Task.Run(async () => {
                await readyToDial.SignalAndWait();
                using (var bus = factory.BusCreate(url, false))
                using (var ctx = bus.CreateAsyncContext(factory))
                {
                    await readyToSend.SignalAndWait();
                    var _ = await ctx.Receive(CancellationToken.None);
                    messageReceipt.Signal();
                }
            });
            await AssertWait(2000, messageReceipt.WaitAsync(), bus0Task, bus1Task, bus2Task);
        }
    }
}
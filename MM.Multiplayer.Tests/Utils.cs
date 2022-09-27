using Lidgren.Network;
using System.Diagnostics;

namespace MM.Multiplayer.Tests;

internal static class Utils
{
    private static readonly Random random = new Random();

    public static void AlwaysAccept(NetIncomingMessage msg, out DummyPlayer player)
    {
        msg.SenderConnection.Approve();
        player = new DummyPlayer(msg.SenderConnection, $"Player, ID: {random.Next()}");
    }

    public static void AssertWithTimeout(int timeoutMs, Func<bool> check, string failMessage)
    {
        var watch = new Stopwatch();
        watch.Start();
        while (true)
        {
            if (check())
                break;

            if (watch.Elapsed.TotalMilliseconds > timeoutMs)
            {
                Assert.Fail(failMessage);
                return;
            }

            Thread.Sleep(1);
        }
    }
}

﻿using Lidgren.Network;
using MM.Multiplayer.Internal;
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

    public static HostScope StartLocalHost(out GameClient client, out GameServer<DummyPlayer> server)
    {
        client = new GameClient(new NetPeerConfiguration("TEST"));
        server = new GameServer<DummyPlayer>(new NetPeerConfiguration("TEST")
        {
            Port = new Random().Next(5000, 10000)
        }, AlwaysAccept);

        static void Init(ObjectTracker tracker) => tracker.RegisterType<DummyObj>();

        Init(client.ObjectTracker);
        Init(server.ObjectTracker);

        server.Start();
        client.Start();

        var c = client;
        var s = server;

        var scope = new HostScope();
        new Thread(() => TickClientAndServer(c, s, scope)).Start();

        AssertWithTimeout(1000, () => s.Status == NetPeerStatus.Running, "Server failed to start.");

        client.Connect("localhost", server.Configuration.Port);
        AssertWithTimeout(2000, () => c.ConnectionStatus == NetConnectionStatus.Connected, "Client failed to connect.");

        Assert.Equal(1, server.ConnectionsCount);

        return scope;
    }

    private static void TickClientAndServer(GameClient client, GameServer<DummyPlayer> server, HostScope scope)
    {
        while (scope.IsRunning)
        {
            server.Tick();
            client.Tick();
            Net.Tick++;
            Thread.Sleep(16);
        }
    }

    public class HostScope : IDisposable
    {
        public bool IsRunning { get; private set; } = true;

        public void Dispose()
        {
            IsRunning = false;
        }
    }
}

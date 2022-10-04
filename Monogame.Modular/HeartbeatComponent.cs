using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace MM.Core;

public class HeartbeatComponent : GameComponent
{
    public class Heartbeat
    {
        public double Interval = 1.0;
        public event Action Beat;

        internal double Accumulator;

        internal void Call()
        {
            Beat?.Invoke();
        }
    }

    private readonly List<Heartbeat> heartbeats = new List<Heartbeat>();
    private readonly Stopwatch watch = new Stopwatch();

    public HeartbeatComponent(Game game) : base(game) { }

    public override void Initialize()
    {
        base.Initialize();
        watch.Start();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        foreach (var h in heartbeats)
        {
            h.Accumulator += watch.Elapsed.TotalSeconds;
            if (h.Accumulator >= h.Interval)
            {
                h.Call();
                h.Accumulator -= h.Interval;
            }
        }

        watch.Restart();
    }

    public Heartbeat Add(Action beat, double interval)
    {
        if (beat == null)
            throw new ArgumentNullException(nameof(beat));

        var h = new Heartbeat
        {
            Interval = interval
        };
        h.Beat += beat;

        return Add(h);
    }

    public Heartbeat Add(Heartbeat h)
    {
        if (h == null)
            return null;

        Debug.Assert(!heartbeats.Contains(h), "Already registered that heartbeat");
        heartbeats.Add(h);
        return h;
    }
}

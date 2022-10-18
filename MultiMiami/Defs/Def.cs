﻿using MM.Define;

namespace MultiMiami.Defs
{
    public abstract class Def : IDef
    {
        public string ID { get; set; }

        public string Label;

        public virtual void PostLoad()
        {
        }

        public virtual void LatePostLoad()
        {
        }

        public virtual void ConfigErrors(ConfigErrorReporter config)
        {
        }

        public override string ToString() => $"[{ID}] '{Label ?? "no-name"}'";
    }
}

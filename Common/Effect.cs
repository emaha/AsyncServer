using System;

namespace Common
{
    public enum EffectType { POSITIVE, NEGATIVE }

    public abstract class Effect
    {
        public EffectType EffectType { get; set; }

        protected Effect()
        {
        }

        public void Tick(Character target)
        {
            Console.WriteLine("Effect tick");
        }
    }

    public class AreaOfEffect
    {
        public void Tick()
        {
        }
    }
}
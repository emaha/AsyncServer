using System;

namespace Common
{
    public abstract class Effect
    {
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
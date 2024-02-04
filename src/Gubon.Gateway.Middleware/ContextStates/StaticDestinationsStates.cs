using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Model;
using static Gubon.Gateway.Middleware.ContextStates.RecordDesinationsStates;

namespace Gubon.Gateway.Middleware.ContextStates
{
    public class StaticDestinationsStates
    {
        public readonly DateTime StartDateTime = DateTime.Now;
        public StaticDestinationsStates() { }
        public static StaticDestinationsStates current = new StaticDestinationsStates();
        public AtomicCounter counter_all { get; } = new AtomicCounter();
        public AtomicCounter counter_2xx { get; } = new AtomicCounter();
        public AtomicCounter counter_3xx { get; } = new AtomicCounter();
        public AtomicCounter counter_4xx { get; } = new AtomicCounter();
        public AtomicCounter counter_5xx { get; } = new AtomicCounter();
        public readonly ConcurrentDictionary<string, StateAtomicCounter> _routes = new(StringComparer.OrdinalIgnoreCase);
        //public readonly ConcurrentDictionary<string, StateAtomicCounter> _clusters = new(StringComparer.OrdinalIgnoreCase);
        public readonly ConcurrentDictionary<string, StateAtomicCounter> _destinations = new(StringComparer.OrdinalIgnoreCase);
        public readonly ConcurrentDictionary<string, StateAtomicCounter> _request = new(StringComparer.OrdinalIgnoreCase);
        

        //public readonly ConcurrentDictionary<string, AtomicCounter> _clusters_2xx = new(StringComparer.OrdinalIgnoreCase);
        //public readonly ConcurrentDictionary<string, AtomicCounter> _clusters_4xx = new(StringComparer.OrdinalIgnoreCase);
        //public readonly ConcurrentDictionary<string, AtomicCounter> _clusters_5xx = new(StringComparer.OrdinalIgnoreCase);
        //public readonly ConcurrentDictionary<string, AtomicCounter> _destinations_2xx = new(StringComparer.OrdinalIgnoreCase);
        //public readonly ConcurrentDictionary<string, AtomicCounter> _destinations_4xx = new(StringComparer.OrdinalIgnoreCase);
        //public readonly ConcurrentDictionary<string, AtomicCounter> _destinations_5xx = new(StringComparer.OrdinalIgnoreCase);

        //public IEnumerable<AtomicCounter> GetRoutes()
        //{
        //    foreach (var (_, route) in _routes)
        //    {
        //        yield return route;
        //    }
        //}

        //public IEnumerable<AtomicCounter> GetClusters()
        //{
        //    foreach (var (_, route) in _clusters)
        //    {
        //        yield return route;
        //    }
        //}

        //public IEnumerable<AtomicCounter> GetDestinations()
        //{
        //    foreach (var (_, route) in _destinations)
        //    {
        //        yield return route;
        //    }
        //}

    }
    public sealed class AtomicCounter
    {
        private int _value;

        /// <summary>
        /// Gets the current value of the counter.
        /// </summary>
        public int Value
        {
            get => Volatile.Read(ref _value);
            set => Volatile.Write(ref _value, value);
        }

        /// <summary>
        /// Atomically increments the counter value by 1.
        /// </summary>
        public int Increment()
        {
            return Interlocked.Increment(ref _value);
        }

        /// <summary>
        /// Atomically decrements the counter value by 1.
        /// </summary>
        public int Decrement()
        {
            return Interlocked.Decrement(ref _value);
        }

        /// <summary>
        /// Atomically resets the counter value to 0.
        /// </summary>
        public void Reset()
        {
            Interlocked.Exchange(ref _value, 0);
        }
    }

    public sealed class StateAtomicCounter
    {
        public AtomicCounter AtomicCounter2xx = new AtomicCounter();
        public AtomicCounter AtomicCounter4xx = new AtomicCounter();
        public AtomicCounter AtomicCounter5xx = new AtomicCounter();

        public AtomicCounter AtomicCounter400 = new AtomicCounter();
        public AtomicCounter AtomicCounter401 = new AtomicCounter();
        public AtomicCounter AtomicCounter403 = new AtomicCounter();
        public AtomicCounter AtomicCounter405 = new AtomicCounter();


        public AtomicCounter AtomicCounter500 = new AtomicCounter();
        public AtomicCounter AtomicCounter502 = new AtomicCounter();
        public AtomicCounter AtomicCounter503 = new AtomicCounter();
        public AtomicCounter AtomicCounter504 = new AtomicCounter();

    }
}

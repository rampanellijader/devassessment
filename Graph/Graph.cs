using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Graph
{
    public interface IGraph<T>
    {
        IObservable<IEnumerable<T>> RoutesBetween(T source, T target);
    }

    public class Graph<T> : IGraph<T>
    {
        readonly Dictionary<T, IEnumerable<T>> dict;
        public Graph(IEnumerable<ILink<T>> links)
        {
            dict = links.GroupBy(l => l.Source)
                         .ToDictionary(k => k.Key,
                                       k => k.Select(l => l.Target));
        }

        public IObservable<IEnumerable<T>> RoutesBetween(T source, T target)
        {
            var routes = new List<IEnumerable<T>>();
            var queue = new Queue<IEnumerable<T>>();
            queue.Enqueue(new List<T> { source });
            while (queue.Any())
            {
                var currentRoute = queue.Dequeue();
                var currentLastNode = currentRoute.Last();
                if (dict.TryGetValue(currentLastNode, out var targets))
                {
                    foreach (var newTarget in targets.Where(t => !currentRoute.Contains(t)))
                    {
                        var nextList = new List<T>(currentRoute) { newTarget };

                        if (EqualityComparer<T>.Default.Equals(newTarget, target))
                            routes.Add(nextList.AsReadOnly());
                        else
                            queue.Enqueue(nextList);
                    }
                }
            }
            return routes.ToObservable();
        }
    }
}
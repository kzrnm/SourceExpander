// Port of https://github.com/naminodarie/ac-library-csharp
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace SourceExpander
{
    /// <summary>
    /// Topological sort by Strongly Connected Component Graph
    /// </summary>
    internal class TopologicalSort
    {
        public static (int[] Groups, IEnumerable<string> Dependencies)[] SCC(ISourceFileInfoSlim[] infos)
        {
            var dic = infos.Select((s, i) => (Info: s, Index: i))
                .ToDictionary(t => t.Info.FileName);
            var typeDic = new Dictionary<string, List<int>>();
            foreach (var (raw, i) in dic.Values)
            {
                foreach (var ty in raw.TypeNames)
                {
                    if (!typeDic.TryGetValue(ty, out var list))
                        typeDic[ty] = list = new();
                    list.Add(i);
                }
            }

            var graph = new InternalSCCGraph(dic.Count);
            foreach (var (raw, i) in dic.Values)
            {
                foreach (var ty in raw.UsedTypeNames)
                    if (typeDic.TryGetValue(ty, out var deps))
                        foreach (var dep in deps)
                            if (i != dep)
                                graph.AddEdge(i, dep);
            }
            var (groups, ids) = graph.SCC();
            var edges = graph.edges;
            var reslut = new (int[] Groups, IEnumerable<string> Dependencies)[groups.Length];
            for (int i = reslut.Length - 1; i >= 0; i--)
            {
                var depFiles = new HashSet<string>();
                foreach (var ix in groups[i])
                {
                    depFiles.Add(infos[ix].FileName);
                    foreach (var to in edges[ix])
                    {
                        if (ids[to] > i)
                            depFiles.UnionWith(reslut[ids[to]].Dependencies);
                    }
                }
                reslut[i] = (groups[i], depFiles);
            }

            return reslut;
        }

        private class CSR<TEdge>
        {
            public readonly int[] Start;

            public readonly TEdge[] EList;
            public CSR(int n, HashSet<TEdge>[] edges)
            {
                Start = new int[n + 1];
                EList = new TEdge[edges.Length];
                for (int from = 0; from < edges.Length; from++)
                {
                    Start[from + 1] += edges[from].Count;
                }

                for (int i = 1; i <= n; i++)
                {
                    Start[i] += Start[i - 1];
                }

                var counter = (int[])Start.Clone();
                for (int from = 0; from < edges.Length; from++)
                    foreach (var e in edges[from])
                    {
                        EList[counter[from]++] = e;
                    }
            }
        }
        [DebuggerDisplay("Vertices = {_n}, Edges = {edges.Count}")]
        private class InternalSCCGraph
        {
            private readonly int _n;
            public readonly HashSet<int>[] edges;

            public int VerticesNumbers => _n;

            public InternalSCCGraph(int n)
            {
                _n = n;
                edges = new HashSet<int>[n];
                for (int i = 0; i < edges.Length; i++)
                    edges[i] = new();
            }

            public void AddEdge(int from, int to) => edges[from].Add(to);

            public (int groupNum, int[] ids) SCCIDs()
            {
                // R. Tarjan のアルゴリズム
                var g = new CSR<int>(_n, edges);
                int nowOrd = 0;
                int groupNum = 0;
                var visited = new Stack<int>(_n);
                var low = new int[_n];
                var ord = Enumerable.Repeat(-1, _n).ToArray();
                var ids = new int[_n];

                for (int i = 0; i < ord.Length; i++)
                {
                    if (ord[i] == -1)
                    {
                        DFS(i);
                    }
                }

                foreach (ref var x in ids.AsSpan())
                {
                    // トポロジカル順序にするには逆順にする必要がある。
                    x = groupNum - 1 - x;
                }

                return (groupNum, ids);

                //void DFS(int v)
                //{
                //    low[v] = nowOrd;
                //    ord[v] = nowOrd++;
                //    visited.Push(v);
                //    // 頂点 v から伸びる有向辺を探索する。
                //    for (int i = g.Start[v]; i < g.Start[v + 1]; i++)
                //    {
                //        int to = g.EList[i].To;
                //        if (ord[to] == -1)
                //        {
                //            DFS(to);
                //            low[v] = Math.Min(low[v], low[to]);
                //        }
                //        else
                //        {
                //            low[v] = Math.Min(low[v], ord[to]);
                //        }
                //    }
                //    // v がSCCの根である場合、強連結成分に ID を割り振る。
                //    if (low[v] == ord[v])
                //    {
                //        while (true)
                //        {
                //            int u = visited.Pop();
                //            ord[u] = _n;
                //            ids[u] = groupNum;
                //            if (u == v)
                //            {
                //                break;
                //            }
                //        }
                //        groupNum++;
                //    }
                //}
                void DFS(int v)
                {
                    var stack = new Stack<(int v, int childIndex)>();
                    stack.Push((v, g.Start[v]));
                DFS: while (stack.Count > 0)
                    {
                        int ci;
                        (v, ci) = stack.Pop();

                        if (ci == g.Start[v])
                        {
                            low[v] = nowOrd;
                            ord[v] = nowOrd++;
                            visited.Push(v);
                        }
                        else if (ci < 0)
                        {
                            ci = -ci;
                            int to = g.EList[ci - 1];
                            low[v] = Math.Min(low[v], low[to]);
                        }

                        // 頂点 v から伸びる有向辺を探索する。
                        for (; ci < g.Start[v + 1]; ci++)
                        {
                            int to = g.EList[ci];
                            if (ord[to] == -1)
                            {
                                stack.Push((v, -(ci + 1)));
                                stack.Push((to, g.Start[to]));
                                goto DFS;
                            }
                            else
                            {
                                low[v] = Math.Min(low[v], ord[to]);
                            }
                        }

                        // v がSCCの根である場合、強連結成分に ID を割り振る。
                        if (low[v] == ord[v])
                        {
                            while (true)
                            {
                                int u = visited.Pop();
                                ord[u] = _n;
                                ids[u] = groupNum;

                                if (u == v)
                                {
                                    break;
                                }
                            }

                            groupNum++;
                        }
                    }
                }
            }

            public (int[][] Groups, int[] Ids) SCC()
            {
                var (groupNum, ids) = SCCIDs();
                var groups = new int[groupNum][];
                var counts = new int[groupNum];
                var seen = new int[groupNum];

                foreach (var x in ids)
                    counts[x]++;

                for (int i = 0; i < groupNum; i++)
                    groups[i] = new int[counts[i]];

                for (int i = 0; i < ids.Length; i++)
                    groups[ids[i]][seen[ids[i]]++] = i;

                return (groups, ids);
            }
        }
    }
}

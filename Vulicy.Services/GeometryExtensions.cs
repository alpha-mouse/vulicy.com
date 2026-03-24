using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Linemerge;

namespace Vulicy.Services;

public static class GeometryExtensions
{
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);

    public static Geometry ToMerged(this LineMerger lineMerger)
    {
        var mergedLines = lineMerger.GetMergedLineStrings();
        return mergedLines.Count switch
        {
            0 => GeometryFactory.CreateLineString(Array.Empty<Coordinate>()),
            1 => mergedLines[0],
            _ => GeometryFactory.CreateMultiLineString(mergedLines.Cast<LineString>().ToArray()),
        };
    }

    public static Geometry ToGeometry(this Envelope envelope)
        => GeometryFactory.ToGeometry(envelope);

    // no idea how it works, asked AI to give it to me
    public static List<Polygon> GetConvexHullsForClusters(IList<Geometry> geometries)
    {
        if (geometries == null || geometries.Count == 0)
            return new List<Polygon>();

        // Build STR-tree for spatial indexing
        var tree = new NetTopologySuite.Index.Strtree.STRtree<int>();
        for (int i = 0; i < geometries.Count; i++)
        {
            tree.Insert(geometries[i].EnvelopeInternal, i);
        }

        var unionFind = new UnionFind(geometries.Count);

        for (int i = 0; i < geometries.Count; i++)
        {
            var candidates = tree.Query(geometries[i].EnvelopeInternal);
            foreach (int j in candidates)
            {
                if (j > i && geometries[i].Intersects(geometries[j]))
                {
                    unionFind.Union(i, j);
                }
            }
        }

        // Group by cluster
        var clusters = new Dictionary<int, List<Geometry>>();
        for (int i = 0; i < geometries.Count; i++)
        {
            int root = unionFind.Find(i);
            if (!clusters.ContainsKey(root))
            {
                clusters[root] = new List<Geometry>();
            }
            clusters[root].Add(geometries[i]);
        }

        // Create convex hulls
        var convexHulls = new List<Polygon>();
        var factory = geometries[0].Factory;

        foreach (var cluster in clusters.Values)
        {
            var allCoordinates = new List<Coordinate>();
            foreach (var geom in cluster)
            {
                allCoordinates.AddRange(geom.Coordinates);
            }

            var uniqueCoords = allCoordinates.Distinct(new CoordinateComparer()).ToArray();

            if (uniqueCoords.Length < 3)
            {
                // Skip if not enough points for a polygon
                continue;
            }

            var convexHull = new ConvexHull(uniqueCoords, factory);
            var hull = convexHull.GetConvexHull();

            if (hull is Polygon polygon)
            {
                convexHulls.Add(polygon);
            }
        }

        return convexHulls;
    }

    // Union-Find (Disjoint Set) implementation
    public class UnionFind
    {
        private int[] parent;
        private int[] rank;

        public UnionFind(int size)
        {
            parent = new int[size];
            rank = new int[size];
            for (int i = 0; i < size; i++)
            {
                parent[i] = i;
                rank[i] = 0;
            }
        }

        public int Find(int x)
        {
            if (parent[x] != x)
            {
                parent[x] = Find(parent[x]); // Path compression
            }
            return parent[x];
        }

        public void Union(int x, int y)
        {
            int rootX = Find(x);
            int rootY = Find(y);

            if (rootX == rootY)
                return;

            // Union by rank
            if (rank[rootX] < rank[rootY])
            {
                parent[rootX] = rootY;
            }
            else if (rank[rootX] > rank[rootY])
            {
                parent[rootY] = rootX;
            }
            else
            {
                parent[rootY] = rootX;
                rank[rootX]++;
            }
        }
    }

    // Coordinate comparer for removing duplicates
    private class CoordinateComparer : IEqualityComparer<Coordinate>
    {
        public bool Equals(Coordinate x, Coordinate y)
        {
            if (x == null || y == null) return x == y;
            return x.Equals2D(y);
        }

        public int GetHashCode(Coordinate obj)
        {
            return obj == null ? 0 : obj.GetHashCode();
        }
    }
}
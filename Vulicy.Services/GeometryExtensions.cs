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
}
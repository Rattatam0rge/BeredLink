namespace FeedbackMap.Services;

/// <summary>Geografiske hjelpefunksjoner.</summary>
public static class Geo
{
    private const double EarthRadiusMeters = 6_371_000;

    /// <summary>Avstand i meter mellom to koordinater (haversine).</summary>
    public static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * EarthRadiusMeters * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double degrees) => degrees * Math.PI / 180;
}

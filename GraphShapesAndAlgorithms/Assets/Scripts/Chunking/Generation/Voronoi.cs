using Map;
namespace Generation
{
    public static class Voronoi
    {
        const int relaxation = 5;
        public static VoronoiMapData GenerateVoronoi(IslandShapeEnum islandShape, int size, int numOfPoints)
        {
            int seed = Seed.InitializeRandom();
            VoronoiMapData map = new VoronoiMapData(numOfPoints, size * GridMetrics.Width, relaxation);
            map.NewIsland(islandShape, numOfPoints, seed);

            return map;
        }
    }
}

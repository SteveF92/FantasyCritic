namespace FantasyCritic.MySQL.Entities;
internal class AveragePositionPointsEntity
{
    public AveragePositionPointsEntity()
    {

    }

    public AveragePositionPointsEntity(int pickPosition, decimal averagePoints)
    {
        PickPosition = pickPosition;
        AveragePoints = averagePoints;
    }

    public int PickPosition { get; set; }
    public decimal AveragePoints { get; set; }
}

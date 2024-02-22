namespace GLChart.Core.CollisionDetection
{
    public abstract class CellFactory
    {
        public abstract ICollisionCell CreateCell(Boundary2D boundary);
    }
}
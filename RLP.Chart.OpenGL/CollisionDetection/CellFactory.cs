namespace RLP.Chart.OpenGL.CollisionDetection
{
    public abstract class CellFactory
    {
        public abstract ICollisionCell CreateCell(Boundary2D boundary);
    }
}
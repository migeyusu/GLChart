namespace RLP.Chart.OpenGL.CollisionDetection
{
    public abstract class CellFactory
    {
        public abstract ICell CreateCell(Boundary2D boundary);
    }
}
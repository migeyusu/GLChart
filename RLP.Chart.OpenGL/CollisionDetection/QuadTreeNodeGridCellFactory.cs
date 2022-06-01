namespace RLP.Chart.OpenGL.CollisionDetection
{
    public class QuadTreeNodeGridCellFactory : CellFactory
    {
        public override ICell CreateCell(Boundary2D boundary)
        {
            return new QuadTreeNodeGridCell(boundary);
        }
    }
}
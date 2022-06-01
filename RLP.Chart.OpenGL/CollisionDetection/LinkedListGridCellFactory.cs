namespace RLP.Chart.OpenGL.CollisionDetection
{
    public class LinkedListGridCellFactory : CellFactory
    {
        public override ICell CreateCell(Boundary2D boundary)
        {
            return new LinkedListGridCell();
        }
    }
}
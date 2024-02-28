namespace GLChart.WPF.Render.CollisionDetection
{
    public class LinkedListGridCellFactory : CellFactory
    {
        public override ICollisionCell CreateCell(Boundary2D boundary)
        {
            return new LinkedListGridCell();
        }
    }
}
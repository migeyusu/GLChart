using System.Collections.Generic;

namespace RLP.Chart.Interface.Abstraction
{
    public interface IGeometryCollection<T> where T: IGeometry
    {
        void AddGeometry(T geometry);

        void AddGeometries(IList<T> geometries);

        void ResetWith(IList<T> geometries);

        void ResetWith(T geometry);
        
        void Clear();
    }
}
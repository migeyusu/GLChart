using System.Collections.Generic;

namespace GLChart.WPF.Base
{
    /// <summary>
    /// 集合体的集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGeometryCollection<T> where T : IGeometry
    {
        void Add(T geometry);

        void AddRange(IList<T> geometries);

        void Remove(T geometry);

        void RemoveAt(int index);

        void Insert(int index, T geometry);

        void ResetWith(IList<T> geometries);

        void ResetWith(T geometry);

        void Clear();
    }
}
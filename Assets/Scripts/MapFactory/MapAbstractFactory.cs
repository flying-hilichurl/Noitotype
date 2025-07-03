using UnityEngine;

namespace MapFactory
{
    public abstract class MapAbstractFactory : MonoBehaviour
    {
        public abstract void CreateMap();

        public abstract void ClearMap();
    }
}
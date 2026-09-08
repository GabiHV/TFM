
using System.Collections;

namespace App.ROSUtilities.Services
{
    public interface IROSService<T>
    {
        public T GetResult();
        public bool HasResult();
    }
}

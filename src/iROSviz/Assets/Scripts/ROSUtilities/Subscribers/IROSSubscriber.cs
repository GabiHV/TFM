using System.Collections;

namespace App.ROSUtilities.Subscribers
{
    public interface IROSSubscriber<T>
    {
        public T GetResult();
        public bool HasResult();
    }
}

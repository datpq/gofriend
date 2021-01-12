using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace goFriend.Helpers
{
    public class DphObservableCollection<T> : ObservableCollection<T>
    {
        public DphObservableCollection() : base() { }
        public DphObservableCollection(IEnumerable<T> collection) : base(collection) {}
        public DphObservableCollection(List<T> list) : base(list) {}

        public void FireEventCollectionChanged()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}

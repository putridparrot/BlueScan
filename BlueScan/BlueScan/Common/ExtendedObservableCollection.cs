using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BlueScan.Common
{
    /// <summary>
    /// A dispatcher aware observable collection. As the default ObservableCollection does
    /// not marshal changes onto the UI thread, this class handled such marshalling as well
    /// as offering the ability to Begin and End updates, so trying to only fire update events
    /// when necessary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtendedObservableCollection<T> : ObservableCollection<T>,
        IItemChanged

    {
        /// <summary>
        /// Raised when an item within the collection has changed
        /// </summary>
        public event PropertyChangedEventHandler ItemChanged;

        private ReferenceCounter _updating;

        /// <summary>
        /// Default constructor creates an empty collection
        /// </summary>
        public ExtendedObservableCollection() :
            base()
        {
        }

        /// <summary>
        /// Constructor adds items from the supplied
        /// list to the collection
        /// </summary>
        /// <param name="list"></param>
        public ExtendedObservableCollection(List<T> list) :
            base(list)
        {
        }

        /// <summary>
        /// Constructore adds the supplied enumerable items
        /// to the collection
        /// </summary>
        /// <param name="collection"></param>
        public ExtendedObservableCollection(IEnumerable<T> collection) :
            base(collection)
        {
        }

        /// <summary>
        /// Adds multiple items to the collection via an IEnumerable.
        /// Switches off change notifications whilst this is happening.
        /// </summary>
        /// <param name="e"></param>
        public void AddRange(IEnumerable<T> e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            try
            {
                BeginUpdate();

                foreach (var item in e)
                {
                    Add(item);
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        /// <summary>
        /// Used internally to track Begin/EndUpdate usage
        /// </summary>
        /// <returns></returns>
        private ReferenceCounter GetOrCreateUpdating()
        {
            return _updating ?? (_updating = new ReferenceCounter());
        }

        /// <summary>
        /// Supresses collection change notifications, incrementing
        /// the update ref count.
        /// </summary>
        public void BeginUpdate()
        {
            GetOrCreateUpdating().AddRef();
        }

        /// <summary>
        /// Turns collection change notifications back on when 
        /// update ref count is zero
        /// </summary>
        public void EndUpdate()
        {
            if (GetOrCreateUpdating().Release() == 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Sorts the collection in place, i.e. makes changes to 
        /// the collection. Supresses notification change events
        /// whilst this happens
        /// </summary>
        /// <param name="comparison"></param>
        public void Sort(Comparison<T> comparison)
        {
            try
            {
                BeginUpdate();

                ListExtensions.Sort(this, comparison);
            }
            finally
            {
                EndUpdate();
            }
        }

        /// <summary>
        /// Event is called when the collection changes
        /// </summary>
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// When the collection changes but is in update mode, no changes propogate. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (GetOrCreateUpdating().Count <= 0)
            {
                //base.OnCollectionChanged(e);
                // Taken from http://stackoverflow.com/questions/2104614/updating-an-observablecollection-in-a-separate-thread
                // to allow marshalling onto the UI thread, seems a neat solution
                var eventHandler = CollectionChanged;
                if (eventHandler != null)
                {
#if !NETSTANDARD2_0
                    var dispatcher = (from NotifyCollectionChangedEventHandler n in eventHandler.GetInvocationList()
                                      let dpo = n.Target as DispatcherObject
                                      where dpo != null
                                      select dpo.Dispatcher).FirstOrDefault();

                    if (dispatcher != null && !dispatcher.CheckAccess())
                    {
                        dispatcher.BeginInvoke(DispatcherPriority.DataBind, (Action)(() => OnCollectionChanged(e)));
                    }
                    else
                    {
#endif
                    foreach (NotifyCollectionChangedEventHandler n in eventHandler.GetInvocationList())
                    {
                        n.Invoke(this, e);
                    }
#if !NETSTANDARD2_0
                    }
#endif
                }
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsEmpty)));
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is INotifyPropertyChanged propertyChanged)
                    {
                        propertyChanged.PropertyChanged += ItemPropertyChanged;
                    }
                }
            }
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is INotifyPropertyChanged propertyChanged)
                    {
                        propertyChanged.PropertyChanged -= ItemPropertyChanged;
                    }
                }
            }
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsEmpty)));
        }

        /// <summary>
        /// When an item within the collection (which supports INotifyPropertyChanged)
        /// changes, the ItemChange event is raised
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        protected virtual void ItemPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (ItemChanged != null)
            {
                ItemChanged(sender, propertyChangedEventArgs);
            }
        }

        /// <summary>
        /// Get whether the collection is empty, this is useful
        /// if you want to bind to a boolean which reports
        /// when the collection goes from empty to not empty 
        /// and vice versa.
        /// </summary>
        public bool IsEmpty => Count <= 0;
    }

    /// <summary>
    /// Simple reference counter, it doesn't store
    /// anything except the reference count - so is basically
    /// a threadsafe counter
    /// </summary>
    public class ReferenceCounter
    {
        private int _refCount;
        private readonly object _syncObject = new object();

        /// <summary>
        /// Increment the reference count
        /// </summary>
        /// <returns></returns>
        public int AddRef()
        {
            lock (_syncObject)
                return ++_refCount;
        }

        /// <summary>
        /// Decrement the reference count
        /// </summary>
        /// <returns></returns>
        public int Release()
        {
            lock (_syncObject)
                return _refCount != 0 ? --_refCount : 0;
        }

        /// <summary>
        /// Reset the reference count to zero
        /// </summary>
        public void Reset()
        {
            lock (_syncObject)
                _refCount = 0;
        }

        /// <summary>
        /// Get the current reference count
        /// </summary>
        public int Count
        {
            get
            {
                lock (_syncObject)
                    return _refCount;
            }
        }
    }

    /// <summary>
    /// Defines the interface which collections
    /// should implement to fire item change events
    /// </summary>
    public interface IItemChanged
    {
        /// <summary>
        /// The ItemChanged event
        /// </summary>
        event PropertyChangedEventHandler ItemChanged;
    }

    public static class ListExtensions
    {
        /// <summary>
        /// Very simplistic implementation of AddRange for those IList implementations which 
        /// don't support it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="items"></param>
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            if (list != null)
            {
                foreach (T item in items)
                {
                    list.Add(item);
                }
            }
        }

        /// <summary>
        /// A simple AddRange which allows the user to include or exclude items, for example adding all items
        /// except those with an notional > 1000000
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="items"></param>
        /// <param name="filter"></param>
        /// <param name="function"></param>
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items, ListFilter filter, Predicate<T> function)
        {
            if (list != null)
            {
                if (function == null)
                {
                    list.AddRange(items);
                }
                else
                {
                    foreach (T item in items)
                    {
                        bool result = function(item);
                        if (filter == ListFilter.Include && result || filter == ListFilter.Exclude && !result)
                        {
                            list.Add(item);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sort for all list implementations, using a quick sort along with a supplied comparison function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="comparison"></param>
        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            if (list != null && list.Count > 0)
            {
                Sort(list, 0, list.Count - 1, comparison);
            }
        }

        public static void Sort<T>(this IList<T> list, IComparer comparer)
        {
            if (list != null && list.Count > 0 && comparer != null)
            {
                Sort(list, 0, list.Count - 1, (a, b) => comparer.Compare(a, b));
            }
        }

        public delegate int CompareValues<TSearchItem, TListItem>(TSearchItem searchItem, TListItem listItem);

        public static int BinarySearch<TSearchItem, TListItem>(this IList<TListItem> list, TSearchItem searchItem, CompareValues<TSearchItem, TListItem> matcher)
        {
            return BinarySearch(list, 0, list.Count - 1, searchItem, matcher);
        }

        public static int BinarySearch<TSearchItem, TListItem>(this IList<TListItem> list, int lowerBound, int upperBound, TSearchItem searchItem, CompareValues<TSearchItem, TListItem> matcher)
        {
            if (lowerBound > upperBound)
                throw new ArgumentOutOfRangeException("lowerBound must be less than or equal to upperBound");
            if (upperBound >= list.Count)
                throw new ArgumentOutOfRangeException("upperBound must be less than the size of the collection");

            int start = lowerBound;
            int end = upperBound;
            while (start <= end)
            {
                int mid = start + (end - start) / 2;

                int match = matcher(searchItem, list[mid]);
                if (match == 0)
                    return mid;

                if (match < 0)
                {
                    end = mid - 1;
                }
                else
                {
                    start = mid + 1;
                }
            }
            return -1;
        }

        public static int BinarySearchInsertionPoint<TSearchItem, TListItem>(this IList<TListItem> list, TSearchItem searchItem, CompareValues<TSearchItem, TListItem> matcher)
        {
            return BinarySearchInsertionPoint(list, 0, list.Count - 1, searchItem, matcher);
        }

        public static int BinarySearchInsertionPoint<TSearchItem, TListItem>(this IList<TListItem> list, int lowerBound,
            int upperBound, TSearchItem searchItem, CompareValues<TSearchItem, TListItem> matcher)
        {
            if (lowerBound > upperBound)
                throw new ArgumentOutOfRangeException("lowerBound must be less than or equal to upperBound");
            if (upperBound >= list.Count)
                throw new ArgumentOutOfRangeException("upperBound must be less than the size of the collection");

            var highIndex = upperBound;
            var lowIndex = lowerBound;

            while (lowIndex <= highIndex)
            {
                var mid = lowIndex + (highIndex - lowIndex) / 2;
                var test = matcher(searchItem, list[mid]);
                if (test > 0)
                    lowIndex = mid + 1;
                else if (test < 0)
                    highIndex = mid - 1;
                else
                {
                    // found a match, now find first value greater than match
                    for (int i = mid; i < highIndex; i++)
                    {
                        if (matcher(searchItem, list[i]) < 0)
                            return i;
                    }
                    // if there's no value larger than return upperbound
                    return upperBound + 1;
                }
            }

            return highIndex < 0 ? 0 : lowIndex;
        }

        public static int BinarySearch<T>(this IList<T> list, T item, Comparison<T> matcher)
        {
            return BinarySearch(list, 0, list.Count - 1, item, matcher);
        }

        public static int BinarySearch<T>(this IList<T> list, int lowerBound, int upperBound, T item, Comparison<T> matcher)
        {
            if (lowerBound > upperBound)
                throw new ArgumentOutOfRangeException("lowerBound must be less than or equal to upperBound");
            if (upperBound >= list.Count)
                throw new ArgumentOutOfRangeException("upperBound must be less than the size of the collection");

            int start = lowerBound;
            int end = upperBound;
            while (start <= end)
            {
                int mid = start + (end - start) / 2;

                int match = matcher(item, list[mid]);
                if (match == 0)
                    return mid;

                if (match < 0)
                {
                    end = mid - 1;
                }
                else
                {
                    start = mid + 1;
                }
            }
            return -1;
        }

        /// <summary>
        /// Basically an IndexOf for lists but with the ability to supply a matcher method,
        /// thus enabling us to find an index using part of an object that's not a key/reference
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="matcher"></param>
        /// <returns></returns>
        public static int FindIndex<T>(this IList<T> list, Predicate<T> matcher)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (matcher(list[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static void RemoveDuplicates<T>(this IList<T> list, Func<T, T, int> comparer)
        {
            IList<T> newList = list.Distinct(new ComparerImpl<T>(comparer)).ToList();
            list.Clear();
            list.AddRange(newList);
        }

        /// <summary>
        /// Basically a find method that enables us to use a matcher to check for a valid item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="matcher"></param>
        /// <returns></returns>
        public static T Find<T>(this IList<T> list, Predicate<T> matcher)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (matcher(list[i]))
                {
                    return list[i];
                }
            }
            return default(T);
        }

        /// <summary>
        /// Adds an iten to a sorted list. It is expected the list is either sorted 
        /// or as items are added they're put in order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <param name="comparison"></param>
        public static void Add<T>(this IList<T> list, T item, Comparison<T> comparison)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (comparison(item, list[i]) < 0)
                {
                    list.Insert(i, item);
                    return;
                }
            }

            list.Add(item);
        }

        // standard quick sort implementation
        private static void Sort<T>(IList<T> list, int left, int right, Comparison<T> comparison)
        {
            int i = left;
            int j = right;
            T x = list[(left + right) / 2];
            while (i <= j)
            {
                while (comparison(list[i], x) < 0)
                {
                    i++;
                }
                while (comparison(x, list[j]) < 0)
                {
                    j--;
                }
                if (i <= j)
                {
                    T tmp = list[i];
                    list[i++] = list[j];
                    list[j--] = tmp;
                }
            }
            if (left < j)
            {
                Sort(list, left, j, comparison);
            }
            if (i < right)
            {
                Sort(list, i, right, comparison);
            }
        }
    }

    /// <summary>
    /// Implements IComparer&lt;T&gt;, IEqualityComparer&lt;T&gt; 
    /// and IComparer
    /// </summary>
    /// <typeparam name="T">The type of being compared</typeparam>
    public class ComparerImpl<T> : IComparer<T>, IEqualityComparer<T>, IComparer
    {
        private readonly Func<T, T, int> _objectComparer;
        private readonly Func<T, int> _objectHash;

        /// <summary>
        /// Constructor take a comparison function which expects two types of T and
        /// returns an integer where a value less than 0 indicates the first item is 
        /// before the second, a value of 0 indicates they're equivalent 
        /// and a value greater than 0 indicates the first item is after
        /// the second item.
        /// </summary>
        /// <param name="objectComparer"></param>
        public ComparerImpl(Func<T, T, int> objectComparer) :
            this(objectComparer, o => 0)
        {
        }

        /// <summary>
        /// Constructor take a comparison function which expects two types of T and
        /// returns an integer where a value less than 0 indicates the first item is 
        /// before the second, a value of 0 indicates they're equivalent 
        /// and a value greater than 0 indicates the first item is after
        /// the second item. This constructor also takes a function for the object 
        /// hash code method.
        /// </summary>
        /// <param name="objectComparer">A function to compare two items of type T</param>
        /// <param name="objectHash">A function to return the hash code for a given instance of type T</param>
        /// <exception cref="System.NullReferenceException">Thrown when the objectComparer is null</exception>
        public ComparerImpl(Func<T, T, int> objectComparer, Func<T, int> objectHash)
        {
            _objectComparer = objectComparer ?? throw new NullReferenceException("Comparer function cannot be null");
            _objectHash = objectHash ?? throw new NullReferenceException("Hash code function cannot be null");
        }

        /// <summary>
        /// Calls the supplied comparison method
        /// </summary>
        /// <param name="x">The first item</param>
        /// <param name="y">The second item to compare with the first</param>
        /// <returns>Returns the value from the comparison method</returns>
        public int Compare(T x, T y)
        {
            return _objectComparer(x, y);
        }

        /// <summary>
        /// Compares whether the first and second item are equivalent
        /// by calling the supplied comparison method.
        /// </summary>
        /// <param name="x">The first item</param>
        /// <param name="y">The second item to compare with the first</param>
        /// <returns>True if equivalent or False otherwise</returns>
        public bool Equals(T x, T y)
        {
            return Compare(x, y) == 0;
        }

        /// <summary>
        /// Calls the suppled has code method
        /// </summary>
        /// <param name="obj">The object to get the hash code from</param>
        /// <returns>The result of the hash method</returns>
        public int GetHashCode(T obj)
        {
            return _objectHash(obj);
        }

        /// <summary>
        /// Calls the supplied comparison method
        /// </summary>
        /// <param name="x">The first item</param>
        /// <param name="y">The second item to compare with the first</param>
        /// <returns>Returns the value from the comparison method</returns>
        public int Compare(object x, object y)
        {
            return Compare((T)x, (T)y);
        }
    }

    /// <summary>
    /// The enumeration indicates whether an item should be
    /// included or excluded for a list
    /// </summary>
    public enum ListFilter
    {
        /// <summary>
        /// Include an item or items within a list
        /// </summary>
        Include,
        /// <summary>
        /// Exclude an item or items from a list
        /// </summary>
        Exclude
    }
}

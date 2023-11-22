namespace BlueScan.Common
{
    /// <summary>
    /// A FixedSizeObservableCollection extends the ExtendedObservableCollection
    /// to allow the collection to grow to a specified size and no exceed that size.
    /// Any additional items added/inserted will result in the first item
    /// (as per a Queue) being removed from the collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedSizeObservableCollection<T> : ExtendedObservableCollection<T>
    {
        /// <summary>
        /// Constructs a collection of a fixed size. 
        /// </summary>
        /// <param name="size"></param>
        public FixedSizeObservableCollection(int size)
        {
            Size = size;
        }

        /// <summary>
        /// Gets the size that the collection is fixed to
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Override of the InsertItem to handle any item removals
        /// as a result of an insert.
        /// </summary>
        /// <param name="index">The index the item is inserted to</param>
        /// <param name="item">The item being inserted</param>
        protected override void InsertItem(int index, T item)
        {
            try
            {
                BeginUpdate();

                base.InsertItem(index, item);
                while (Count > Size)
                {
                    Remove(this[0]);
                }
            }
            finally
            {
                EndUpdate();
            }
        }
    }
}

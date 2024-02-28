using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GLChart.WPF.Base
{
    public class NotifyCollectionChangedEventArgs<T> : EventArgs
    {
        private NotifyCollectionChangedAction _action;
        private IList<T> _newItems;
        private IList<T> _oldItems;
        private int _newStartingIndex = -1;
        private int _oldStartingIndex = -1;

        public static NotifyCollectionChangedEventArgs<T> ResetArgs { get; } =
            new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Reset);

        public static NotifyCollectionChangedEventArgs<T> AppendArgs(T t)
        {
            return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Add, t);
        }

        public static NotifyCollectionChangedEventArgs<T> AppendRangeArgs(IList<T> list)
        {
            return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Add, list);
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            if (action != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("WrongActionForCtor", nameof(action));
            this.InitializeAdd(action, (IList<T>)null, -1);
        }


        public NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction action,
            T changedItem)
        {
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove &&
                action != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", nameof(action));
            if (action == NotifyCollectionChangedAction.Reset)
            {
                this.InitializeAdd(action, new T[] { changedItem }, -1);
            }
            else
                this.InitializeAddOrRemove(action, (IList<T>)new List<T>()
                {
                    changedItem
                }, -1);
        }


        public NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction action,
            T changedItem,
            int index)
        {
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove &&
                action != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", nameof(action));
            if (action == NotifyCollectionChangedAction.Reset)
            {
                this.InitializeAdd(action, new T[] { changedItem }, index);
            }
            else
                this.InitializeAddOrRemove(action, (IList<T>)new List<T>
                {
                    changedItem
                }, index);
        }


        public NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction action,
            IList<T> changedItems)
        {
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove &&
                action != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", nameof(action));
            if (action == NotifyCollectionChangedAction.Reset)
            {
                this.InitializeAdd(action, changedItems, -1);
            }
            else
            {
                if (changedItems == null)
                    throw new ArgumentNullException(nameof(changedItems));
                this.InitializeAddOrRemove(action, changedItems, -1);
            }
        }

        public NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction action,
            IList<T> changedItems,
            int startingIndex)
        {
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove &&
                action != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor", nameof(action));
            if (action == NotifyCollectionChangedAction.Reset)
            {
                this.InitializeAdd(action, changedItems, startingIndex);
            }
            else
            {
                if (changedItems == null)
                    throw new ArgumentNullException(nameof(changedItems));
                if (startingIndex < -1)
                    throw new ArgumentException("IndexCannotBeNegative", nameof(startingIndex));
                this.InitializeAddOrRemove(action, changedItems, startingIndex);
            }
        }

        public NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction action,
            T newItem,
            T oldItem)
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new ArgumentException(
                    "WrongActionForCtor", nameof(action));
            this.InitializeMoveOrReplace(action, (IList<T>)new List<T>
            {
                newItem
            }, new List<T> { oldItem }, -1, -1);
        }


        public NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction action,
            T newItem,
            T oldItem,
            int index)
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new ArgumentException(
                    "WrongActionForCtor", nameof(action));
            var oldStartingIndex = index;
            this.InitializeMoveOrReplace(action, (IList<T>)new List<T>
            {
                newItem
            }, new List<T> { oldItem }, index, oldStartingIndex);
        }


        public NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction action,
            IList<T> newItems,
            IList<T> oldItems)
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new ArgumentException(
                    "WrongActionForCtor", nameof(action));
            if (newItems == null)
                throw new ArgumentNullException(nameof(newItems));
            if (oldItems == null)
                throw new ArgumentNullException(nameof(oldItems));
            this.InitializeMoveOrReplace(action, newItems, oldItems, -1, -1);
        }


        public NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction action,
            IList<T> newItems,
            IList<T> oldItems,
            int startingIndex)
        {
            if (action != NotifyCollectionChangedAction.Replace)
                throw new ArgumentException(
                    "WrongActionForCtor", nameof(action));
            if (newItems == null)
                throw new ArgumentNullException(nameof(newItems));
            if (oldItems == null)
                throw new ArgumentNullException(nameof(oldItems));
            this.InitializeMoveOrReplace(action, newItems, oldItems, startingIndex, startingIndex);
        }


        public NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction action,
            T changedItem,
            int index,
            int oldIndex)
        {
            if (action != NotifyCollectionChangedAction.Move)
                throw new ArgumentException(
                    "WrongActionForCtor", nameof(action));
            if (index < 0)
                throw new ArgumentException("IndexCannotBeNegative", nameof(index));
            var objArray = new List<T> { changedItem };
            this.InitializeMoveOrReplace(action, (IList<T>)objArray, (IList<T>)objArray, index, oldIndex);
        }


        public NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction action,
            IList<T> changedItems,
            int index,
            int oldIndex)
        {
            if (action != NotifyCollectionChangedAction.Move)
                throw new ArgumentException(
                    "WrongActionForCtor", nameof(action));
            if (index < 0)
                throw new ArgumentException("IndexCannotBeNegative", nameof(index));
            this.InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
        }

        internal NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction action,
            IList<T> newItems,
            IList<T> oldItems,
            int newIndex,
            int oldIndex)
        {
            this._action = action;
            this._newItems = newItems == null ? (IList<T>)null : new ReadOnlyCollection<T>(newItems);
            this._oldItems = oldItems == null ? (IList<T>)null : new ReadOnlyCollection<T>(oldItems);
            this._newStartingIndex = newIndex;
            this._oldStartingIndex = oldIndex;
        }

        private void InitializeAddOrRemove(
            NotifyCollectionChangedAction action,
            IList<T> changedItems,
            int startingIndex)
        {
            if (action == NotifyCollectionChangedAction.Add)
            {
                this.InitializeAdd(action, changedItems, startingIndex);
            }
            else
            {
                if (action != NotifyCollectionChangedAction.Remove)
                    return;
                this.InitializeRemove(action, changedItems, startingIndex);
            }
        }

        private void InitializeAdd(
            NotifyCollectionChangedAction action,
            IList<T> newItems,
            int newStartingIndex)
        {
            this._action = action;
            this._newItems = newItems == null ? (IList<T>)null : new ReadOnlyCollection<T>(newItems);
            this._newStartingIndex = newStartingIndex;
        }

        private void InitializeRemove(
            NotifyCollectionChangedAction action,
            IList<T> oldItems,
            int oldStartingIndex)
        {
            this._action = action;
            this._oldItems = oldItems == null ? null : new ReadOnlyCollection<T>(oldItems);
            this._oldStartingIndex = oldStartingIndex;
        }

        private void InitializeMoveOrReplace(
            NotifyCollectionChangedAction action,
            IList<T> newItems,
            IList<T> oldItems,
            int startingIndex,
            int oldStartingIndex)
        {
            this.InitializeAdd(action, newItems, startingIndex);
            this.InitializeRemove(action, oldItems, oldStartingIndex);
        }


        public NotifyCollectionChangedAction Action
        {
            get => this._action;
        }

        public IList<T> NewItems
        {
            get => this._newItems;
        }


        public IList<T> OldItems
        {
            get => this._oldItems;
        }

        public int NewStartingIndex
        {
            get => this._newStartingIndex;
        }

        public int OldStartingIndex
        {
            get => this._oldStartingIndex;
        }
    }
}
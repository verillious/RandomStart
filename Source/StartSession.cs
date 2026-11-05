using System;

namespace RandomStartMod
{
    // Identity, not a process-wide boolean, decides which game owns overrides.
    internal sealed class StartSession<TOwner, TState> where TOwner : class
    {
        private WeakReference<TOwner> owner;
        private TState state;
        internal void Begin(TOwner value, TState data) { owner = new WeakReference<TOwner>(value); state = data; }
        internal TState For(TOwner value) => owner != null && owner.TryGetTarget(out TOwner target) && ReferenceEquals(target, value) ? state : default(TState);
        internal void End(TOwner value)
        {
            if (owner == null || !owner.TryGetTarget(out TOwner target) || !ReferenceEquals(target, value)) return;
            owner = null;
            state = default(TState);
        }
    }

    internal sealed class ScopedValue<T>
    {
        internal T Value { get; private set; }
        internal IDisposable Enter(T value)
        {
            T previous = Value;
            Value = value;
            return new Restore(() => Value = previous);
        }
        private sealed class Restore : IDisposable
        {
            private Action restore;
            internal Restore(Action action) { restore = action; }
            public void Dispose() { Action action = restore; restore = null; action?.Invoke(); }
        }
    }
}

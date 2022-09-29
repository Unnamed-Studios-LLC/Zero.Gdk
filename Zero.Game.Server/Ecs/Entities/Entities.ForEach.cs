using System;

namespace Zero.Game.Server
{
    public unsafe partial class Entities
    {
        public void ForEach<T1>(QueryFunc<T1> func)
            where T1 : unmanaged
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new Query<T1>(func);
            ForEach(query);
        }

        public void ForEach<T1, T2>(QueryFunc<T1, T2> func)
            where T1 : unmanaged
            where T2 : unmanaged
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new Query<T1, T2>(func);
            ForEach(query);
        }

        public void ForEach<T1, T2, T3>(QueryFunc<T1, T2, T3> func)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new Query<T1, T2, T3>(func);
            ForEach(query);
        }

        public void ForEach<T1, T2, T3, T4>(QueryFunc<T1, T2, T3, T4> func)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new Query<T1, T2, T3, T4>(func);
            ForEach(query);
        }

        public void ForEach<T1, T2, T3, T4, T5>(QueryFunc<T1, T2, T3, T4, T5> func)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new Query<T1, T2, T3, T4, T5>(func);
            ForEach(query);
        }

        public void ForEach<T1, T2, T3, T4, T5, T6>(QueryFunc<T1, T2, T3, T4, T5, T6> func)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
            where T6 : unmanaged
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new Query<T1, T2, T3, T4, T5, T6>(func);
            ForEach(query);
        }

        public void ForEach(EntityQueryFunc func)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new EntityQuery(func);
            ForEach(query);
        }

        public void ForEach<T1>(EntityQueryFunc<T1> func)
            where T1 : unmanaged
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new EntityQuery<T1>(func);
            ForEach(query);
        }

        public void ForEach<T1, T2>(EntityQueryFunc<T1, T2> func)
            where T1 : unmanaged
            where T2 : unmanaged
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new EntityQuery<T1, T2>(func);
            ForEach(query);
        }

        public void ForEach<T1, T2, T3>(EntityQueryFunc<T1, T2, T3> func)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new EntityQuery<T1, T2, T3>(func);
            ForEach(query);
        }

        public void ForEach<T1, T2, T3, T4>(EntityQueryFunc<T1, T2, T3, T4> func)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new EntityQuery<T1, T2, T3, T4>(func);
            ForEach(query);
        }

        public void ForEach<T1, T2, T3, T4, T5>(EntityQueryFunc<T1, T2, T3, T4, T5> func)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new EntityQuery<T1, T2, T3, T4, T5>(func);
            ForEach(query);
        }

        public void ForEach<T1, T2, T3, T4, T5, T6>(EntityQueryFunc<T1, T2, T3, T4, T5, T6> func)
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
            where T6 : unmanaged
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var query = new EntityQuery<T1, T2, T3, T4, T5, T6>(func);
            ForEach(query);
        }
    }
}

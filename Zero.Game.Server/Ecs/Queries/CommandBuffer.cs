using System;
using System.Collections.Generic;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public sealed class CommandBuffer
    {
        private readonly List<Action> _actions = new(100);

        /// <summary>
        /// Adds an action to be processed
        /// </summary>
        /// <param name="action"></param>
        public void Add(Action action)
        {
            lock (_actions)
            {
                _actions.Add(action);
            }
        }

        /// <summary>
        /// Executes command in the buffer
        /// </summary>
        /// <returns></returns>
        internal void Execute()
        {
            for (int i = 0; i < _actions.Count; i++)
            {
                try
                {
                    _actions[i].Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e, "An error occured during a buffered command");
                }
            }
            _actions.Clear();
        }
    }
}

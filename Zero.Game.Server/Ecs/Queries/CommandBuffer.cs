using System;
using System.Collections.Generic;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public class CommandBuffer
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
        public void Execute()
        {
            int i;
            do
            {
                for (i = 0; i < _actions.Count; i++)
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

                lock (_actions)
                {
                    if (i == _actions.Count)
                    {
                        _actions.Clear();
                    }
                    else
                    {
                        _actions.RemoveRange(0, i);
                    }
                }
            }
            while (i > 0); // while loop to continue execution until the buffer is empty (in case commands added more commands)
        }
    }
}

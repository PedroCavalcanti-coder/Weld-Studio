using System;
using System.Collections.Generic;

namespace WeldStudio.Core
{
    /// <summary>Undo/redo stack of <see cref="ICommand"/>s.</summary>
    public sealed class CommandHistory
    {
        public const int DefaultCapacity = 200;

        private readonly List<ICommand> undoStack = new List<ICommand>();
        private readonly Stack<ICommand> redoStack = new Stack<ICommand>();
        private readonly int capacity;
        private bool sealedTop = true;

        public CommandHistory(int capacity = DefaultCapacity)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be positive.");
            this.capacity = capacity;
        }

        /// <summary>Raised whenever what can be undone or redone changes.</summary>
        public event Action Changed;

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;
        public string UndoLabel => CanUndo ? undoStack[undoStack.Count - 1].Label : null;
        public string RedoLabel => CanRedo ? redoStack.Peek().Label : null;

        /// <summary>
        /// Executes <paramref name="command"/> and records it, merging it into the previous command when
        /// that one accepts it and no <see cref="Seal"/> happened in between. Clears the redo stack.
        /// </summary>
        public void Execute(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            command.Execute();
            redoStack.Clear();

            bool merged = !sealedTop && CanUndo && undoStack[undoStack.Count - 1].TryMergeWith(command);
            if (!merged)
            {
                undoStack.Add(command);
                if (undoStack.Count > capacity) undoStack.RemoveAt(0);
            }
            sealedTop = false;
            Changed?.Invoke();
        }

        /// <summary>Ends the current merge window, e.g. when the user releases a slider.</summary>
        public void Seal() => sealedTop = true;

        public bool Undo()
        {
            if (!CanUndo) return false;

            ICommand command = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);
            command.Undo();
            redoStack.Push(command);
            sealedTop = true;
            Changed?.Invoke();
            return true;
        }

        public bool Redo()
        {
            if (!CanRedo) return false;

            ICommand command = redoStack.Pop();
            command.Execute();
            undoStack.Add(command);
            sealedTop = true;
            Changed?.Invoke();
            return true;
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            sealedTop = true;
            Changed?.Invoke();
        }
    }
}

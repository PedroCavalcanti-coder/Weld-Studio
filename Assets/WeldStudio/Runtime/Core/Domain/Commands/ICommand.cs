namespace WeldStudio.Core
{
    /// <summary>A reversible change to a character, recorded by <see cref="CommandHistory"/> for undo/redo.</summary>
    public interface ICommand
    {
        /// <summary>Short description shown in the UI, e.g. "Equip T-Shirt".</summary>
        string Label { get; }

        void Execute();
        void Undo();

        /// <summary>
        /// Absorbs <paramref name="next"/> (already executed) into this command so both are undone as one step,
        /// e.g. all the values produced while dragging a slider. Return false to record it separately.
        /// </summary>
        bool TryMergeWith(ICommand next);
    }
}

using UnityEngine;

namespace CreakwoodManor.Controls
{
    /// <summary>
    /// Everything a character needs from whoever is controlling it.
    /// Characters read from this interface instead of the keyboard directly,
    /// so online co-op can later swap in a network input source without
    /// touching the movement code.
    /// </summary>
    public interface IPlayerInput
    {
        /// <summary>Movement direction, each axis from -1 to 1.</summary>
        Vector2 Move { get; }

        /// <summary>True only on the frame the jump button was pressed.</summary>
        bool JumpPressed { get; }

        /// <summary>True while the jump button is held down.</summary>
        bool JumpHeld { get; }

        /// <summary>True only on the frame interact was pressed (human).</summary>
        bool InteractPressed { get; }

        /// <summary>True only on the frame possess was pressed (ghost).</summary>
        bool PossessPressed { get; }

        /// <summary>True only on the frame the object ability was pressed (ghost).</summary>
        bool AbilityPressed { get; }

        /// <summary>True only on the frame restart was pressed (either player).</summary>
        bool RestartPressed { get; }
    }
}

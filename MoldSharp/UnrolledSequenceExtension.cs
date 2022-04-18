using C4N.Collections.Sequence;
using StaticStateMachine;
namespace MoldSharp;

static partial class UnrolledSequenceExtension
{
    public static bool StartsWith(this UnrolledSequence<char> sequence, string str, StringComparison comparison)
    {
        return sequence.StartsWith(str.AsSpan(), comparison);
    }

    public static bool StartsWith(this UnrolledSequence<char> sequence, string str)
    {
        return sequence.StartsWith(str, StringComparison.Ordinal);
    }

    public static (MachineState<TAssociated> State, int Evaluated) StartsWith<TStateMachine, TArg, TAssociated>(this UnrolledSequence<TArg> sequence, TStateMachine stateMachine)
        where TStateMachine : IStateMachine<TArg, TAssociated>
    {
        var lastState = stateMachine.State;
        var lastEvaluated = 0;
        var evaluated = 0;
        foreach (var buffer in sequence.EnumerateBuffer())
        {
            foreach (var chara in buffer)
            {
                evaluated++;
                var terminal = !stateMachine.Transition(chara);
                var state = stateMachine.State;
                if (terminal)
                {
                    if (state.Accept) return (state, evaluated);
                    return (lastState, lastEvaluated);
                }
                if (state.Accept)
                {
                    lastEvaluated = evaluated;
                    lastState = state;
                }
            }
        }
        return (stateMachine.State, evaluated);
    }
}
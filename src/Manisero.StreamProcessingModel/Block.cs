using System;

namespace Manisero.StreamProcessingModel
{
    public interface IBlock
    {
        Type InputType { get; }
        Type OutputType { get; }

        string Name { get; }
        Func<object, object> Body { get; }
        bool Parallel { get; }
        Action OnCompleted { get; }
    }

    public static class Block
    {
        public static Block<TInput, TOutput> CreateFunctionBlock<TInput, TOutput>(
            string name,
            Func<TInput, TOutput> body,
            bool parallel = false,
            Action onCompleted = null)
        {
            return new Block<TInput, TOutput>(name, body, parallel, onCompleted);
        }

        public static Block<TInput, TInput> CreateActionBlock<TInput>(
            string name,
            Action<TInput> body,
            bool parallel = false,
            Action onCompleted = null)
        {
            Func<TInput, TInput> bodyFunc = x =>
            {
                body(x);
                return x;
            };

            return CreateFunctionBlock(name, bodyFunc, parallel, onCompleted);
        }
    }

    public class Block<TInput, TOutput> : IBlock
    {
        public Type InputType { get; } = typeof(TInput);
        public Type OutputType { get; } = typeof(TOutput);

        public string Name { get; }

        public Func<TInput, TOutput> Body { get; }
        Func<object, object> IBlock.Body => x => Body((TInput)x);

        public bool Parallel { get; }

        public Action OnCompleted { get; }

        public Block(
            string name,
            Func<TInput, TOutput> body,
            bool parallel = false,
            Action onCompleted = null)
        {
            Name = name;
            Body = body;
            Parallel = parallel;
            OnCompleted = onCompleted;
        }
    }
}

using MoldSharp;
using System.Text;

Console.ReadLine();

[Mold("testfile.txt")]
partial class MoldTest<T>
    where T : struct
{
    struct Context
    {
        public Context()
        {
            this.builder = new StringBuilder();
        }

        StringBuilder builder;

        public void Append<TItem>(TItem value)
        {
            this.builder.Append(value?.ToString());
        }

        public void AppendLiteral(string literal)
        {
            this.builder.Append(literal);
        }

        public override string ToString()
        {
            return this.builder.ToString();
        }
    }

    public MoldTest(T[] items)
    {
        this.items = items; 
    }

    private T[] items;

    private Context GetTransformContext() => new();
}
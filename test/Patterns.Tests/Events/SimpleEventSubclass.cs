namespace Iommands.Tests.Events
{
    public class SimpleEventSubclass : SimpleEvent
    {
        public SimpleEventSubclass(int value) : base(value)
        {
        }

        public SimpleEventSubclass(object source, int value) 
            : base(source, value)
        {
        }
    }
}

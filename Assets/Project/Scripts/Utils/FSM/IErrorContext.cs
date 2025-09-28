using System;

namespace Utils.Fsm
{
    public interface IErrorContext
    {
        Exception? Exception { get; set; }
    }
}

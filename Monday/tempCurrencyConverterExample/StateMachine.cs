using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrenciesCommodities
{
    public enum Condition { WorkingC, WorkingG, DBerror, FileReadError, Waiting };

    class StateMachine
    {
        public static Condition state = Condition.WorkingC;
    }
}

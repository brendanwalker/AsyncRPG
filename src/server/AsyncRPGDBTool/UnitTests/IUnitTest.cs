using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncRPGDBTool.UnitTests
{
    interface IUnitTest
    {
        string GetTestName();
        bool RunTest();
    }
}

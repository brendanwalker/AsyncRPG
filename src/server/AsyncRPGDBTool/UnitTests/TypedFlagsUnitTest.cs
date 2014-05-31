using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using AsyncRPGSharedLib.Common;

namespace AsyncRPGDBTool.UnitTests
{
    class TypedFlagsUnitTest : IUnitTest
    {
        private TextWriter _logger;

        private enum TestEnum
        {
            _flag0,
            _flag1,
            _flag2,
            _flag3,
            _flag4,
            _flag5,
            _flag6,
            _flag7,
            _flag8,
            _flag9,
            _flag10,
            _flag11,
            _flag12,
            _flag13,
            _flag14,
            _flag15,
            _flag16,
            _flag17,
            _flag18,
            _flag19,
            _flag20,
            _flag21,
            _flag22,
            _flag23,
            _flag24,
            _flag25,
            _flag26,
            _flag27,
            _flag28,
            _flag29,
            _flag30,
            _flag31
        }

        public TypedFlagsUnitTest(TextWriter logger)
        {
            _logger = logger;
        }

        public string GetTestName()
        {
            return "TypedFlags";
        }

        public bool RunTest()
        {
            bool success = true;

            {
                TypedFlags<TestEnum> flags = new TypedFlags<TestEnum>();

                Debug.Assert(flags.IsEmpty());
                success &= flags.IsEmpty();

                for (TestEnum bitIndex = TestEnum._flag0; bitIndex <= TestEnum._flag31; ++bitIndex)
                {
                    Debug.Assert(!flags.Test(bitIndex));
                    success &= !flags.Test(bitIndex);

                    flags.Set(bitIndex, true);
                    Debug.Assert(flags.Test(bitIndex));
                    success &= flags.Test(bitIndex);

                    flags.Set(bitIndex, false);
                    Debug.Assert(!flags.Test(bitIndex));
                    success &= !flags.Test(bitIndex);
                }
            }


            {
                TypedFlags<TestEnum> flagsA = new TypedFlags<TestEnum>();
                TypedFlags<TestEnum> flagsB = new TypedFlags<TestEnum>();

                flagsB.Set(TestEnum._flag10, true);

                TypedFlags<TestEnum> flagsC = new TypedFlags<TestEnum>(flagsB);

                Debug.Assert(flagsA != flagsB);
                success &= flagsA != flagsB;

                Debug.Assert(flagsB == flagsC);
                success &= flagsB == flagsC;
            }

            {
                TypedFlags<TestEnum> flags = new TypedFlags<TestEnum>(
                    TypedFlags<TestEnum>.FLAG(TestEnum._flag4) |
                    TypedFlags<TestEnum>.FLAG(TestEnum._flag9));


                for (TestEnum bitIndex = TestEnum._flag0; bitIndex <= TestEnum._flag31; ++bitIndex)
                {
                    if (bitIndex == TestEnum._flag4 || bitIndex == TestEnum._flag9)
                    {
                        Debug.Assert(flags.Test(bitIndex));
                        success &= flags.Test(bitIndex);
                    }
                    else
                    {
                        Debug.Assert(!flags.Test(bitIndex));
                        success &= !flags.Test(bitIndex);
                    }
                }
            }

            return success;
        }
    }
}

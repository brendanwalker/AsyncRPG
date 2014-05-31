using System;
using System.IO;

namespace AsyncRPGDBTool.UnitTests
{
    class UnitTestDriver
    {
        private TextWriter _logger;

        public UnitTestDriver(TextWriter logger)
        {
            _logger = logger;
        }

        public bool RunUnitTests(Command command)
        {
            bool success = true;
            int passCount = 0;
            int totalTests = 0;

            IUnitTest[] unitTests = new IUnitTest[] {
                new UnionFindUnionTest(_logger),
                new TypedFlagsUnitTest(_logger)
            };

            _logger.WriteLine("-Running Test Suite-");
            foreach (IUnitTest unitTest in unitTests)
            {
                if (unitTest.RunTest())
                {
                    _logger.WriteLine(string.Format(" {0} Passes", unitTest.GetTestName()));
                    passCount++;
                }
                else
                {
                    _logger.WriteLine(string.Format(" {0} FAILED!", unitTest.GetTestName()));
                    success = false;
                }

                totalTests++;
            }

            _logger.WriteLine("{0}/{1} Tests Passed", passCount, totalTests);
            _logger.WriteLine(success ? "ALL TESTS PASSED" : "ONE OR MORE TESTS FAILED!");

            return success;
        }
    }

}

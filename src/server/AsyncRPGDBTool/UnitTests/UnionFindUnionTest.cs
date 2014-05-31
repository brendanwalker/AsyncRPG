using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using AsyncRPGSharedLib.Navigation;

namespace AsyncRPGDBTool.UnitTests
{
    class UnionFindUnionTest : IUnitTest
    {
        private TextWriter _logger;

        public UnionFindUnionTest(TextWriter logger)
        {
            _logger = logger;
        }

        public string GetTestName()
        {
            return "UnionFind";
        }

        public bool RunTest()
        {
            string[] elements = new string[10] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };
            UnionFind<string> unionFind = new UnionFind<string>();
            bool success = true;

            // Add in entries
            for (int i = 0; i < 10; i++)
            {
                unionFind.AddElement(elements[i]);
            }

            // Verify that the roots are setup correctly
            for (int i = 0; i < 10; i++)
            {
                success &= unionFind.GetElement(i).Equals(elements[i]);
                Debug.Assert(success);
                success &= unionFind.FindRootIndex(i) == i;
                Debug.Assert(success);
                success &= unionFind.FindRootIndex(elements[i]) == i;
                Debug.Assert(success);
            }

            // Verify that no one is initially connected to anyone else
            for (int i = 1; i < 10; i++)
            {
                success &= !unionFind.AreElementsConnected(i - 1, i);
                Debug.Assert(success);
                success &= !unionFind.AreElementsConnected(elements[i - 1], elements[i]);
                Debug.Assert(success);
            }

            // Union the first half of the nodes together
            for (int i = 1; i < 5; i++)
            {
                unionFind.Union(elements[i - 1], elements[i]);
            }
            // Union the last half of the nodes together
            for (int i = 6; i < 10; i++)
            {
                unionFind.Union(elements[i - 1], elements[i]);
            }

            // Verify that every element in the first half is connected 
            // to every other element in the first half
            for (int i = 0; i < 5; i++)
            {
                for (int j = i + 1; j < 5; j++)
                {
                    success &= unionFind.AreElementsConnected(i, j);
                    Debug.Assert(success);
                    success &= unionFind.AreElementsConnected(elements[i], elements[j]);
                    Debug.Assert(success);
                }
            }

            // Verify that every element in the second half is connected 
            // to every other element in the second half
            for (int i = 5; i < 10; i++)
            {
                for (int j = i + 1; j < 10; j++)
                {
                    success &= unionFind.AreElementsConnected(i, j);
                    Debug.Assert(success);
                    success &= unionFind.AreElementsConnected(elements[i], elements[j]);
                    Debug.Assert(success);
                }
            }

            // Verify that no element in the first half is connected 
            // to any other element in the second half
            for (int i = 0; i < 5; i++)
            {
                for (int j = i + 5; j < 10; j++)
                {
                    success &= !unionFind.AreElementsConnected(i, j);
                    Debug.Assert(success);
                    success &= !unionFind.AreElementsConnected(elements[i], elements[j]);
                    Debug.Assert(success);
                }
            }

            return success;
        }
    }
}

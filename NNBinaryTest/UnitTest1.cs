using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NNBinaryTest
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// 具体的なコードのテストではなく仕様の確認など
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            byte b = 255;
            sbyte sb = (sbyte)b;
            Assert.AreEqual(-1, sb);
            byte b2 = b;
            Assert.AreEqual(b2, b);
        }
    }
}

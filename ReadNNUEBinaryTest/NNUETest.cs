using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReadNNBinary;
using System.IO;
using System.Linq;

namespace NNBinaryTest
{
    [TestClass]
    public class NNUETest
    {
        [TestMethod]
        public void TestGetBytes()
        {

            NNUE nnue = new NNUE();

            byte[] bytes = File.ReadAllBytes(@"resource\nn.bin");
            nnue.SetBytes(bytes);
            byte[] bytes2 = nnue.GetBytes();

            Assert.IsTrue(bytes.SequenceEqual(bytes2));
        }
    }
}

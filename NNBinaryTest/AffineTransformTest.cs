using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReadNNBinary;
using System.IO;
using System.Linq;

namespace NNBinaryTest
{
    [TestClass]
    public class AffineTransformTest
    {
        const int originalNNUEBytes = 64217066;
        const int originalNNUEFeatureTransformParamStart = 194;

        [TestMethod]
        public void TestGetBytes()
        {
            //テスト用nn.binを読み込んで、FeatureTransForm.GetBytes()の結果が元のバイト列と同じかどうか確かめます。

            byte[] bytes = File.ReadAllBytes(@"resource\nn.bin");

            NNUE nnue = new NNUE();
            nnue.SetBytes(bytes);
            byte[] bytes2 = nnue.FeatureAffineTransform.GetBytes();

            byte[] bytes3 = new byte[bytes2.Length];
            Array.Copy(bytes, originalNNUEFeatureTransformParamStart, bytes3, 0, bytes3.Length);


            Assert.IsTrue(bytes2.SequenceEqual(bytes3));
       }

        [TestMethod]
        public void TestEndian()
        {
            short s = 258;

            byte[] ary1 = new byte[2];

            ary1[0] = (byte)s;
            ary1[1] = (byte)(s >> 8);

            byte[] ary2 = BitConverter.GetBytes(s);

            Assert.AreEqual(ary1[0], ary2[0]);
            Assert.AreEqual(ary1[1], ary2[1]);
        }


    }
}

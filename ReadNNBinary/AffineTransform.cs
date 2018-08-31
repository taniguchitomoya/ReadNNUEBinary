using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadNNBinary
{

    /// <summary>
    /// アフィン変換のパラメーターに関する処理を行うクラスです。
    /// 
    /// 
    /// memo このプログラムではClippedReLUなどパラメーターを持たない部分は省略して書きます。
    ///  　　NNUEの場合はAffineTransformの配列やリスト等のみでNNの構造を表すことがあります。
    /// </summary>
    public class AffineTransform
    {
        public int InputDimension { get; private set; }
        public int OutputDimension { get; private set; }

        /// <summary>
        /// ファイル上でのバイト数を取得します
        /// 
        /// header部分4バイトは含まれません
        /// </summary>
        private int bytesLength
        {
            get
            {
                return OutputDimension * 4 + InputDimension * OutputDimension;
            }
        }


        /// <summary>
        /// バイアス項を表します。
        /// 
        /// 初期化時とサイズの違う配列を設定したときの動作は保証されません。
        /// </summary>
        public int[] Bias;

        /// <summary>
        /// 重みを表します。
        /// 
        /// 初期化時とサイズの違う配列を設定したときの動作は保証されません。
        /// </summary>
        public sbyte[,] Weight;

        public AffineTransform(int inputDimension, int outputDimension) {
            this.InputDimension = inputDimension;
            this.OutputDimension = outputDimension;
            this.Bias = new int[outputDimension];
            this.Weight = new sbyte[outputDimension, inputDimension];
        }

        /// <summary>
        /// NNUE評価関数ファイルからこのアフィン変換部分のパラメーターを取得します。
        /// </summary>
        /// <param name="bytes">NNUE評価関数ファイル全体</param>
        /// <param name="offset">このアフィン変換部分のスタート位置</param>
        /// <returns>次のアフィン変換部分のスタート位置（最後のレイヤーまで到達しているならばbytes.Lengthと一致）</returns>
        public int SetBytes(byte[] bytes, int offset) {

            for (int i = 0; i < OutputDimension; i++) {
                Bias[i] = BitConverter.ToInt32(bytes, offset);
                offset += 4;
            }

            for (int i = 0; i < OutputDimension; i++)
                for (int j = 0; j < InputDimension; j++)
                {
                    Weight[i, j] = (sbyte)bytes[offset++];
                }
            return offset;
        }


        /// <summary>
        /// このアフィン変換のパラメーターのバイト列を取得します
        /// （ヘッダ部分は含まない）
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            byte[] ret = new byte[bytesLength];
            int offset = 0;

            //bias
            for (int i = 0; i < OutputDimension; i++)
            {
                int s = Bias[i];
                //little endian
                ret[offset++] = (byte)s;
                ret[offset++] = (byte)(s >> 8);
                ret[offset++] = (byte)(s >> 16);
                ret[offset++] = (byte)(s >> 24);
            }

            //weight
            for (int i = 0; i < OutputDimension; i++)
                for (int j = 0; j < InputDimension; j++)
                {
                    sbyte s = Weight[i, j];
                    ret[offset++] = (byte)s;
                }

            System.Diagnostics.Debug.Assert(offset == bytesLength);
            return ret;
        }

    }
}

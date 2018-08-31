using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadNNBinary
{
    /// <summary>
    /// 特徴量のアフィン変換を表します
    /// 
    /// 特徴量のアフィン変換部分は他のアフィン変換とは違う部分があるので別処理をしています。
    /// </summary>
    public class FeatureAffineTransform
    {
        public int InputDimension { get; private set; }
        public int OutputDimension { get; private set; }

        /// <summary>
        /// ファイル上でのバイト数を取得します
        /// 
        /// header部分4バイトは含まれません
        /// </summary>
        private int bytesLength {
            get
            {
                return OutputDimension * 2 + InputDimension * OutputDimension * 2;
            }
        }


        /// <summary>
        /// バイアス項を表します。初期化時とサイズの違う配列を設定したときの動作は保証されません。
        /// </summary>
        public short[] Bias;

        /// <summary>
        /// 重みを表します。初期化時とサイズの違う配列を設定したときの動作は保証されません。
        /// 
        /// 差分計算を効率よく行う観点から添え字が他のアフィン変換と逆です
        /// </summary>
        public short[,] Weight;


        /// <summary>
        /// 入力と出力の数を与えて初期化します。
        /// </summary>
        /// <param name="inputDimension"></param>
        /// <param name="outputDimension">（手番を考慮して2倍にする前の値を入力してください）</param>
        public FeatureAffineTransform(int inputDimension, int outputDimension)
        {
            this.InputDimension = inputDimension;
            this.OutputDimension = outputDimension;
            this.Bias = new short[outputDimension];
            //差分計算を効率よく行う観点から添え字が他のアフィン変換と逆です
            this.Weight = new short[inputDimension, outputDimension];

        }

        /// <summary>
        /// NNUE評価関数ファイルからこのアフィン変換部分のパラメーターを取得します。
        /// </summary>
        /// <param name="bytes">NNUE評価関数ファイル全体</param>
        /// <param name="offset">このアフィン変換部分のスタート位置</param>
        /// <returns>どこまで読んだかを表すオフセット値</returns>
        public int Input(byte[] bytes, int offset)
        {
            for (int i = 0; i < OutputDimension; i++)
            {
                Bias[i] = BitConverter.ToInt16(bytes, offset);
                offset += 2;
            }

            //添え字が他のアフィン変換と逆です
            for (int i = 0; i < InputDimension; i++)
                for (int j = 0; j < OutputDimension; j++)
                {
                    Weight[i, j] = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
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
                short s = Bias[i];
                //little endian
                ret[offset++] = (byte)s;
                ret[offset++] = (byte)(s >> 8);
            }

            //weight
            for (int i = 0; i < InputDimension; i++)
                for (int j = 0; j < OutputDimension; j++)
                {
                    short s = Weight[i,j];
                    //little endian
                    ret[offset++] = (byte)s;
                    ret[offset++] = (byte)(s >> 8);
                }

            System.Diagnostics.Debug.Assert(offset == bytesLength);
            return ret;
        }
    }
}

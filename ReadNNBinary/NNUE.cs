using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadNNBinary
{

    /// <summary>
    /// NNUE評価関数を表します
    /// 
    /// 内部的にはNNUE評価関数ファイルのバイト列そのもの、ヘッダ情報なども保管します
    /// </summary>
    public class NNUE
    {
        /// <summary>
        /// Architecture文字列
        /// </summary>
        public string Architecture { get; private set; }

        private const int featureTransformHeaderSize = 4;
        private const int networkHeaderSize = 4;


        /// <summary>
        /// 評価関数ファイルのバイト列
        /// </summary>
        private byte[] bytes;

        /// <summary>
        /// 入力特徴量の数
        /// </summary>
        private int featureDimensions;

        /// <summary>
        /// 変換後の特徴量の数
        /// </summary>
        private int transformedFeatureDimensions { get { return networkDimensions[0] / 2; } }

        /// <summary>
        /// 各layerのノードの数が入っている配列
        /// </summary>
        private int[] networkDimensions;

        /// <summary>
        /// 入力特徴量のアフィン変換のパラメーター
        /// </summary>
        public FeatureAffineTransform FeatureAffineTransform { get; set; }

        /// <summary>
        /// アフィン変換のパラメーター（入力特徴量の変換部分を除く）
        /// </summary>
        public List<AffineTransform> Network { get; set; }

        /// <summary>
        /// ファイル上の特徴量のアフィン変換のスタート位置
        /// 
        /// 先頭にheader 4byteが来ることに留意する
        /// </summary>
        private int featureTransformStart;

        /// <summary>
        /// ファイル上のNNのスタート位置
        /// 
        /// 先頭にheader 4byteが来ることに留意する
        /// </summary>
        private int networkStart;

        /// <summary>
        /// オリジナルのパラメーターでNNUEを初期化します
        /// </summary>
        public NNUE() :this(125388, 512, 32, 32, 1){
        }

        /// <summary>
        /// 引数で指定したパラメーターでNNUEを初期化します
        /// </summary>
        /// <param name="featureDimensions">特徴量の数</param>
        /// <param name="networkDimensions">各レイヤーのノード数（nodeCount[0]は手番を考慮して2倍した後の数）</param>
        public NNUE(int featureDimensions, params int[] networkDimensions) {
            this.featureDimensions = featureDimensions;

            int length = networkDimensions.Length;
            this.networkDimensions = new int[length];
            Array.Copy(networkDimensions, this.networkDimensions, length);


            Network = new List<AffineTransform>();

            FeatureAffineTransform = new FeatureAffineTransform(this.featureDimensions, transformedFeatureDimensions);
            for (int i = 0; i < length - 1; i++) {
                AffineTransform at = new AffineTransform(networkDimensions[i], networkDimensions[i + 1]);
                Network.Add(at);
            }
        }

        /// <summary>
        /// NNUE評価関数ファイルのバイト列を設定します。
        /// </summary>
        /// <param name="bytes"></param>
        public void SetBytes(byte[] bytes) {
            this.bytes = bytes;
            input(bytes);
        }

        /// <summary>
        /// 入力処理
        /// </summary>
        /// <param name="bytes"></param>
        private void input(byte[] bytes)
        {
            int offset = 0;
            int version = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            int hash = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            int size = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            Architecture = System.Text.Encoding.UTF8.GetString(bytes,offset,size);
            offset += size;

            System.Diagnostics.Debug.WriteLine(size);
            System.Diagnostics.Debug.WriteLine(Architecture);
            System.Diagnostics.Debug.WriteLine(offset);
            //end header

            featureTransformStart = offset;

            int header = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            offset = FeatureAffineTransform.Input(bytes, offset);

            System.Diagnostics.Debug.WriteLine(offset);
            //end feature affine transform

            networkStart = offset;

            int header2 = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            foreach (AffineTransform at in Network)
                offset = at.SetBytes(bytes, offset);

            //end of file
            System.Diagnostics.Debug.WriteLine(bytes.Length);
            System.Diagnostics.Debug.WriteLine(offset);
        }



        /// <summary>
        /// NNUEファイルのバイト列を取得します
        /// 
        /// FeatureAffineTransformプロパティやNetworkプロパティを変更している場合、その変更を反映したものを出力します。
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes() {

            byte[] ret = new byte[bytes.Length];
            Array.Copy(bytes, ret, bytes.Length);

            byte[] bytesFeatureTransform = FeatureAffineTransform.GetBytes();
            bytesFeatureTransform.CopyTo(ret, featureTransformStart + featureTransformHeaderSize);

            int offset = networkStart + networkHeaderSize;
            foreach (AffineTransform af in Network)
            {
                byte[] b = af.GetBytes();
                b.CopyTo(ret, offset);
                offset += b.Length;
            }

            return ret;
        }



    }
}

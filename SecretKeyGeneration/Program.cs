using NBitcoin;
using System;
using System.IO;
using System.Reflection;

namespace SecretKeyGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            //private keyとビットコインアドレスを作成
            var bitcoinPrivateKey = (new Key()).GetWif(Network.TestNet);
            var address = bitcoinPrivateKey.GetAddress();
            string[] contents = { bitcoinPrivateKey.ToString(), address.ToString() };

            //ファイルに保存(追加保存)
            var baseDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            File.AppendAllLines(Path.Combine(baseDirectory, "key.txt"), contents);

            //この後、Bitcoin Core(testnet)を使って保存したビットコインアドレスにBTCを送信する
        }
    }
}

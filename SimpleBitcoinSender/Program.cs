using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleBitcoinSender
{
    //参考にしたサイト：https://programmingblockchain.gitbooks.io/programmingblockchain-japanese/content/
    //勉強用の為、例外処理とかはなく最低限のソースコードになっている
    class Program
    {
        static void Main(string[] args)
        {
            //コマンドライン引数から送信情報取得
            SenderInfo.TargetBtcAddress = args[0];      //送信対象のBitcoin address
            SenderInfo.TargetTxID = args[1];            //送信対象のtransaction id
            SenderInfo.Coin = decimal.Parse(args[2]);   //BTC

            //ファイル(key.txt)からprivate key取得
            var bitcoinPrivateKey = GetBitcoinPrivateKey();
            if (bitcoinPrivateKey != null)
            {
                //伝搬(ブロードキャスト)
                QBitNinjaClient client = new QBitNinjaClient(bitcoinPrivateKey.Network);
                var transaction = GetTransaction(bitcoinPrivateKey, client);
                BroadcastResponse broadcastResponse = client.Broadcast(transaction).Result;
                if (!broadcastResponse.Success)
                {
                    Console.WriteLine("ErrorCode: " + broadcastResponse.Error.ErrorCode);
                    Console.WriteLine("Error message: " + broadcastResponse.Error.Reason);
                }
                else
                {
                    Console.WriteLine("Success! You can check out the hash of the transaciton in any block explorer:");
                    Console.WriteLine(transaction.GetHash());
                }
            }

            Console.ReadLine();
        }

        private static BitcoinSecret GetBitcoinPrivateKey()
        {
            BitcoinSecret result = null;
            var baseDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            string keyFile = Path.Combine(baseDirectory, "key.txt");
            if (File.Exists(keyFile))
            {
                string[] keyAndAddress = File.ReadAllLines(keyFile);
                if (keyAndAddress.Length >= 2)
                {
                    result = new BitcoinSecret(keyAndAddress[0]);
                }
                else
                {
                    Console.WriteLine("Private key is invalid.");
                }
            }
            else
            {
                Console.WriteLine("There is no private key description file.");
            }

            return result;
        }

        private static Transaction GetTransaction(BitcoinSecret bitcoinPrivateKey, QBitNinjaClient client)
        {
            //トランザクション
            var transaction = new Transaction();

            //TransactionResponseの取得
            var transactionId = uint256.Parse(SenderInfo.TargetTxID);
            GetTransactionResponse transactionResponse = client.GetTransaction(transactionId).Result;

            //受け取ったコイン取得
            var receivedCoins = transactionResponse.ReceivedCoins;

            //トランザクションインプットの作成
            var outPointToSpend = GetOutPointToSpend(bitcoinPrivateKey, receivedCoins);
            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = outPointToSpend,
                ScriptSig = bitcoinPrivateKey.ScriptPubKey
            });

            //トランザクションアウトプットの作成
            SetTxOutputs(bitcoinPrivateKey, transaction, receivedCoins, outPointToSpend);

            //署名
            transaction.Sign(bitcoinPrivateKey, false);

            return transaction;
        }

        private static void SetTxOutputs(BitcoinSecret bitcoinPrivateKey, Transaction transaction, List<ICoin> receivedCoins, OutPoint outPointToSpend)
        {
            var hallOfTheMakersAddress = new BitcoinPubKeyAddress(SenderInfo.TargetBtcAddress, bitcoinPrivateKey.Network);
            // ビットコインをいくら送金先に送りたいか
            var hallOfTheMakersAmount = new Money(SenderInfo.Coin, MoneyUnit.BTC);
            var minerFee = new Money(0.0001m, MoneyUnit.BTC);//とりあえず固定
            // UTXOからいくらトータルでビットコインを 使いたいか。
            var txInAmount = receivedCoins[(int)outPointToSpend.N].TxOut.Value;
            Money changeBackAmount = txInAmount - hallOfTheMakersAmount - minerFee;
            TxOut hallOfTheMakersTxOut = new TxOut()
            {
                Value = hallOfTheMakersAmount,
                ScriptPubKey = hallOfTheMakersAddress.ScriptPubKey
            };

            TxOut changeBackTxOut = new TxOut()
            {
                Value = changeBackAmount,
                ScriptPubKey = bitcoinPrivateKey.ScriptPubKey
            };
            transaction.Outputs.Add(hallOfTheMakersTxOut);
            transaction.Outputs.Add(changeBackTxOut);
        }

        private static OutPoint GetOutPointToSpend(BitcoinSecret bitcoinPrivateKey, List<ICoin> receivedCoins)
        {
            OutPoint outPointToSpend = null;
            foreach (var coin in receivedCoins)
            {
                if (coin.TxOut.ScriptPubKey == bitcoinPrivateKey.ScriptPubKey)
                {
                    outPointToSpend = coin.Outpoint;
                }
            }
            if (outPointToSpend == null)
                throw new Exception("エラー：どのトランザクションアウトプットも送ったコインのScriptPubKeyを持ってない！");

            return outPointToSpend;
        }

        class SenderInfo
        {
            public static string TargetTxID { get; set; }
            public static string TargetBtcAddress { get; set; }
            public static decimal Coin { get; set; }
        }
    }
}

■概要
このソリューションはC#を使ってビットコインを送信する事を目的として作成した勉強用プログラムである。
ビットコインの知識0からだいたい2日ぐらいでかけて作成。
1日目：ビットコインの概要を本やサイトを見て勉強、またBitcoin Coreをインストールして使ってみる。
2日目：NBitcoinのサイト(https://programmingblockchain.gitbooks.io/programmingblockchain-japanese/content/)を見て作成。


■使い方
①SecretKeyGenerationプロジェクトを実行し、keyファイルを作成する。
  keyファイルには、1行目に秘密鍵、2行目にビットコインアドレスが記述される。
  keyファイルはReadMe.txtと同フォルダに作成される。

②Bitcoin Core(testnet)で、keyファイルのビットコインアドレスへビットコインを送信する。

③Bitcoin Core(testnet)やblock explorerなどを使い、②で送信した時のトランザクションIDを取得する。

④SimpleBitcoinSenderのコマンドライン引数に
		送信先ビットコインアドレス
		トランザクションID（③で取得したもの）
		送信するビットコイン
  の3つを指定して実行する。

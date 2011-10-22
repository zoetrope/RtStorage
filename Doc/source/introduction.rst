
-----------------------
イントロダクション
-----------------------

RtStorageとは
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
RtStorageはRTミドルウェアのためのデータ記録・再生用ツールです。

* RTコンポーネントのOutPortから出力されたデータをファイルに記録することができます。
* 保存したデータを、RTコンポーネントのInPortに対して再生することができます。
* データの再生は保存と同じタイミングで行われます。また、任意の位置から再生を開始することができます。
* 保存されたデータは、いくつかの検索条件で簡単に見つけ出すことができます。
* IDLファイルを書かなくても、ユーザが独自定義したデータを扱うことができます。
* 保存されたデータの解析を行うことができます。


対象RTミドルウェア
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
RtStorageは、以下のRTミドルウェアに対応しています。

* OpenRTM-aist(C++版) 1.0.0-RELEASE
* OpenRTM-aist(C++版) 1.1.0-RC2
* OpenRTM-aist(Java版) 1.0.0-RELEASE
* OpenRTM-aist(Java版) 1.1.0-RC1
* OpenRTM-aist(Python版) 1.0.1-RELEASE
* OpenRTM-aist(Python版) 1.1.0-RC1

RTミドルウェアに関する情報については http://www.openrtm.org/ を参照してください。

ライセンス
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Copyright (c) 2011 zoetrope. All Rights Reserved.
Licensed undear a `Microsoft Permissive License (Ms-PL)`_.

.. _`Microsoft Permissive License (Ms-PL)`: http://chainingassertion.codeplex.com/license


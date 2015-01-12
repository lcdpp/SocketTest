using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class EventTest : MonoBehaviour {

	private Button m_button_login;
	private InputField m_input_account;

	// Use this for initialization
	void Start () {
		m_button_login = this.GetComponent<Button> ();
		m_input_account = this.GetComponent<InputField>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
		
	public void OnClickLoginButton(){
		ConnectServer();
	}

	public void ConnectServer()
	{
/*		IPAddress ipAdr = IPAddress.Parse("101.226.182.5") ;
		IPEndPoint ipEp = new IPEndPoint(ipAdr, 7000);
		Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		UnityEngine.Debug.Log("请求连接");
		clientSocket.Connect(ipEp);
		if(clientSocket.Connected)
		{
			UnityEngine.Debug.Log("连接成功，准备发送数据");
		}
		clientSocket.Shutdown(SocketShutdown.Both);
		clientSocket.Close();*/

		LoginUserCmd.stUserRequestTDCodeCmd cmd = new LoginUserCmd.stUserRequestTDCodeCmd();
		cmd.Init();
		cmd.loginCmd.nullCmd.dwTimeStamp = 721078554;
		cmd.game = 1;
		cmd.zone = 3002;
		cmd.wdNetType = 0;
	//	cmd.macAddr = Enumerable.Repeat<char>('1', 13).ToArray();
	//	cmd.uuid = Enumerable.Repeat<byte>(0x01, 25).ToArray();
		cmd.macAddr = new char[13];
		cmd.macAddr[0] = '7';
		cmd.macAddr[1] = '4';
		cmd.macAddr[2] = 'D';
		cmd.macAddr[3] = '0';
		cmd.macAddr[4] = '2';
		cmd.macAddr[5] = 'B';
		cmd.macAddr[6] = 'C';
		cmd.macAddr[7] = '4';
		cmd.macAddr[8] = 'B';
		cmd.macAddr[9] = '9';
		cmd.macAddr[10] = 'E';
		cmd.macAddr[11] = 'D';
		cmd.macAddr[12] = (char)0;

		cmd.uuid = new byte[25];
		cmd.uuid[0] = 0x1a;
		cmd.uuid[1] = 0x56;
		cmd.uuid[2] = 0x63;
		cmd.uuid[3] = 0x56;
		cmd.uuid[4] = 0xe3;
		cmd.uuid[5] = 0x56;
		cmd.uuid[6] = 0x43;
		cmd.uuid[7] = 0x56;
		cmd.uuid[8] = 0xe3;
		cmd.uuid[9] = 0x55;
		cmd.uuid[10] = 0xc3;
		cmd.uuid[11] = 0x56;
		cmd.uuid[12] = 0xc3;
		cmd.uuid[13] = 0x55;
		cmd.uuid[14] = 0xc3;
		cmd.uuid[15] = 0x56;
		cmd.uuid[16] = 0xd3;
		cmd.uuid[17] = 0x56;
		cmd.uuid[18] = 0xc3;
		cmd.uuid[19] = 0x55;
		cmd.uuid[20] = 0xc3;
		cmd.uuid[21] = 0x56;
		cmd.uuid[22] = 0xb3;
		cmd.uuid[23] = 0x55;
		cmd.uuid[24] = 0x93;

// 		byte[] buffer = JFSocket.StructToBytes(cmd);
// 		LoginUserCmd.stUserRequestTDCodeCmd cmd2 = (LoginUserCmd.stUserRequestTDCodeCmd)JFSocket.BytesToStruct(buffer, typeof(LoginUserCmd.stUserRequestTDCodeCmd));
// 		Debug.Log("cmd:" + cmd2.loginCmd.nullCmd.byCmd + " parar:" + cmd2.loginCmd.nullCmd.byParam);
// 
// 		byte[] buffer_zipped = Compression.ZipCompression.compressMemory(buffer);
// 		Debug.Log("zip length:" + buffer_zipped.Length);
// 
// 		byte[] buffer_unzip = Compression.ZipCompression.decompressMemory(buffer_zipped);
// 		Debug.Log("unzip length" + buffer_unzip.Length);
// 
// 		Rc5_v2.Rc5 rc5 = new Rc5_v2.Rc5();
// 		MemoryStream inputStream = new MemoryStream();
// 		inputStream.Write(buffer, 0, buffer.Length);
// 		inputStream.Seek(0, SeekOrigin.Begin);
// 		MemoryStream outputStream = new MemoryStream();
// 		rc5.Encrypt(inputStream, outputStream);
// 
// 		MemoryStream decodeStream = new MemoryStream();
// 		outputStream.Seek(0, SeekOrigin.Begin);
// 		rc5.Decrypt(outputStream, decodeStream);
// 
// 		byte[] buffer_rc5 = decodeStream.ToArray();
// 		UnityEngine.Debug.Log("decode rc5 length : " + buffer_rc5.Length);

/*        byte[] buffer = new byte[14] { 0x68, 0x78, 0x6c, 0x7f, 0x66, 0x33, 0xcc, 0xcc, 0xcc, 0xcc, 0x3a, 0x54, 0x33, 0x01 };
        LoginUserCmd.stUserVerifyVerCmd cmd2 = new LoginUserCmd.stUserVerifyVerCmd();
        cmd2 = (LoginUserCmd.stUserVerifyVerCmd)JFSocket.BytesToStruct(buffer, cmd2.GetType());
        byte[] zip_buffer = Compression.ZipCompression.compressMemory(buffer);

        JFSocket.stPackageHeader header = new JFSocket.stPackageHeader();
        header.flag = JFSocket.PACKET_FLAG_ZIP;

        //发送长度
        uint sendsize = ((uint)zip_buffer.Length + (uint)Marshal.SizeOf(header) + (uint)7) & (uint)(0xfffffff8);

		header.flag |= (uint)sendsize - (uint)Marshal.SizeOf(header);
		header.flag |= JFSocket.PACKET_FLAG_ENC;
		byte[] headdata = JFSocket.StructToBytes(header);
		
		byte[] senddata = new byte[sendsize];
		Array.Copy(headdata, 0, senddata, 0, headdata.Length);
		Array.Copy(zip_buffer, 0, senddata, headdata.Length, zip_buffer.Length);
		
		MyRC5.test_encrypt(senddata);*/

        //RC5加密
//         MemoryStream inputStream = new MemoryStream(senddata);
//         MemoryStream outputStream = new MemoryStream();
//         byte[] key = new byte[16] { 0x3f, 0x79, 0xd5, 0xe2, 0x4a, 0x8c, 0xb6, 0xc1, 0xaf, 0x31, 0x5e, 0xc7, 0xeb, 0x9d, 0x6e, 0xcb };
//         Rc5_v2.Rc5 m_Login_EncDec = new Rc5_v2.Rc5(key, 12);
//         m_Login_EncDec.Encrypt(inputStream, outputStream);
// 
//         senddata = outputStream.ToArray();

		JFSocket.GetInstance().SendMessage(cmd);
	}
}

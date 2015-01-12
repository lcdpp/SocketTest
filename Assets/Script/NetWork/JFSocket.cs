using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class JFSocket
{
	public const uint PACKET_MAX_SIZE = 0x00ffffff;
	public const uint PACKET_FLAG_ENC = 0x80000000;
	public const uint PACKET_FLAG_ZIP = 0x40000000;

	private MyRC5 m_Login_EncDec;

	public struct stMsg
	{
		public uint size;
		public byte[] buffer;
	}

	//结构体序列化
	[System.Serializable]
	//1字节对齐 iphone 和 android上可以1字节对齐
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct stPackageHeader
	{
		public uint flag;
	}

	//Socket客户端对象
	private Socket clientSocket;

	//接收消息线程
	Thread m_mecieve_thread;

	//接收队列锁
	private static object m_recieve_lock;

	//收到的数据包
	public Queue<stMsg> m_recieve_package;

	//单例模式
	private static JFSocket instance;
	public static JFSocket GetInstance()
	{
		if (instance == null)
		{
			instance = new JFSocket();
		}
		return instance;
	}  	
	
	//单例的构造函数
	JFSocket()
	{
		//登录时用RC5加密
		byte[] keyData = new byte[16]{0x3f, 0x79, 0xd5, 0xe2, 0x4a, 0x8c, 0xb6, 0xc1, 0xaf, 0x31, 0x5e, 0xc7, 0xeb, 0x9d, 0x6e, 0xcb};
		m_Login_EncDec = new MyRC5(keyData);

        //创建消息队列锁
        m_recieve_lock = new object();

        //创建消息队列
        m_recieve_package = new Queue<stMsg>();
        
        //创建Socket对象， 这里我的连接类型是TCP
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	}

	~JFSocket()
	{
		Disconnect();
	}

	public void Connect()
    {
        //创建Socket对象， 这里我的连接类型是TCP
        if (!clientSocket.Connected)
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		//服务器IP地址
        IPAddress ipAddress = IPAddress.Parse("101.226.182.5");
		//服务器端口
		IPEndPoint ipEndpoint = new IPEndPoint (ipAddress, 7000);
		//这是一个异步的建立连接，当连接建立成功时调用connectCallback方法
		IAsyncResult result = clientSocket.BeginConnect (ipEndpoint,new AsyncCallback (connectCallback),clientSocket);
		//这里做一个超时的监测，当连接超过5秒还没成功表示超时
		bool success = result.AsyncWaitHandle.WaitOne( 5000, true );
		if ( !success )
		{
			//超时
			Disconnect();
			Debug.Log("connect Time Out");
		}else
		{
			//与socket建立连接成功，开启线程接受服务端数据。
			m_mecieve_thread = new Thread(new ThreadStart(ReceiveSorket));
			m_mecieve_thread.IsBackground = true;
			m_mecieve_thread.Start();
			
			LoginUserCmd.SendGameVersion();
		}
	}

	public void Disconnect()
	{
		if(clientSocket != null 
			&& clientSocket.Connected)
		{
			clientSocket.Shutdown(SocketShutdown.Both);
			clientSocket.Close();
			Debug.Log("Close Socket");
		}
		clientSocket = null;

		// 等待线程结束
		if(m_mecieve_thread != null)
			m_mecieve_thread.Join();
		
		lock(m_recieve_lock)
		{
			m_recieve_package.Clear();
		}
	}
	
	private void connectCallback(IAsyncResult asyncConnect)
	{
		Debug.Log("connectSuccess");
	}
	
	private void ReceiveSorket()
	{
		//在这个线程中接受服务器返回的数据
		while (true)
		{
			if(!clientSocket.Connected)
			{
				//与服务器断开连接跳出循环
				Debug.Log("[ReceiveSocket] Socket disconnect and Break Thread");
				clientSocket.Close();
				break;
			}
			try
			{
				//接受数据保存至bytes当中
				byte[] bytes = new byte[65536];
				//Receive方法中会一直等待服务端回发消息
				//如果没有回发会一直在这里等着。
				int i = clientSocket.Receive(bytes);
				if(i <= 0)
				{
					clientSocket.Close();
					Debug.Log("[ReceiveSocket] Recieve data " + i + ", close socket");
					continue;
				}	

				//储存到消息对列
				OnRecvData(bytes, (uint)i);
				Debug.Log("[ReceiveSocket] receive one package. size : " + i);
			}
			catch (Exception e)
			{
				Debug.Log("[ReceiveSocket] Socket recieve error." + e);
				clientSocket.Close();
				break;
			}
		}
	}

	private void OnRecvData(byte[] data, uint size)
	{
		// 解密
		byte[] encData = new byte[(size + 7) & 0xfffffff8];
		Array.Copy(data, encData, encData.Length);
		m_Login_EncDec.Decrypt(encData);

		stPackageHeader header = new stPackageHeader();
		byte[] headerdata = new byte[Marshal.SizeOf(header)];
		Array.Copy(encData, 0, headerdata, 0, headerdata.Length);
		header = (stPackageHeader)BytesToStruct(headerdata, header.GetType());

		uint datasize = header.flag & PACKET_MAX_SIZE;
		header.flag = header.flag & (~PACKET_MAX_SIZE);

		// 数据完整
		if (datasize + headerdata.Length <= size)
		{
			if((header.flag & PACKET_FLAG_ZIP) != 0)
			{
				byte[] zipData = new byte[datasize];
				Array.Copy(encData, headerdata.Length, zipData, 0, zipData.Length);
				byte[] cmdData = Compression.ZipCompression.decompressMemory(zipData);
				PushToRecvQueue(cmdData);
			}
			else
			{
				uint cmdsize = datasize;
				byte[] cmdData = new byte[cmdsize];
				Array.Copy(encData, headerdata.Length, cmdData, 0, cmdData.Length);
				PushToRecvQueue(cmdData);
			}
		}
	}

	private void PushToRecvQueue(byte[] bytes)
	{
		lock(m_recieve_lock)
		{
			stMsg _msg = new stMsg();
			_msg.size = (uint)bytes.Length;
			_msg.buffer = bytes;
			m_recieve_package.Enqueue(_msg) ;
		}
	}

	public stMsg PopRecvMsg()
	{
		lock(m_recieve_lock)
		{
			if(m_recieve_package.Count > 0)
				return m_recieve_package.Dequeue();

			return new stMsg();
		}
	}
	
	//向服务端发送一条字符串
	//一般不会发送字符串 应该是发送数据包
	public void SendMessage(string str)
	{
		byte[] msg = Encoding.UTF8.GetBytes(str);
		
		if(!clientSocket.Connected)
		{
			clientSocket.Close();
			Debug.Log("[ERROR][SendMessage]socket not connected");
			return;
		}
		try
		{
			//int i = clientSocket.Send(msg);
			IAsyncResult asyncSend = clientSocket.BeginSend (msg,0,msg.Length,SocketFlags.None,new AsyncCallback (sendCallback),clientSocket);
			bool success = asyncSend.AsyncWaitHandle.WaitOne( 5000, true );
			if ( !success )
			{
				clientSocket.Close();
				Debug.Log("[ERROR][SendMessage]Failed to SendMessage server. Close Socket");
			}
		}
		catch
		{
			Debug.Log("send message error" );
		}
	}
	
	//向服务端发送数据包，也就是一个结构体对象
	public void SendMessage(object obj)
	{		
		if(!clientSocket.Connected)
		{
			clientSocket.Close();
			Debug.Log("[ERROR][SendMessage]socket not connected");

			Debug.Log("[ERROR][SendMessage]retry connect");
			Connect();

			if(!clientSocket.Connected)
			{
				clientSocket.Close();
				Debug.Log("[ERROR][SendMessage]retry conncet failed");
				return;
			}
		}

		try
		{
			Debug.Log("Send Message");

			//先得到数据包的长度
			short size = (short)Marshal.SizeOf(obj);
			//把结构体对象转换成数据包，也就是字节数组
			byte[] data = StructToBytes(obj);

			//数据压缩
			byte[] zip_data = Compression.ZipCompression.compressMemory(data);

			stPackageHeader header = new stPackageHeader();
			header.flag = PACKET_FLAG_ZIP;

			//发送长度，8字节对齐
			uint sendsize = ((uint)zip_data.Length + (uint)Marshal.SizeOf(header) + (uint)7) & (uint)(0xfffffff8);

			header.flag |= (uint)sendsize - (uint)Marshal.SizeOf(header);
			header.flag |= PACKET_FLAG_ENC;
			byte[] headdata = StructToBytes(header);

			byte[] senddata = new byte[sendsize];
			Array.Copy(headdata, 0, senddata, 0, headdata.Length);
			Array.Copy(zip_data, 0, senddata, headdata.Length, zip_data.Length);

            //RC5加密
			m_Login_EncDec.Encrpyt(senddata);
			
			//向服务端异步发送这个字节数组
			IAsyncResult asyncSend = clientSocket.BeginSend (senddata,0,(int)sendsize,SocketFlags.None,new AsyncCallback (sendCallback),clientSocket);
			//监测超时
			bool success = asyncSend.AsyncWaitHandle.WaitOne( 5000, true );
			if ( !success )
			{
				clientSocket.Close();
				Debug.Log("[ERROR][SendMessage]Failed to SendMessage server. Close Socket");
			}
		}
		catch (Exception e)
		{
			Debug.Log("send message error: " + e );
		}
	}
	
	//结构体转字节数组
	public static byte[] StructToBytes(object structObj)
	{
		
		int size = Marshal.SizeOf(structObj);
		IntPtr buffer =  Marshal.AllocHGlobal(size);
		try
		{
			Marshal.StructureToPtr(structObj,buffer,false);
			byte[]  bytes  =   new byte[size];
			Marshal.Copy(buffer, bytes,0,size);
			return bytes;
		}
		finally
		{
			Marshal.FreeHGlobal(buffer);
		}
	}

	//字节数组转结构体
	public static object BytesToStruct(byte[] bytes, Type strcutType)
	{
		int size = Marshal.SizeOf(strcutType);
		IntPtr buffer = Marshal.AllocHGlobal(size);
		try
		{
			Marshal.Copy(bytes,0,buffer,size);
			return  Marshal.PtrToStructure(buffer, strcutType);
		}
		finally
		{
			Marshal.FreeHGlobal(buffer);
		}   
		
	}
	
	private void sendCallback (IAsyncResult asyncSend)
	{
		
	}
}
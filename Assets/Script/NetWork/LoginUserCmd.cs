using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class LoginUserCmd
{
	const byte LOGON_USERCMD = 104;

	//结构体序列化
	[System.Serializable]
	//1字节对齐 iphone 和 android上可以1字节对齐
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct stNullUserCmd
	{
		public byte byCmd;
		public byte byParam;
		public uint dwTimeStamp;
	}

	//结构体序列化
	[System.Serializable]
	//1字节对齐 iphone 和 android上可以1字节对齐
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct stLoginUserCmd
	{
		public stNullUserCmd nullCmd;

		public void Init()
		{
			nullCmd.byCmd = LOGON_USERCMD;
		}
	}
	
	// 请求二维码
	const byte USER_REQUEST_TDCODE_PARA = 18;

	//结构体序列化
	[System.Serializable]
	//1字节对齐 iphone 和 android上可以1字节对齐
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct stUserRequestTDCodeCmd
	{
		public stLoginUserCmd loginCmd;

		public ushort game;
		public ushort zone;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
		public char[] macAddr;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
		public byte[] uuid;
		
		public ushort wdNetType;

		public void Init()
		{
			loginCmd.Init();
			loginCmd.nullCmd.byParam = USER_REQUEST_TDCODE_PARA;
		}
	}

	// 客户端验证版本
	const byte USER_VERIFY_VER_PARA = 120;
	//结构体序列化
	[System.Serializable]
	//1字节对齐 iphone 和 android上可以1字节对齐
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct stUserVerifyVerCmd
	{
		public stLoginUserCmd loginCmd;

		public uint reserve;	// 保留字段
		public uint version;

		public void Init()
		{
			loginCmd.Init();
			loginCmd.nullCmd.byParam = USER_VERIFY_VER_PARA;
		}
	}

	public static void SendGameVersion()
	{
		stUserVerifyVerCmd cmd = new stUserVerifyVerCmd();
		cmd.Init();
		cmd.version = 20141227;
		JFSocket.GetInstance().SendMessage(cmd);
	}
}

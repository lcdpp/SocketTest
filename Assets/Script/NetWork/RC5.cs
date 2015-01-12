/*
 * programmed by george batres
 * according to RC5 algorithm
 * with a little change
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace Rc5_v2
{
	class Rc5
	{
		uint[] s;
		uint[] l;
		uint b, u, t, c;
		byte[] key;
		int rounds;
		
		public Rc5()
		{
			string str = "georgebatres";
			key = GetKeyFromString(str);
			rounds = 16;
			b = (uint)key.Length;
			u = 4;
			t = (uint)(34);
			c = 12 / u;
			s = new uint[34];
			l = new uint[12];
			
			GenerateKey(key, rounds);
		}
		
		public Rc5(string password,int round)
		{
			key = GetKeyFromString(password);
			rounds = round;
			b = (uint)key.Length;
			u = 4;
			t = (uint)(2 * rounds + 2);
			c = Math.Max(b, 1) / u;
			s = new uint[2 * rounds + 2];
			l = new uint[key.Length];
			
			GenerateKey(key, rounds);
		}
		
		public Rc5(byte[] password, int round)
		{
			rounds = round;
			key = password;
			b = (uint)password.Length;
			u = 4;
			t = (uint)(2 * rounds + 2);
			c = Math.Max(b, 1) / u;
			s = new uint[2 * rounds + 2];
			l = new uint[password.Length];
			
			GenerateKey(key, rounds);
		}
		//to circulate int left
		private uint leftRotate(uint x, int offset)
		{
			uint t1, t2;
			t1 = x >> (32 - offset);
			t2 = x << offset;
			return t1 | t2;
		}
		//to circulate int right
		private uint RightRotate(uint x, int offset)
		{
			uint t1, t2;
			t1 = x << (32 - offset);
			t2 = x >> offset;
			return t1 | t2;
		}
		//encryption operation on two block
		private void Encode(ref uint r1, ref uint r2, int rounds)
		{
			r1 = r1 + s[0];
			r2 = r2 + s[1];
			for (int i = 1; i <= rounds; i++)
			{
				r1 = leftRotate(r1 ^ r2, (int)r2) + s[2 * i];
				r2 = leftRotate(r2 ^ r1, (int)r1) + s[2 * i + 1];
			}
		}
		//decryption operation on two block
		private void Decode(ref uint r1, ref uint r2, int rounds)
		{
			for (int i = rounds; i >= 1; i--)
			{
				r2 = (RightRotate(r2 - s[2 * i + 1], (int)r1)) ^ r1;
				r1 = (RightRotate(r1 - s[2 * i], (int)r2)) ^ r2;
			}
			r2 = r2 - s[1];
			r1 = r1 - s[0];
		}
		
		private void GenerateKey(byte[] key, int rounds)
		{
			uint P32 = uint.Parse("b7e15163", System.Globalization.NumberStyles.HexNumber);
			uint Q32 = uint.Parse("9e3779b9", System.Globalization.NumberStyles.HexNumber);
			
			
			for (int i = key.Length - 1; i >= 0; i--)
			{
				l[i] = leftRotate((uint)i, 8) + key[i];
			}
			
			s[0] = P32;
			for (int i = 1; i <= t - 1; i++)
			{
				s[i] = s[i - 1] + Q32;
			}
			
			uint ii, jj;
			ii = jj = 0;
			uint x, y;
			x = y = 0;
			uint v = 3 * Math.Max(t, c);
			//mixing key arrayes
			for (int counter = 0; counter <= v; counter++)
			{
				x = s[ii] = leftRotate((s[ii] + x + y), 3);
				y = l[jj] = leftRotate((l[jj] + x + y), (int)(x + y));
				ii = (ii + 1) % t;
				jj = (jj + 1) % c;
			}
		}
		//convert key from string to byte array
		private byte[] GetKeyFromString(string str)
		{
			char[] mykeyinchar = str.ToCharArray();
			byte[] mykeyinbytes = new byte[mykeyinchar.Length];
			for (int i = 0; i < mykeyinchar.Length; i++)
			{
				mykeyinbytes[i] = (byte)mykeyinchar[i];
			}
			return mykeyinbytes;
		}
		
		public void Encrypt(Stream streamreader,Stream streamwriter)
		{
			uint r1, r2;
			
			System.IO.BinaryReader br = new System.IO.BinaryReader(streamreader);
			System.IO.BinaryWriter bw = new System.IO.BinaryWriter(streamwriter);
			long filelength = streamreader.Length;
			while (filelength > 0)
			{
				try
				{
					r1 = br.ReadUInt32();
					try
					{
						r2 = br.ReadUInt32();
					}
					catch
					{
						r2 = 0;
					}
				}
				catch
				{
					r1 = r2 = 0;
				}
				
				Encode(ref r1, ref r2, rounds);
				bw.Write(r1);
				bw.Write(r2);
				
				filelength -= 8;
			}
			
		//	streamreader.Close();
		//	streamwriter.Close();
			
		}
		
		public void Decrypt(Stream streamreader,Stream streamwriter)
		{
			uint r1, r2;
			
			System.IO.BinaryReader br = new System.IO.BinaryReader(streamreader);
			System.IO.BinaryWriter bw = new System.IO.BinaryWriter(streamwriter);
			long filelength = streamreader.Length;
			while (filelength > 0)
			{
				try
				{
					r1 = br.ReadUInt32();
					r2 = br.ReadUInt32();
					Decode(ref r1, ref r2, rounds);
					
					if (!(r1 == 0 && r2 == 0 && (filelength - 8 <= 0)))
					{
						bw.Write(r1);
						bw.Write(r2);
					}
					if (r2 == 0 && (filelength - 8 <= 0))
					{
						bw.Write(r1);
					}
					filelength -= 8;
				}
				catch
				{
				//	System.Windows.Forms.MessageBox.Show("May be U try to decrypt an normal file (plain file)", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
					UnityEngine.Debug.Log("May be U try to decrypt an normal file (plain file)");
					return;
				}
			}
			
		//	streamreader.Close();
		//	streamwriter.Close();
		}
		
	}
}


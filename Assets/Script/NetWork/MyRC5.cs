using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

class MyRC5
{
    const int RC5_32_BLOCK = 8;
    const int RC5_32_KEY_LENGTH = 16;
    
    const int RC5_8_ROUNDS = 8;
    const int RC5_12_ROUNDS = 12;
    const int RC5_16_ROUNDS = 16;

    const uint RC5_32_MASK = 0XFFFFFFFF;

    const ushort RC5_16_P = 0xB7E1;
    const ushort RC5_16_Q = 0x9E37;
    const uint RC5_32_P = 0xB7E15163;
    const uint RC5_32_Q = 0x9E3779B9;
    const ulong RC5_64_P = 0xB7E151628AED2A6B;
    const ulong RC5_64_Q = 0x9E3779B97F4A7C15;

    struct rc5_key_st
    {
        public int rounds;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2 * (RC5_16_ROUNDS + 1))]
        public uint[] data;
    }

	rc5_key_st key;

	public MyRC5(byte[] keyData)
	{
		RC5_32_SET_KEY(ref key, 16, keyData, 12);
	}

    void c2ln(byte[] c, ref uint index, ref uint l1, ref uint l2, ref uint n)
    {
        index += n;
        l1 = 0;
        l2 = 0;
        if (n >= 8)
            l2 = ((uint)(c[--index])) << 24;
        if (n >= 7)
            l2 |= ((uint)(c[--index])) << 16;
        if (n >= 6)
            l2 |= ((uint)(c[--index])) << 8;
        if (n >= 5)
            l2 |= ((uint)(c[--index]));
        if (n >= 4)
            l1 = ((uint)(c[--index])) << 24;
        if (n >= 3)
            l1 |= ((uint)(c[--index])) << 16;
        if (n >= 2)
            l1 |= ((uint)(c[--index])) << 8;
        if (n >= 1)
            l1 |= ((uint)(c[--index]));
    }

    void c2l(byte[] c, ref uint index, ref uint l)
    {
        l = ((uint)(c[index++]));
        l |= ((uint)(c[index++])) << 8;
        l |= ((uint)(c[index++])) << 16;
        l |= ((uint)(c[index++])) << 24;
    }

    //to circulate int left
    private uint ROTATE_l32(uint a, uint n)
    {
        uint t1, t2;
        t1 = a >> (32 - (int)n);
        t2 = a << (int)n;
        return t1 | t2;
    }

    //to circulate int right
    private uint ROTATE_r32(uint a, uint n)
    {
        uint t1, t2;
        t1 = a << (32 - (int)n);
        t2 = a >> (int)n;
        return t1 | t2;
    }

    void E_RC5_32(ref uint a, ref uint b, uint[] s, uint n)
    {
        a ^= b;
        a = ROTATE_l32(a, b);
        a += s[n];
        a &= RC5_32_MASK;
        b ^= a;
        b = ROTATE_l32(b, a);
        b += s[n + 1];
        b &= RC5_32_MASK;
    }

    void D_RC5_32(ref uint a, ref uint b, uint[] s, uint n)
    {
        b -= s[n + 1];
        b &= RC5_32_MASK;
        b = ROTATE_r32(b, a);
        b ^= a;
        a -= s[n];
        a &= RC5_32_MASK;
        a = ROTATE_r32(a, b);
        a ^= b;
    }

    void RC5_32_SET_KEY(ref rc5_key_st key, int len, byte[] data, int rounds)
    {
        uint[] L = new uint[64];
        uint l = 0;
        uint ll = 0;
        uint A = 0;
        uint B = 0;
        uint k = 0;

        int i = 0;
        int j = 0;
        int m = 0;
        int c = 0;
        int t = 0;
        int ii = 0;
        int jj = 0;

        uint data_index = 0;

        if((rounds != RC5_16_ROUNDS)
            && (rounds != RC5_12_ROUNDS)
            && (rounds != RC5_8_ROUNDS))
            rounds = RC5_16_ROUNDS;

        key.rounds = rounds;
        key.data = new uint[2 * (RC5_16_ROUNDS + 1)];

        uint[] S = key.data;
        j = 0;
        for(i = 0; i <= (len - 8); i += 8)
        {
            c2l(data, ref data_index, ref l);
            L[j++] = l;
            c2l(data, ref data_index, ref l);
            L[j++] = l;
        }
        ii = len - i;
        if(ii != 0)
        {
            k = (uint)len & 0x07;
            c2ln(data, ref data_index, ref l, ref ll, ref k);
        }

        c = (len + 3) / 4;
        t = (rounds + 1) * 2;
        S[0] = RC5_32_P;

        for(i = 1; i < t; i++)
            S[i] = (S[i-1] + RC5_32_Q) & RC5_32_MASK;

        j = (t > c) ? t : c;
        j *= 3;
        ii = jj = 0;
        A = B = 0;

        for(i = 0; i < j; i++)
        {
            k = (S[ii] + A + B) & RC5_32_MASK;
            A = S[ii] = ROTATE_l32(k, (uint)3);
            m = (int)(A + B);
            k = (L[jj] + A + B) & RC5_32_MASK;
            B = L[jj] = ROTATE_l32(k, (uint)m);
            if(++ii >= t) ii = 0;
            if(++jj >= c) jj = 0;
        }
    }

    void RC5_32_encrypt(uint[] d, ref rc5_key_st key)
    {
        uint a = 0;
        uint b = 0;
        uint[] s = key.data;
        a = d[0] + s[0];
        b = d[1] + s[1];
        E_RC5_32(ref a, ref b, s, 2);
        E_RC5_32(ref a, ref b, s, 4);
        E_RC5_32(ref a, ref b, s, 6);
        E_RC5_32(ref a, ref b, s, 8);
        E_RC5_32(ref a, ref b, s, 10);
        E_RC5_32(ref a, ref b, s, 12);
        E_RC5_32(ref a, ref b, s, 14);
        E_RC5_32(ref a, ref b, s, 16);
        if(key.rounds == 12)
        {
            E_RC5_32(ref a, ref b, s, 18);
            E_RC5_32(ref a, ref b, s, 20);
            E_RC5_32(ref a, ref b, s, 22);
            E_RC5_32(ref a, ref b, s, 24);
        }
        else if(key.rounds == 16)
        {
            E_RC5_32(ref a, ref b, s, 18);
            E_RC5_32(ref a, ref b, s, 20);
            E_RC5_32(ref a, ref b, s, 22);
            E_RC5_32(ref a, ref b, s, 24);
            E_RC5_32(ref a, ref b, s, 26);
            E_RC5_32(ref a, ref b, s, 28);
            E_RC5_32(ref a, ref b, s, 30);
            E_RC5_32(ref a, ref b, s, 32);
        }
        d[0] = a;
        d[1] = b;
    }

    void RC5_32_decrypt(uint[] d, ref rc5_key_st key)
    {
        uint a = 0;
        uint b = 0;
        uint[] s = key.data;
        a = d[0];
        b = d[1];
        if(key.rounds == 16)
        {
            D_RC5_32(ref a, ref b, s, 32);
            D_RC5_32(ref a, ref b, s, 30);
            D_RC5_32(ref a, ref b, s, 28);
            D_RC5_32(ref a, ref b, s, 26);
            D_RC5_32(ref a, ref b, s, 24);
            D_RC5_32(ref a, ref b, s, 22);
            D_RC5_32(ref a, ref b, s, 20);
            D_RC5_32(ref a, ref b, s, 18);
        }
        else if(key.rounds == 12)
        {
            D_RC5_32(ref a, ref b, s, 24);
            D_RC5_32(ref a, ref b, s, 22);
            D_RC5_32(ref a, ref b, s, 20);
            D_RC5_32(ref a, ref b, s, 18);
        }
        D_RC5_32(ref a, ref b, s, 16);
        D_RC5_32(ref a, ref b, s, 14);
        D_RC5_32(ref a, ref b, s, 12);
        D_RC5_32(ref a, ref b, s, 10);
        D_RC5_32(ref a, ref b, s, 8);
        D_RC5_32(ref a, ref b, s, 6);
        D_RC5_32(ref a, ref b, s, 4);
        D_RC5_32(ref a, ref b, s, 2);
        d[0] = a - s[0];
        d[1] = b - s[1];
    }

    public void Encrpyt(byte[] data)
    {
		uint count = (uint)data.Length / 8;
        for (int i = 0; i < count; ++i)
        {
            uint[] buff_encode = new uint[2];
			buff_encode[0] = System.BitConverter.ToUInt32(data, i * 8);
			buff_encode[1] = System.BitConverter.ToUInt32(data, i * 8 + 4);
            RC5_32_encrypt(buff_encode, ref key);
			Array.Copy(BitConverter.GetBytes(buff_encode[0]), 0, data, i * 8, 4);
			Array.Copy(BitConverter.GetBytes(buff_encode[1]), 0, data, i * 8 + 4, 4);
        }
    }

	public void Decrypt(byte[] data)
	{
		uint count = (uint)data.Length / 8;
		for (int i = 0; i < count; ++i)
		{
			uint[] data_decode = new uint[2];
			data_decode[0] = System.BitConverter.ToUInt32(data, i * 8);
			data_decode[1] = System.BitConverter.ToUInt32(data, i * 8 + 4);
			RC5_32_decrypt(data_decode, ref key);
			Array.Copy(BitConverter.GetBytes(data_decode[0]), 0, data, i * 8, 4);
			Array.Copy(BitConverter.GetBytes(data_decode[1]), 0, data, i * 8 + 4, 4);
		}
	}
}
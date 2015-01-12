using UnityEngine;
using System.Collections;
using System.IO;

namespace Compression
{
	public class ZipCompression
	{
        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[input.Length * 2];
            int len;
			while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public static void compressFile(string inFile, string outFile)
        {
            System.IO.FileStream outFileStream = new System.IO.FileStream(outFile, System.IO.FileMode.Create);
            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outFileStream, zlib.zlibConst.Z_DEFAULT_COMPRESSION);
            System.IO.FileStream inFileStream = new System.IO.FileStream(inFile, System.IO.FileMode.Open);
            try
            {
                CopyStream(inFileStream, outZStream);
				outZStream.finish();
            }
            finally
            {
                outZStream.Close();
                outFileStream.Close();
                inFileStream.Close();
            }
        }

        public static byte[] compressMemory(byte[] inBuff)
        {
            MemoryStream inMemStream = new MemoryStream(inBuff);
            MemoryStream outMemStream = new MemoryStream();
            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outMemStream, zlib.zlibConst.Z_DEFAULT_COMPRESSION);
            try
            {
                CopyStream(inMemStream, outZStream);
				outZStream.finish();
            }
            finally
            {
                outZStream.Close();
                inMemStream.Close();
            }
            return outMemStream.ToArray();
        }

        public static void decompressFile(string inFile, string outFile)
        {
            System.IO.FileStream outFileStream = new System.IO.FileStream(outFile, System.IO.FileMode.Create);
            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outFileStream);
            System.IO.FileStream inFileStream = new System.IO.FileStream(inFile, System.IO.FileMode.Open);
            try
            {
                CopyStream(inFileStream, outZStream);
				outZStream.finish();
            }
            finally
            {
                outZStream.Close();
                outFileStream.Close();
                inFileStream.Close();
            }
        }

        public static byte[] decompressMemory(byte[] inBuff)
        {
            MemoryStream inMemStream = new MemoryStream(inBuff);
			MemoryStream outMemStream = new MemoryStream();
			zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outMemStream);
            try
            {
				CopyStream(inMemStream, outZStream);
				outZStream.finish();
            }
            finally
            {
				outZStream.Close();
                inMemStream.Close();
            }
            return outMemStream.ToArray();
        }
	}
}
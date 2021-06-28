using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GnarlyGameStudio.Serializer
{
    public class BridgeStream
    {
        public byte[] Buffer { get; set; }
        public int ReadIndex { get; private set; }
        public int WriteIndex { get; private set; }
        private int _capacity;
        private const int DefaultCapacity = 16;
        public bool Empty => WriteIndex == 0;
        public bool HasMore => WriteIndex > ReadIndex;
        public bool CheckMore(int needIndex) => WriteIndex >= ReadIndex + needIndex;

        public BridgeStream()
        {
            Clear();
        }

        public BridgeStream(byte[] data)
        {
            Buffer = data;
            _capacity = Buffer.Length;
            WriteIndex = _capacity;
        }

        public void SetCapacity(int length)
        {
            _capacity = length;
            WriteIndex = _capacity;
        }

        public void AppendToSource(byte[] data, int length)
        {
            if (Buffer == null)
            {
                Buffer = new byte[DefaultCapacity];
                _capacity = Buffer.Length;
                WriteIndex = 0;
            }

            GrowBuffer(length);
            System.Buffer.BlockCopy(data, 0, Buffer, WriteIndex, length);
            WriteIndex += length;
        }

        private void WriteByteArray(byte[] bytes)
        {
            GrowBuffer(bytes.Length);
            System.Buffer.BlockCopy(bytes, 0, Buffer, WriteIndex, bytes.Length);
            WriteIndex += bytes.Length;
        }

        private void GrowBuffer(int length)
        {
            var isNeedResize = false;
            while (WriteIndex + length > _capacity)
            {
                _capacity *= 2;
                isNeedResize = true;
            }

            if (isNeedResize)
            {
                var resizeBuffer = new byte[_capacity];
                System.Buffer.BlockCopy(Buffer, 0, resizeBuffer, 0, WriteIndex);
                Buffer = resizeBuffer;
            }
        }

        public void Write(string value)
        {
            if (value == null)
                value = string.Empty;
            var bytes = Encoding.UTF8.GetBytes(value);
            Write(bytes.Length);
            WriteByteArray(bytes);
        }

        public unsafe void Write(int value)
        {
            GrowBuffer(4);

            fixed (byte* bufferPointer = Buffer)
            {
                *(int*) (bufferPointer + WriteIndex) = value;
            }

            WriteIndex += 4;
        }

        public void Write(byte value)
        {
            GrowBuffer(1);
            Buffer[WriteIndex] = value;
            WriteIndex += 1;
        }

        public byte ReadByte()
        {
            var value = Buffer[ReadIndex];
            ReadIndex++;
            return value;
        }

        public unsafe void Write(float value)
        {
            GrowBuffer(4);

            fixed (byte* bufferPointer = Buffer)
            {
                *(float*) (bufferPointer + WriteIndex) = value;
            }

            WriteIndex += 4;
        }


        public unsafe void Write(long value)
        {
            GrowBuffer(8);

            fixed (byte* bufferPointer = Buffer)
            {
                *(long*) (bufferPointer + WriteIndex) = value;
            }

            WriteIndex += 8;
        }

        public void Write(int[] value)
        {
            Write(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                Write(value[i]);
            }
        }

        public void Write(List<int> value)
        {
            Write(value.Count);
            for (var i = 0; i < value.Count; i++)
            {
                Write(value[i]);
            }
        }

        public void Write(List<string> value)
        {
            Write(value.Count);
            for (var i = 0; i < value.Count; i++)
            {
                Write(value[i]);
            }
        }

        public void Write(List<float> value)
        {
            Write(value.Count);
            for (var i = 0; i < value.Count; i++)
            {
                Write(value[i]);
            }
        }


        public byte[] Encode()
        {
            var finalArray = new byte[WriteIndex];
            System.Buffer.BlockCopy(Buffer, 0, finalArray, 0, WriteIndex);
            return finalArray;
        }

        public int ReadInt()
        {
            var value = ToInt(Buffer, ReadIndex);
            ReadIndex += 4;
            return value;
        }

        private int ToInt(byte[] buffer, int startIndex)
        {
            return buffer[startIndex] | (buffer[startIndex + 1] << 8) | (buffer[startIndex + 2] << 16) |
                   (buffer[startIndex + 3] << 24);
        }

        private unsafe float ToFloat(byte[] buffer, int startIndex)
        {
            var val = ToInt(buffer, startIndex);
            return *(float*) &val;
        }

        public int[] ReadIntArray()
        {
            var length = ReadInt();
            var value = new int[length];
            for (var i = 0; i < value.Length; i++)
            {
                value[i] = ReadInt();
            }

            return value;
        }

        public List<int> ReadIntList()
        {
            var length = ReadInt();
            var value = new List<int>(length);
            for (var i = 0; i < length; i++)
            {
                value.Add(ReadInt());
            }

            return value;
        }

        public List<string> ReadStringList()
        {
            var length = ReadInt();
            var value = new List<string>(length);
            for (var i = 0; i < length; i++)
            {
                value.Add(ReadString());
            }

            return value;
        }

        public List<float> ReadFloatList()
        {
            var length = ReadInt();
            var value = new List<float>(length);
            for (var i = 0; i < length; i++)
            {
                value.Add(ReadFloat());
            }

            return value;
        }

        public float ReadFloat()
        {
            var value = ToFloat(Buffer, ReadIndex);
            ReadIndex += 4;
            return value;
        }

        public List<long> ReadLongList()
        {
            var length = ReadInt();
            var value = new List<long>(length);
            for (var i = 0; i < length; i++)
            {
                value.Add(ReadLong());
            }

            return value;
        }

        public long ReadLong()
        {
            var data = BitConverter.ToInt64(Buffer, ReadIndex);
            ReadIndex += 8;
            return data;
        }

        public string ReadString()
        {
            var length = ReadInt();
            var value = Encoding.UTF8.GetString(Buffer, ReadIndex, length);
            ReadIndex += length;
            return value;
        }

        public void Write(byte[] data)
        {
            Write(data.Length);
            WriteByteArray(data);
        }

        public void Write(Vector3 vector3)
        {
            Write(vector3.x);
            Write(vector3.y);
            Write(vector3.z);
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        public byte[] ReadByteArray()
        {
            var length = ReadInt();
            var data = new byte[length];
            if (length != 0)
                Array.Copy(Buffer, ReadIndex, data, 0, data.Length);
            ReadIndex += length;
            return data;
        }

        public void Write(BridgeStream bridgeStream)
        {
            if (bridgeStream == null)
            {
                Write(0);
                return;
            }

            var data = bridgeStream.Encode();
            Write(data);
        }

        public BridgeStream ReadStream()
        {
            var packet = new BridgeStream(ReadByteArray());
            return packet;
        }

        public void Write(IBridgeSerializer bridgeSerializer)
        {
            var packet = new BridgeStream();
            bridgeSerializer.Write(packet);
            Write(packet);
        }

        public T Read<T>() where T : IBridgeSerializer, new()
        {
            var returnObject = new T();
            var packet = ReadStream();
            returnObject.Read(packet);

            return returnObject;
        }

        public IBridgeSerializer Read(Type type)
        {
            var returnObject = (IBridgeSerializer) Activator.CreateInstance(type);
            var packet = ReadStream();
            returnObject.Read(packet);
            return returnObject;
        }

        public void Clear()
        {
            Buffer = new byte[DefaultCapacity];
            _capacity = DefaultCapacity;
            WriteIndex = 0;
            ReadIndex = 0;
        }

        public void ClearKeepBuffer()
        {
            WriteIndex = 0;
            ReadIndex = 0;
        }

        public void Write(Quaternion quaternion)
        {
            Write(quaternion.x);
            Write(quaternion.y);
            Write(quaternion.z);
            Write(quaternion.w);
        }

        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public void WriteArray<T>(T[] bridgeSerializer) where T : IBridgeSerializer
        {
            Write(bridgeSerializer.Length);
            foreach (var serializer in bridgeSerializer)
            {
                Write(serializer);
            }
        }

        public T[] ReadArray<T>() where T : IBridgeSerializer, new()
        {
            var length = ReadInt();
            var array = new T[length];

            for (var i = 0; i < length; i++)
            {
                array[i] = Read<T>();
            }

            return array;
        }

        public void WriteList<T>(List<T> bridgeSerializer) where T : IBridgeSerializer
        {
            Write(bridgeSerializer.Count);
            foreach (var serializer in bridgeSerializer)
            {
                Write(serializer);
            }
        }

        public List<T> ReadList<T>() where T : IBridgeSerializer, new()
        {
            var length = ReadInt();
            var list = new List<T>(length);

            for (var i = 0; i < length; i++)
            {
                list.Add(Read<T>());
            }

            return list;
        }

        public IList ReadList(Type type)
        {
            var length = ReadInt();
            var genericListType = typeof(List<>).MakeGenericType(type);
            var list = (IList) Activator.CreateInstance(genericListType);

            for (var i = 0; i < length; i++)
            {
                list.Add(Read(type));
            }

            return list;
        }

        public void Write(bool data)
        {
            Write((byte) (data ? 1 : 0));
        }

        public bool ReadBool()
        {
            return ReadByte() == 1;
        }

        public static implicit operator bool(BridgeStream stream)
        {
            return stream != null;
        }
    }
}
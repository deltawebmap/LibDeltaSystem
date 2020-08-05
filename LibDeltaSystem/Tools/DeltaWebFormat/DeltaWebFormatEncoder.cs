using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Linq;
using LibDeltaSystem.Tools.DeltaWebFormat.Attributes;
using Newtonsoft.Json;

namespace LibDeltaSystem.Tools.DeltaWebFormat
{
    public class DeltaWebFormatEncoder
    {
        public Stream output;
        private List<string> nameTable;
        private uint sign;
        private Type dataType;

        public DeltaWebFormatEncoder(Stream output, Type dataType)
        {
            this.output = output;
            this.dataType = dataType;
            nameTable = new List<string>();
        }

        public void Encode(object[] data, Dictionary<byte, byte[]> customData)
        {
            //Build name table
            foreach (var o in data)
                BuildNameTable(o);

            //Write file info
            WriteFixedString("DWMX"); //File type
            WriteInt32(nameTable.Count); //Name table length
            WriteUInt32(1); //File version
            WriteUInt32((uint)data.Length); //Number of items

            //Write custom data
            var customKeys = customData.Keys;
            WriteByte((byte)customKeys.Count);
            foreach(var k in customKeys)
            {
                WriteByte(k);
                WriteByte((byte)customData[k].Length);
                WriteBytes(customData[k]);
            }

            //Write name table
            foreach (var n in nameTable)
                WriteLongLengthedString(n);

            //Write struct definition
            WriteStructDefinitions(dataType);

            //Write all items
            foreach (var n in data)
                SerializeObject(n);

            //Finally, write an end of file notice
            //This also serves as a check to make sure that the file was correctly read
            WriteFixedString("https://deltamap.net");
        }

        /* TYPES IN STRUCT:
         * IND  LEN NAME                CUSTOM DATA
         * 0    0   Name table string    
         * 1    ?   Object              Definition of object    
         * 2    0   Bool
         * 3    0   String              
         * 4    0   Object Array
         * 5    0   Bool Array
         * 6    0   Int32 Array
         * 7    0   Float Array
         * 8    0   Int32
         * 9    0   Float
         * 10   0   DateTime (as string)
         * 11   0   String Array
         */

        private void WriteStructDefinitions(Type rt)
        {
            var props = rt.GetProperties();
            WriteByte((byte)props.Length);
            foreach (var p in props)
            {
                //Write type
                byte type = GetTypeId(p);
                WriteByte(type);

                //Write name
                WriteShortLengthedString(p.Name);

                //Write extras
                if(type == 1)
                {
                    //Write props for object
                    WriteStructDefinitions(p.PropertyType);    
                }
                if(type == 4)
                {
                    //Write props for array type
                    WriteStructDefinitions(p.PropertyType.GetElementType());
                }
            }
        }

        private void BuildNameTable(object d)
        {
            //Look for entries
            var tType = d.GetType();
            var props = d.GetType().GetProperties();
            foreach(var p in props)
            {
                bool flagUseObject = p.CustomAttributes.Where(x => x.AttributeType == typeof(WebFormatAttributeUseObject)).Count() > 0;

                //If this is an object, read it
                if (p.PropertyType.IsClass && flagUseObject)
                {
                    //Read this
                    object data = p.GetValue(d);
                    if(data != null)
                    {
                        BuildNameTable(data);
                    }
                }

                //If this is an array, read all
                if(p.PropertyType.IsArray && p.PropertyType.IsClass && flagUseObject)
                {
                    object[] data = (object[])p.GetValue(d);
                    if(data != null)
                    {
                        foreach(var dd in data)
                        {
                            if (dd == null)
                                continue;
                            BuildNameTable(dd);
                        }
                    }
                }

                //If this is a string, check if it is a name table element
                if(p.PropertyType == typeof(string))
                {
                    bool flagUseNameTable = p.CustomAttributes.Where(x => x.AttributeType == typeof(WebFormatAttributeUseNameTable)).Count() > 0;
                    if(flagUseNameTable)
                    {
                        //Read this and add it to the name table
                        string data = (string)p.GetValue(d);
                        if(data != null && !nameTable.Contains(data))
                        {
                            nameTable.Add(data);
                        }
                    }
                }
            }
        }

        private byte GetTypeId(PropertyInfo info)
        {
            //Get some info
            Type pType = info.PropertyType;

            //Get flags
            bool flagUseNameTable = info.CustomAttributes.Where(x => x.AttributeType == typeof(WebFormatAttributeUseNameTable)).Count() > 0;
            bool flagUseObject = info.CustomAttributes.Where(x => x.AttributeType == typeof(WebFormatAttributeUseObject)).Count() > 0;

            //Return type
            if (pType == typeof(string) && flagUseNameTable)
                return 0;
            if (pType == typeof(bool))
                return 2;
            if (pType == typeof(string))
                return 3;
            if (pType == typeof(bool[]) && pType.IsArray)
                return 5;
            if ((pType == typeof(int[]) || pType == typeof(short[])) && pType.IsArray)
                return 6;
            if (pType == typeof(float[]) && pType.IsArray)
                return 7;
            if (pType == typeof(int) || pType == typeof(short))
                return 8;
            if (pType == typeof(float))
                return 9;
            if (pType == typeof(DateTime))
                return 10;
            if (pType == typeof(string[]) && pType.IsArray)
                return 11;
            if (pType == typeof(double))
                return 12;
            if (pType == typeof(ushort))
                return 13;

            //Default values
            if (pType.IsClass && pType.IsArray && flagUseObject)
                return 4;
            if (pType.IsClass && flagUseObject)
                return 1;
            throw new NotSupportedException();
        }

        private void SerializeObject(object d)
        {
            var props = d.GetType().GetProperties();
            foreach (var p in props)
            {
                //Read info
                bool flagUseNameTable = p.CustomAttributes.Where(x => x.AttributeType == typeof(WebFormatAttributeUseNameTable)).Count() > 0;
                bool flagUseObject = p.CustomAttributes.Where(x => x.AttributeType == typeof(WebFormatAttributeUseObject)).Count() > 0;

                //Set up
                byte type = GetTypeId(p);
                byte flags = 0;
                object field = p.GetValue(d);

                //Check if it's null
                if(field == null)
                {
                    flags |= 1 << 7;
                    WriteByte(flags);
                    continue;
                }

                //Make room for the flags
                long start = output.Position;
                WriteByte(0);

                //Write
                if (type == 0)
                {
                    //Name table
                    WriteUInt16((ushort)nameTable.IndexOf((string)field));
                }
                else if (type == 1)
                {
                    //Object
                    SerializeObject(field);
                }
                else if (type == 2)
                {
                    //Bool
                    //We're just going to set a flag
                    if ((bool)field)
                        flags |= 1 << 0;
                }
                else if (type == 3)
                {
                    //String
                    //Get bytes
                    byte[] data = Encoding.UTF8.GetBytes((string)field);

                    //Write if this uses a short string (1 byte length) or long string (2 bytes length)
                    if (data.Length > byte.MaxValue)
                    {
                        WriteUInt16((ushort)data.Length);
                        flags |= 1 << 0;
                    }
                    else
                    {
                        WriteByte((byte)data.Length);
                    }

                    //Write
                    WriteBytes(data);
                }
                else if (type == 4)
                {
                    //Write array of objects
                    WriteArray<object>(ref flags, field, (object w) =>
                    {
                        SerializeObject(w);
                    });
                }
                else if (type == 5)
                {
                    //TODO
                    WriteArray<bool>(ref flags, field, (bool w) =>
                    {
                        if (w)
                            WriteByte(0x01);
                        else
                            WriteByte(0x00);
                    });
                }
                else if (type == 6)
                {
                    //TODO
                    WriteArray<int>(ref flags, field, (int w) =>
                    {
                        WriteInt32(w);
                    });
                }
                else if (type == 7)
                {
                    //TODO
                    WriteArray<float>(ref flags, field, (float w) =>
                    {
                        float r = (float)w;
                        byte[] data = BitConverter.GetBytes(r);
                        if (data.Length != 4)
                            throw new NotSupportedException();
                        WriteBytes(data);
                    });
                }
                else if (type == 8)
                {
                    WriteInt32((int)field);
                }
                else if (type == 9)
                {
                    //TODO
                    float r = (float)field;
                    byte[] data = BitConverter.GetBytes(r);
                    if (data.Length != 4)
                        throw new NotSupportedException();
                    WriteBytes(data);
                }
                else if (type == 10)
                {
                    //DateTime
                    //Write in number of seconds since Jan 1, 2020
                    int seconds = (int)(((DateTime)field) - new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                    WriteInt32(seconds);
                }
                else if (type == 11)
                {
                    //Determine the length to use
                    int max = 0;
                    foreach (var e in (string[])field)
                    {
                        if (e != null)
                            max = Math.Max(max, Encoding.UTF8.GetByteCount(e));
                    }

                    //Set flags for length
                    bool useLongString = (max > byte.MaxValue);
                    if (useLongString)
                        flags |= 1 << 4;

                    //Write strings
                    WriteArray<string>(ref flags, field, (string w) =>
                    {
                        if (!useLongString)
                            WriteShortLengthedString(w);
                        else
                            WriteLongLengthedString(w);
                    });
                }
                else if (type == 12)
                {
                    //Double. We're going to convert this to a single though
                    double dataDouble = (double)field;
                    float dataSingle = (float)dataDouble;
                    byte[] data = BitConverter.GetBytes(dataSingle);
                    if (data.Length != 4)
                        throw new NotSupportedException();
                    WriteBytes(data);
                }
                else if (type == 13)
                {
                    WriteUInt16((ushort)field);
                }
                else
                {
                    throw new NotSupportedException();
                }

                //Write flags
                long end = output.Position;
                output.Position = start;
                WriteByte(flags);
                output.Position = end;
            }
        }

        delegate void WriteArray_WriteEntry<T>(T data);
        private void WriteArray<T>(ref byte flags, object value, WriteArray_WriteEntry<T> write)
        {
            //Get length
            T[] data = (T[])value;
            int length = data.Length;

            //Determine length
            if (data.Length > byte.MaxValue)
            {
                WriteUInt16((ushort)data.Length);
                flags |= 1 << 0;
            }
            else
            {
                WriteByte((byte)data.Length);
            }

            //Count null entries
            int nullCount = 0;
            foreach(var d in data)
            {
                if (d == null)
                    nullCount++;
            }

            //If there are any null entries, but not all of them are, we'll need to include null flags
            bool nullFlags = (nullCount > 0) && (nullCount < data.Length);
            if(nullFlags)
            {
                flags |= 1 << 1;
            }

            //If all are null, don't write anything but a flag signaling that
            bool allNull = nullCount == data.Length;
            if (allNull)
            {
                flags |= 1 << 2;
                return;
            }

            //Write entries
            foreach(var d in data)
            {
                //Add flag in this
                if(nullFlags)
                {
                    if(d == null)
                    {
                        WriteByte(0x01);
                    } else
                    {
                        WriteByte(0x00);
                    }
                }

                //Write
                if(d != null)
                    write(d);
            }
        }

        private void WriteByte(byte d)
        {
            output.WriteByte(d);
        }

        private void WriteBytes(byte[] data)
        {
            output.Write(data, 0, data.Length);
        }

        private void WriteEndianBytes(byte[] data)
        {
            WriteBytes(data);
        }

        private void WriteFixedString(string data)
        {
            WriteBytes(Encoding.ASCII.GetBytes(data));
        }

        private void WriteShortLengthedString(string s)
        {
            byte[] data = Encoding.UTF8.GetBytes(s);
            if (data.Length > byte.MaxValue)
                throw new Exception("String too long for short string!");
            WriteByte((byte)data.Length);
            WriteBytes(data);
        }

        private void WriteLongLengthedString(string s)
        {
            byte[] data = Encoding.UTF8.GetBytes(s);
            if (data.Length > ushort.MaxValue)
                throw new Exception("String too long for long string!");
            WriteBytes(BitConverter.GetBytes(((ushort)data.Length)));
            WriteBytes(data);
        }

        private void WriteInt32(int value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        private void WriteUInt32(uint value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        private void WriteInt16(short value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        private void WriteUInt16(ushort value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
    }
}

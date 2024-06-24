using System;
using System.Collections.Generic;
using System.IO;
using ComponentAce.Compression.Libs.zlib;
using Lidgren.Network;
using Newtonsoft.Json;
using SWFServer.Data.Entities;

namespace SWFServer.Data
{
    public static class Util
    {
        public static Vector2w[] offset4 = new[] {new Vector2w(0, 1), new Vector2w(0, -1), new Vector2w(1, 0), new Vector2w(-1, 0)};
        public static Vector3w[] offset3d4 = new[] {new Vector3w(0, 1, 0), new Vector3w(0, -1, 0), new Vector3w(1, 0, 0), new Vector3w(-1, 0, 0)};
        public static Vector2w[] offset8 = new[]
        {
            new Vector2w(0, 1), new Vector2w(0, -1), new Vector2w(1, 0), new Vector2w(-1, 0),
            new Vector2w(1, 1), new Vector2w(1, -1), new Vector2w(-1, -1), new Vector2w(-1, 1)
        };

        public static Vector3w[] offset3d8 = new[]
        {
            new Vector3w(0, 1, 0), new Vector3w(0, -1, 0), new Vector3w(1, 0, 0), new Vector3w(-1, 0, 0),
            new Vector3w(1, 1, 0), new Vector3w(1, -1, 0), new Vector3w(-1, -1, 0), new Vector3w(-1, 1, 0)
        };

        public static Random rnd = new Random();

        public static int GetDistance(Vector2w p1, Vector2w p2)
        {
            return Math.Max(Math.Abs(p1.x - p2.x), Math.Abs(p1.y - p2.y));
        }

        public static Vector2w GetDirection(Vector2w p1, Vector2w p2)
        {
            var d = p2 - p1;
            d.Normalize();
            return d;
        }

        public static bool IsDiagonal(Vector2w p1, Vector2w p2)
        {
            if (p1.x == p2.x || p1.y == p2.y)
                return false;
            return true;
        }

        public static bool IsDiagonal(int x1, int y1, int x2, int y2)
        {
            if (x1 == x2 || y1 == y2)
                return false;
            return true;
        }

        public static Vector2w GetRandom4(Vector2w p)
        {
            int r = rnd.Next(0, 4);
            return p + offset4[r];
        }

        public static int Range(int minValue, int maxValue)
        {
            return rnd.Next(minValue, maxValue);
        }

        public static double Range()
        {
            return rnd.NextDouble();
        }

        public static float Clamp(float val, float min, float max)
        {
            if (val < min)
                val = min;
            if (val > max)
                val = max;
            return val;
        }

        public static void Log(string filename, string text)
        {
            using (StreamWriter writer = File.AppendText(filename))
            {
                writer.WriteLine(text);
            }
        }

        public static void LogDate(string filename, string text)
        {
            using (StreamWriter writer = File.AppendText(filename))
            {
                writer.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + text);
            }
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static string ReadString(NetIncomingMessage msg)
        {
            int size = msg.ReadInt32();
            char[] text = new char[size];
            for (int i = 0; i < size; i++)
                text[i] = (char)msg.ReadUInt16();
            return new string(text);
        }

        public static void WriteString(NetOutgoingMessage msg, string str)
        {
            msg.Write((int)str.Length);
            for (int i = 0; i < str.Length; i++)
                msg.Write((ushort)str[i]);
        }

        public static string ReadString(BinaryReader reader)
        {
            int size = reader.ReadInt32();
            char[] text = new char[size];
            for (int i = 0; i < size; i++)
                text[i] = (char)reader.ReadUInt16();
            return new string(text);
        }

        public static void WriteString(BinaryWriter writer, string str)
        {
            writer.Write((int)str.Length);
            for (int i = 0; i < str.Length; i++)
                writer.Write((ushort)str[i]);
        }

        public static void CopyStream(Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[65536];
            int len;
            while ((len = input.Read(buffer, 0, 65536)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public static void CompressData(Stream inStream, out MemoryStream outStream)
        {
            inStream.Seek(0, SeekOrigin.Begin);
            outStream = new MemoryStream();
            ZOutputStream zip = new ZOutputStream(outStream, zlibConst.Z_DEFAULT_COMPRESSION);
            CopyStream(inStream, zip);
            zip.finish();
            outStream.Seek(0, SeekOrigin.Begin);
        }

        public static void DecompressData(Stream inStream, out MemoryStream outStream)
        {
            inStream.Seek(0, SeekOrigin.Begin);
            outStream = new MemoryStream();
            ZOutputStream zip = new ZOutputStream(outStream);
            CopyStream(inStream, zip);
            zip.finish();
            outStream.Seek(0, SeekOrigin.Begin);
        }

        public static Vector2w ToGrid(Vector2f p)
        {
            int ax = (int) Math.Floor(p.x / GameConst.mapGrid);
            int ay = (int) Math.Floor(p.y / GameConst.mapGrid);
            return new Vector2w(ax, ay);
        }

        public static Vector2w ToMapGrid(Vector2w p)
        {
            return new Vector2w(p.x / GameConst.mapGrid, p.y / GameConst.mapGrid);
        }

        public static Vector2w ToMapUnitGrid(Vector2w p)
        {
            return new Vector2w(p.x / GameConst.mapUnitGrid, p.y / GameConst.mapUnitGrid);
        }

        public static Vector2w SizeMapUnitGrid(Vector2w p)
        {
            var s = new Vector2w(p.x / GameConst.mapUnitGrid, p.y / GameConst.mapUnitGrid);
            if (p.x % GameConst.mapUnitGrid != 0)
                s.x++;
            if (p.y % GameConst.mapUnitGrid != 0)
                s.y++;
            return s;
        }

        public static Vector2w GridToMapPos(Vector2w p)
        {
            return new Vector2w(p.x * GameConst.mapGrid, p.y * GameConst.mapGrid);
        }

        public static Vector2f ToVector2F(Vector2w pos)
        {
            return new Vector2f(pos.x * GameConst.cellSize, pos.y * GameConst.cellSize);
        }

        public static Vector2w ToVector2W(Vector2f pos)
        {
            if (pos.x < 0)
                return Vector2w.Empty;
            if (pos.y < 0)
                return Vector2w.Empty;

            return new Vector2w((int)(pos.x / GameConst.cellSize), (int)(pos.y / GameConst.cellSize));
        }
        /*
        public static T JsonRead<T>(BinaryReader reader)
        {
            string s = ReadString(reader);
            return JsonConvert.DeserializeObject<T>(s);
        }

        public static void JsonWrite<T>(BinaryWriter writer, T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            WriteString(writer, json);
        }
        */
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace SWFServer.Data
{
    public struct Vector2wTime
    {
        public Vector2w pos;
        public double time;

        public Vector2wTime(Vector2w pos, double time)
        {
            this.pos = pos;
            this.time = time;
        }

        public static Vector2wTime Read(NetIncomingMessage reader)
        {
            double t = reader.ReadDouble();
            var p = new Vector2w(reader.ReadInt16(), reader.ReadInt16());
            var v = new Vector2wTime(p, t);

            return v;
        }

        public void Write(NetOutgoingMessage writer)
        {
            writer.Write(time);
            writer.Write(pos.x);
            writer.Write(pos.y);
        }
    }

}

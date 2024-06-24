using System.Collections.Generic;
using System.IO;

namespace SWFServer.Data
{
    public class UserRating
    {
        public uint Id;
        public float val;

        public void Write(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(val);
        }

        public static UserRating Read(BinaryReader reader)
        {
            UserRating r = new UserRating();
            r.Id = reader.ReadUInt32();
            r.val = reader.ReadSingle();
            return r;
        }
    }
    public static class Rating
    {
        private static List<UserRating> ratings = new List<UserRating>();


        private static object locker = new object();
        private static string dataPath = "data/game/rating.dat";


        public static void Init()
        {
            Load();
        }

        public static void Terminate()
        {
            Save();
        }

        private static void Load()
        {
            if (!File.Exists(dataPath))
                return;

            using (var stream = File.Open(dataPath, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    int cnt = reader.ReadInt32();
                    for (int i = 0; i < cnt; i++)
                    {
                        UserRating r = new UserRating();
                        r.Id = reader.ReadUInt32();
                        r.val = reader.ReadSingle();
                        ratings.Add(r);
                    }
                }
            }
        }

        private static void Save()
        {
            lock (locker)
            {
                using (var stream = File.Open(dataPath, FileMode.Create))
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        writer.Write(ratings.Count);

                        foreach (var r in ratings)
                        {
                            writer.Write(r.Id);
                            writer.Write(r.val);
                        }
                    }
                }
            }
        }

        public static void UpdateRating(uint userId, float val)
        {
            lock (locker)
            {
                var it = ratings.Find(f => f.Id == userId);
                if (it == null)
                {
                    it = new UserRating() {Id = userId, val = val};
                    ratings.Add(it);
                }
                else
                {
                    it.val = val;
                }
            }
        }

        public static List<UserRating> GetRatings()
        {
            lock (locker)
            {
                List<UserRating> list = new List<UserRating>(ratings);
                list.RemoveAll(f => f.val < 125);
                list.Sort(Compare);
                if (list.Count > 50)
                {
                    list.RemoveRange(50, list.Count - 50);
                }

                return list;
            }
        }

        private static int Compare(UserRating x, UserRating y)
        {
            if (x.val < y.val)
                return 1;

            if (x.val > y.val)
                return -1;

            return 0;
        }
    }
}

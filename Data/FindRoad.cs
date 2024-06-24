using System;
using System.Collections.Generic;

namespace SWFServer.Data
{
    public static class FindRoad
    {
        private static int[,] findMap = new int[256, 256];

        private static Vector2w[] offset = new[]
        {
        new Vector2w(1, 0), new Vector2w(0, 1), new Vector2w(-1, 0), new Vector2w(0, -1),
        new Vector2w(1, 1), new Vector2w(-1, 1), new Vector2w(-1, -1), new Vector2w(1, -1)
    };

        public static bool Find(Vector2w startPos, Vector2w endPos, WRect rect, Map map, List<Vector2w> outRoad, uint userId)
        {
            if (startPos == endPos)
                return false;
            for (int x = 0; x < rect.w; x++)
            {
                for (int y = 0; y < rect.h; y++)
                {
                    findMap[x, y] = 1001;
                }
            }

            if (endPos.x - rect.x < 0 || endPos.y - rect.y < 0)
            {
                Console.WriteLine("error find road");
            }

            findMap[startPos.x - rect.x, startPos.y - rect.y] = 1000;
            findMap[endPos.x - rect.x, endPos.y - rect.y] = 0;

            int index = 0;

            bool res = false;
            bool exit = false;
            int c = 0;
            do
            {
                res = false;
                for (int x = 0; x < rect.w; x++)
                {
                    for (int y = 0; y < rect.h; y++)
                    {
                        if (findMap[x, y] == index)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                Vector2w p = new Vector2w(x + offset[i].x + rect.x, y + offset[i].y + rect.y);
                                if (map.IsMap(p) && rect.Contains(p))
                                {
                                    if (findMap[p.x - rect.x, p.y - rect.y] == 1000)
                                    {
                                        exit = true;
                                        break;
                                    }

                                    if (findMap[p.x - rect.x, p.y - rect.y] == 1001 /*&& map.IsMove(p, userId, false)*/)
                                    {
                                        if (i > 3)
                                            findMap[p.x - rect.x, p.y - rect.y] = index + 3;
                                        else
                                            findMap[p.x - rect.x, p.y - rect.y] = index + 2;
                                        res = true;
                                        c = 4;
                                    }
                                }
                            }
                        }

                        if (exit)
                            break;
                    }
                    if (exit)
                        break;
                }
                if (exit)
                    break;
                index++;
                c--;
            } while (res || c > 0);

            //outRoad.Add(startPos);

            int memIndex = 1000;
            Vector2w pos = startPos;
            do
            {
                int memDir = -1;
                Vector2w s = new Vector2w(rect.x, rect.y);
                for (int i = 0; i < 8; i++)
                {
                    Vector2w p = pos + offset[i];
                    if (map.IsMap(p) && p.x - s.x >= 0 && p.x - s.x < rect.w && p.y - s.y >= 0 && p.y - s.y < rect.h)
                    {
                        if (findMap[p.x - s.x, p.y - s.y] < memIndex)
                        {
                            memIndex = findMap[p.x - s.x, p.y - s.y];
                            memDir = i;
                        }
                    }
                }
                if (memDir == -1)
                    return false;
                pos = pos + offset[memDir];
                outRoad.Add(new Vector2w(pos.x, pos.y));
                if (outRoad.Count > 500)
                    return true;
            } while (memIndex != 0);

            return true;
        }
    }
}

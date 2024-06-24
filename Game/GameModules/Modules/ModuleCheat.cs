using System.IO;
using SWFServer.Data;
using SWFServer.Data.Entities;
using SWFServer.Data.Net;

namespace SWFServer.Game.GameModules.Modules
{
    public class ModuleCheat: GameModule
    {
        private ModuleUsers moduleUsers;
        private ModuleUnits moduleUnits;


        public ModuleCheat(ModuleUsers moduleUsers, ModuleUnits moduleUnits)
        {
            this.moduleUsers = moduleUsers;
            this.moduleUnits = moduleUnits;
        }

        public override void Update(float dt)
        {
            UpdateCheat();
        }

        public void UpdateCheat()
        {
            if (File.Exists("data/bonus.txt"))
            {
                StreamReader reader = new StreamReader(File.Open("data/bonus.txt", FileMode.Open));
                string s;
                while (true)
                {
                    s = reader.ReadLine();
                    if (string.IsNullOrEmpty(s))
                        break;

                    string[] v = s.Split(new[] { ' ' });
                    if (v.Length == 2)
                    {
                        uint id;
                        ushort itemId;
                        if (!uint.TryParse(v[0], out id))
                            break;

                        if (v[1] == "-1")
                        {
                            User user = moduleUsers.GetUser(id);
                            if (user != null)
                            {
                                WorldManager.UserManager.AddUserMoney(user.Id, 10000);
                            }
                        }
                        else
                        {
                            if (!ushort.TryParse(v[1], out itemId))
                                break;

                            User user = moduleUsers.GetUser(id);
                            if (user != null)
                            {
                                //Unit unit = d.GetUnit(valUint);
                            }
                        }
                    }
                }
                reader.Close();
                File.Delete("data/bonus.txt");
            }
        }

        public override void Terminate()
        {
        }

        public bool TryCheat(User user, MsgClientChat msg)
        {
            string txt = msg.text;
            if (txt[0] == '/')
            {
                if (user.Role != UserRole.admin) return true;

                txt = txt.Remove(0, 1);
                var s = txt.Split(' ');
                if (s.Length > 1)
                {
                    if (s[0] == "add")
                    {
                        int n = 0;
                        if (int.TryParse(s[1], out n))
                        {
                            user.Money += n;
                            user.SendMsg(new MsgServer(){UserId = user.Id, Data = new MsgServerMoney(user.Money), Type = MsgServerType.money});
                        }
                        else
                        {
                            if (s.Length > 2)
                            {
                                string itemId = s[1];
                                if (Info.EntityInfo.dicItemsStr.ContainsKey(itemId))
                                {
                                    int c = 0;
                                    if (int.TryParse(s[2], out c))
                                    {
                                        var unit = moduleUnits.GetUnit(user);
                                        if (unit != null)
                                        {
                                            var it = unit.Entities.Entities.Find(f => f.Id == Info.EntityInfo[itemId].id);
                                            if (it == null || it.Count == null || Info.EntityInfo[itemId].stackCount<2)
                                            {
                                                it = Entity.Create(itemId);
                                                if (it.Count != null)
                                                    it.Count.Value = c;
                                                unit.Entities.Entities.Add(it);
                                            }
                                            else
                                                it.Count.Value += c;

                                            user.SendMsg(new MsgServer(user.Id, MsgServerType.inventory, new MsgServerInventory(unit.Entities.Entities, Vector2w.Empty)));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}

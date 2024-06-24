using Newtonsoft.Json;

namespace SWFServer.Data.Entities
{
    public static class Info
    {
        public static EntityInfo EntityInfo;

        public static void Init(string jsonEntity)
        {
            EntityInfo = JsonConvert.DeserializeObject<EntityInfo>(jsonEntity);
            EntityInfo.Init();
        }
    }
}

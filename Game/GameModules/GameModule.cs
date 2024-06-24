using SWFServer.Data;

namespace SWFServer.Game.GameModules
{
    public abstract class GameModule
    {
        protected Rnd Rnd = new Rnd();
        public abstract void Update(float dt);
        public abstract void Terminate();

    }
}

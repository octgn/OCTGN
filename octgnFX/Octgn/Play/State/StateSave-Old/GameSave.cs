namespace Octgn.Play.State
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    [Serializable]
	[Obsolete]
    public sealed class GameSave
    {
        public Dictionary<Type, IStateSave> States { get; set; }

        private GameSave()
        {
            this.States = new Dictionary<Type, IStateSave>();
        }

        public static GameSave Create()
        {
            var ret = new GameSave();
            ret.Add(Program.GameEngine);

            return ret;
        }

        public void Add<T>(T save) where T : class
        {
            var cs = StateSave<T>.Create(save);
            cs.Serialize();
            this.States.Add(typeof(T), cs);
            cs.SaveState();
        }

        public T Get<T>() where T : class
        {
            var state = this.States[typeof(T)];
            state.LoadState();
            return state.GetInstance() as T;
        }

        public static byte[] Serialize(GameSave save)
        {
            using (var ms = new MemoryStream())
            {
                var br = new BinaryFormatter();
                br.Serialize(ms, save);
                ms.Flush();
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        public static GameSave Deserialize(byte[] bytes)
        {
            GameSave ret = null;
            using (var ms = new MemoryStream(bytes.ToArray()))
            {
                var br = new BinaryFormatter();
                ret = (GameSave)br.Deserialize(ms);
            }
            return ret;
        }
    }
}
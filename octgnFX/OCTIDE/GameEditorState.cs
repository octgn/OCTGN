namespace OCTIDE
{
    using System;
    using System.IO;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;

    [Serializable]
    public class GameEditorState: IGameEditorState
    {
        #region Static Shits
        private static GameEditorState Context { get; set; }
        public static GameEditorState Get()
        {
            return Context;
        }
        public static void Set(GameEditorState state)
        {
            Context = state;
        }

        public static void Open(string path)
        {
            if (!File.Exists(path))
                throw new UserMessageException("File doesn't exist.");

            try
            {
                var bytes = File.ReadAllBytes(path);
                using (var ms = new MemoryStream(bytes))
                {
                    ms.Position = 0;
                    var bf = new BinaryFormatter();
                    var state = bf.Deserialize(ms) as IGameEditorState;
                    var fi = new FileInfo(path);
                    Context = new GameEditorState()
                                  {
                                      GamePath = fi.Directory.FullName,
                                      GameProjPath = Path.Combine(fi.Directory.FullName, fi.Name),
                                      Id = state.Id,
                                      Name = state.Name
                                  };
                    FireGameOpened(Context);
                }
            }
            catch (PathTooLongException)
            {
                throw new UserMessageException(
                    "The file path is too long. Please move your project somewhere else and try again.");
            }
            catch (DirectoryNotFoundException)
            {
                throw new UserMessageException("The project directory could be found.");
            }
            catch (FileNotFoundException)
            {
                throw new UserMessageException("The project file could not be found.");
            }
            catch (IOException)
            {
                throw new UserMessageException("There was an unknown file error.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new UserMessageException("The project file is in use by another program.");
            }
            catch (SecurityException)
            {
                throw new UserMessageException("You need elevated privileges to save this project.");
            }
            catch (SerializationException)
            {
                throw new UserMessageException("This is not a o8proj file.");
            }
            catch (Exception)
            {
                throw new UserMessageException("Something happened. I'm not sure what. Call 911.");
            }
        }

        public static void Close()
        {
            FireGameClosed(Get());
            Context = null;
        }

        public static GameEditorState CreateGame(string location, string name)
        {
            var path = Path.Combine(location, name);
            if(string.IsNullOrWhiteSpace(location))
                throw new ValidationException("Please enter a location");
            if(string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Please enter a game name");
            if(Directory.Exists(Path.Combine(location,name)))
                throw new ValidationException("The directory " + path + " already exists");
            Directory.CreateDirectory(path);
            var ret = new GameEditorState();
            ret.GamePath = path;
            ret.Id = Guid.NewGuid();
            ret.Name = name;
            ret.GameProjPath = Path.Combine(path, name + ".o8proj");
            Set(ret);
            FireGameCreated(Context);
            return ret;
        }

        public static event EventHandler GameCreated; 

        public static event EventHandler GameOpened;

        public static event EventHandler GameClosed;

        protected static void FireGameClosed(GameEditorState state)
        {
            var handler = GameClosed;
            if (handler != null)
            {
                handler(state, EventArgs.Empty);
            }
        }

        protected static void FireGameOpened(GameEditorState state)
        {
            var handler = GameOpened;
            if (handler != null)
            {
                handler(state, EventArgs.Empty);
            }
        }

        protected static void FireGameCreated(GameEditorState state)
        {
            var handler = GameCreated;
            if (handler != null)
            {
                handler(state, EventArgs.Empty);
            }
        }

        #endregion Static Shits

        public string GamePath { get; set; }

        public string GameProjPath { get; set; }

        public string Name { get; set; }

        public Guid Id { get; private set; }

        public void Save()
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, this as IGameEditorState);
                    ms.Position = 0;
                    File.WriteAllBytes(this.GameProjPath, ms.ToArray());
                }
            }
            catch (PathTooLongException)
            {
                throw new UserMessageException("The file path is too long. Please move your project somewhere else and try again. Nothing will be saved.");
            }
            catch (DirectoryNotFoundException)
            {
                throw new UserMessageException("The project directory could be found. Nothing will be saved.");
            }
            catch (FileNotFoundException)
            {
                throw new UserMessageException("The project file could not be found. Nothing will be saved.");
            }
            catch (IOException)
            {
                throw new UserMessageException("There was an unknown file error. Nothing will be saved.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new UserMessageException("The project file is in use by another program. Nothing will be saved.");
            }
            catch (SecurityException)
            {
                throw new UserMessageException("You need elevated privileges to save this project. Nothing will be saved.");
            }
            catch (Exception)
            {
                throw new UserMessageException("Something happened. I'm not sure what. Nothing will be saved.");
            }
        }
    }
    public interface IGameEditorState
    {
        Guid Id { get; }
        String Name { get; }
    }
}
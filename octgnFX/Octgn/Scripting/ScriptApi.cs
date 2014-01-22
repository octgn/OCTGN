using System.Linq;
using Octgn.Library;
using Octgn.Library.Exceptions;
using Octgn.Scripting.Versions;

namespace Octgn.Scripting
{
    public class ScriptApi
    {
        public ScriptBase Script { get; set; }

        public ScriptApi(GameEngine engine)
        {
            if (X.Instance.Debug || Program.DeveloperMode)
            {
                Script = ScriptBase.Scripts.FirstOrDefault(x => x.Version == engine.Definition.ScriptVersion);
            }
            else
            {
                Script = ScriptBase.LiveScripts.FirstOrDefault(x => x.Version == engine.Definition.ScriptVersion);
            }
			if(Script == null)
				throw new UserMessageException("Game definition is incorrect. Script API '{0}' doesn't exist or can't be used.",engine.Definition.ScriptVersion);
        }
    }
}
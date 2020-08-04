using System;

namespace Octgn.Sdk.Extensibility.Desktop
{
    public class ClickContext
    {
        public GamePlugin GamePlugin { get; }

        public ClickContext(GamePlugin gamePlugin) {
            GamePlugin = gamePlugin ?? throw new ArgumentNullException(nameof(gamePlugin));
        }
    }
}
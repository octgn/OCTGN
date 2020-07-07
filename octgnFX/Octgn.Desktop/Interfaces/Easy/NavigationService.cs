using System;

namespace Octgn.Desktop.Interfaces.Easy
{
    public class NavigationService
    {
        public event EventHandler<NavigateEventArgs> Navigate;

        public NavigationService() { }


        public void NavigateTo(Screen screen) {
            if (screen == null) throw new ArgumentNullException(nameof(screen));

            screen.NavigationService = this;

            var args = new NavigateEventArgs() {
                Destination = screen
            };

            var handlers = Navigate?.GetInvocationList();

            if (handlers == null || handlers.Length == 0) return;

            foreach (EventHandler<NavigateEventArgs> handler in handlers) {
                handler.Invoke(this, args);

                if (args.IsHandled) break;
            }
        }
    }
}

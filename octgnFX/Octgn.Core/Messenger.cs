using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Octgn.Core
{
    //TODO: Test this whole class
    public class Messenger : IMessenger
    {
        public static IMessenger Default = new Messenger();

        private readonly Dictionary<Type, List<Registration>> _registrations = new Dictionary<Type, List<Registration>>();

        public void Send<T>(T message) {
            Registration[] registrations;

            lock (_registrations) {
                if (!_registrations.TryGetValue(typeof(T), out var typeRegistrations)) {
                    return;
                }

                registrations = typeRegistrations.ToArray();
            }

            var _ = Task
                .Run(() => Send<T>(message, registrations))
                .Error((exception) => RaiseError(exception));
        }

        private async Task Send<T>(T message, IEnumerable<Registration> registrations) {
            foreach (var registration in registrations) {
                dynamic action = registration.Action;

                var isTask = typeof(Task).IsAssignableFrom(action.Method.ReturnType);

                if (isTask) {
                    await action(message);
                } else {
                    action(message);
                }
            }
        }

        public void Register<T>(object recipient, Func<T, Task> action) {
            if (!_registrations.TryGetValue(typeof(T), out var typeRegistrations)) {
                _registrations.Add(typeof(T), typeRegistrations = new List<Registration>());
            }

            typeRegistrations.Add(new Registration(recipient, action));
        }

        private void RaiseError(Exception exception) {
            throw new NotImplementedException();
        }

        private class Registration
        {
            public object Recipient { get; }

            public Delegate Action { get; }

            public Registration(object recipient, Delegate action) {
                Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
                Action = action ?? throw new ArgumentNullException(nameof(action));
            }
        }
    }

    public interface IMessenger
    {
        void Send<T>(T message);
        void Register<T>(object recipient, Func<T, Task> action);
    }
}
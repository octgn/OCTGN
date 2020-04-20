using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Octgn.LoadTester
{
    public class Tester
    {
        public Tester() {

        }

        public Task Run(CancellationToken cancellationToken) {
            var nodes = new List<Node>();

            var nodeTasks = new List<Task>();

            for (var i = 0; i < 10; i++) {
                var node = new Node();

                var nodeTask = node.Run(cancellationToken);

                nodes.Add(node);

                nodeTasks.Add(nodeTask);
            }

            return Task.WhenAll(nodeTasks);
        }
    }
}

using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.TestUtils;
using GeoAPI.Extensions.Networks;
using SharpTestsEx;

using log4net;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Extensions.Tests.Features;
using NUnit.Framework;
using ValidationAspects;

namespace NetTopologySuite.Extensions.Tests.Networks
{
    [TestFixture]
    public class NetworkTest
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NetworkTest));
        
        [SetUp]
        public void SetUp()
        {
            LogHelper.ConfigureLogging();
        }

        [Test]
        public void NetworkWithDisconnectedNodesMustBeInvalid()
        {
            var network = new Network
                              {
                                  Nodes = {new Node {Name = "node1"}, new Node {Name = "node2"}}
                              };

            var result = network.Validate();
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(2, result.Messages.Count());
        }

        [Test]
        public void NoUnnecessaryEventBubbling()
        {
            var node1 = new Node { Name = "node1" };
            var node2 = new Node { Name = "node2" };
            var branch = new Branch { Name = "branch1", Source = node1, Target = node2 };

            var network = new Network
            {
                Nodes = { node1, node2 },
                Branches = { branch }
            };

            var eventCount = 0;
            ((INotifyPropertyChanged) network).PropertyChanged += delegate { eventCount++; };

            node1.Name = "node1_";

            eventCount
                .Should().Be.EqualTo(1);
        }

        [Test]
        public void ExpectIncomingOutGoingBranchesSet()
        {
            var node1 = new Node { Name = "node1" };
            var node2 = new Node { Name = "node2" };
            var branch1 = new Branch { Name = "branch1", Source = node1, Target = node2 };

            var network = new Network
            {
                Nodes = { node1, node2 },
                Branches = { branch1 }
            };
            node1.IncomingBranches.Count.Should().Be.EqualTo(0);
            node1.OutgoingBranches.Count.Should().Be.EqualTo(1);
            node2.IncomingBranches.Count.Should().Be.EqualTo(1);
            node2.OutgoingBranches.Count.Should().Be.EqualTo(0);
        }

        [Test]
        public void OutgoingIncomingBranchesShouldBeChangedWhenBranchIsRemoved()
        {
            var node1 = new Node { Name = "node1" };
            var node2 = new Node { Name = "node2" };
            var node3 = new Node { Name = "node3" };
            var branch1 = new Branch { Name = "branch1", Source = node1, Target = node2 };
            var branch2 = new Branch { Name = "branch2", Source = node2, Target = node3 };

            var network = new Network
            {
                Nodes = { node1, node2, node3 },
                Branches = { branch1, branch2 }
            };

            node1.IncomingBranches.Count.Should().Be.EqualTo(0);
            node1.OutgoingBranches.Count.Should().Be.EqualTo(1);
            node2.IncomingBranches.Count.Should().Be.EqualTo(1);
            node2.OutgoingBranches.Count.Should().Be.EqualTo(1);
            node3.IncomingBranches.Count.Should().Be.EqualTo(1);
            node3.OutgoingBranches.Count.Should().Be.EqualTo(0);

            network.Branches.Remove(branch2);

            node1.IncomingBranches.Count.Should().Be.EqualTo(0);
            node1.OutgoingBranches.Count.Should().Be.EqualTo(1);
            node2.IncomingBranches.Count.Should().Be.EqualTo(1);
            node2.OutgoingBranches.Count.Should().Be.EqualTo(0);
        }

        [Test]
        public void IsConnectedToMultipleBranchesIsChangedWhenBranchIsRemoved()
        {
            var node1 = new Node { Name = "node1" };
            var node2 = new Node { Name = "node2" };
            var node3 = new Node { Name = "node3" };
            var branch1 = new Branch { Name = "branch1", Source = node1, Target = node2 };
            var branch2 = new Branch { Name = "branch2", Source = node2, Target = node3 };
            var network = new Network
                              {
                                  Nodes = { node1, node2, node3 },
                                  Branches = { branch1, branch2 }
                              };

            Assert.IsTrue(node2.IsConnectedToMultipleBranches);

            network.Branches.Remove(branch2);

            Assert.IsFalse(node2.IsConnectedToMultipleBranches);
        }

        [Test]
        public void CloneNetwork()
        {
            var node1 = new Node { Name = "node1" };
            var node2 = new Node { Name = "node2" };
            var branch = new Branch { Name = "branch1", Source = node1, Target = node2 };
            branch.BranchFeatures.Add(new SimpleBranchFeature() { Branch = branch });

            var network = new Network
            {
                Name = "network1",
                Nodes = { node1, node2 },
                Branches = { branch }
            };

            INetwork clonedNetwork = (INetwork) network.Clone();
            clonedNetwork.Name = "clone";

            clonedNetwork.Branches.Count.Should().Be.EqualTo(1);
            clonedNetwork.Nodes.Count.Should().Be.EqualTo(2);
            clonedNetwork.Branches[0].Name.Should().Be.EqualTo("branch1");
            clonedNetwork.Branches[0].Source.IncomingBranches.Count.Should().Be.EqualTo(0);
            clonedNetwork.Branches[0].Source.OutgoingBranches[0].Should().Be.EqualTo(clonedNetwork.Branches[0]);
            clonedNetwork.Branches[0].Target.IncomingBranches.Count.Should().Be.EqualTo(1);
            clonedNetwork.Branches[0].GetType().Should().Be.EqualTo(network.Branches[0].GetType());

            Assert.AreEqual(clonedNetwork, clonedNetwork.Branches[0].Network);
            Assert.AreEqual(clonedNetwork, clonedNetwork.Branches[0].BranchFeatures[0].Network);
        }

        [Test]
        public void CloneNetworkwithBranch()
        {
            var node1 = new Node { Name = "node1" };
            var node2 = new Node {Name = "node2"};
            var channel = new Branch {Name = "branch1", Source = node1, Target = node2};
            channel.BranchFeatures.Add(new SimpleBranchFeature() {Branch = channel});

            var network = new Network
                              {
                                  Name = "network1",
                                  Nodes = {node1, node2},
                                  Branches = {channel}
                              };

            INetwork clonedNetwork = (INetwork) network.Clone();
            clonedNetwork.Branches[0].GetType().Should().Be.EqualTo(network.Branches[0].GetType());

        }

        [Test]
        public void AddingBranchToNetworkShouldNotResetName()
        {
            var startNode = new Node { Name = "start" };
            var endNode = new Node { Name = "end" };
            var name = "SomeName";

            var branch = new Branch(startNode, endNode) { Name = name };

            var network = new Network
                              {
                                  Name = "network1",
                                  Nodes = { startNode, endNode },
                              };

            network.Branches.Add(branch); // results in branche with name "Branche1"

            branch.Name.Should("Branch name was unexpectedly changed.").Be.EqualTo(name);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ShortestPathOnNetwork()
        {
            var network = GetNetworkForShortestPath();
            var nodeA = network.Nodes.First(n => n.Name == "A");
            var nodeC = network.Nodes.First(n => n.Name == "C");
            var edgeAB1 = network.Branches.First(b => b.Name == "A->B1");
            var edgeB1C = network.Branches.First(b => b.Name == "B1->C");

            var shortestPath = network.GetShortestPath(nodeA, nodeC, b => b.Name.Length);

            log.Debug(String.Join(" -> ", shortestPath.Select(b => b.Name).ToArray()));

            Assert.AreEqual(2, shortestPath.Count());
            Assert.AreEqual(edgeAB1, shortestPath.First());
            Assert.AreEqual(edgeB1C, shortestPath.Last());
        }

        [Test]
        public void ShortestPathReturnsEmptyCollectionWhenNoPathFound()
        {
            var network = GetNetworkForShortestPath();
            var nodeA = network.Nodes.First(n => n.Name == "A");
            var nodeC = network.Nodes.First(n => n.Name == "C");

            var path = network.GetShortestPath(null, nodeC, b => b.Name.Length);
            Assert.AreEqual(0, path.Count());
            path = network.GetShortestPath(nodeA, null, b => b.Name.Length);
            Assert.AreEqual(0, path.Count());
            path = network.GetShortestPath(new Node("X"), new Node("Y"), b => b.Name.Length);
            Assert.AreEqual(0, path.Count());
        }

        private static Network GetNetworkForShortestPath()
        {
            var network = new Network();

            var nodeA = new Node {Name = "A"};
            var nodeB1 = new Node {Name = "B1_IAmShort"};
            var nodeB2 = new Node {Name = "B2_PleaseAvoidMeIAmLooooooooooooong"};
            var nodeC = new Node {Name = "C"};

            var edgeAB1 = new Branch(nodeA, nodeB1) { Name = "A->B1" };
            var edgeAB2 = new Branch(nodeA, nodeB2) { Name = "A->B2" };
            var edgeB1C = new Branch(nodeB1, nodeC) { Name = "B1->C" };
            var edgeB2C = new Branch(nodeB2, nodeC) { Name = "B2->C" };

            network.Nodes.Add(nodeA);
            network.Nodes.Add(nodeB1);
            network.Nodes.Add(nodeB2);
            network.Nodes.Add(nodeC);

            network.Branches.Add(edgeAB1);
            network.Branches.Add(edgeAB2);
            network.Branches.Add(edgeB1C);
            network.Branches.Add(edgeB2C);
            return network;
        }
    }
}
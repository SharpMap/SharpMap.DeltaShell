using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Actions;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Extensions.Tests.Coverages;
using NetTopologySuite.Extensions.Tests.Features;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Converters.WellKnownText;
using SharpMapTestUtils.TestClasses;

namespace NetTopologySuite.Extensions.Tests.Networks
{
    [TestFixture]
    public class NetworkHelperTest
    {
        private MockRepository mocks = new MockRepository();

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }

        /// <summary>
        /// Creates a simple test network of 1 branch amd 2 nodes. The branch has '3' parts, in the center of
        /// the first aand last is a cross section.
        ///                 n
        ///                /
        ///               /
        ///              cs
        ///             /
        ///     -------/
        ///    /
        ///   cs
        ///  /
        /// n
        /// </summary>
        /// <returns></returns>
        private static INetwork CreateTestNetwork()
        {
            var network = new Network();
            var branch1 = new Branch
            {
                Geometry = new LineString(new[]
                                                         {
                                                             new Coordinate(0, 0), new Coordinate(30, 40),
                                                             new Coordinate(70, 40), new Coordinate(100, 100)
                                                         })
            };

            var node1 = new Node { Network = network, Geometry = new Point(new Coordinate(0, 0)) ,Name = "StartNode"};
            var node2 = new Node { Network = network, Geometry = new Point(new Coordinate(100, 100)) ,Name = "EndNode"};

            branch1.Source = node1;
            branch1.Target = node2;

            network.Branches.Add(branch1);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            return network;
        }

        [Test]
        public void GetShortetsRouteOnASingleBranchReversed()
        {
            var network = CreateTestNetwork();
            var branch1 = network.Branches[0];
            var segments = NetworkHelper.GetShortestPathBetweenBranchFeaturesAsNetworkSegments(network,new NetworkLocation(branch1,20),
                new NetworkLocation(branch1,10));
            Assert.AreEqual(1,segments.Count);
            Assert.AreEqual(20, segments[0].Chainage);
            Assert.AreEqual(10, segments[0].EndChainage);
        }

        [Test]
        public void GetBranchFeatureChainageFromGeometry()
        {
            var network = CreateTestNetwork();
            var branch1 = network.Branches[0];
            var chainage = NetworkHelper.GetBranchFeatureChainageFromGeometry(branch1,
                                                                          new LineString(new Coordinate[]
                                                                                             {
                                                                                                 new Coordinate(0, 30),
                                                                                                 new Coordinate(100, 30)
                                                                                             }));

            Assert.AreEqual(37.5, chainage);
        }

        [Test]
        public void GetShortestRouteOnASingleBranch()
        {
            var network = CreateTestNetwork();
            var branch1 = network.Branches[0];
            var segments = NetworkHelper.GetShortestPathBetweenBranchFeaturesAsNetworkSegments(network, new NetworkLocation(branch1, 10),
                new NetworkLocation(branch1, 20));
            Assert.AreEqual(1, segments.Count);
            Assert.AreEqual(10,segments[0].Chainage);
            Assert.AreEqual(20, segments[0].EndChainage);
        }
        [Test]
        public void CreateLightCopyNetworkOldItemsAsAttributes()
        {
            var network = CreateTestNetwork();
            var lightNetwork = NetworkHelper.CreateLightNetworkCopyWithOldItemsAsAttributes(network, null);
            Assert.AreEqual(network.Branches.Count, lightNetwork.Branches.Count);
            Assert.AreEqual(network.Nodes.Count, lightNetwork.Nodes.Count);
            Assert.AreEqual(lightNetwork.Nodes[0], lightNetwork.Branches[0].Source);
            Assert.AreEqual(lightNetwork.Nodes[1], lightNetwork.Branches[0].Target);
        }

        [Test]
        public void GetShortestPathBetweeTwoBranchFeatures()
        {
            var network = new Network();

            var node1 = new Node {Network = network, Geometry = new Point(new Coordinate(0, 0)), Name = "node1"};
            var node2 = new Node {Network = network, Geometry = new Point(new Coordinate(0, 100)), Name = "node2"};
            var node3 = new Node {Network = network, Geometry = new Point(new Coordinate(100, 0)), Name = "node3"};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Branch
                              {
                                  Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 0 100)"),
                                  Source = node1,
                                  Target = node2,
                                  Name = "branch1"
                              };
            var branch2 = new Branch
                              {
                                  Geometry = GeometryFromWKT.Parse("LINESTRING (0 100, 100 0)"),
                                  Source = node2,
                                  Target = node3,
                                  Name = "branch2"
                              };
            var branch3 = new Branch
                              {
                                  Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 0 0)"),
                                  Source = node3,
                                  Target = node1,
                                  Name = "branch3"
                              };
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            network.Branches.Add(branch3);

            var networkLocation1 = new NetworkLocation
                                       {
                                           Geometry = new Point(new Coordinate(90, 0)),
                                           Branch = branch1,
                                           Chainage = 90,
                                           Name = "source"
                                       };
            var networkLocation2 = new NetworkLocation
                                       {
                                           Geometry = new Point(new Coordinate(0, 90)),
                                           Branch = branch2,
                                           Chainage = 90,
                                           Name = "target"
                                       };

            var segments = NetworkHelper.GetShortestPathBetweenBranchFeaturesAsNetworkSegments(network, networkLocation1,
                                                                                               networkLocation2);

            Assert.AreEqual(2, segments.Count);
            Assert.IsTrue(segments[0].DirectionIsPositive);
            Assert.AreEqual(90, segments[0].Chainage);
            Assert.AreEqual(100, segments[0].EndChainage);
        }

        [Test]
        public void GetShortestPathBetweenTwoBranchFeaturesReversed()
        {
            var network = new Network();

            var node1 = new Node { Network = network, Geometry = new Point(new Coordinate(0, 0)), Name = "node1" };
            var node2 = new Node { Network = network, Geometry = new Point(new Coordinate(0, 100)), Name = "node2" };
            var node3 = new Node { Network = network, Geometry = new Point(new Coordinate(100, 0)), Name = "node3" };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Branch
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 0 100)"),
                Source = node1,
                Target = node2,
                Name = "branch1"
            };
            var branch2 = new Branch
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 100, 100 0)"),
                Source = node2,
                Target = node3,
                Name = "branch2"
            };
            var branch3 = new Branch
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 0 0)"),
                Source = node3,
                Target = node1,
                Name = "branch3"
            };
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            network.Branches.Add(branch3);

            var networkLocation1 = new NetworkLocation
            {
                Geometry = new Point(new Coordinate(90, 0)),
                Branch = branch1,
                Chainage = 90,
                Name = "source"
            };
            var networkLocation2 = new NetworkLocation
            {
                Geometry = new Point(new Coordinate(0, 90)),
                Branch = branch2,
                Chainage = 90,
                Name = "target"
            };

            var segments = NetworkHelper.GetShortestPathBetweenBranchFeaturesAsNetworkSegments(network, networkLocation2,
                                                                                               networkLocation1);

            Assert.AreEqual(2, segments.Count);
            Assert.IsFalse(segments[0].DirectionIsPositive);
            Assert.AreEqual(90, segments[0].Chainage);
            Assert.AreEqual(0, segments[0].EndChainage);
        }

        [Test]
        public void GetShortestPathSingleBranchReversed()
        {
            var network = new Network();

            var node1 = new Node { Network = network, Geometry = new Point(new Coordinate(0, 0)), Name = "node1" };
            var node2 = new Node { Network = network, Geometry = new Point(new Coordinate(0, 100)), Name = "node2" };
            var node3 = new Node { Network = network, Geometry = new Point(new Coordinate(100, 0)), Name = "node3" };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Branch
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 0 100)"),
                Source = node1,
                Target = node2,
                Name = "branch1"
            };
            var branch2 = new Branch
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 100, 100 0)"),
                Source = node2,
                Target = node3,
                Name = "branch2"
            };
            var branch3 = new Branch
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 0 0)"),
                Source = node3,
                Target = node1,
                Name = "branch3"
            };
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            network.Branches.Add(branch3);

            var networkLocation1 = new NetworkLocation
            {
                Geometry = new Point(new Coordinate(90, 0)),
                Branch = branch1,
                Chainage = 90,
                Name = "source"
            };
            var networkLocation2 = new NetworkLocation
            {
                Geometry = new Point(new Coordinate(0, 40)),
                Branch = branch1,
                Chainage = 40,
                Name = "target"
            };

            var segments = NetworkHelper.GetShortestPathBetweenBranchFeaturesAsNetworkSegments(network, networkLocation1,
                                                                                               networkLocation2);

            Assert.AreEqual(1, segments.Count);
            Assert.IsFalse(segments[0].DirectionIsPositive);
            Assert.AreEqual(90, segments[0].Chainage);
            Assert.AreEqual(40, segments[0].EndChainage);
        }

        [Test]
        public void UpdateLengthAfterSplitNonGeometryBasedBranchWithLength0ThrowsException()
        {
            // note ExpectedException(typeof(InvalidOperationException)) no longer thrown.
            // offset and thus SplitBranchAtNode use geometry based value

            var network = new Network();
            network.Branches.Add(new Branch
            {
                Source = new Node("n1"),
                Target = new Node("n2"),
                Geometry = GeometryFromWKT.Parse("LINESTRING(0 0, 100 0)")
            });

            var branch = network.Branches.First();
            Assert.AreEqual(100.0, branch.Length);

            branch.IsLengthCustom = true;
            branch.Length = 0;

            var node = NetworkHelper.SplitBranchAtNode(network.Branches.First(), 50).NewNode;
            Assert.AreEqual(0, node.IncomingBranches[0].Length);
            Assert.AreEqual(0, node.OutgoingBranches[0].Length);
        }

        /// <summary>
        /// Splits a branch where IsCustomLength is set.
        /// Splitting is done at geometry based length.
        /// Split branch get updated length based on original branch
        /// </summary>
        [Test]
        public void UpdateLengthAfterSplitNonGeometryBasedBranch()
        {
            var network = new Network();
            network.Branches.Add(new Branch
            {
                Source = new Node("n1"),
                Target = new Node("n2"),
                IsLengthCustom = true,
                Length = 1000,
                Geometry = GeometryFromWKT.Parse("LINESTRING(0 0, 100 0)")
            });

            var node = NetworkHelper.SplitBranchAtNode(network.Branches.First(), 20).NewNode;

            Assert.AreEqual(200, node.IncomingBranches[0].Length);
            Assert.AreEqual(800, node.OutgoingBranches[0].Length);
        }

        [Test]
        public void UpdateLengthAfterSplitGeometryBasedBranch()
        {
            var network = new Network();
            network.Branches.Add(new Branch
            {
                Source = new Node("n1"),
                Target = new Node("n2"),
                IsLengthCustom = false,
                Length = 1000,
                Geometry = GeometryFromWKT.Parse("LINESTRING(0 0, 100 0)")
            });

            var node = NetworkHelper.SplitBranchAtNode(network.Branches.First(), 20).NewNode;

            Assert.AreEqual(20, node.IncomingBranches[0].Length);
            Assert.AreEqual(80, node.OutgoingBranches[0].Length);
        }

        [Test]
        public void SplitBranchFeaturesWithLengthIntoTwoFeaturesDuringSplitBranch()
        {
            // n1              f1                   n2              n1          f1_1  n3  f1_2            n2
            // O--------==================----------O       ==>     O--------=========O=========----------O
            //                  ^
            //                split

            var network = new Network();
            var node1 = new Node("n1");
            var node2 = new Node("n2");
            
            var branch = new Branch("Branch",node1, node2, 100)
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING(0 0, 100 0)")
            };

            var f1 = new SimpleBranchFeature
                         {
                             Name = "f1", 
                             Length = 50, 
                             Geometry = GeometryFromWKT.Parse("LINESTRING(25 0, 75 0)")
                         };

            network.Nodes.AddRange(new[]{node1,node2});
            network.Branches.Add(branch);
            NetworkHelper.AddBranchFeatureToBranch(f1, branch, 25);

            NetworkHelper.SplitBranchAtNode(branch, 50);

            Assert.AreEqual(2, network.Branches.Count);
            Assert.AreEqual(3, network.Nodes.Count);

            var branch2 = network.Branches[1];

            Assert.AreEqual(1, branch.BranchFeatures.Count);
            Assert.AreEqual(1, branch2.BranchFeatures.Count);

            var f2 = branch2.BranchFeatures[0];

            Assert.AreEqual(25, f1.Length);
            Assert.AreEqual(25, f2.Length);
            Assert.AreEqual(25, f1.Geometry.Length);
            Assert.AreEqual(25, f2.Geometry.Length);
            Assert.AreEqual(25, f1.Chainage);
            Assert.AreEqual(0, f2.Chainage);
            Assert.AreEqual("f1_1", f1.Name);
            Assert.AreEqual("f1_2", f2.Name);
        }

        [Test]
        public void SplitBranchFeaturesWithLengthIntoTwoFeaturesWhereTheFeatureWithTheLongestLengthShouldBeOrginalFeature()
        {
            // n1              f1                   n2              n1      f1_1 n3   f1_2                n2
            // O--------==================----------O       ==>     O--------====O==============----------O
            //              ^                                                     (f1_B = original feature)
            //            split

            var network = new Network();
            var node1 = new Node("n1");
            var node2 = new Node("n2");

            var branch = new Branch("Branch", node1, node2, 100)
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING(0 0, 100 0)")
            };

            var f1 = new SimpleBranchFeature
            {
                Name = "f1",
                Length = 50,
                Geometry = GeometryFromWKT.Parse("LINESTRING(25 0, 75 0)")
            };

            network.Nodes.AddRange(new[] { node1, node2 });
            network.Branches.Add(branch);
            NetworkHelper.AddBranchFeatureToBranch(f1, branch, 25);

            NetworkHelper.SplitBranchAtNode(branch, 40);

            Assert.AreEqual(2, network.Branches.Count);
            Assert.AreEqual(3, network.Nodes.Count);

            var branch2 = network.Branches[1];

            Assert.AreEqual(1, branch.BranchFeatures.Count);
            Assert.AreEqual(1, branch2.BranchFeatures.Count);

            var feature1 = branch.BranchFeatures[0];
            var feature2 = branch2.BranchFeatures[0];

            Assert.AreEqual(15, feature1.Length);
            Assert.AreEqual(35, feature2.Length);
            Assert.AreEqual(15, feature1.Geometry.Length);
            Assert.AreEqual(35, feature2.Geometry.Length);
            Assert.AreEqual(25, feature1.Chainage);
            Assert.AreEqual(0, feature2.Chainage);
            Assert.AreEqual("f1_1", feature1.Name);
            Assert.AreEqual("f1_2", feature2.Name);

            Assert.AreEqual(f1, feature2);
        }

        [Test]
        public void SplitBranchFeaturesWithLengthIntoTwoFeaturesWithBranchWithCustomLength()
        {
            // n1              f1                   n2              n1          f1_1  n3  f1_2            n2
            // O--------==================----------O       ==>     O--------=========O=========----------O
            //                  ^
            //                split

            var network = new Network();
            var node1 = new Node("n1");
            var node2 = new Node("n2");

            var branch = new Branch("Branch", node1, node2, 100)
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING(0 0, 100 0)"),
                IsLengthCustom = true,
                Length = 200
            };

            var f1 = new SimpleBranchFeature
            {
                Name = "f1",
                Length = 100,
                Chainage = 50,
                Geometry = GeometryFromWKT.Parse("LINESTRING(25 0, 75 0)")
            };

            network.Nodes.AddRange(new[] { node1, node2 });
            network.Branches.Add(branch);
            NetworkHelper.AddBranchFeatureToBranch(f1, branch, 50);

            // split is geometry based --> offset 50 (geometry based) == 100 m (custom length format)
            NetworkHelper.SplitBranchAtNode(branch, 50);

            Assert.AreEqual(2, network.Branches.Count);
            Assert.AreEqual(3, network.Nodes.Count);

            var branch2 = network.Branches[1];

            Assert.AreEqual(1, branch.BranchFeatures.Count);
            Assert.AreEqual(1, branch2.BranchFeatures.Count);

            var f2 = branch2.BranchFeatures[0];

            Assert.AreEqual(50, f1.Length);
            Assert.AreEqual(50, f2.Length);
            Assert.AreEqual(25, f1.Geometry.Length);
            Assert.AreEqual(25, f2.Geometry.Length);
            Assert.AreEqual(50, f1.Chainage);
            Assert.AreEqual(0, f2.Chainage);
            Assert.AreEqual("f1_1", f1.Name);
            Assert.AreEqual("f1_2", f2.Name);
        }

        [Test]
        public void UpdateChainagesRegardlessOfOrderingAfterSplitBranch()
        {
            var network = new Network();
            var branch = new Branch
                             {
                                 Source = new Node("n1"),
                                 Target = new Node("n2"),
                                 IsLengthCustom = false,
                                 Length = 1000,
                                 Geometry = GeometryFromWKT.Parse("LINESTRING(0 0, 100 0)")
                             };
            network.Branches.Add(branch);

            var f1 = new SimpleBranchFeature();
            var f2 = new SimpleBranchFeature();
            var f3 = new SimpleBranchFeature();
            var f4 = new SimpleBranchFeature();

            NetworkHelper.AddBranchFeatureToBranch(f1, branch, 5);
            NetworkHelper.AddBranchFeatureToBranch(f2, branch, 50);
            NetworkHelper.AddBranchFeatureToBranch(f3, branch, 15);
            NetworkHelper.AddBranchFeatureToBranch(f4, branch, 10);
            
            NetworkHelper.SplitBranchAtNode(network.Branches.First(), 20);

            Assert.AreEqual(5, f1.Chainage);
            Assert.AreEqual(30, f2.Chainage);
            Assert.AreEqual(15, f3.Chainage);
            Assert.AreEqual(10, f4.Chainage);
        }

        [Test]
        public void GetNeighboursOnBranchNone()
        {
            var network = new Network();
            network.Branches.Add(new Branch
            {
                Source = new Node("n1"),
                Target = new Node("n2"),
                Geometry = GeometryFromWKT.Parse("LINESTRING(0 0, 100 0)")
            });

            IBranchFeature before;
            IBranchFeature after;
            NetworkHelper.GetNeighboursOnBranch(network.Branches[0], 100, out before, out after);

            Assert.IsNull(before);
            Assert.IsNull(after);
        }

        [Test]
        public void GetNeighboursOnBranch()
        {
            var network = new Network();
            Branch branch = new Branch
                                 {
                                     Source = new Node("n1"),
                                     Target = new Node("n2"),
                                     Geometry = GeometryFromWKT.Parse("LINESTRING(0 0, 100 0)")
                                 };

            network.Branches.Add(branch);

            for (int i=1; i<10; i++)
            {
                branch.BranchFeatures.Add(new SimpleBranchFeature {Branch = branch, Chainage = 10*i});
            }

            IBranchFeature before = null;
            IBranchFeature after = null;

            NetworkHelper.GetNeighboursOnBranch(branch, 5, out before, out after);

            Assert.IsNull(before);
            Assert.AreEqual(branch.BranchFeatures[0], after);

            NetworkHelper.GetNeighboursOnBranch(branch, 45, out before, out after);

            Assert.AreEqual(branch.BranchFeatures[3], before);
            Assert.AreEqual(branch.BranchFeatures[4], after);

            NetworkHelper.GetNeighboursOnBranch(branch, 50, out before, out after);

            Assert.AreEqual(branch.BranchFeatures[3], before);
            Assert.AreEqual(branch.BranchFeatures[5], after);

            NetworkHelper.GetNeighboursOnBranch(branch, 95, out before, out after);

            Assert.AreEqual(branch.BranchFeatures[8], before);
            Assert.IsNull(after);
        }

        [Test]
        public void NetworkLocationsAreNotAddedToBranchFeatures()
        {
            //issue pinpoints the problem of issue 2358
            var branch = new Branch
            {
                Source = new Node("n1"),
                Target = new Node("n2"),
                Geometry = GeometryFromWKT.Parse("LINESTRING(219478.546875 495899.46875,219434.578125 495917.3125,219202.234375 495979,219074.015625 496013.59375,219046.5 496021.15625,218850.078125 496073.59375,218732.421875 496105.65625,218572.546875 496148.9375,218473.515625 496173.90625,218463.546875 496176.65625,218454.734375 496178.9375,218264.84375 496229.65625,218163.8125 496256.53125,218073.453125 496280.59375,217862.65625 496336.71875,217672.5 496387.3125,217487.9375 496436.4375,217293.546875 496488.1875,217103.90625 496538.65625,216945.078125 496580.9375,216748.34375 496633.3125,216581.8125 496677.65625,216380.8125 496731.15625,216205.03125 496777.9375,215978.53125 496838.21875,215819.328125 496880.59375,215646.3125 496926.65625,215563.609375 496948.6875,215502.453125 496966.28125,215501.234375 496966.625,215448 496981.40625,215216.5 497069.90625,215083.546875 497124.40625,214913.109375 497191.34375,214733.015625 497260.15625,214557.265625 497329.3125,214369.578125 497403.1875,214147.828125 497490.4375,213947.328125 497569.34375,213883.34375 497594.0625,213862.03125 497601.0625,213848.140625 497603.0625,213627.5625 497609.375,213513.796875 497612.53125,213494.71875 497614.9375,213471.40625 497621.9375,213325.015625 497681.5,213317.515625 497684.40625,213049.609375 497788.21875,212896.21875 497844.78125,212675.125 497868.21875,212426.046875 497843.375,212282.171875 497830.03125,212042.1875 497810.25,211798.59375 497791.46875,211515.140625 497766.625,211509.109375 497766.09375,211234.28125 497741.8125,211070.875 497729.75,210538.3125 497760.78125,210445.53125 497768.15625,210408.921875 497763.28125,210372.796875 497752.5625,210325.953125 497745.0625,210279.109375 497742.5625,210237.625 497753.625,210209.734375 497769.71875,210001.203125 497891.03125,209796.765625 498006.6875,209763.90625 498020.5625,209722.265625 498029.3125,209691.59375 498026.40625,209600.1875 498008.53125,209498.828125 497991.75,209441.78125 497987.15625,209390.375 497987.96875,209337.359375 497996.8125,208898.703125 498072.90625,208331.625 498166.03125,208286.40625 498172.8125)")
            };
            NetworkHelper.AddBranchFeatureToBranch(new NetworkLocation(branch,1), branch);
            Assert.AreEqual(0,branch.BranchFeatures.Count);
        }

        [Test]
        public void InterpolateTest()
        {
            Assert.AreEqual(25.0, NetworkHelper.InterpolateDouble(0.0, 100.0, 25.0, 0, 100), 1.0e-6);
            Assert.AreEqual(0.0, NetworkHelper.InterpolateDouble(0.0, 100.0, 0.0, 0, 100), 1.0e-6);
            Assert.AreEqual(100.0, NetworkHelper.InterpolateDouble(0.0, 100.0, 100.0, 0, 100), 1.0e-6);

            Assert.AreEqual(25.0, NetworkHelper.InterpolateDouble(10.0, 90.0, 25.0, 10, 90), 1.0e-6);
            Assert.AreEqual(10.0, NetworkHelper.InterpolateDouble(10.0, 90.0, 10.0, 10, 90), 1.0e-6);
            Assert.AreEqual(90.0, NetworkHelper.InterpolateDouble(10.0, 90.0, 90.0, 10, 90), 1.0e-6);

            Assert.AreEqual(25.0, NetworkHelper.InterpolateDouble(100.0, 0.0, 25.0, 100, 0), 1.0e-6);
            Assert.AreEqual(0.0, NetworkHelper.InterpolateDouble(100.0, 0.0, 0.0, 100, 0), 1.0e-6);
            Assert.AreEqual(100.0, NetworkHelper.InterpolateDouble(100.0, 0.0, 100.0, 100, 0), 1.0e-6);

            Assert.AreEqual(25.0, NetworkHelper.InterpolateDouble(90.0, 10.0, 25.0, 90, 10), 1.0e-6);
            Assert.AreEqual(10.0, NetworkHelper.InterpolateDouble(90.0, 10.0, 10.0, 90, 10), 1.0e-6);
            Assert.AreEqual(90.0, NetworkHelper.InterpolateDouble(90.0, 10.0, 90.0, 90, 10), 1.0e-6);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void InterpolateBeforeInterval()
        {
            Assert.AreEqual(100.0, NetworkHelper.InterpolateDouble(10.0, 90.0, 100.0, 10, 90), 1.0e-6);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void InterpolateAfterInterval()
        {
            Assert.AreEqual(100.0, NetworkHelper.InterpolateDouble(10.0, 90.0, 100.0, 10, 90), 1.0e-6);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Jira)]
        public void CreateRouteForBranchWithCustomLengthTools3068()
        {
            var network = new Network();

            var branch = new Branch
            {
                Geometry = new LineString(new[]
                     {
                         new Coordinate(0, 0), new Coordinate(1000, 0)
                     })
            };

            var start = new Node { Network = network, Geometry = new Point(new Coordinate(0, 0)), Name = "StartNode" };
            var end = new Node { Network = network, Geometry = new Point(new Coordinate(1000, 0)), Name = "EndNode" };

            branch.Source = start;
            branch.Target = end;

            network.Branches.Add(branch);
            network.Nodes.Add(start);
            network.Nodes.Add(end);

            branch.IsLengthCustom = true;
            branch.Length = 1500;

            // extract path via networklocation. networklocations and branchfeatures are all always given 'userlength based'
            var path = NetworkHelper.GetShortestPathBetweenBranchFeaturesAsNetworkSegments(network,
                                                       new NetworkLocation(branch, 100),
                                                       new NetworkLocation(branch, 1400));
            Assert.AreEqual(1, path.Count);

            // Expect route to have length based on start and end location
            Assert.AreEqual(1300.0, path[0].Length, 1.0e-6);
            // geometry will have relative length based on branch.geometry.Length and branch.Length
            Assert.AreEqual(1300.0*(1000.0/1500.0), path[0].Geometry.Length, 1.0e-6);
        }

        [Test]
        public void SplitBranchUsesEditAction()
        {
            var network = GetNetworkWithSingleBranch(1000);
            var branch = network.Branches.First();
            int isEditingChangeCount = 0;
            IBranch newBranch = null, splittedBranch = null;
            ((INotifyPropertyChanged) network).PropertyChanged += (s, e) =>
                                                                      {
                                                                          if (e.PropertyName == "IsEditing")
                                                                          {
                                                                              isEditingChangeCount++;

                                                                              newBranch =
                                                                                  ((BranchSplitAction)
                                                                                   network.CurrentEditAction).NewBranch;
                                                                              splittedBranch = ((BranchSplitAction)
                                                                                   network.CurrentEditAction).SplittedBranch;
                                                                          }
                                                                      };

            var result = NetworkHelper.SplitBranchAtNode(branch, 50);

            Assert.AreEqual(2, isEditingChangeCount); // Set to true, and set to false
            Assert.IsNull(network.CurrentEditAction);
            Assert.AreEqual(result.NewBranch, newBranch);
            Assert.AreEqual(branch, splittedBranch);
        }

        [Test]
        public void MergeBranchesWithCustomLengthSumsTheCustomLength()
        {
            //arrange : network with 2 branches
            var network = RouteHelperTest.GetSnakeNetwork(false, 2);

            var branch1 = network.Branches[0];
            var branch2 = network.Branches[1];

            branch1.IsLengthCustom = true;
            branch1.Length = 40.0;

            branch2.IsLengthCustom = true;
            branch2.Length = 33.0;

            //action! the 2nd node is the connection node...remove it
            NetworkHelper.MergeNodeBranches(network.Nodes[1],network);

            Assert.AreEqual(73.0,branch1.Length);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void MergeBranchesWithCustomLength(bool isCustomLength)
        {
            //arrange : network with 2 branches
            var network = RouteHelperTest.GetSnakeNetwork(false, 2);
            if (isCustomLength) network.Branches.ForEach(b => b.IsLengthCustom = true);

            NetworkHelper.AddBranchFeatureToBranch(new TestBranchFeature(), network.Branches[0], 10.0);
            NetworkHelper.AddBranchFeatureToBranch(new TestBranchFeature(), network.Branches[0], 60.0);
            NetworkHelper.AddBranchFeatureToBranch(new TestBranchFeature(), network.Branches[1], 30.0);
            NetworkHelper.AddBranchFeatureToBranch(new TestBranchFeature(), network.Branches[1], 90.0);

            //action! the 2nd node is the connection node...remove it
            NetworkHelper.MergeNodeBranches(network.Nodes[1], network);

            Assert.AreEqual(4, network.BranchFeatures.Count());
            var expectedChainage = new[] {10.0, 60.0, 130.0, 190.0};
            var index = 0;
            foreach (var branchFeature in network.BranchFeatures)
            {
                Assert.AreEqual(network.Branches[0], branchFeature.Branch);
                Assert.AreEqual(expectedChainage[index++], branchFeature.Chainage);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Cannot merge branch 'jan' and 'klaas'. One has a custom length and the other does not.")]
        public void CanNotMergeABranchWithCustomAndNonCustomLength()
        {
            //arrange : network with 2 branches : one custom and the other is not
            var network = RouteHelperTest.GetSnakeNetwork(false, 2);

            var branch1 = network.Branches[0];
            var branch2 = network.Branches[1];

            branch1.Name = "jan";
            branch1.IsLengthCustom = true;
            branch1.Length = 40.0;

            branch2.Name = "klaas";

            //action! this should throw an exception
            NetworkHelper.MergeNodeBranches(network.Nodes[1], network);
        }

        /// <summary>
        /// Branch property of cross section should be updated when the cross section
        /// is added to or removed from a branch.
        /// </summary>
        [Test]
        public void AddCrossSectionToBranch()
        {
            var branchFeature = mocks.Stub<IBranchFeature>();
            var branch = new Branch(new Node("from"), new Node("To"));
            NetworkHelper.AddBranchFeatureToBranch(branchFeature, branch, branchFeature.Chainage);
            Assert.AreEqual(branch, branchFeature.Branch);
        }
        
        [Test]
        public void MapChainageTest()
        {
            var branch = new Branch
                             {
                                 Geometry = new LineString(new[]
                                                               {
                                                                   new Coordinate(0, 0), new Coordinate(0, 50),
                                                               }),
                                 IsLengthCustom = true,
                                 Length = 100.0
                             };
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(10*i, NetworkHelper.MapChainage(branch, 20.0*i), BranchFeature.Epsilon);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Jira)] //TOOLS-7021
        public void GetNearestBranchShouldNotThrowForNullGeometry()
        {
            var nearestBranch = NetworkHelper.GetNearestBranch(new[] { new Branch { Geometry = null }, new Branch { Geometry = null }, new Branch { Geometry = null } },
                                                               new Point(0, 0), 5);
            Assert.IsNull(nearestBranch);

            var branchToBeFound = new Branch {Geometry = new LineString(new[] {new Coordinate(0.2, 0.4), new Coordinate(0.6, 3)})};
            nearestBranch =
                NetworkHelper.GetNearestBranch(
                    new[]
                        {
                            branchToBeFound,
                            new Branch {Geometry = null}, 
                            new Branch {Geometry = new LineString(new[] {new Coordinate(0.6, 0.4), new Coordinate(3, 1.6)})}
                        },
                    new Point(0, 0), 5);
            Assert.IsNotNull(nearestBranch);
            Assert.IsTrue(branchToBeFound == nearestBranch);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Jira)] //TOOLS-7624
        public void GetShortestPathShouldStayWithinNetworkBranches()
        {
            // Create network with 2 orthogonal branches

            var network = new Network();
            var branch1 = new Branch
            {
                Name = "Branch1",
                Geometry = new LineString(new[]
                     {
                         new Coordinate(0, 0), new Coordinate(100, 0)
                     })
            };
            var branch2 = new Branch
            {
                Name = "Branch2",
                Geometry = new LineString(new[]
                     {
                         new Coordinate(100, 0), new Coordinate(100, 100)
                     })
            };
            var start = new Node { Network = network, Geometry = new Point(new Coordinate(0, 0)), Name = "StartNode" };
            var middle = new Node { Network = network, Geometry = new Point(new Coordinate(100, 0)), Name = "MiddleNode"};
            var end = new Node { Network = network, Geometry = new Point(new Coordinate(100, 100)), Name = "EndNode" };
            branch1.Source = start;
            branch1.Target = middle;
            branch2.Source = middle;
            branch2.Target = end;
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            network.Nodes.Add(start);
            network.Nodes.Add(middle);
            network.Nodes.Add(end);

            // Add a location to each node

            var startLocation = new NetworkLocation(branch1, 0);
            var middleLocation = new NetworkLocation(branch1, 100);
            var endLocation = new NetworkLocation(branch2, 100);

            // Compute the segment over the second branch

            var segments = NetworkHelper.GetShortestPathBetweenBranchFeaturesAsNetworkSegments(network, middleLocation, endLocation);

            // Validate

            Assert.AreEqual(1, segments.Count, "Unexpected number of segments found");

            var segment = segments.FirstOrDefault();

            Assert.AreEqual(0, segment.Chainage, "Incorrect offset of segment calculated");
            Assert.AreEqual(100, segment.EndChainage, "Incorrect final offset of segment calculated.");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Jira)] // TOOLS-9546
        public void MapChainageShouldNotThrowWhenFeatureNotOnBranch()
        {
            var featureNotOnBranch = new SimpleBranchFeature { Chainage = 600 };
            Assert.AreEqual(600, NetworkHelper.MapChainage(featureNotOnBranch));

            Assert.AreEqual(600, NetworkHelper.MapChainage(null, 600));

            Assert.AreEqual(600, NetworkHelper.MapChainage(null, null, 600));
        }

        private static Network GetNetworkWithSingleBranch(int branchLength)
        {
            var network = new Network();

            var branch = new Branch
                             {
                                 Geometry = new LineString(new[]
                                                               {
                                                                   new Coordinate(0, 0), new Coordinate(branchLength, 0)
                                                               })
                             };

            var start = new Node { Network = network, Geometry = new Point(new Coordinate(0, 0)), Name = "StartNode" };
            var end = new Node { Network = network, Geometry = new Point(new Coordinate(branchLength, 0)), Name = "EndNode" };

            branch.Source = start;
            branch.Target = end;

            network.Branches.Add(branch);
            network.Nodes.Add(start);
            network.Nodes.Add(end);
            return network;
        }
    }
}
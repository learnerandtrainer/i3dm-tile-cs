﻿using I3dm.Tile;
using NUnit.Framework;
using System;
using System.IO;
using System.Numerics;

namespace i3dm.tile.tests
{
    public class I3dmWriterTest
    {
        [Test]
        public void WriteB3dmTest()
        {
            // arrange
            var i3dmExpectedfile = File.OpenRead(@"testfixtures/tree.i3dm");
            var i3dmExpected = I3dmReader.ReadI3dm(i3dmExpectedfile);
            var positions = i3dmExpected.Positions;
            Assert.IsTrue(positions.Count == 25);

            var treeGlb = File.ReadAllBytes(@"testfixtures/tree.glb");
            var i3dm = new I3dm.Tile.I3dm(positions, treeGlb);
            i3dm.I3dmHeader.GltfFormat = 1;
            i3dm.FeatureTable.IsEastNorthUp = true;
            i3dm.BatchTableJson = @"{""Height"":[20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20]} ";
            // i3dm.FeatureTableJson = @"{""INSTANCES_LENGTH"":25,""EAST_NORTH_UP"":true,""POSITION"":{""byteOffset"":0}}";

            // act
            var result = @"testfixtures/tree_actual.i3dm";
            I3dmWriter.WriteI3dm(result, i3dm);

            var i3dmActualStream = File.OpenRead(@"testfixtures/tree_actual.i3dm");
            var i3dmActual = I3dmReader.ReadI3dm(i3dmActualStream);

            // Assert
            Assert.IsTrue(i3dmActual.I3dmHeader.Version== 1);
            Assert.IsTrue(i3dmActual.I3dmHeader.Magic== "i3dm");
            Assert.IsTrue(i3dmActual.I3dmHeader.GltfFormat == 1);
            Assert.IsTrue(i3dmActual.I3dmHeader.BatchTableJsonByteLength == 88);
            Assert.IsTrue(i3dmActual.I3dmHeader.FeatureTableJsonByteLength == 72); // 
            Assert.IsTrue(i3dmActual.I3dmHeader.FeatureTableBinaryByteLength == 25*4*3); // note: is 304 in original file?
            Assert.IsTrue(i3dmActual.I3dmHeader.ByteLength == 282064); // Note  is: 282072 originally)
            Assert.IsTrue(i3dmActual.I3dmHeader.BatchTableBinaryByteLength == 0);
            Assert.IsTrue(i3dmActual.Positions.Count == 25);
            Assert.IsTrue(i3dmActual.FeatureTable.IsEastNorthUp== true);
            Assert.IsTrue(i3dmActual.Positions[0].Equals(new Vector3(1214947.2f, -4736379f, 4081540.8f)));
            var stream = new MemoryStream(i3dmActual.GlbData);
            var glb = SharpGLTF.Schema2.ModelRoot.ReadGLB(stream);
            Assert.IsTrue(glb.Asset.Version.Major == 2.0);
            Assert.IsTrue(glb.Asset.Generator == "COLLADA2GLTF");
        }

        const int BYTES_TO_READ = sizeof(Int64);

        static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            if (first.Length != second.Length)
                return false;

            if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);
                    fs2.Read(two, 0, BYTES_TO_READ);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }

            return true;
        }


    }
}
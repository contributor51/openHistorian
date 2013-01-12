﻿//******************************************************************************************************
//  SubFileStreamTest.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/10/2011 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using openHistorian.IO;
using openHistorian.IO.Unmanaged;

namespace openHistorian.FileStructure.Test
{
    /// <summary>
    /// Provides a stream that converts the virtual addresses of the internal feature files to physical address
    /// Also provides a way to copy on write to support the versioning file system.
    /// </summary>
    [TestFixture()]
    public class SubFileStreamTest
    {
        static int BlockSize = 4096;
        static int BlockDataLength = BlockSize - FileStructureConstants.BlockFooterLength;

        [Test()]
        public void Test()
        {
            Assert.AreEqual(Globals.BufferPool.AllocatedBytes, 0L);

            DiskIo stream = new DiskIo(BlockSize, new MemoryStream(), 0);
            TestReadAndWrites(stream);
            TestReadAndWritesWithCommit(stream);
            TestReadAndWritesToDifferentFilesWithCommit(stream);
            TestBinaryStream(stream);
            stream.Dispose();
            Assert.IsTrue(true);
            Assert.AreEqual(Globals.BufferPool.AllocatedBytes, 0L);

        }
        static void TestBinaryStream(DiskIo stream)
        {
            FileHeaderBlock header = new FileHeaderBlock(BlockSize, stream, OpenMode.Create, AccessMode.ReadWrite);
            SubFileMetaData node = header.CreateNewFile(Guid.NewGuid());
            header.CreateNewFile(Guid.NewGuid());
            header.CreateNewFile(Guid.NewGuid());

            SubFileStream ds = new SubFileStream(BlockSize, stream, node, header, AccessMode.ReadWrite);
            BinaryStreamTest.Test(ds);
        }

        static void TestReadAndWrites(DiskIo stream)
        {
            FileHeaderBlock header = new FileHeaderBlock(BlockSize, stream, OpenMode.Create, AccessMode.ReadWrite);
            SubFileMetaData node = header.CreateNewFile(Guid.NewGuid());
            header.CreateNewFile(Guid.NewGuid());
            header.CreateNewFile(Guid.NewGuid());

            SubFileStream ds = new SubFileStream(BlockSize, stream, node, header, AccessMode.ReadWrite);
            TestSingleByteWrite(ds);
            TestSingleByteRead(ds);

            TestCustomSizeWrite(ds, 5);
            TestCustomSizeRead(ds, 5);

            TestCustomSizeWrite(ds, BlockDataLength + 20);
            TestCustomSizeRead(ds, BlockDataLength + 20);
            header.WriteToFileSystem(stream);
        }

        static void TestReadAndWritesWithCommit(DiskIo stream)
        {
            FileHeaderBlock header;
            SubFileMetaData node;
            SubFileStream ds, ds1, ds2;
            //Open The File For Editing
            header = new FileHeaderBlock(BlockSize, stream, OpenMode.Open, AccessMode.ReadWrite);
            node = header.Files[0];
            ds = new SubFileStream(BlockSize, stream, node, header, AccessMode.ReadWrite);
            TestSingleByteWrite(ds);
            header.WriteToFileSystem(stream);

            header = new FileHeaderBlock(BlockSize, stream, OpenMode.Open, AccessMode.ReadOnly);
            node = header.Files[0];
            ds1 = ds = new SubFileStream(BlockSize, stream, node, header, AccessMode.ReadOnly);
            TestSingleByteRead(ds);

            //Open The File For Editing
            header = new FileHeaderBlock(BlockSize, stream, OpenMode.Open, AccessMode.ReadWrite);
            node = header.Files[0];
            ds = new SubFileStream(BlockSize, stream, node, header, AccessMode.ReadWrite);
            TestCustomSizeWrite(ds, 5);
            header.WriteToFileSystem(stream);

            header = new FileHeaderBlock(BlockSize, stream, OpenMode.Open, AccessMode.ReadOnly);
            node = header.Files[0];
            ds2 = ds = new SubFileStream(BlockSize, stream, node, header, AccessMode.ReadOnly);
            TestCustomSizeRead(ds, 5);

            //Open The File For Editing
            header = new FileHeaderBlock(BlockSize, stream, OpenMode.Open, AccessMode.ReadWrite);
            node = header.Files[0];
            ds = new SubFileStream(BlockSize, stream, node, header, AccessMode.ReadWrite);
            TestCustomSizeWrite(ds, BlockDataLength + 20);
            header.WriteToFileSystem(stream);

            header = new FileHeaderBlock(BlockSize, stream, OpenMode.Open, AccessMode.ReadOnly);
            node = header.Files[0];
            ds = new SubFileStream(BlockSize, stream, node, header, AccessMode.ReadOnly);
            TestCustomSizeRead(ds, BlockDataLength + 20);

            //check old versions of the file
            TestSingleByteRead(ds1);
            TestCustomSizeRead(ds2, 5);
        }

        static void TestReadAndWritesToDifferentFilesWithCommit(DiskIo stream)
        {
            FileHeaderBlock header;

            SubFileStream ds;
            //Open The File For Editing
            header = new FileHeaderBlock(BlockSize, stream, OpenMode.Open, AccessMode.ReadWrite);
            ds = new SubFileStream(BlockSize, stream, header.Files[0], header, AccessMode.ReadWrite);
            TestSingleByteWrite(ds);
            ds = new SubFileStream(BlockSize, stream, header.Files[1], header, AccessMode.ReadWrite);
            TestCustomSizeWrite(ds, 5);
            ds = new SubFileStream(BlockSize, stream, header.Files[2], header, AccessMode.ReadWrite);
            TestCustomSizeWrite(ds, BlockDataLength + 20);
            header.WriteToFileSystem(stream);

            header = new FileHeaderBlock(BlockSize, stream, OpenMode.Open, AccessMode.ReadOnly);
            ds = new SubFileStream(BlockSize, stream, header.Files[0], header, AccessMode.ReadOnly);
            TestSingleByteRead(ds);
            ds = new SubFileStream(BlockSize, stream, header.Files[1], header, AccessMode.ReadOnly);
            TestCustomSizeRead(ds, 5);
            ds = new SubFileStream(BlockSize, stream, header.Files[2], header, AccessMode.ReadOnly);
            TestCustomSizeRead(ds, BlockDataLength + 20);

        }


        internal static void TestSingleByteWrite(SubFileStream ds)
        {
            //ds.Position = 0;
            //for (int x = 0; x < 10000; x++)
            //{
            //    ds.WriteByte((byte)x);
            //}
            //ds.Flush();
        }
        internal static void TestSingleByteRead(SubFileStream ds)
        {
            //ds.Position = 0;
            //for (int x = 0; x < 10000; x++)
            //{
            //    if ((byte)x != ds.ReadByte())
            //        throw new Exception();
            //}
        }

        internal static void TestCustomSizeWrite(SubFileStream ds, int length)
        {
            //Random r = new Random(length);

            //ds.Position = 0;
            //byte[] buffer = new byte[25];

            //for (int x = 0; x < 1000; x++)
            //{
            //    for (int i = 0; i < buffer.Length; i++)
            //    {
            //        buffer[i] = (byte)r.Next();
            //    }
            //    ds.Write(buffer, 0, r.Next(25));
            //}
            //ds.Flush();
        }

        internal static void TestCustomSizeRead(SubFileStream ds, int seed)
        {
            //Random r = new Random(seed);

            //byte[] buffer = new byte[25];
            //byte[] buffer2 = new byte[25];
            //ds.Position = 0;
            //for (int x = 0; x < 1000; x++)
            //{
            //    for (int i = 0; i < buffer.Length; i++)
            //    {
            //        buffer[i] = (byte)r.Next();
            //    }
            //    int length = r.Next(25);
            //    ds.Read(buffer2, 0, length);

            //    for (int i = 0; i < length; i++)
            //    {
            //        if (buffer[i] != buffer2[i])
            //            throw new Exception();
            //    }
            //}
            //ds.Flush();
        }

    }
}

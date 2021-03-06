﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using GSF.IO;
using GSF.Security.Authentication;
using NUnit.Framework;

namespace GSF.Security
{
    [TestFixture]
    public class Scram_Test
    {
        Stopwatch m_sw = new Stopwatch();

        [Test]
        public void Test1()
        {
            m_sw.Reset();

            var net = new NetworkStreamSimulator();

            var sw = new Stopwatch();
            var sa = new ScramServer();
            sw.Start();
            sa.Users.AddUser("user1", "password1", 10000, 1, HashMethod.Sha256);
            sw.Stop();
            System.Console.WriteLine(sw.Elapsed.TotalMilliseconds);
            ThreadPool.QueueUserWorkItem(Client1, net.ClientStream);
            var user = sa.AuthenticateAsServer(net.ServerStream, new byte[] { 100, 29 });
            user = sa.AuthenticateAsServer(net.ServerStream, new byte[] { 100, 29 });
            if (user == null)
                throw new Exception();

            Thread.Sleep(100);
        }

        void Client1(object state)
        {
            Stream client = (Stream)state;
            var sa = new ScramClient("user1", "password1");
            sa.AuthenticateAsClient(client, new byte[] { 100, 29 });
            m_sw.Start();
            var success = sa.AuthenticateAsClient(client, new byte[] { 100, 29 });
            m_sw.Stop();
            System.Console.WriteLine(m_sw.Elapsed.TotalMilliseconds);
            if (!success)
                throw new Exception();
        }

        [Test]
        public void TestMultiple()
        {
            Test1();
            Test1();
            Test1();
            Test1();
            Test1();
            Test1();
            Test1();
        }

    }
}

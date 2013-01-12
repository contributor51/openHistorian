﻿//******************************************************************************************************
//  RemoteHistorian_DataReader.cs - Gbtc
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
//  12/8/2012 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************

using System;

namespace openHistorian.Communications
{
    public partial class RemoteHistorian
    {
        class HistorianDataReader : HistorianDataReaderBase
        {
            Action m_onDispose;
            RemoteHistorian m_client;
            PointReader m_reader;
            bool m_closed;
            public HistorianDataReader(RemoteHistorian client, Action onDispose)
            {
                m_onDispose = onDispose;
                m_client = client;
            }

            public override void Dispose()
            {
                Close();
            }

            public override IPointStream Read(KeyParserPrimary key1, KeyParserSecondary key2, DataReaderOptions readerOptions)
            {
                if (m_reader != null)
                    throw new Exception("Sockets do not support concurrent readers.");

                m_client.m_stream.Write((byte)ServerCommand.Read);
                key1.Save(m_client.m_stream);
                key2.Save(m_client.m_stream);
                readerOptions.Save(m_client.m_stream);
                m_client.m_netStream.Flush();
                return new PointReader(m_client, () => m_reader = null);
            }

            public override void Close()
            {
                if (!m_closed)
                {
                    if (m_reader != null)
                        m_reader.Cancel();

                    m_client.m_stream.Write((byte)ServerCommand.DisconnectReader);
                    m_client.m_netStream.Flush();
                    m_onDispose();
                }
            }

            class PointReader : IPointStream
            {
                bool m_completed;
                ulong m_key1 = 0, m_key2 = 0, m_value1 = 0, m_value2 = 0;
                RemoteHistorian m_client;
                Action m_onComplete;

                public PointReader(RemoteHistorian client, Action onComplete)
                {
                    m_client = client;
                    m_onComplete = onComplete;
                }

                public bool Read(out ulong key1, out ulong key2, out ulong value1, out ulong value2)
                {
                    if (!m_completed && m_client.m_stream.ReadBoolean())
                    {
                        m_key1 ^= m_client.m_stream.Read7BitUInt64();
                        m_key2 ^= m_client.m_stream.Read7BitUInt64();
                        m_value1 ^= m_client.m_stream.Read7BitUInt64();
                        m_value2 ^= m_client.m_stream.Read7BitUInt64();
                        key1 = m_key1;
                        key2 = m_key2;
                        value1 = m_value1;
                        value2 = m_value2;
                        return true;
                    }
                    else
                    {
                        key1 = 0;
                        key2 = 0;
                        value1 = 0;
                        value2 = 0;
                        Complete();
                        return false;
                    }
                }

                public void Cancel()
                {
                    if (m_completed)
                        return;
                    m_client.m_stream.Write((byte)ServerCommand.CancelRead);
                    m_client.m_netStream.Flush();
                    //flush the rest of the data off of the receive queue.
                    while (m_client.m_stream.ReadBoolean())
                    {
                        m_client.m_stream.Read7BitUInt64();
                        m_client.m_stream.Read7BitUInt64();
                        m_client.m_stream.Read7BitUInt64();
                        m_client.m_stream.Read7BitUInt64();
                    }
                    Complete();
                }

                void Complete()
                {
                    m_completed = true;
                    m_onComplete();
                }
            }
        }
    }
}

﻿//******************************************************************************************************
//  IPointStream.cs - Gbtc
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
//******************************************************************************************************


namespace openHistorian
{

    /// <summary>
    /// Represents a fundamental way to stream points.
    /// </summary>
    public interface IPointStream
    {
        /// <summary>
        /// Gets the next point in the stream.
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns>True if the data is valid. False if the end of the stream has been encountered.</returns>
        bool Read(out ulong key1, out ulong key2, out ulong value1, out ulong value2);
        /// <summary>
        /// Prematurely stops the execution of the stream.
        /// Once canceled, no more points will be returned.
        /// </summary>
        void Cancel();
    }
}

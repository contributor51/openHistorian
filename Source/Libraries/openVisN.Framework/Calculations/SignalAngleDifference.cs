﻿//******************************************************************************************************
//  SignalAngleDifference.cs - Gbtc
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
//  12/12/2012 - Steven E. Chisholm
//       Generated original version of source code. 
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using openHistorian.Data.Query;

namespace openVisN.Calculations
{
    class SignalAngleDifference : CalculationMethod
    {
        MetadataBase m_newSignal;

        public SignalAngleDifference(MetadataBase signal, MetadataBase signalReference)
            : base(signal, signalReference)
        {
            m_newSignal = new MetadataDouble(Guid.NewGuid(), null, "", "", this);
        }

        public void GetPoints(out MetadataBase newSignal)
        {
            newSignal = m_newSignal;
        }

        public override void Calculate(IDictionary<Guid, SignalDataBase> signals)
        {
            Dependencies[0].Calculate(signals);
            Dependencies[1].Calculate(signals);

            var signal = signals[Dependencies[0].UniqueId];
            var signalReference = signals[Dependencies[1].UniqueId];

            var newSignal = TryGetSignal(m_newSignal, signals);

            if (newSignal == null || newSignal.IsComplete)
                return;

            int pos = 0;
            int posRef = 0;

            while (pos < signal.Count && posRef < signalReference.Count)
            {
                ulong time, timeRef;
                double ang, angRef;
                signal.GetData(pos, out time, out ang);
                signalReference.GetData(posRef, out timeRef, out angRef);

                if (time == timeRef)
                {
                    pos++;
                    posRef++;

                    double delta = ang - angRef;
                    if (delta > 180.0)
                        delta -= 360.0;
                    if (delta < -180.0)
                        delta += 360.0;

                    newSignal.AddData(time, delta);
                }
                else
                {
                    if (time < timeRef)
                        pos++;
                    else
                        posRef++;
                }


            }
            newSignal.Completed();
        }

        SignalDataBase TryGetSignal(MetadataBase signal, IDictionary<Guid, SignalDataBase> results)
        {
            SignalDataBase data;
            if (results.TryGetValue(signal.UniqueId, out data))
                return data;
            return null;
        }
    }
}

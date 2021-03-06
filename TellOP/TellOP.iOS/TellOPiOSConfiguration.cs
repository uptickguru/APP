﻿// <copyright file="TellOPiOSConfiguration.cs" company="University of Murcia">
// Copyright © 2016 University of Murcia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Alessandro Menti</author>

#pragma warning disable SA1300
namespace TellOP.iOS
#pragma warning restore SA1300
{
    /// <summary>
    /// A class containing IDs and secrets specific to TellOP.iOS.
    /// </summary>
    internal sealed class TellOPiOSConfiguration
    {
        /// <summary>
        /// The application ID for the HockeyApp service.
        /// </summary>
        internal const string HockeyAppId = "cbcccbfe5e014a73ba42edac352905eb";

        // TODO: fill in the following field with the HockeyApp secret.

        /// <summary>
        /// The secret for the HockeyApp service.
        /// </summary>
        internal const string HockeyAppSecret = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="TellOPiOSConfiguration"/> class.
        /// </summary>
        private TellOPiOSConfiguration()
        {
        }
    }
}

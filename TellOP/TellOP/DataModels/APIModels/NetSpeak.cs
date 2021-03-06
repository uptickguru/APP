// <copyright file="NetSpeak.cs" company="University of Murcia">
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
// <author>Mattia Zago</author>
// <author>Alessandro Menti</author>

namespace TellOP.DataModels.APIModels
{
    using Newtonsoft.Json;

    /// <summary>
    /// A JSON object representing a single entry in the reply returned by the "NetSpeak preceding/following" search
    /// endpoints.
    /// </summary>
    [JsonObject]
    public class NetSpeak
    {
        /// <summary>
        /// Gets or sets the unique ID of the dictionary entry.
        /// </summary>
        [JsonProperty("ID")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the word frequency in the dataset used by NetSpeak.
        /// </summary>
        [JsonProperty("frequency")]
        public int Frequency { get; set; }

        /// <summary>
        /// Gets or sets the word (dictionary entry).
        /// </summary>
        [JsonProperty("word")]
        public string Word { get; set; }
    }
}

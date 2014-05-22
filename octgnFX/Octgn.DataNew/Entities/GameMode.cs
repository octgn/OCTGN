/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Octgn.DataNew.Entities
{
    public class GameMode
    {
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string Image { get; set; }
        public int PlayerCount { get; set; }
        public bool UseTwoSidedTable { get; set; }
    }
}
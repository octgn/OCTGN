// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/
using System;
using System.Collections.Generic;

namespace CassiniDev
{
    [Serializable]
    public class BrowserTestResultItem
    {
        public bool Success { get; set; }
        public string Name { get; set; }
        public int Failures { get; set; }
        public int Total { get; set; }
        public List<string> Log { get; set; }
        public virtual void Parse(string log)
        {
            throw new NotImplementedException();
        }
        public BrowserTestResultItem()
        {
            Items = new Dictionary<string, BrowserTestResultItem>();
            Log = new List<string>();
        }

        public Dictionary<string, BrowserTestResultItem> Items { get; set; }

    }
}
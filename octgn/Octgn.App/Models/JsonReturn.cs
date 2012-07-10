// 
//  JsonReturn.cs
//  
//  Author:
//       Kelly Elton <kelly.elton@skylabsonline.com>
// 
//  Copyright (c) 2012 Kelly Elton - Skylabs Corporation
//  All Rights Reserved.
using System;
using System.Web.Mvc;

namespace Octgn.App
{
	public class JsonReturn
	{
		public string Result{get;set;}
		public string For{get;set;}
		public string Message{get;set;}
		public JsonReturn ()
		{
			
		}
		public JsonResult GetResult()
		{
			var sr = new System.Web.Script.Serialization.JavaScriptSerializer();
			JsonResult res = new JsonResult();
			res.Data = sr.Serialize(this);
			return res;
		}
		public static JsonResult Error(string f,string message)
		{
			var ret = new JsonReturn(){For=f,Message=message,Result="error"};
			return ret.GetResult();
		}
		public static JsonResult Success(string message)
		{
			var ret = new JsonReturn(){Message=message,Result="success"};
			return ret.GetResult();
		}
		public static JsonResult Success()
		{
			return Success("");
		}
	}
}


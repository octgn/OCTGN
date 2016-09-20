/*
 * This file was automatically generated!
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Octgn.Library.Localization;

namespace Octgn.Server
{
	sealed class BinaryParser
	{
		Handler handler;

		public BinaryParser(Handler handler)
		{ this.handler = handler; }

		public void Parse(byte[] data)
		{
			MemoryStream stream = new MemoryStream(data);
			BinaryReader reader = new BinaryReader(stream);
			short length;
			handler.muted = reader.ReadInt32();
			byte method = reader.ReadByte();
			switch (method)
			{
				case 0:
				{
					string arg0 = reader.ReadString();
					handler.Error(arg0);
					break;
				}
				case 1:
				{
					ulong arg0 = reader.ReadUInt64();
					string arg1 = reader.ReadString();
					handler.Boot(arg0, arg1);
					break;
				}
				case 3:
				{
					string arg0 = reader.ReadString();
					ulong arg1 = reader.ReadUInt64();
					string arg2 = reader.ReadString();
					Version arg3 = new Version(reader.ReadString());
					Version arg4 = new Version(reader.ReadString());
					Guid arg5 = new Guid(reader.ReadBytes(16));
					Version arg6 = new Version(reader.ReadString());
					string arg7 = reader.ReadString();
					bool arg8 = reader.ReadBoolean();
					handler.Hello(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
					break;
				}
				case 4:
				{

/// <summary> Copyright (C) 2004  Free Software Foundation, Inc.
/// *
/// Author: Alexander Gnauck AG-Software
/// *
/// This file is part of GNU Libidn.
/// *
/// This library is free software; you can redistribute it and/or
/// modify it under the terms of the GNU Lesser General Public License
/// as published by the Free Software Foundation; either version 2.1 of
/// the License, or (at your option) any later version.
/// *
/// This library is distributed in the hope that it will be useful, but
/// WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
/// Lesser General Public License for more details.
/// *
/// You should have received a copy of the GNU Lesser General Public
/// License along with this library; if not, write to the Free Software
/// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301
/// USA
/// </summary>

using System;

namespace gnu.inet.encoding
{	
	public class StringprepException:System.Exception
	{
		public static System.String CONTAINS_UNASSIGNED = "Contains unassigned code points.";
		public static System.String CONTAINS_PROHIBITED = "Contains prohibited code points.";
		public static System.String BIDI_BOTHRAL = "Contains both R and AL code points.";
		public static System.String BIDI_LTRAL = "Leading and trailing code points not both R or AL.";
		
		public StringprepException(System.String m) : base(m)
		{
		}
	}
}
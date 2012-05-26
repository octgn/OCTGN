using System;
using System.IO;

namespace Octgn.Releaser
{
    public class UpdateAssemblyVersion
    {
        public static bool Update(string fileName, Version vNew, Version vOld)
        {
			if (String.IsNullOrWhiteSpace(fileName) || vNew == null || vOld == null) return false;
			try
			{
				var sFileContents = "";
				using(var sr = new StreamReader(fileName))
				{
					sFileContents = sr.ReadToEnd();
					sr.Close();
				}

				var sOriginal = sFileContents.Remove(0 , sFileContents.IndexOf("AssemblyVersion" , 0 , StringComparison.Ordinal));
				sOriginal = sOriginal.Remove(0 , sOriginal.IndexOf("(\"" , StringComparison.Ordinal) + 2);
				sOriginal = sOriginal.Remove(sOriginal.IndexOf("\")" , StringComparison.Ordinal) ,
					sOriginal.Length - sOriginal.IndexOf("\")" , StringComparison.Ordinal));

				sFileContents = sFileContents.Replace("AssemblyVersion(\"" + vOld + "\")" , "AssemblyVersion(\"" + vNew + "\")");
				sFileContents = sFileContents.Replace("AssemblyFileVersion(\"" + vOld + "\")" ,"AssemblyFileVersion(\"" + vNew + "\")");

				using(var fs = new FileStream(fileName , FileMode.Truncate))
				{
					using(var sw = new StreamWriter(fs))
					{
						sw.Write(sFileContents);
						sw.Close();
						fs.Close();
						return true;
					}
				}
			}
			catch
			{
				return false;
			}
        }
    }
}
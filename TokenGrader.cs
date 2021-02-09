using System;
using System.Collections.Generic;

namespace judge
{
    public class TokenGrader : ICustomGrader
    {
        public bool Grade(Sio inputData, Sio referenceOutput, Sio submissionOutput)
        {
            string a = referenceOutput.Reader.ReadToEnd();
            string b = submissionOutput.Reader.ReadToEnd();
            var spl = a.Split(new []{" ","\n"}, StringSplitOptions.RemoveEmptyEntries);
            var spl2 = b.Split(new []{" ","\n"}, StringSplitOptions.RemoveEmptyEntries);

            var splf = new List<string>();
            var splf2 = new List<string>();
            foreach (var k in spl)
            {
                var p = k.Trim();
                if (p != "\n" && p != "")
                {
                    splf.Add(p);
                }
            }
            foreach (var k in spl2)
            {
                var p = k.Trim();
                if (p != "\n" && p != "")
                {
                    splf2.Add(p);
                }
            }

            if (splf.Count != splf2.Count) return false;
            for (int i = 0; i < splf.Count; i++)
            {
                if (splf[i] != splf2[i]) return false;
            }

            return true;
        }
    }
}
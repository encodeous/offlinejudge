using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace judge
{
    public class ExactGrader : ICustomGrader
    {
        public bool Grade(Sio inputData, Sio referenceOutput, Sio submissionOutput)
        {
            return referenceOutput.Reader.ReadToEnd() == submissionOutput.Reader.ReadToEnd();
        }
    }
}

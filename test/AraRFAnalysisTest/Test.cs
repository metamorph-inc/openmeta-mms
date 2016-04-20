using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AraRFAnalysis;
using Xunit;

namespace AraRFAnalysisTest
{
    public class OptionsTest
    {
        [Fact]
        public void ParseArguments_ValidArguments_DoesNotThrowException()
        {
            var args = "-x 15.0 -y 10.0 -r 0.0 -n -s 1x2 InvertedF.xml".Split();
            var options = new Options();

            Assert.DoesNotThrow(delegate { options.ParseArguments(args); });
        }

        [Fact]
        public void ParseArguments_XYPositionInvalidFormat_ThrowsException()
        {
            var args = "-x cheese -y 10.0 -r 0.0 -n -s 1x2 InvertedF.xml".Split();
            var exception = Assert.Throws<ArgumentException>(delegate { new Options().ParseArguments(args); });
            Assert.True(exception.Message.Contains("format"), "Incorrect exception message: " + exception.Message);

            args = "-x 1.0 -y pear -r 0.0 -n -s 1x2 InvertedF.xml".Split();
            exception = Assert.Throws<ArgumentException>(delegate { new Options().ParseArguments(args); });
            Assert.True(exception.Message.Contains("format"), "Incorrect exception message: " + exception.Message);
        }

        [Fact]
        public void ParseArguments_XYPositionMissing_ThrowsException()
        {
            var args = "-y 10.0 -r 0.0 -n -s 1x2 InvertedF.xml".Split();
            Assert.Throws<ArgumentException>(delegate { new Options().ParseArguments(args); });

            args = "-x 10.0 -r 0.0 -n -s 1x2 InvertedF.xml".Split();
            Assert.Throws<ArgumentException>(delegate { new Options().ParseArguments(args); });
        }

        [Fact]
        public void ParseArguments_XYPositionOutOfRange_ThrowsException()
        {
            var args = "-x -1.0 -y 10.0 -r 0.0 -n -s 1x2 InvertedF.xml".Split();
            Assert.Throws<ArgumentException>(delegate { new Options().ParseArguments(args); });

            args = "-x 100.0 -y 10.0 -r 0.0 -n -s 1x2 InvertedF.xml".Split();
            Assert.Throws<ArgumentException>(delegate { new Options().ParseArguments(args); });

            args = "-x 10.0 -y -1.0 -r 0.0 -n -s 1x2 InvertedF.xml".Split();
            Assert.Throws<ArgumentException>(delegate { new Options().ParseArguments(args); });

            args = "-x 10.0 -y 100.0 -r 0.0 -n -s 1x2 InvertedF.xml".Split();
            Assert.Throws<ArgumentException>(delegate { new Options().ParseArguments(args); });

            args = "-x 10.0 -y 30.0 -r 0.0 -n -s 1x2 InvertedF.xml".Split();
            Assert.Throws<ArgumentException>(delegate { new Options().ParseArguments(args); });

            args = "-x 10.0 -y 30.0 -r 0.0 -n -s 2x2 InvertedF.xml".Split();
            Assert.DoesNotThrow(delegate { new Options().ParseArguments(args); });
        }

        [Fact]
        public void ParseArguments_ModuleSizeMissing_ThrowsException()
        {
            var args = "-x 10.0 -r 0.0 -n InvertedF.xml".Split();
            Assert.Throws<ArgumentException>(delegate { new Options().ParseArguments(args); });
        }

        [Fact]
        public void ParseArguments_DutFilenameMissing_ThrowsException()
        {
            var args = "-x 10.0 -y 0.0 -r 0.0 -s 1x2 -n".Split();
            var options = new Options();

            var exception = Assert.Throws<ArgumentException>(delegate { options.ParseArguments(args); });
            Assert.True(exception.Message.Contains("specified"), "Incorrect exception message: " + exception.Message);
        }

        [Fact]
        public void ParseArguments_SlotIndexMissing_ThrowsException()
        {
            var args = "-x 10.0 -y 0.0 -r 0.0 -s 2x2 InvertedF.xml".Split();
            var exception = Assert.Throws<ArgumentException>(delegate { new Options().ParseArguments(args); });
            Assert.True(exception.Message.ToLower().Contains("slot index has not been specified"), "Incorrect exception message: " + exception.Message);
        }

        [Fact]
        public void ParseArguments_ModuleDoesNotFitSlot_ThrowsException()
        {
            var args = "-x 10.0 -y 0.0 -r 0.0 -i 0 -s 2x2 InvertedF.xml".Split();
            var options = new Options();

            var exception = Assert.Throws<ArgumentException>(delegate { options.ParseArguments(args); });
            Assert.True(exception.Message.Contains("does not fit"), "Incorrect exception message: " + exception.Message);
        }
        
        [Fact]
        public void ParseArguments_BothSlotIndexAndNoEndoDefined_ThrowsException()
        {
            var args = "-x 15.0 -y 10.0 -r 0.0 -i 1 -n -s 1x2 InvertedF.xml".Split();
            var options = new Options();

            var exception = Assert.Throws<ArgumentException>(delegate { options.ParseArguments(args); });
            Assert.True(exception.Message.Contains("slot-index") && exception.Message.Contains("no-endo"), "Incorrect exception message: " + exception.Message);
        }

        [Fact]
        public void ParseArguments_BothSlotIndexAndAllSlotsDefined_ThrowsException()
        {
            var args = "-x 15.0 -y 10.0 -r 0.0 -i 2 -a -s 1x2 InvertedF.xml".Split();
            var options = new Options();

            var exception = Assert.Throws<ArgumentException>(delegate { options.ParseArguments(args); });
            Assert.True(exception.Message.Contains("slot-index") && exception.Message.Contains("all-slots"), "Incorrect exception message: " + exception.Message);
        }

        [Fact]
        public void ParseArguments_BothNoEndoandSarDefined_ThrowsException()
        {
            var args = "-x 15.0 -y 10.0 -r 0.0 -n --sar -s 1x2 InvertedF.xml".Split();
            var options = new Options();

            var exception = Assert.Throws<ArgumentException>(delegate { options.ParseArguments(args); });
            Assert.True(exception.Message.Contains("no-endo") && exception.Message.ToLower().Contains("sar"), "Incorrect exception message: " + exception.Message);
        }
    }
}

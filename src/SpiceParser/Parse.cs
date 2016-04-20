using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;

namespace SpiceLib
{
    /// <summary>
    /// class Parse
    /// Contains methods that extract GME component information from a SPICE file.
    /// See also MOT-207, "Create a C# SPICE parsing library for the CAT SPICE module"
    /// </summary>
    public class Parse
    {
        //----------------- private fields ---------------------
        // Concatenation of cards that could have been continued.
        private string queue;

        // A line read from a deck of cards (file).
        private string card;

        // Object used to hold parsed subcircuit info.
        private ComponentInfo myComponentInfo;

        // Number of lines read from the file,
        // intended to be used for diagnostics.
        private int lineCount;
        private int firstLineNumberInQueue;
        private int lastLineNumberInQueue;

        // Comment delimiters
        private readonly string[] midlineCommentDelims = { ";", "$ ", "//" };

        // One or more whitespace
        static private Regex wsRegex = new Regex(@"\s+");

        // Control for debug messages
        static private bool debugging = false;

        public Parse()
        {
        }

        //----------------- public methods ---------------------

        /// <summary>
        /// Parses a SPICE netlist file to get GME component info.
        /// </summary>
        /// <remarks>Only the first subcircuit is recognized.  Subcircuit parameter values consisting of brace expressions are not recognized.</remarks>
        /// <returns>A SubCircuit object with the parsed info if successful; otherwise throws an exception.</returns>
        public ComponentInfo ParseFile( string fileName )
        {
            Init(); 

            // Read and parse the file
            try
            {
                 if( (fileName == null ) || (!File.Exists( fileName ) ))
                 {
                     throw new Exception(string.Format("Error: File '{0}' does not exist.", fileName) );
                 }

                Boolean commentMode = true;    // For NG SPICE parsing, we start in comment mode,
                // and remain in comment mode until we get a card that doesn't start with a plus.
                // We also transition to comment mode when the first character of a card is "*",
                // or when we find a mid-line comment.


                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.

                using (StreamReader sr = new StreamReader(fileName))
                {
                    // Read cards (lines) from the file until we've processed a subcircuit,
                    // or reach the end of the file.
                    while ((myComponentInfo.name.Length < 1) && ((card = sr.ReadLine()) != null))
                    {
                        lineCount += 1;

                        if (card.Length < 1)
                        {
                            card = " ";  // Treat blank cards lexically like delimiters.
                        }

                        // Check the first character of the card to see if it is
                        // a comment or a continuation.  We can't process a card
                        // until it stops continuing, so the queue is used to accumulate
                        // strings that may be continued.
                        //
                        switch (card[0])
                        {
                            case '*':   // Starts a comment and ends queueing.
                                processQueue(); 
                                commentMode = true;
                                break;

                            case '+':   // Indicates the previous card continues.
                                // Change the leading + to a space:
                                card = ' ' + card.Substring(1);

                                if (!commentMode)   // We only queue non-comments.
                                {
                                    queueThisCard();
                                }
                                break;

                            default:    // Found a card that doesn't start with a '+' or a '*':
                                if (lineCount == 1)  // The first card is an exception since it's always a comment.
                                {
                                    commentMode = true;
                                }
                                else
                                {
                                    processQueue();
                                    commentMode = false;
                                    queueThisCard();
                                }
                                break;
                        }
                    }
                    processQueue();
                }
                if (myComponentInfo.name == "")
                {
                    firstLineNumberInQueue = 1;
                    lastLineNumberInQueue = lineCount;
                    throw new Exception( string.Format("Error: No subcircuit or model definition was found.") );
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                string msg = string.Format( "Exception while parsing file '{0:s}' around lines {2}-{3}:\n{1:s}",
                            fileName, e.Message, firstLineNumberInQueue, lastLineNumberInQueue);
                if (firstLineNumberInQueue == lastLineNumberInQueue)
                {
                    msg = string.Format("Exception while parsing file '{0:s}' at line {2}: {1:s}",
                            fileName, e.Message, firstLineNumberInQueue);
                }

                if (debugging)
                {
                    StackTrace st2 = new StackTrace(e, true);
                    msg += "\n(StackTrace " + st2.ToString().Trim() + ")";
                }
 
                throw new Exception( msg, e);
            }
 
            return myComponentInfo;
        }

        //----------------- private methods ---------------------

        /// <summary>
        /// Initializes the class' fields
        /// </summary>
        private void Init()
        {
            queue = "";
            card = "";
            myComponentInfo = new ComponentInfo();
            lineCount = 0;
            firstLineNumberInQueue = 0;
            lastLineNumberInQueue = 0;
        }

        /// <summary>
        /// Adds the current card to the queue
        /// </summary>
        private void queueThisCard()
        {
            // Check for mid-line comments
            foreach (String commentDelimiter in midlineCommentDelims)
            {
                Int32 substringLength = card.IndexOf(commentDelimiter);
                if (substringLength >= 0)
                {
                    card = card.Substring(0, substringLength);
                }
            }

            if (queue == "")
            {
                firstLineNumberInQueue = lineCount;
            }
            lastLineNumberInQueue = lineCount;
            queue += card;
        }

        /// <summary>
        /// Processes the complete statement accumulated in the queue.
        /// </summary>
        private void processQueue()
        {
            if (queue != "")
            {
                // Prepare to tokenize the queued line.
                // Treat the equals sign as both a tokens and a delimiter.
                String massagedLine = queue.Replace("=", " = ");

                // Treat the parentheses as both tokens and delimiters.
                massagedLine = massagedLine.Replace("(", " ( ");
                massagedLine = massagedLine.Replace(")", " ) ");

                // Treat braces as both tokens and delimiters.
                massagedLine = massagedLine.Replace("{", " { ");
                massagedLine = massagedLine.Replace("}", " } ");

                // Treat commas as delimiters
                massagedLine = massagedLine.Replace(",", " ");

                // Replace multiple whitespaces with a single space
                massagedLine = wsRegex.Replace(massagedLine, " ");

                // Trim leading and trailing whitespace.
                massagedLine = massagedLine.Trim();

                if (massagedLine != "")
                {
                    // Tokenize the line, splitting tokens at spaces.
                    string[] tokens = massagedLine.Split();

                    // Handle the lines we care about:
                    switch (tokens[0].ToUpper() )
                    {
                        case ".SUBCKT":
                            handleSubcircuitStatement(tokens);
                            break;

                        case ".MODEL":
                            handleModelStatement(tokens);
                            break;
                    }

                    /****
                    if (debugging)
                    {
                        foreach (string token in tokens)
                        {
                            Console.WriteLine("// {0}", token);
                        }

                        Console.WriteLine(queue);
                    }
                     * ****/
                }
                queue = "";
            }
        }

        /// <summary>
        /// Handles parsing a .SUBCKT statement, setting mySubCircuit with appropriate values.
        /// </summary>
        /// <param name="tokens">An array of delimited strings for the subcircuit statement.</param>
        /// <returns>bool -- true if ok, false on error.</returns>
        private bool handleSubcircuitStatement(string[] tokens)
        {
            bool rVal = false;
            bool done = false;
            LinkedList<string> tq = new LinkedList<string>(tokens);
            Int32 numberOfTokens = tq.Count;
            if (numberOfTokens < 3)
            {
                // Console.WriteLine("Error: Not enough fields for a subcircuit record; found {0} but need at least {1}.");
                done = true;
                string msg = string.Format( "Error: Not enough fields for a subcircuit record; found {0} but need at least 3.",
                    numberOfTokens );
                throw new Exception( msg );
            }

            if (!done)
            {
                string keyWord = tq.First.Value;
                tq.RemoveFirst();

                if (keyWord.ToUpper() != ".SUBCKT")
                {
                    Console.WriteLine("Error: Unexpected keyword '{0}', expecting '.SUBCKT'.", keyWord);
                    done = true;
                }

            }

            if (!done)
            {
                string scName = tq.First.Value;
                // Check that the name is properly formatted.
                string reason = "";
                if (!isNameStringValid(scName, out reason))
                {
                    string msg = string.Format("Error: '{0}' is not a valid subcircuit name because {1}.", scName, reason);
                    done = true;
                    throw new Exception(msg);
                }
            }
            if (!done)
            {
                rVal = true;
                // Save the valid subcircuit name.
                myComponentInfo.name = tq.First.Value;
                tq.RemoveFirst();

                string currentToken;
                string nextToken;
                bool paramFound = false;

                // Add the pin names to mySubCircuit:
                while ((tq.Count >= 1) && (!paramFound))
                {
                    currentToken = tq.First.Value;
                    tq.RemoveFirst();
                    if (tq.Count > 0)
                    {
                        nextToken = tq.First.Value;
                    }
                    else
                    {
                        nextToken = "";
                    }

                    if (nextToken != "=")
                    {
                        myComponentInfo.pins.Add(currentToken);

                        // MOT-337
                        if (nextToken.ToUpper() == "PARAMS:")
                        {
                            paramFound = true;
                            tq.RemoveFirst();
                        }
                    }
                    else
                    {
                        paramFound = true;
                        tq.AddFirst(currentToken);
                    }
                };

                // Add the parameters to the subcircuit:
                while (paramFound && (tq.Count >= 3))
                {
                    currentToken = tq.First.Value;
                    tq.RemoveFirst();
                    nextToken = tq.First.Value;
                    tq.RemoveFirst();

                    if (nextToken != "=")
                    {
                        string msg = string.Format("Error: subcircuit '{0}', parameter '{1}' needs an equals sign ('=').", 
                            myComponentInfo.name,
                            currentToken );
                        paramFound = false;
                        rVal = false;
                        throw new Exception(msg);
                    }
                    else
                    {
                        string value = tq.First.Value;
                        tq.RemoveFirst();

                        if (value.Contains("{"))
                        {
                            // For clues about parsing brace expressions, see: https://github.com/imr/ngspice/blob/master/src/frontend/numparam/readme.txt
                            // Handling brace-expression values correctly could require ".PARAMS" and ".FUNC" to be parsed.
                            string msg = string.Format( "Error parsing subcircuit '{0}'; brace-expression parameter values are not yet implemented.",
                                myComponentInfo.name );
                            rVal = false;
                            throw new Exception(msg);
                        }
                        else
                        {
                            myComponentInfo.parameters.Add(currentToken, value);
                        }
                    }
                }
                if (tq.Count > 0)
                {
                    Console.WriteLine("Warning! {2} extra tokens at end of '{0}' subcircuit: {1}.", myComponentInfo.name, tq.ToString(), tq.Count);
                    rVal = false;
                }
            }
            return rVal;
        }

        /// <summary>
        /// Checks if a name string is properly formed.
        /// </summary>
        /// <remarks>This is only a stub for now.  Eventually it should check that the name string starts with an alphabetic character, and only contains alphanumeric characters or the symbols " ! # $ % [ ] _ ".</remarks>
        /// <returns>bool -- true if the string is OK, false if it contains an improper character.</returns>
        private bool isNameStringValid(string name, out string explanation)
        {
            bool rVal = true;
            Regex rx = new Regex(@"^[a-zA-Z]"); // Alphabetic characters for first character of the name
            Regex rg = new Regex(@"^[a-zA-Z0-9!#\$%\[\]_]+$");  // Alphanumeric or special characters for the rest of the name's characters
            Regex rInvalid = new Regex(@"[^A-Za-z0-9!#\$%\[\]_]+$");    // Matches characters that are invalid in a name.
            explanation = "OK";

            if (!rx.IsMatch(name))  // Check that the first character is alphabetic.
            {
                // Debug.WriteLine("Error: the name '{0}' needs to start with a letter.", name);
                explanation = string.Format("SPICE names need to start with an upper or lower case letter");
                rVal = false;
            }
            else if (!rg.IsMatch(name))  // Check that the complete name string only contains valid characters.
            {
                // Debug.WriteLine("Error: the name '{0}' contains an unexpected character.", name);
                for( int i = 0; i < name.Length; i++ )
                {
                    char c = name[ i ];
                    var category = Char.GetUnicodeCategory( c );
                    if (rInvalid.IsMatch( c.ToString() ) )
                    {
                        // If the character is printable, print it.
                        if( category == UnicodeCategory.ClosePunctuation ||
                            category == UnicodeCategory.ConnectorPunctuation ||
                            category == UnicodeCategory.CurrencySymbol ||
                            category == UnicodeCategory.DashPunctuation ||
                            category == UnicodeCategory.DecimalDigitNumber ||
                            category == UnicodeCategory.FinalQuotePunctuation ||
                            category == UnicodeCategory.InitialQuotePunctuation ||
                            category == UnicodeCategory.LetterNumber ||
                            category == UnicodeCategory.LowercaseLetter ||
                            category == UnicodeCategory.MathSymbol ||
                            category == UnicodeCategory.OpenPunctuation ||
                            category == UnicodeCategory.OtherLetter ||
                            category == UnicodeCategory.OtherNumber ||
                            category == UnicodeCategory.OtherPunctuation ||
                            category == UnicodeCategory.OtherSymbol ||
                            category == UnicodeCategory.TitlecaseLetter ||
                            category == UnicodeCategory.UppercaseLetter )
                        {
                            explanation = string.Format("the {0} character of the name ('{1}') is not a valid name character",
                                linguisticOrdinal( i + 1 ),
                                c);
                        }
                        else
                        {
                            // If the character isn't printable, show its Unicode representation.
                             explanation = string.Format("the {0} character of the name (U+{1:X4}) is not a valid name character",
                                linguisticOrdinal( i + 1 ),
                                c);
                        }
                    }
                }
                rVal = false;
            }
            return rVal;
        }

        /// <summary>
        /// Handles parsing a .MODEL statement, setting mySubCircuit with appropriate values.
        /// </summary>
        /// <param name="tokens">An array of delimited strings for the subcircuit statement.</param>
        /// <returns>bool -- true if ok, false on error.</returns>
        private bool handleModelStatement(string[] tokens)
        {
            bool rVal = false;
            bool done = false;
            string modelName = "";
            string modelType;
            char elementType;
            List<string> pinList;
            LinkedList<string> tq = new LinkedList<string>(tokens);
            Int32 numberOfTokens = tq.Count;
            if (numberOfTokens < 3)
            {
                Console.WriteLine("Error: Too few tokens in model record.");
                done = true;
            }

            if (!done)
            {
                string keyWord = tq.First.Value;
                tq.RemoveFirst();

                if (keyWord.ToUpper() != ".MODEL")
                {
                    Console.WriteLine("Error: Unexpected keyword '{0}', expecting '.MODEL'.", keyWord);
                    done = true;
                }

            }

            if (!done)
            {
                modelName = tq.First.Value;
                tq.RemoveFirst();
                string reason = "";
                // Check that the name is properly formatted.
                if (!isNameStringValid(modelName, out reason))
                {
                    string msg = string.Format("Error: '{0}' is not a valid model name because {1}.", modelName, reason );
                    done = true;
                    throw new Exception(msg);
                }
            }

            if (!done)
            {
                modelType = tq.First.Value;
                tq.RemoveFirst();

                // Get the element type from the model type
                elementType = getElementTypeFromModelType( modelType );

                // Get the pinList from the element type
                pinList = getPinsFromElementType(elementType);

                // Now the model name and type seem OK.
                rVal = true;

                // Save the model name and type.
                myComponentInfo.name = modelName;
                myComponentInfo.elementType = elementType;
                myComponentInfo.pins = pinList;
            }
            return rVal;
        }

        /// <summary>
        /// Gets the list of PINS corresponding to an element type.
        /// </summary>
        /// R Semiconductor resistor model
        /// C Semiconductor capacitor model
        /// L Inductor model
        /// SW Voltage controlled switch
        /// CSW Current controlled switch
        /// URC Uniform distributed RC model
        /// LTRA Lossy transmission line model
        /// D Diode model
        /// NPN NPN BJT model
        /// PNP PNP BJT model
        /// NJF N-channel JFET model
        /// PJF P-channel JFET model
        /// NMOS N-channel MOSFET model
        /// PMOS P-channel MOSFET model
        /// NMF N-channel MESFET model
        /// PMF P-channel MESFET model

        private List<string> getPinsFromElementType(char elementType)
        {
            List<string> rVal = new List<string> { "PIN1", "PIN2" };

            Dictionary<char, List<string>> elementToPinsDictionary = new Dictionary<char, List<string>>()
            {
                //----------------------------------------------------------------------------------
                // Ngspice element types, from Table 2.1 on page 46 of the Ngspice Users Manual Version 26plus.
                //
                // First letter, pin names in order, element description
				// {'A', new List<string> { "PIN1", "PIN2" } },		// XSPICE code model
				{'B', new List<string> { "PIN+", "PIN-" } },		// Behavioral (arbitrary) source
				{'C', new List<string> { "PIN1", "PIN2" } },		// Capacitor
				{'D', new List<string> { "ANODE", "CATHODE" } },    // Diode
				{'E', new List<string> { "PIN+", "PIN-", "NC+", "NC-" } },		// Voltage-controlled voltage source (VCVS)
				{'F', new List<string> { "PIN+", "PIN-" } },		    // Current-controlled current source (CCCs)
				{'G', new List<string> { "PIN+", "PIN-", "NC+", "NC-"  } },		// Voltage-controlled current source (VCCS)
				{'H', new List<string> { "PIN+", "PIN-"  } },		// Current-controlled voltage source (CCVS)
				{'I', new List<string> { "PIN+", "PIN-" } },		// Current source
				{'J', new List<string> { "DRAIN", "GATE", "SOURCE" } },		// Junction ﬁeld effect transistor (JFET)
				// {'K', new List<string> { "PIN1", "PIN2" } },		// Coupled (Mutual) Inductors
				{'L', new List<string> { "PIN1", "PIN2" } },		// Inductor
				{'M', new List<string> { "DRAIN", "GATE", "SOURCE", "SUBSTRATE" } },		// Metal oxide ﬁeld effect transistor (MOSFET)
				// {'N', new List<string> { "PIN1", "PIN2" } },		// Numerical device for GSS
				{'O', new List<string> { "N1", "N2", "N3", "N4" } },		// Lossy transmission line
				// {'P', new List<string> { "PIN1", "PIN2" } },		// Coupled multiconductor line (CPL)
				{'Q', new List<string> { "COLLECTOR", "BASE", "EMITTER" } },		// Bipolar junction transistor (BJT)
				{'R', new List<string> { "PIN1", "PIN2" } },		// Resistor
				{'S', new List<string> { "PIN1", "PIN2", "NC+", "NC-" } },		// Switch (voltage-controlled)
				{'T', new List<string> { "N1", "N2", "N3", "N4" } },		// Lossless transmission line
				{'U', new List<string> { "N1", "N2", "N3" } },		// Uniformly distributed RC line
				{'V', new List<string> { "PIN+", "PIN-" } },		// Voltage source
				{'W', new List<string> { "PIN1", "PIN2" } },		// Switch (current-controlled)
				// {'X', new List<string> { "PIN1", "PIN2" } },		// Subcircuit
				{'Y', new List<string> { "N1", "N2", "N3", "N4" } },		// Single lossy transmission line (TXL)
				{'Z', new List<string> { "DRAIN", "GATE", "SOURCE" } },		// Metal semiconductor ﬁeld effect transistor (MESFET)
            };
            if (elementToPinsDictionary.ContainsKey(elementType))
            {
                rVal = elementToPinsDictionary[elementType];
            }
            else
            {
                string msg = String.Format("{1}(): Unsupported element type {0}.", elementType, System.Reflection.MethodBase.GetCurrentMethod().Name);
                throw new Exception(msg);
            }

            return rVal;
        }

        /// <summary>
        /// Gets the element type from a model type.
        /// </summary>
        private char getElementTypeFromModelType(string modelType)
        {
            char rVal = '?';

            Dictionary<string, char> modelToElementDictionary = new Dictionary<string, char>()
            {
                //----------------------------------------------------------------------------------
                // Ngspice model types, mash-up from Table 2.3 on page 50 of the Ngspice Users Manual Version 26plus.
                //
                // Model type, element type, element description

                {"R", 'R' },		// Semiconductor resistor model
                {"C", 'C' },		// Semiconductor capacitor model
                {"L", 'L' },		// Inductor model
                {"SW", 'S' },		// Voltage controlled switch
                {"CSW", 'W' },		// Current controlled switch
                {"URC", 'U'},		// Uniform distributed RC model
                {"LTRA", 'O' },		// Lossy transmission line model
                {"D", 'D' },		// Diode model
                {"NPN", 'Q' },		// NPN BJT model
                {"PNP", 'Q' },		// PNP BJT model
                {"NJF", 'J' },		// N-channel JFET model
                {"PJF", 'J'},		// P-channel JFET model
                {"NMOS", 'M' },		// N-channel MOSFET model
                {"PMOS", 'M' },		// P-channel MOSFET model
                {"NMF", 'Z' },		// N-channel MESFET model
                {"PMF", 'Z' },		// P-channel MESFET model
            };

            if (modelToElementDictionary.ContainsKey(modelType))
            {
                rVal = modelToElementDictionary[modelType];
            }
            else
            {
                string msg = String.Format("{1}(): Invalid model type {0}.", modelType, System.Reflection.MethodBase.GetCurrentMethod().Name);
                throw new Exception(msg);
            }
            return rVal;
        }

        private string linguisticOrdinal( int number )
        {
            string rVal = number.ToString();
            if( (number > 0) && (number <= 20) )
            {
                switch (number % 100 )
                {
                    case 1:
                        rVal = "first";
                        break;
                    case 2:
                        rVal = "second";
                        break;
                    case 3:
                        rVal = "third";
                        break;
                    case 4:
                        rVal = "fourth";
                        break;
                    case 5:
                        rVal = "fifth";
                        break;
                    case 6:
                        rVal = "sixth";
                        break;
                    case 7:
                        rVal = "seventh";
                        break;
                    case 8:
                        rVal = "eighth";
                        break;
                    case 9:
                        rVal = "ninth";
                        break;
                    case 10:
                        rVal = "tenth";
                        break;
                    case 11:
                        rVal = "eleventh";
                        break;
                    case 12:
                        rVal = "twelfth";
                        break;
                    case 13:
                        rVal = "thirteenth";
                        break;
                    case 14:
                        rVal = "fourteenth";
                        break;
                    case 15:
                        rVal = "fifteenth";
                        break;
                    case 16:
                        rVal = "sixteenth";
                        break;
                    case 17:
                        rVal = "seventeenth";
                        break;
                    case 18:
                        rVal = "eighteenth";
                        break;
                    case 19:
                        rVal = "nineteenth";
                        break;
                    case 20:
                        rVal = "twentieth";
                        break;
                }
            }
            else if ( number > 0 )
            {    
                switch(number % 100)
                {
                    case 11:
                    case 12:
                    case 13:
                        rVal = number + "th";
                        break;
                    default:
                        switch(number % 10)
                        {
                            case 1:
                                rVal =  number + "st";
                                break;
                            case 2:
                                rVal =  number + "nd";
                                break;
                            case 3:
                                rVal =  number + "rd";
                                break;
                            default:
                                rVal =  number + "th";
                                break;
                        }
                        break;
                }
            }
            return rVal;
        }
    }
}
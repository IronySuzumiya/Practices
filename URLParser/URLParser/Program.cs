using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URLParser
{
    class ParsingException : Exception
    {
        public ParsingException(string message) : base(message) { }
    }

    unsafe class Program
    {
        enum State
        {
            Begin,
            Quotate,
            Escape
        }

        static void RaiseParsingException(char* reader)
        {
            var error = new string(reader);
            if (error != "")
            {
                throw new ParsingException("Invalid symbol at " + error);
            }
            else
            {
                throw new ParsingException("Invalid symbol at the end");
            }
        }

        static string ReadSection(char** reader, State state = State.Begin)
        {
            var section = new StringBuilder();
            if(state == State.Quotate)
            {
                (*reader)++;
            }
            while (**reader != '\0')
            {
                if(**reader == '-' && state != State.Quotate)
                {
                    break;
                }
                if(**reader == '\'')
                {
                    switch (state)
                    {
                        case State.Begin:
                            RaiseParsingException(*reader);
                            return "";
                        case State.Escape:
                            section.Append(**reader);
                            state = State.Quotate;
                            break;
                        case State.Quotate:
                            state = State.Escape;
                            break;
                    }
                }
                else
                {
                    switch (state)
                    {
                        case State.Begin:
                            section.Append(**reader);
                            break;
                        case State.Escape:
                            RaiseParsingException(*reader);
                            return "";
                        case State.Quotate:
                            section.Append(**reader);
                            break;
                    }
                }
                (*reader)++;
            }
            return section.ToString();
        }

        static bool IsGroupDivSymbol(char** reader)
        {
            if(**reader == '-')
            {
                (*reader)++;
                if(**reader == '-')
                {
                    (*reader)++;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if( **reader != '\0')
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        static List<string[]> UnsafeParse(char* input)
        {
            var reader = &input;
            var output = new List<string[]>();
            while(**reader != '\0')
            {
                var group = new List<string>();
                do
                {
                    group.Add(**reader == '\'' ?
                        ReadSection(reader, State.Quotate) : ReadSection(reader));
                }
                while (!IsGroupDivSymbol(reader));
                output.Add(group.ToArray());
            }
            return output;
        }

        static List<string[]> Parse(string input)
        {
            fixed(char* reader = input.Trim())
            {
                try
                {
                    return UnsafeParse(reader);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new List<string[]>();
                }
            }
        }

        static void Main(string[] args)
        {
            var input = Console.ReadLine();
            var output = Parse(input);
            foreach(var group in output)
            {
                foreach(var section in group)
                {
                    Console.Write(section + "\t");
                }
                Console.WriteLine();
            }
        }
    }
}

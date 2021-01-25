using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace judge
{
    public class Sio
    {
        public TextReader Reader;
        public TextWriter Writer;
        public Sio()
        {
            Reader = Console.In;
            Writer = Console.Out;
        }
        
        public Sio(Stream input, Stream output)
        {
            Reader = new StreamReader(input);
            Writer = new StreamWriter(output);
        }
        public Sio(string input)
        {
            Reader = new StringReader(input);
            Writer = null;
        }
        
        public string nextLine()
        {
            return Reader.ReadLine();
        }

        public int nextInt()
        {
            return nextInt((char) 32);
        }

        public int nextInt(char token)
        {
            int number = 0;
            bool negative = false;
            int a = Reader.Read();
            while (a == token || a < 48 || a > 57)
            {
                //skips over every char that is not - or a number
                negative = a == 45;
                a = Reader.Read();
            }

            do
            {
                number = number * 10 + a - 48;
            } while ((a = Reader.Read()) >= 48 && a <= 57 && a != token
            ); // reads until c is less than 0 or greater than 9

            return negative ? -number : number;
        }

        public long nextLong()
        {
            return nextLong((char) 32);
        }

        public long nextLong(char token)
        {
            long number = 0;
            bool negative = false;
            int a = Reader.Read();
            while (a == token || a < 48 || a > 57)
            {
                //skips over every char that is not - or a number
                negative = a == 45;
                a = Reader.Read();
            }

            do
            {
                number = number * 10 + a - 48;
            } while ((a = Reader.Read()) >= 48 && a <= 57 && a != token
            ); // reads until c is less than 0 or greater than 9

            return negative ? -number : number;
        }

        public double nextDouble(char token)
        {
            double number = 0, div = 1;
            int c = Reader.Read();
            bool neg = false;
            while (c == token || c < 48 || c > 57)
            {
                //skips over every char that is not - or a number
                neg = c == 45;
                c = Reader.Read();
            }

            do
            {
                number = number * 10 + c - 48;
            } while ((c = Reader.Read()) >= 48 && c <= 57); // reads until c is less than 0 or greater than 9

            if (c == 46)
            {
                // period
                while ((c = Reader.Read()) >= 48 && c <= 57 && c != token)
                {
                    number += (c - 48) / (div *= 10);
                }
            }

            if (neg)
                return -number;
            return number;
        }

        public double nextDouble()
        {
            return nextDouble((char) 32);
        }

        public int[] nextIntArray(int N)
        {
            int[]
                array = new int[N];
            for (int i = 0; i < N; i++)
            {
                array[i] = nextInt();
            }

            return array;
        }

        public int[] nextIntArray(int N, char token)
        {
            int[]
                array = new int[N];
            for (int i = 0; i < N; i++)
            {
                array[i] = nextInt(token);
            }

            return array;
        }

        public long[] nextLongArray(int N)
        {
            long[]
                array = new long[N];
            for (int i = 0; i < N; i++)
            {
                array[i] = nextLong();
            }

            return array;
        }

        public long[] nextLongArray(int N, char token)
        {
            long[]
                array = new long[N];
            for (int i = 0; i < N; i++)
            {
                array[i] = nextLong(token);
            }

            return array;
        }

        public double[] nextDoubleArray(int N)
        {
            double[]
                array = new double[N];
            for (int i = 0; i < N; i++)
            {
                array[i] = nextDouble();
            }

            return array;
        }

        public double[] nextDoubleArray(int N, char token)
        {
            double[]
                array = new double[N];
            for (int i = 0; i < N; i++)
            {
                array[i] = nextDouble(token);
            }

            return array;
        }

        public String[] nextStringArray(int N)
        {
            String[]
                array = new String[N];
            for (int i = 0; i < N; i++)
            {
                array[i] = next();
            }

            return array;
        }

        public String[] nextStringArray(int N, char token)
        {
            String[]
                array = new String[N];
            for (int i = 0; i < N; i++)
            {
                array[i] = nextToken(token);
            }

            return array;
        }

        public String[] nextStringArray(int N, char[] tokens)
        {
            String[]
                array = new String[N];
            for (int i = 0; i < N; i++)
            {
                array[i] = nextToken(tokens);
            }

            return array;
        }

        public string nextToken(char token)
        {
            StringBuilder str = new StringBuilder();
            int a = Reader.Read();
            while (a == 10 || a == token) // skips over the token and newline
                a = Reader.Read();
            while (a != 10 && a != token && a != -1)
            {
                // reads until it hits a whitespace, newline or token
                str.Append((char) a);
                a = Reader.Read();
            }

            return str.ToString();
        }

        public string nextToken(char[] token)
        {
            StringBuilder str = new StringBuilder();
            int a = Reader.Read();
            while (a == 10)
            {
                // skips over the token and newline
                bool stopFlag = false;
                foreach (char c in token)
                {
                    if (a == c)
                    {
                        stopFlag = true;
                        break;
                    }
                }

                if (stopFlag) break;
                a = Reader.Read();
            }

            while (a != 10 && a != -1)
            {
                // reads until it hits a newline or token
                bool stopFlag = false;
                foreach (char c in token)
                {
                    if (a == c)
                    {
                        stopFlag = true;
                        break;
                    }
                }

                if (stopFlag) break;
                str.Append((char) a);
                a = Reader.Read();
            }

            return str.ToString();
        }

        public string next()
        {
            return nextToken((char) 32);
        }

        public void print(char[] charseq)
        {
            foreach (char c in charseq)
            {
                Writer.Write(c);
            }
        }

        public void print(string str)
        {
            print(str.ToCharArray());
        }

        public void print(int number)
        {
            print((number).ToString());
        }

        public void println(string str)
        {
            print(str + "\n");
        }

        public void println(int number)
        {
            print((number) + "\n");
        }

        public void println()
        {
            print(((char) 10));
        }
    }
}

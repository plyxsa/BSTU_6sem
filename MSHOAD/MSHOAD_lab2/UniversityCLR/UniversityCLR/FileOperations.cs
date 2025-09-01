using System;
using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using System.IO;

namespace UniversityCLR
{
    public class FileOperations
    {
        [SqlFunction]
        public static void WriteToFile(SqlString content, SqlString filePath)
        {
            try
            {
                File.WriteAllText(filePath.Value, content.Value);
            }
            catch (Exception ex)
            {
                SqlContext.Pipe.Send("Error: " + ex.Message);
            }
        }
    }

    [SqlUserDefinedType(Format.UserDefined, MaxByteSize = 100)]
    public struct PhoneNumber : IBinarySerialize, INullable
    {
        private string number;

        public PhoneNumber(string number)
        {
            if (string.IsNullOrEmpty(number) || number.Length < 12)
            {
                throw new ArgumentException("Invalid phone number.");
            }

            string pattern = @"^\+375\s?\(?\d{2}\)?\s?\d{3}-\d{2}-\d{2}$";
            Regex regex = new Regex(pattern);
            if (!regex.IsMatch(number))
            {
                throw new ArgumentException("Invalid phone number format. The number must match the Belarusian format.");
            }

            this.number = number;
        }

        public string Number
        {
            get { return number; }
        }

        public override string ToString()
        {
            return this.number;
        }

        public static PhoneNumber Parse(SqlString input)
        {
            return new PhoneNumber(input.Value);
        }

        public SqlString ToSqlString()
        {
            return new SqlString(this.number);
        }

        public static PhoneNumber Null
        {
            get { return new PhoneNumber(""); }
        }

        public bool IsNull
        {
            get { return string.IsNullOrEmpty(this.number); }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.number);
        }

        public void Read(BinaryReader reader)
        {
            this.number = reader.ReadString();
        }
    }
}

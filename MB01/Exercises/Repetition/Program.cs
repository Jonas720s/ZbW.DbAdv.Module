using System;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace AdoNetRep
{
    class Program
    {
        static void Main(string[] args)
        {
            do
            {
                Run();
                Console.Write("Run again (y/n)? ");
            } while (Console.ReadLine()?.Trim().ToLower() != "n");

        }

        static void Run()
        {
            Console.WriteLine("Dumping the tables from the Contacts.mdb database");
            Console.WriteLine();

            using var con = new OleDbConnection("provider=Microsoft.ACE.OLEDB.16.0; data source=./Contacts.mdb");
            con.Open();

            ListTables(con);

            Console.Write("Enter table name (leave empty for \"Contact\"): ");
            string table = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(table))
                table = "Contact";

            if(!ListColumns(con, table))
            {
                return;
            }

            Console.Write("Enter comma-separated fields to select (leave empty for *): ");
            string fieldsInput = Console.ReadLine();
            string fields = string.IsNullOrWhiteSpace(fieldsInput) ? "*" : NormalizeFields(fieldsInput);

            Console.WriteLine("Enter a Filter (SQL WHERE clause without the word WHERE), or leave empty:");
            string filter = Console.ReadLine();

            using var trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
            using var cmd = con.CreateCommand();
            cmd.Transaction = trans;
            try
            {
                string sql = $"SELECT {fields} FROM [{table}]";
                if (!String.IsNullOrEmpty(filter))
                {
                    sql += " WHERE " + filter;
                }
                cmd.CommandText = sql;
                Execute(cmd);
                trans.Commit();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();

                try { trans.Rollback(); } catch { }
            }
        }

        static void ListTables(OleDbConnection con)
        {
            Console.WriteLine("Tables found in the database:");
            DataTable tables = con.GetSchema("Tables");
            foreach (DataRow row in tables.Rows)
            {
                string tableType = row["TABLE_TYPE"]?.ToString() ?? "";
                string tableName = row["TABLE_NAME"]?.ToString() ?? "";

                if (tableType.Equals("TABLE", StringComparison.OrdinalIgnoreCase) &&
                    !tableName.StartsWith("MSys", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(tableName);
                }
            }
            Console.WriteLine();
        }

        static bool ListColumns(OleDbConnection con, string table)
        {
            Console.WriteLine($"Columns in [{table}]:");
            using var cmd = con.CreateCommand();
            cmd.CommandText = $"SELECT * FROM [{table}] WHERE 1=0";
            using var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
            if (reader.FieldCount == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Table [{table}] does not exist or has no columns.");
                Console.WriteLine();
                Console.ResetColor();
                return false;
            }
            for (int i = 0; i < reader.FieldCount; i++)
            {
                Console.WriteLine($"{i}: {reader.GetName(i)} ({reader.GetFieldType(i).Name})");
            }
            Console.WriteLine();
            return true;
        }

        static string NormalizeFields(string input)
        {
            var parts = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                var p = parts[i].Trim();
                if (p == "*" || p.Contains("(") || p.Contains("]") || p.Contains("["))
                {
                    parts[i] = p;
                }
                else
                {
                    parts[i] = "[" + p + "]";
                }
            }
            return string.Join(", ", parts);
        }

        private static void Execute(IDbCommand cmd)
        {
            Console.WriteLine(cmd.CommandText);
            using var r = cmd.ExecuteReader();
            object[] row = new object[r.FieldCount];
            while (r.Read())
            {
                int cols = r.GetValues(row);
                for (int i = 0; i < cols; i++)
                {
                    Console.Write(row[i] + "\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
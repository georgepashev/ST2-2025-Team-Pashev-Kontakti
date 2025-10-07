using Kontakti.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Kontakti.Data
{
    // Sealed Singleton Database class for Contact model using SQLite
    public sealed class Database
    {
        private static readonly Lazy<Database> _instance = new Lazy<Database>(() => new Database());
        private readonly string _connectionString;
        private const string DatabaseFileName = "contacts.db";

        private Database()
        {
            // Set up SQLite database file path
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabaseFileName);
            _connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        public static Database Instance => _instance.Value;

        private void InitializeDatabase()
        {
            if (!File.Exists(DatabaseFileName))
            {
                SQLiteConnection.CreateFile(DatabaseFileName);
            }

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string tableCmd = @"
                CREATE TABLE IF NOT EXISTS Contacts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    PhoneNumber TEXT,
                    AddressLine1 TEXT,
                    AddressLine2 TEXT
                )";
                using (var command = new SQLiteCommand(tableCmd, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // Create
        public Contact AddContact(Contact contact)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    @"INSERT INTO Contacts (Name, Email, PhoneNumber, AddressLine1, AddressLine2)
                      VALUES (@Name, @Email, @PhoneNumber, @AddressLine1, @AddressLine2);
                      SELECT last_insert_rowid();", connection);

                cmd.Parameters.AddWithValue("@Name", contact.Name);
                cmd.Parameters.AddWithValue("@Email", contact.Email);
                cmd.Parameters.AddWithValue("@PhoneNumber", (object)contact.PhoneNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AddressLine1", (object)contact.AddressLine1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AddressLine2", (object)contact.AddressLine2 ?? DBNull.Value);

                contact.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return contact;
        }

        // Read All
        public List<Contact> GetAllContacts()
        {
            var contacts = new List<Contact>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("SELECT * FROM Contacts", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contacts.Add(new Contact
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            PhoneNumber = reader["PhoneNumber"] as string,
                            AddressLine1 = reader["AddressLine1"] as string,
                            AddressLine2 = reader["AddressLine2"] as string
                        });
                    }
                }
            }
            return contacts;
        }

        // Read by Id
        public Contact GetContactById(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("SELECT * FROM Contacts WHERE Id = @Id", connection);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Contact
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            PhoneNumber = reader["PhoneNumber"] as string,
                            AddressLine1 = reader["AddressLine1"] as string,
                            AddressLine2 = reader["AddressLine2"] as string
                        };
                    }
                }
            }
            return null;
        }

        // Update
        public bool UpdateContact(Contact updatedContact)
        {
            if (updatedContact == null)
                throw new ArgumentNullException(nameof(updatedContact));

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    @"UPDATE Contacts SET
                        Name = @Name,
                        Email = @Email,
                        PhoneNumber = @PhoneNumber,
                        AddressLine1 = @AddressLine1,
                        AddressLine2 = @AddressLine2
                      WHERE Id = @Id", connection);

                cmd.Parameters.AddWithValue("@Name", updatedContact.Name);
                cmd.Parameters.AddWithValue("@Email", updatedContact.Email);
                cmd.Parameters.AddWithValue("@PhoneNumber", (object)updatedContact.PhoneNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AddressLine1", (object)updatedContact.AddressLine1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AddressLine2", (object)updatedContact.AddressLine2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", updatedContact.Id);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Delete
        public bool DeleteContact(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("DELETE FROM Contacts WHERE Id = @Id", connection);
                cmd.Parameters.AddWithValue("@Id", id);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Вътре в класа Database

        // Помощен метод за безопасно търсене с LIKE
        private static string EscapeLike(string value)
        {
            if (value == null) return null;
            // Ескейпваме \, %, _
            return value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
        }

        /// <summary>
        /// Търсене по име, телефон и/или e-mail. Всички параметри са опционални.
        /// По подразбиране търси частично (LIKE '%term%'), case-insensitive.
        /// exact=true прави точно съвпадение (=).
        /// Може да се подадат и limit/offset за странициране.
        /// </summary>
        public List<Contact> SelectContacts(
            string name = null,
            string phone = null,
            string email = null,
            bool exact = false,
            int? limit = null,
            int? offset = null)
        {
            var results = new List<Contact>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                var whereClauses = new List<string>();
                var cmd = new SQLiteCommand();
                cmd.Connection = connection;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (exact)
                    {
                        whereClauses.Add("Name = @Name COLLATE NOCASE");
                        cmd.Parameters.AddWithValue("@Name", name.Trim());
                    }
                    else
                    {
                        var term = "%" + EscapeLike(name.Trim()) + "%";
                        whereClauses.Add("Name LIKE @Name ESCAPE '\\' COLLATE NOCASE");
                        cmd.Parameters.AddWithValue("@Name", term);
                    }
                }

                if (!string.IsNullOrWhiteSpace(phone))
                {
                    if (exact)
                    {
                        whereClauses.Add("PhoneNumber = @Phone COLLATE NOCASE");
                        cmd.Parameters.AddWithValue("@Phone", phone.Trim());
                    }
                    else
                    {
                        var term = "%" + EscapeLike(phone.Trim()) + "%";
                        whereClauses.Add("PhoneNumber LIKE @Phone ESCAPE '\\' COLLATE NOCASE");
                        cmd.Parameters.AddWithValue("@Phone", term);
                    }
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    if (exact)
                    {
                        whereClauses.Add("Email = @Email COLLATE NOCASE");
                        cmd.Parameters.AddWithValue("@Email", email.Trim());
                    }
                    else
                    {
                        var term = "%" + EscapeLike(email.Trim()) + "%";
                        whereClauses.Add("Email LIKE @Email ESCAPE '\\' COLLATE NOCASE");
                        cmd.Parameters.AddWithValue("@Email", term);
                    }
                }

                var sql = "SELECT * FROM Contacts";
                if (whereClauses.Count > 0)
                {
                    sql += " WHERE " + string.Join(" AND ", whereClauses);
                }
                sql += " ORDER BY Name COLLATE NOCASE, Id";

                if (limit.HasValue)
                {
                    sql += " LIMIT @Limit";
                    cmd.Parameters.AddWithValue("@Limit", limit.Value);

                    if (offset.HasValue && offset.Value > -1)
                    {
                        sql += " OFFSET @Offset";
                        cmd.Parameters.AddWithValue("@Offset", offset.Value);
                    }
                }

                cmd.CommandText = sql;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new Contact
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"]?.ToString(),
                            Email = reader["Email"]?.ToString(),
                            PhoneNumber = reader["PhoneNumber"] as string,
                            AddressLine1 = reader["AddressLine1"] as string,
                            AddressLine2 = reader["AddressLine2"] as string
                        });
                    }
                }
            }

            return results;
        }

    }
}
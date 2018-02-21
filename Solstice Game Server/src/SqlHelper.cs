using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {

    public class SqlHelper {

        public static Character[] GetCharactersFromUsername(string username) {
            List<Character> characters = new List<Character>();

            string sql = "SELECT * FROM Characters WHERE Owner='" + username.ToLower() + "'";

            SQLiteCommand command = new SQLiteCommand(sql, Program.SqlConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while(reader.Read()) {
                characters.Add(new Character() {
                    Id = Convert.ToInt32(reader["Id_Local"]),
                    Name = reader["Name"].ToString(),
                    Sex = Convert.ToInt32(reader["Sex"]),
                    HairStyle = Convert.ToInt32(reader["HairStyle"]),
                    HairColour = Convert.ToInt32(reader["HairColour"]),
                });
            }

            return characters.ToArray();
        }

        public static void SaveCharacter(Character character, string username) {
            string sql = "INSERT INTO Characters (Owner, Id_Local, Name, Sex, HairStyle, HairColour) values ('{0}', {1}, '{2}', {3}, {4}, {5})";
            sql = String.Format(sql, username.ToLower(), character.Id, character.Name, character.Sex, character.HairStyle, character.HairColour);
            SQLiteCommand command = new SQLiteCommand(sql, Program.SqlConnection);
            command.ExecuteNonQuery();
        }

        public static void DeleteCharacter(string username, int id) {
            string sql = "DELETE FROM Characters WHERE Owner='{0}' AND Id_Local={1}";
            sql = String.Format(sql, username.ToLower(), id);
            SQLiteCommand command = new SQLiteCommand(sql, Program.SqlConnection);
            command.ExecuteNonQuery();
        }
    }
}

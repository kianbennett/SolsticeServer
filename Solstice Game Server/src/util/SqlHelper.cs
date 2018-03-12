using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.Data.Sqlite;

namespace SolsticeGameServer {

    public class SqlHelper {

        public static PlayerData[] GetCharactersFromUsername(string username) {
            List<PlayerData> characters = new List<PlayerData>();

            string sql = "SELECT * FROM Characters WHERE Owner='" + username.ToLower() + "'";

            SqliteCommand command = new SqliteCommand(sql, Program.SqlConnection);
            SqliteDataReader reader;

            try {
                reader = command.ExecuteReader();
            } catch (Exception e) {
                Console.WriteLine("[ERROR] Sql error: " + e.Message);
                return characters.ToArray();
            }

            while (reader.Read()) {
                characters.Add(new PlayerData() {
                    LocalId = Convert.ToByte(reader["IdLocal"]),
                    Name = reader["Name"].ToString(),
                    Sex = Convert.ToByte(reader["Sex"]),
                    Hair = Convert.ToByte(reader["Hair"]),
                    Class = Convert.ToByte(reader["Class"]),

                    EquipmentOutfit = Convert.ToByte(reader["EquipOutfit"]),
                    EquipmentHead = Convert.ToByte(reader["EquipHead"]),
                    EquipmentFace = Convert.ToByte(reader["EquipFace"]),
                    EquipmentEyes = Convert.ToByte(reader["EquipEyes"]),
                    EquipmentGloves = Convert.ToByte(reader["EquipGloves"]),
                    EquipmentBack = Convert.ToByte(reader["EquipBack"]),
                    EquipmentArmL = Convert.ToByte(reader["EquipArmL"]),
                    EquipmentArmR = Convert.ToByte(reader["EquipArmR"]),
                    EquipmentShoes = Convert.ToByte(reader["EquipShoes"]),
                    EquipmentMSlot = Convert.ToInt16(reader["EquipMSlot"]),
                    EquipmentAcc1 = Convert.ToByte(reader["EquipAcc1"]),
                    EquipmentAcc2 = Convert.ToByte(reader["EquipAcc2"]),
                    EquipmentAcc3 = Convert.ToByte(reader["EquipAcc3"]),
                    Kron = Convert.ToInt32(reader["Kron"]),

                    Level = Convert.ToByte(reader["Level"]),
                    Power = Convert.ToByte(reader["Power"]),
                    Stamina = Convert.ToByte(reader["Stamina"]),
                    Agility = Convert.ToByte(reader["Agility"]),
                    Intelligence = Convert.ToByte(reader["Intelligence"]),
                    Mentality = Convert.ToByte(reader["Mentality"]),
                    Wisdom = Convert.ToByte(reader["Wisdom"]),
                    MoveSpeed = Convert.ToByte(reader["MoveSpeed"]),
                    Hp = Convert.ToInt16(reader["Hp"]),
                    Mp = Convert.ToInt16(reader["Mp"]),
                    SkillPoints = Convert.ToByte(reader["SkillPoints"]),
                    PvpWin = Convert.ToInt32(reader["PvpWin"]),
                    PvpLost = Convert.ToInt32(reader["PvpLost"]),
                    PvpPoint = Convert.ToByte(reader["PvpPoint"]),

                    MapId = Convert.ToByte(reader["MapId"]),
                    PosX = Convert.ToByte(reader["PosX"]),
                    PosY = Convert.ToByte(reader["PosY"]),
                    Direction = Convert.ToByte(reader["Direction"]),
                });
            }

            return characters.ToArray();
        }

        public static PlayerData GetPlayerDataFromName(string username, string name) {
            PlayerData[] characters = GetCharactersFromUsername(username).Where(o => o.Name == name).ToArray();
            return characters.Length > 0 ? characters[0] : null;
        }

        public static void SaveCharacter(PlayerData data, string username) {
            string sql = 
                "INSERT INTO Characters (" +
                    "Owner, IdLocal, Name, Sex, Hair, Class, " +
                    "EquipOutfit, EquipHead, EquipFace, EquipEyes, EquipGloves, EquipBack, EquipArmL, EquipArmR, EquipShoes, EquipMSlot, EquipAcc1, EquipAcc2, EquipAcc3, Kron, " +
                    "Level, Power, Stamina, Agility, Intelligence, Mentality, Wisdom, MoveSpeed, Hp, Mp, SkillPoints, PvpWin, PvpLost, PvpPoint, " +
                    "MapId, PosX, PosY, Direction" +
                ") values (" +
                    "'{0}', {1}, '{2}', {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37}" +
                ")";
            sql = String.Format(sql,
                username.ToLower(), data.LocalId, data.Name, data.Sex, data.Hair, data.Class,
                data.EquipmentOutfit, data.EquipmentHead, data.EquipmentFace, data.EquipmentEyes, data.EquipmentGloves, data.EquipmentBack, data.EquipmentArmL, data.EquipmentArmR, data.EquipmentShoes, data.EquipmentMSlot, data.EquipmentAcc1, data.EquipmentAcc2, data.EquipmentAcc3, data.Kron,
                data.Level, data.Power, data.Stamina, data.Agility, data.Intelligence, data.Mentality, data.Wisdom, data.MoveSpeed, data.Hp, data.Mp, data.SkillPoints, data.PvpWin, data.PvpLost, data.PvpPoint,
                data.MapId, data.PosX, data.PosY, data.Direction
            );

            SqliteCommand command = new SqliteCommand(sql, Program.SqlConnection);
            try {
                command.ExecuteNonQuery();
            } catch(Exception e) {
                Console.WriteLine("[ERROR] Sql error: " + e.Message);
            }
        }

        public static void SaveBuddyList(string charName, List<string> buddyList) {
            string buddyStr = "";
            for(int i = 0; i < buddyList.Count; i++) {
                if (i > 0) buddyStr += ",";
                buddyStr += buddyList[i];
            }
            string sql = "UPDATE Characters SET BuddyList='{0}' WHERE Name='{1}'";
            sql = string.Format(sql, buddyStr, charName);

            SqliteCommand command = new SqliteCommand(sql, Program.SqlConnection);
            try {
                command.ExecuteNonQuery();
            } catch (Exception e) {
                Console.WriteLine("[ERROR] Sql error: " + e.Message);
            }
        }

        public static List<string> GetBuddyList(string charName) {
            List<string> buddyList = new List<string>();

            string sql = "SELECT BuddyList FROM Characters WHERE Name='" + charName + "'";

            SqliteCommand command = new SqliteCommand(sql, Program.SqlConnection);
            SqliteDataReader reader;

            try {
                reader = command.ExecuteReader();
            } catch (Exception e) {
                Console.WriteLine("[ERROR] Sql error: " + e.Message);
                return buddyList;
            }

            while (reader.Read()) {
                if(!reader.IsDBNull(0)) buddyList.AddRange(reader["BuddyList"].ToString().Split(",".ToCharArray()));
            }

            return buddyList;
        }

        public static void DeleteCharacter(string username, int id) {
            string sql = "DELETE FROM Characters WHERE Owner='{0}' AND Id_Local={1}";
            sql = String.Format(sql, username.ToLower(), id);
            SqliteCommand command = new SqliteCommand(sql, Program.SqlConnection);
            try {
                command.ExecuteNonQuery();
            } catch (Exception e) {
                Console.WriteLine("[ERROR] Sql error: " + e.Message);
            }
        }
    }
}

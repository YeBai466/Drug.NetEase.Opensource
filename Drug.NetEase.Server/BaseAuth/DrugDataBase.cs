using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Drug.NetEase.Server.Extensions;
using MySql.Data.MySqlClient;

namespace Drug.NetEase.Server.BaseAuth;

public static class DrugDataBase
{
    private static MySqlConnection _connection;

    public static bool Connect()
    {
        var connectionString = "server=127.0.0.1;user=Drug;password=fCYL7KJSYNpxwJFp;database=drug";
        _connection = new MySqlConnection(connectionString);
        try
        {
            _connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static DrugDataBaseEntities.UserEntity GetUserByUserId(int userId)
    {
        var query = "SELECT * FROM usertable WHERE userid = @userid";

        using (var command = new MySqlCommand(query, _connection))
        {
            command.Parameters.AddWithValue("@userid", userId);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var user = new DrugDataBaseEntities.UserEntity
                    {
                        userid = reader.GetInt32(0),
                        username = reader.GetString(1),
                        registeraddress = reader.GetString(2),
                        usertoken = reader.GetString(3),
                        password = reader.GetString(4),
                        createtime = reader.GetString(5),
                        expiredtime = reader.GetString(6),
                        machinecode = reader.GetString(7),
                        banmessage = reader.GetString(8)
                    };
                    return user;
                }
            }
        }

        return null;
    }

    public static DrugDataBaseEntities.UserEntity GetUserByUsername(string username)
    {
        var query = "SELECT * FROM usertable WHERE username = @username";

        using (var command = new MySqlCommand(query, _connection))
        {
            command.Parameters.AddWithValue("@username", username);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var user = new DrugDataBaseEntities.UserEntity
                    {
                        userid = reader.GetInt32(0),
                        username = reader.GetString(1),
                        registeraddress = reader.GetString(2),
                        usertoken = reader.GetString(3),
                        password = reader.GetString(4),
                        createtime = reader.GetString(5),
                        expiredtime = reader.GetString(6),
                        machinecode = reader.GetString(7),
                        banmessage = reader.GetString(8)
                    };
                    return user;
                }
            }
        }

        return null;
    }
    public static DrugDataBaseEntities.UserEntity GetUserByRegisterAddress(string registerAddress)
    {
        var query = "SELECT * FROM usertable WHERE registeraddress = @registeraddress";

        using (var command = new MySqlCommand(query, _connection))
        {
            command.Parameters.AddWithValue("@registeraddress", registerAddress);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var user = new DrugDataBaseEntities.UserEntity
                    {
                        userid = reader.GetInt32(0),
                        username = reader.GetString(1),
                        registeraddress = reader.GetString(2),
                        usertoken = reader.GetString(3),
                        password = reader.GetString(4),
                        createtime = reader.GetString(5),
                        expiredtime = reader.GetString(6),
                        machinecode = reader.GetString(7),
                        banmessage = reader.GetString(8)
                    };
                    return user;
                }
            }
        }

        return null;
    }
    public static List<DrugDataBaseEntities.UserEntity> GetUserByMachineCode(string machineCode)
    {
        var query = "SELECT * FROM usertable WHERE machinecode = @machinecode";

        using (var command = new MySqlCommand(query, _connection))
        {
            command.Parameters.AddWithValue("@machinecode", machineCode);

            using (var reader = command.ExecuteReader())
            {
                var userList = new List<DrugDataBaseEntities.UserEntity>();
                while (reader.Read())
                {
                    var user = new DrugDataBaseEntities.UserEntity
                    {
                        userid = reader.GetInt32(0),
                        username = reader.GetString(1),
                        registeraddress = reader.GetString(2),
                        usertoken = reader.GetString(3),
                        password = reader.GetString(4),
                        createtime = reader.GetString(5),
                        expiredtime = reader.GetString(6),
                        machinecode = reader.GetString(7),
                        banmessage = reader.GetString(8)
                    };
                    userList.Add(user);
                }

                return userList;
            }
        }
    }
    public static DrugDataBaseEntities.UserEntity GetUserByUserToken(string userToken)
    {
        var query = "SELECT * FROM usertable WHERE usertoken = @usertoken";

        using (var command = new MySqlCommand(query, _connection))
        {
            command.Parameters.AddWithValue("@usertoken", userToken);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var user = new DrugDataBaseEntities.UserEntity
                    {
                        userid = reader.GetInt32(0),
                        username = reader.GetString(1),
                        registeraddress = reader.GetString(2),
                        usertoken = reader.GetString(3),
                        password = reader.GetString(4),
                        createtime = reader.GetString(5),
                        expiredtime = reader.GetString(6),
                        machinecode = reader.GetString(7),
                        banmessage = reader.GetString(8)
                    };
                    return user;
                }
            }
        }

        return null;
    }
    public static bool RegisterUser(string username, string password, string registerAddress, string machineCode)
    {
        // 创建时间
        var createTime = Convert.ToString(new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds());

        // 将用户信息插入到数据库表中
        var query = "INSERT INTO usertable (username, password, registeraddress, usertoken, createtime, expiredtime, machinecode, banmessage) VALUES (@username, @password, @registeraddress, @usertoken, @createtime, @expiredtime, @machinecode, @banmessage)";


        using (var command = new MySqlCommand(query, _connection))
        {
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            command.Parameters.AddWithValue("@registeraddress", registerAddress);
            command.Parameters.AddWithValue("@usertoken", "");
            command.Parameters.AddWithValue("@createtime", createTime);
            command.Parameters.AddWithValue("@expiredtime", createTime);
            command.Parameters.AddWithValue("@machinecode", machineCode);
            command.Parameters.AddWithValue("@banmessage", "");

            try
            {
                var rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception e)
            {
                $"注册 {username} 失败".Log();
                e.ToString().Log();
                return false;
            }
        }
    }
    public static string UpdateUserToken(int userId)
    {
        var userToken = Convert.ToBase64String(SHA256.Create()
            .ComputeHash(Encoding.UTF8.GetBytes($"{userId}{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}")));
        var query = "UPDATE usertable SET usertoken = @usertoken WHERE userid = @userid";

        using (var command = new MySqlCommand(query, _connection))
        {
            command.Parameters.AddWithValue("@usertoken", userToken);
            command.Parameters.AddWithValue("@userid", userId);

            command.ExecuteNonQuery();
        }

        return userToken;
    }
}
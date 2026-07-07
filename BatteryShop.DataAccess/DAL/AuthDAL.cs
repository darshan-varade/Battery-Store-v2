using System;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using BatteryShop.DataAccess.Models;
using Serilog;

namespace BatteryShop.DataAccess.DAL
{
    public class AuthDAL
    {
        private Database db;

        public AuthDAL()
        {
            this.db = DatabaseFactory.CreateDatabase();
        }

        public int OwnerRegister(string ownerName, string ownerPhone, string ownerEmail, string passwordHash, int roleId = 2)
        {
            DbCommand cmd = db.GetStoredProcCommand("ownerRegister");
            db.AddInParameter(cmd, "@OwnerName", DbType.String, ownerName);
            db.AddInParameter(cmd, "@OwnerPhone", DbType.String, ownerPhone);
            db.AddInParameter(cmd, "@OwnerEmail", DbType.String, ownerEmail);
            db.AddInParameter(cmd, "@PasswordHash", DbType.String, passwordHash);
            db.AddInParameter(cmd, "@RoleId", DbType.Int32, roleId);

            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in OwnerRegister");
                throw;
            }
        }

        public OwnerModel OwnerLogin(string email)
        {
            OwnerModel model = null;

            DbCommand cmd = db.GetStoredProcCommand("ownerLogin");
            db.AddInParameter(cmd, "@Email", DbType.String, email);

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        model = new OwnerModel
                        {
                            OwnerId = Convert.ToInt32(reader["ownerId"]),
                            OwnerName = reader["ownerName"].ToString(),
                            OwnerEmail = reader["ownerEmail"].ToString(),
                            PasswordHash = reader["passwordHash"].ToString(),
                            RoleId = Convert.ToInt32(reader["roleId"]),
                            RoleName = reader["roleName"].ToString()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in OwnerLogin");
                throw;
            }

            return model;
        }

        public bool OwnerCheckEmail(string email)
        {
            DbCommand cmd = db.GetStoredProcCommand("ownerCheckEmail");
            db.AddInParameter(cmd, "@Email", DbType.String, email);

            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in OwnerCheckEmail");
                throw;
            }
        }
    }
}

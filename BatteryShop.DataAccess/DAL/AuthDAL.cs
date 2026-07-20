using System;
using System.Collections.Generic;
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

        public int OwnerRegister(string ownerName, string ownerPhone, string ownerEmail, string passwordHash)
        {
            DbCommand cmd = db.GetStoredProcCommand("ownerRegister");
            db.AddInParameter(cmd, "@OwnerName", DbType.String, ownerName);
            db.AddInParameter(cmd, "@OwnerPhone", DbType.String, ownerPhone);
            db.AddInParameter(cmd, "@OwnerEmail", DbType.String, ownerEmail);
            db.AddInParameter(cmd, "@PasswordHash", DbType.String, passwordHash);

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

        public byte? GetApprovalStatus(string email)
        {
            DbCommand cmd = db.GetSqlStringCommand("SELECT isApproved FROM batteryCredentials WHERE ownerEmail = @Email");
            db.AddInParameter(cmd, "@Email", DbType.String, email);

            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null && result != DBNull.Value ? Convert.ToByte(result) : (byte?)null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetApprovalStatus");
                throw;
            }
        }

        public List<OwnerListModel> GetAllOwners()
        {
            List<OwnerListModel> list = new List<OwnerListModel>();

            DbCommand cmd = db.GetStoredProcCommand("ownerGetAllList");

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new OwnerListModel
                        {
                            OwnerId = Convert.ToInt32(reader["ownerId"]),
                            OwnerName = reader["ownerName"].ToString(),
                            OwnerPhone = reader["ownerPhone"].ToString(),
                            OwnerEmail = reader["ownerEmail"].ToString(),
                            RoleName = reader["roleName"].ToString(),
                            IsApproved = reader["isApproved"] != DBNull.Value ? Convert.ToByte(reader["isApproved"]) : (byte?)null,
                            LastLogin = reader["lastLogin"] != DBNull.Value ? Convert.ToDateTime(reader["lastLogin"]) : (DateTime?)null,
                            CreatedAt = Convert.ToDateTime(reader["createdAt"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetAllOwners");
                throw;
            }

            return list;
        }

        public void SetApprovalStatus(int ownerId, byte? isApproved, int modifiedBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("ownerSetApprovalStatus");
            db.AddInParameter(cmd, "@OwnerId", DbType.Int32, ownerId);

            if (isApproved.HasValue)
                db.AddInParameter(cmd, "@IsApproved", DbType.Byte, isApproved.Value);
            else
                db.AddInParameter(cmd, "@IsApproved", DbType.Byte, DBNull.Value);

            db.AddInParameter(cmd, "@ModifiedBy", DbType.Int32, modifiedBy);

            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SetApprovalStatus");
                throw;
            }
        }

        public int CreateRefreshToken(int ownerId, string hash, DateTime expiresAt)
        {
            DbCommand cmd = db.GetStoredProcCommand("refreshTokenCreate");
            db.AddInParameter(cmd, "@OwnerId", DbType.Int32, ownerId);
            db.AddInParameter(cmd, "@RefreshTokenHash", DbType.String, hash);
            db.AddInParameter(cmd, "@ExpiresAt", DbType.DateTime, expiresAt);

            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CreateRefreshToken");
                throw;
            }
        }

        public RefreshTokenModel GetRefreshTokenByHash(string hash)
        {
            DbCommand cmd = db.GetStoredProcCommand("refreshTokenGetByHash");
            db.AddInParameter(cmd, "@Hash", DbType.String, hash);

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        return new RefreshTokenModel
                        {
                            RefreshTokenId = Convert.ToInt32(reader["refreshTokenId"]),
                            OwnerId = Convert.ToInt32(reader["ownerId"]),
                            OwnerName = reader["ownerName"].ToString(),
                            OwnerEmail = reader["ownerEmail"].ToString(),
                            RoleName = reader["roleName"].ToString(),
                            ExpiresAt = Convert.ToDateTime(reader["expiresAt"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetRefreshTokenByHash");
                throw;
            }

            return null;
        }

        public int RotateRefreshToken(int oldTokenId, string newHash, DateTime newExpiresAt, int ownerId)
        {
            DbCommand cmd = db.GetStoredProcCommand("refreshTokenRotate");
            db.AddInParameter(cmd, "@OldRefreshTokenId", DbType.Int32, oldTokenId);
            db.AddInParameter(cmd, "@NewRefreshTokenHash", DbType.String, newHash);
            db.AddInParameter(cmd, "@NewExpiresAt", DbType.DateTime, newExpiresAt);
            db.AddInParameter(cmd, "@OwnerId", DbType.Int32, ownerId);

            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in RotateRefreshToken");
                throw;
            }
        }

        public void MarkOtpUsed(int otpId)
        {
            DbCommand cmd = db.GetStoredProcCommand("otpMarkUsed");
            db.AddInParameter(cmd, "@OtpId", DbType.Int32, otpId);

            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in MarkOtpUsed");
                throw;
            }
        }

        public int CreateOtpByEmail(string email, string otpCode, DateTime expiresAt)
        {
            DbCommand cmd = db.GetStoredProcCommand("otpCreateByEmail");
            db.AddInParameter(cmd, "@OtpEmail", DbType.String, email);
            db.AddInParameter(cmd, "@OtpCode", DbType.String, otpCode);
            db.AddInParameter(cmd, "@ExpiresAt", DbType.DateTime, expiresAt);

            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CreateOtpByEmail");
                throw;
            }
        }

        public int? ValidateOtpByEmail(string email, string otpCode)
        {
            DbCommand cmd = db.GetStoredProcCommand("otpValidateByEmail");
            db.AddInParameter(cmd, "@OtpEmail", DbType.String, email);
            db.AddInParameter(cmd, "@OtpCode", DbType.String, otpCode);

            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : (int?)null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ValidateOtpByEmail");
                throw;
            }
        }

        public DateTime? GetLatestOtpTimeByEmail(string email)
        {
            DbCommand cmd = db.GetSqlStringCommand("SELECT MAX(createdAt) FROM batteryOtp WHERE otpEmail = @Email");
            db.AddInParameter(cmd, "@Email", DbType.String, email);

            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null && result != DBNull.Value ? (DateTime?)Convert.ToDateTime(result) : null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetLatestOtpTimeByEmail");
                throw;
            }
        }

        public void RevokeRefreshToken(int refreshTokenId)
        {
            DbCommand cmd = db.GetStoredProcCommand("refreshTokenRevoke");
            db.AddInParameter(cmd, "@RefreshTokenId", DbType.Int32, refreshTokenId);

            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in RevokeRefreshToken");
                throw;
            }
        }
    }
}

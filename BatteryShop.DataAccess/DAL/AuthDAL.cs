using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using BatteryShop.DataAccess.Models;
using BatteryShop.DataAccess.ViewModels;
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

        public List<PendingOwnerViewModel> GetPendingOwners()
        {
            List<PendingOwnerViewModel> list = new List<PendingOwnerViewModel>();

            DbCommand cmd = db.GetStoredProcCommand("pendingOwnerGetList");

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new PendingOwnerViewModel
                        {
                            PendingOwnerId = Convert.ToInt32(reader["pendingOwnerId"]),
                            OwnerName = reader["ownerName"].ToString(),
                            OwnerPhone = reader["ownerPhone"].ToString(),
                            OwnerEmail = reader["ownerEmail"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["createdAt"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetPendingOwners");
                throw;
            }

            return list;
        }

        public int ApprovePendingOwner(int pendingOwnerId, int approvedBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("pendingOwnerApprove");
            db.AddInParameter(cmd, "@PendingOwnerId", DbType.Int32, pendingOwnerId);
            db.AddInParameter(cmd, "@ApprovedBy", DbType.Int32, approvedBy);

            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ApprovePendingOwner");
                throw;
            }
        }

        public void RejectPendingOwner(int pendingOwnerId)
        {
            DbCommand cmd = db.GetStoredProcCommand("pendingOwnerReject");
            db.AddInParameter(cmd, "@PendingOwnerId", DbType.Int32, pendingOwnerId);

            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in RejectPendingOwner");
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

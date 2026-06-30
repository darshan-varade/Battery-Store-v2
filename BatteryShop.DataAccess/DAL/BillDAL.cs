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
    public class BillDAL
    {
        private Database db;

        public BillDAL()
        {
            this.db = DatabaseFactory.CreateDatabase();
        }

        public List<BillModel> BillGetList(BillViewModel vm)
        {
            vm.PageNumber = vm.PageNumber <= 0 ? 1 : vm.PageNumber;
            vm.PageSize = vm.PageSize <= 0 ? 100000 : vm.PageSize;
            vm.SearchTerm = vm.SearchTerm ?? "";
            vm.Phone = vm.Phone ?? "";

            List<BillModel> list = new List<BillModel>();

            DbCommand cmd = db.GetStoredProcCommand("billGetList");

            db.AddInParameter(cmd, "@PageNumber", DbType.Int32, vm.PageNumber);
            db.AddInParameter(cmd, "@PageSize", DbType.Int32, vm.PageSize);
            db.AddInParameter(cmd, "@SearchTerm", DbType.String, vm.SearchTerm);
            db.AddInParameter(cmd, "@Phone", DbType.String, vm.Phone);
            db.AddInParameter(cmd, "@DateFrom", DbType.Date, (object)vm.DateFrom ?? DBNull.Value);
            db.AddInParameter(cmd, "@DateTo", DbType.Date, (object)vm.DateTo ?? DBNull.Value);
            db.AddInParameter(cmd, "@SortColumn", DbType.String, vm.SortColumn ?? "billId");
            db.AddInParameter(cmd, "@SortDirection", DbType.String, vm.SortDirection ?? "DESC");
            db.AddOutParameter(cmd, "@TotalRows", DbType.Int32, 0);

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        BillModel item = new BillModel
                        {
                            BillId = Convert.ToInt32(reader["billId"]),
                            UserId = Convert.ToInt32(reader["userId"]),
                            UserFullName = reader["userFullName"].ToString(),
                            UserPhone = reader["userPhone"].ToString(),
                            DateOfSale = Convert.ToDateTime(reader["dateOfSale"]),
                            TotalAmount = Convert.ToDecimal(reader["totalAmount"]),
                            PaidAmount = Convert.ToDecimal(reader["paidAmount"])
                        };
                        list.Add(item);
                    }
                }
                vm.TotalRows = db.GetParameterValue(cmd, "@TotalRows") != DBNull.Value
                    ? Convert.ToInt32(db.GetParameterValue(cmd, "@TotalRows"))
                    : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in BillGetList");
                throw;
            }
            return list;
        }

        public BillModel BillGetById(int billId)
        {
            BillModel item = null;

            DbCommand cmd = db.GetStoredProcCommand("billGetById");
            db.AddInParameter(cmd, "@BillId", DbType.Int32, billId);

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        item = new BillModel
                        {
                            BillId = Convert.ToInt32(reader["billId"]),
                            UserId = Convert.ToInt32(reader["userId"]),
                            UserFullName = reader["userFullName"].ToString(),
                            UserPhone = reader["userPhone"].ToString(),
                            DateOfSale = Convert.ToDateTime(reader["dateOfSale"]),
                            TotalAmount = Convert.ToDecimal(reader["totalAmount"]),
                            PaidAmount = Convert.ToDecimal(reader["paidAmount"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in BillGetById");
                throw;
            }

            return item;
        }

        public void BillDelete(int billId)
        {
            DbCommand cmd = db.GetStoredProcCommand("billDelete");
            db.AddInParameter(cmd, "@BillId", DbType.Int32, billId);

            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in BillDelete");
                throw;
            }
        }

        public List<VehicleModelViewModel> GetBillItemTypes()
        {
            List<VehicleModelViewModel> list = new List<VehicleModelViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("fetchBillItemTypes");

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new VehicleModelViewModel
                        {
                            TypeId = Convert.ToInt32(reader["TypeId"]),
                            TypeName = reader["TypeName"].ToString(),
                            BrandId = Convert.ToInt32(reader["BrandId"]),
                            itemPrice = reader["ItemPrice"] != DBNull.Value ? Convert.ToDecimal(reader["ItemPrice"]) : 0,
                            oldItemPrice = reader["OldItemPrice"] != DBNull.Value ? Convert.ToDecimal(reader["OldItemPrice"]) : 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetBillItemTypes");
                throw;
            }
            return list;
        }

        public List<OldItemStatusViewModel> GetOldItemStatusList()
        {
            List<OldItemStatusViewModel> list = new List<OldItemStatusViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("FetchOldItemStatusValues");

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new OldItemStatusViewModel
                        {
                            OldItemStatusId = Convert.ToInt32(reader["oldItemStatusId"]),
                            OldItemStatusName = reader["oldItemStatusName"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetOldItemStatusList");
                throw;
            }
            return list;
        }

        public List<VehicleBrandViewModel> GetVehicleBrands()
        {
            List<VehicleBrandViewModel> list = new List<VehicleBrandViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("FetchVehicleBrands");

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new VehicleBrandViewModel
                        {
                            BrandId = Convert.ToInt32(reader["vehicleBrandId"]),
                            BrandName = reader["vehicleBrandName"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetVehicleBrands");
                throw;
            }
            return list;
        }

        public List<VehicleModelInfoViewModel> GetVehicleModelsByBrand(int brandId)
        {
            List<VehicleModelInfoViewModel> list = new List<VehicleModelInfoViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("FetchVehicleModelsByBrand");
            db.AddInParameter(cmd, "@BrandId", DbType.Int32, brandId);

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new VehicleModelInfoViewModel
                        {
                            VehicleModelId = Convert.ToInt32(reader["vehicleModelId"]),
                            VehicleModelName = reader["vehicleModelName"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetVehicleModelsByBrand");
                throw;
            }
            return list;
        }

        public List<Dictionary<string, object>> FetchAvailableSerials(int brandId, int typeId, int count)
        {
            var list = new List<Dictionary<string, object>>();
            DbCommand cmd = db.GetStoredProcCommand("fetchAvailableSerials");
            db.AddInParameter(cmd, "@BrandId", DbType.Int32, brandId);
            db.AddInParameter(cmd, "@TypeId", DbType.Int32, typeId);
            db.AddInParameter(cmd, "@Count", DbType.Int32, count);

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        list.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in FetchAvailableSerials");
                throw;
            }
            return list;
        }

        public decimal GetDiscountPercentage(int itemTypeId, DateTime oldItemDateOfSale)
        {
            DbCommand cmd = db.GetStoredProcCommand("getDiscountPercentage");
            db.AddInParameter(cmd, "@ItemTypeId", DbType.Int32, itemTypeId);
            db.AddInParameter(cmd, "@OldItemDateOfSale", DbType.Date, oldItemDateOfSale);
            db.AddOutParameter(cmd, "@DiscountPercent", DbType.Decimal, 0);

            try
            {
                db.ExecuteNonQuery(cmd);
                var val = db.GetParameterValue(cmd, "@DiscountPercent");
                return val != DBNull.Value ? Convert.ToDecimal(val) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetDiscountPercentage");
                throw;
            }
        }

        public int AddVehicleInfo(int modelId, string regNumber, int ownerId)
        {
            DbCommand cmd = db.GetStoredProcCommand("vehicleInfoAdd");
            db.AddInParameter(cmd, "@ModelId", DbType.Int32, modelId);
            db.AddInParameter(cmd, "@RegNumber", DbType.String, regNumber);
            db.AddInParameter(cmd, "@CreatedBy", DbType.Int32, ownerId);

            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddVehicleInfo");
                throw;
            }
        }

        public int GetAvailableCount(int brandId, int typeId)
        {
            DbCommand cmd = db.GetStoredProcCommand("getAvailableCount");
            db.AddInParameter(cmd, "@BrandId", DbType.Int32, brandId);
            db.AddInParameter(cmd, "@TypeId", DbType.Int32, typeId);

            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetAvailableCount");
                throw;
            }
        }

        public int BillAdd(int? customerId, string customerName, string customerPhone, string customerCity,
            DateTime dateOfSale, decimal totalAmount, decimal paidAmount, string itemsJson)
        {
            DbCommand cmd = db.GetStoredProcCommand("billAdd");
            db.AddInParameter(cmd, "@CustomerId", DbType.Int32, (object)customerId ?? DBNull.Value);
            db.AddInParameter(cmd, "@CustomerName", DbType.String, customerName ?? "");
            db.AddInParameter(cmd, "@CustomerPhone", DbType.String, customerPhone ?? "");
            db.AddInParameter(cmd, "@CustomerCity", DbType.String, customerCity ?? "");
            db.AddInParameter(cmd, "@DateOfSale", DbType.Date, dateOfSale);
            db.AddInParameter(cmd, "@TotalAmount", DbType.Decimal, totalAmount);
            db.AddInParameter(cmd, "@PaidAmount", DbType.Decimal, paidAmount);
            db.AddInParameter(cmd, "@ItemsJson", DbType.String, itemsJson);
            db.AddInParameter(cmd, "@CreatedBy", DbType.Int32, 1);

            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in BillAdd");
                throw;
            }
        }
    }
}

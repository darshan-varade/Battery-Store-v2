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
    }
}

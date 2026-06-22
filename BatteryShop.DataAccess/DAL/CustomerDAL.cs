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
    public class CustomerDAL
    {
        private Database db;

        public CustomerDAL()
        {
            this.db = DatabaseFactory.CreateDatabase();
        }

        public List<CustomerModel> CustomerGetList(CustomerViewModel vm)
        {
            vm.PageNumber = vm.PageNumber <= 0 ? 1 : vm.PageNumber;
            vm.PageSize = vm.PageSize <= 0 ? 100000 : vm.PageSize;
            vm.SearchTerm = vm.SearchTerm ?? "";
            vm.Phone = vm.Phone ?? "";

            List<CustomerModel> list = new List<CustomerModel>();

            DbCommand cmd = db.GetStoredProcCommand("customerGetList");

            db.AddInParameter(cmd, "@PageNumber", DbType.Int32, vm.PageNumber);
            db.AddInParameter(cmd, "@PageSize", DbType.Int32, vm.PageSize);
            db.AddInParameter(cmd, "@SearchTerm", DbType.String, vm.SearchTerm);
            db.AddInParameter(cmd, "@Phone", DbType.String, vm.Phone);
            db.AddInParameter(cmd, "@CityId", DbType.Int32, (object)vm.CityId ?? DBNull.Value);
            db.AddInParameter(cmd, "@SortColumn", DbType.String, vm.SortColumn ?? "userId");
            db.AddInParameter(cmd, "@SortDirection", DbType.String, vm.SortDirection ?? "ASC");
            db.AddOutParameter(cmd, "@TotalRows", DbType.Int32, 0);

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        CustomerModel item = new CustomerModel
                        {
                            UserId = Convert.ToInt32(reader["userId"]),
                            UserFullName = reader["userFullName"].ToString(),
                            UserPhone = reader["userPhone"].ToString(),
                            CityId = Convert.ToInt32(reader["cityId"]),
                            UserCity = reader["userCity"].ToString(),
                            UserBalance = Convert.ToDecimal(reader["userBalance"]),
                            IsActive = Convert.ToBoolean(reader["isActive"]),
                            CreatedAt = Convert.ToDateTime(reader["createdAt"]),
                            CreatedBy = reader["createdBy"].ToString(),
                            LastModifiedAt = Convert.ToDateTime(reader["lastModifiedAt"]),
                            LastModifiedBy = reader["lastModifiedBy"].ToString()
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
                Log.Error(ex, "Error in CustomerGetList");
                throw;
            }
            return list;
        }

        public CustomerUpdateViewModel CustomerGetById(int userId)
        {
            CustomerUpdateViewModel item = null;

            DbCommand cmd = db.GetStoredProcCommand("customerGetById");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        item = new CustomerUpdateViewModel
                        {
                            UserId = Convert.ToInt32(reader["userId"]),
                            UserFullName = reader["userFullName"].ToString(),
                            UserPhone = reader["userPhone"].ToString(),
                            CityName = reader["userCity"].ToString(),
                            UserBalance = Convert.ToDecimal(reader["userBalance"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerGetById");
                throw;
            }

            return item;
        }

        public List<CustomerModel> CustomerSearch(string term)
        {
            List<CustomerModel> list = new List<CustomerModel>();

            DbCommand cmd = db.GetStoredProcCommand("customerSearch");
            db.AddInParameter(cmd, "@SearchTerm", DbType.String, term ?? "");

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        CustomerModel item = new CustomerModel
                        {
                            UserId = Convert.ToInt32(reader["userId"]),
                            UserFullName = reader["userFullName"].ToString(),
                            UserPhone = reader["userPhone"].ToString(),
                            CityId = Convert.ToInt32(reader["cityId"]),
                            UserCity = reader["cityName"].ToString()
                        };
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerSearch");
                throw;
            }

            return list;
        }

        public List<CustomerModel> CustomerSearchByPhone(string term)
        {
            List<CustomerModel> list = new List<CustomerModel>();

            DbCommand cmd = db.GetStoredProcCommand("customerSearchByPhone");
            db.AddInParameter(cmd, "@SearchTerm", DbType.String, term ?? "");

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        CustomerModel item = new CustomerModel
                        {
                            UserId = Convert.ToInt32(reader["userId"]),
                            UserFullName = reader["userFullName"].ToString(),
                            UserPhone = reader["userPhone"].ToString(),
                            CityId = Convert.ToInt32(reader["cityId"]),
                            UserCity = reader["cityName"].ToString()
                        };
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerSearchByPhone");
                throw;
            }

            return list;
        }

        public void CustomerAdd(CustomerUpdateViewModel vm)
        {
            DbCommand cmd = db.GetStoredProcCommand("customerAdd");
            db.AddInParameter(cmd, "@FullName", DbType.String, vm.UserFullName);
            db.AddInParameter(cmd, "@Phone", DbType.String, vm.UserPhone);
            db.AddInParameter(cmd, "@CityName", DbType.String, vm.CityName);
            db.AddInParameter(cmd, "@CreatedBy", DbType.Int32, 1);

            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerAdd");
                throw;
            }
        }

        public void CustomerUpdate(CustomerUpdateViewModel vm)
        {
            DbCommand cmd = db.GetStoredProcCommand("customerUpdate");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, vm.UserId);
            db.AddInParameter(cmd, "@FullName", DbType.String, vm.UserFullName);
            db.AddInParameter(cmd, "@Phone", DbType.String, vm.UserPhone);
            db.AddInParameter(cmd, "@CityName", DbType.String, vm.CityName);
            db.AddInParameter(cmd, "@ModifiedBy", DbType.Int32, 1);

            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerUpdate");
                throw;
            }
        }

        public void CustomerDelete(int userId)
        {
            DbCommand cmd = db.GetStoredProcCommand("customerDelete");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);

            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerDelete");
                throw;
            }
        }

        public List<CityListViewModel> CustomerGetDistinctCities()
        {
            List<CityListViewModel> list = new List<CityListViewModel>();

            DbCommand cmd = db.GetStoredProcCommand("customerGetDistinctCities");

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        CityListViewModel item = new CityListViewModel
                        {
                            CityId = Convert.ToInt32(reader["cityId"]),
                            CityName = reader["cityName"].ToString()
                        };
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CustomerGetDistinctCities");
                throw;
            }

            return list;
        }
    }
}
